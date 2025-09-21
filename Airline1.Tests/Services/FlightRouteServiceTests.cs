using Xunit;
using Moq;
using Airline1.Services.Implementations;
using Airline1.Repositories.Interfaces;
using Airline1.Dtos.Requests;
using Airline1.Dtos.Responses;
using Airline1.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airline1.Tests.Services
{
    public class FlightRouteServiceTests
    {
        private readonly Mock<IFlightRouteRepository> _mockRepo;
        private readonly Mock<IAirportRepository> _mockAirportRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly FlightRouteService _service;

        public FlightRouteServiceTests()
        {
            _mockRepo = new Mock<IFlightRouteRepository>();
            _mockAirportRepo = new Mock<IAirportRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new FlightRouteService(_mockRepo.Object, _mockAirportRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenOriginNotFound()
        {
            var request = new CreateFlightRouteRequest { OriginAirportId = 1, DestinationAirportId = 2 };
            _mockAirportRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(false);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenDestinationNotFound()
        {
            var request = new CreateFlightRouteRequest { OriginAirportId = 1, DestinationAirportId = 2 };
            _mockAirportRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _mockAirportRepo.Setup(r => r.ExistsAsync(2)).ReturnsAsync(false);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenRouteExists()
        {
            var request = new CreateFlightRouteRequest { OriginAirportId = 1, DestinationAirportId = 2 };
            _mockAirportRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _mockAirportRepo.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetByOriginDestinationAsync(1, 2)).ReturnsAsync(new FlightRoute());
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_ReturnsResponse_WhenValid()
        {
            var request = new CreateFlightRouteRequest { OriginAirportId = 1, DestinationAirportId = 2 };
            var model = new FlightRoute();
            var added = new FlightRoute();
            var response = new FlightRouteResponse();
            _mockAirportRepo.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
            _mockAirportRepo.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetByOriginDestinationAsync(1, 2)).ReturnsAsync((FlightRoute)null);
            _mockMapper.Setup(m => m.Map<FlightRoute>(request)).Returns(model);
            _mockRepo.Setup(r => r.AddAsync(model)).ReturnsAsync(added);
            _mockMapper.Setup(m => m.Map<FlightRouteResponse>(added)).Returns(response);
            var result = await _service.CreateAsync(request);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedList()
        {
            var models = new List<FlightRoute> { new FlightRoute() };
            var responses = new List<FlightRouteResponse> { new FlightRouteResponse() };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(models);
            _mockMapper.Setup(m => m.Map<FlightRouteResponse>(It.IsAny<FlightRoute>())).Returns(responses[0]);
            var result = await _service.GetAllAsync();
            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((FlightRoute)null);
            var result = await _service.GetByIdAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsResponse_WhenFound()
        {
            var model = new FlightRoute();
            var response = new FlightRouteResponse();
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockMapper.Setup(m => m.Map<FlightRouteResponse>(model)).Returns(response);
            var result = await _service.GetByIdAsync(1);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((FlightRoute)null);
            var request = new UpdateFlightRouteRequest();
            var result = await _service.UpdateAsync(1, request);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenOriginNotFound()
        {
            var model = new FlightRoute();
            var request = new UpdateFlightRouteRequest { OriginAirportId = 2 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockAirportRepo.Setup(r => r.ExistsAsync(2)).ReturnsAsync(false);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(1, request));
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenDestinationNotFound()
        {
            var model = new FlightRoute();
            var request = new UpdateFlightRouteRequest { DestinationAirportId = 3 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockAirportRepo.Setup(r => r.ExistsAsync(3)).ReturnsAsync(false);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(1, request));
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenDuplicateRoute()
        {
            var model = new FlightRoute { Id = 1 };
            var request = new UpdateFlightRouteRequest { OriginAirportId = 2, DestinationAirportId = 3 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockAirportRepo.Setup(r => r.ExistsAsync(2)).ReturnsAsync(true);
            _mockAirportRepo.Setup(r => r.ExistsAsync(3)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.GetByOriginDestinationAsync(2, 3)).ReturnsAsync(new FlightRoute { Id = 2 });
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(1, request));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsResponse_WhenValid()
        {
            var model = new FlightRoute { Id = 1 };
            var request = new UpdateFlightRouteRequest();
            var response = new FlightRouteResponse();
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockRepo.Setup(r => r.GetByOriginDestinationAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((FlightRoute)null);
            _mockMapper.Setup(m => m.Map(request, model));
            _mockRepo.Setup(r => r.UpdateAsync(model)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<FlightRouteResponse>(model)).Returns(response);
            _mockAirportRepo.Setup(r => r.ExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            var result = await _service.UpdateAsync(1, request);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((FlightRoute)null);
            var result = await _service.DeleteAsync(1);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenFound()
        {
            var model = new FlightRoute();
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockRepo.Setup(r => r.DeleteAsync(model)).Returns(Task.CompletedTask);
            var result = await _service.DeleteAsync(1);
            Assert.True(result);
        }
    }
}
