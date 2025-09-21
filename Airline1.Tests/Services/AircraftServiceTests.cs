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
    public class AircraftServiceTests
    {
        private readonly Mock<IAircraftRepository> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AircraftService _service;

        public AircraftServiceTests()
        {
            _mockRepo = new Mock<IAircraftRepository>();
            _mockMapper = new Mock<IMapper>();
            _service = new AircraftService(_mockRepo.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateAsync_Throws_WhenTailNumberExists()
        {
            var request = new CreateAircraftRequest { TailNumber = "T1" };
            _mockRepo.Setup(r => r.GetByTailNumberAsync("T1")).ReturnsAsync(new Aircraft());
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_ReturnsResponse_WhenValid()
        {
            var request = new CreateAircraftRequest { TailNumber = "T2" };
            var model = new Aircraft();
            var added = new Aircraft();
            var response = new AircraftResponse();
            _mockRepo.Setup(r => r.GetByTailNumberAsync("T2")).ReturnsAsync((Aircraft)null);
            _mockMapper.Setup(m => m.Map<Aircraft>(request)).Returns(model);
            _mockRepo.Setup(r => r.AddAsync(model)).ReturnsAsync(added);
            _mockMapper.Setup(m => m.Map<AircraftResponse>(added)).Returns(response);
            var result = await _service.CreateAsync(request);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsMappedList()
        {
            var models = new List<Aircraft> { new Aircraft() };
            var responses = new List<AircraftResponse> { new AircraftResponse() };
            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(models);
            _mockMapper.Setup(m => m.Map<AircraftResponse>(It.IsAny<Aircraft>())).Returns(responses[0]);
            var result = await _service.GetAllAsync();
            Assert.Single(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Aircraft)null);
            var result = await _service.GetByIdAsync(1);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsResponse_WhenFound()
        {
            var model = new Aircraft();
            var response = new AircraftResponse();
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockMapper.Setup(m => m.Map<AircraftResponse>(model)).Returns(response);
            var result = await _service.GetByIdAsync(1);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Aircraft)null);
            var request = new UpdateAircraftRequest();
            var result = await _service.UpdateAsync(1, request);
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsResponse_WhenFound()
        {
            var model = new Aircraft();
            var request = new UpdateAircraftRequest();
            var response = new AircraftResponse();
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockMapper.Setup(m => m.Map(request, model));
            _mockRepo.Setup(r => r.UpdateAsync(model)).Returns(Task.CompletedTask);
            _mockMapper.Setup(m => m.Map<AircraftResponse>(model)).Returns(response);
            var result = await _service.UpdateAsync(1, request);
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Aircraft)null);
            var result = await _service.DeleteAsync(1);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenFound()
        {
            var model = new Aircraft();
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(model);
            _mockRepo.Setup(r => r.DeleteAsync(model)).Returns(Task.CompletedTask);
            var result = await _service.DeleteAsync(1);
            Assert.True(result);
        }
    }
}
