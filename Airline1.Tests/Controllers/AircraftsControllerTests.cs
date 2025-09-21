using Xunit;
using Moq;
using Airline1.Controllers;
using Airline1.Services.Interfaces;
using Airline1.Dtos.Requests;
using Airline1.Dtos.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Airline1.Tests.Controllers
{
    public class AircraftsControllerTests
    {
        private readonly Mock<IAircraftService> _mockService;
        private readonly AircraftsController _controller;

        public AircraftsControllerTests()
        {
            _mockService = new Mock<IAircraftService>();
            _controller = new AircraftsController(_mockService.Object);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WhenValid()
        {
            var request = new CreateAircraftRequest();
            var response = new AircraftResponse { Id = 1 };
            _mockService.Setup(s => s.CreateAsync(request)).ReturnsAsync(response);

            var result = await _controller.Create(request);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.Equal(response, createdResult.Value);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithList()
        {
            var list = new List<AircraftResponse> { new AircraftResponse() };
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(list);

            var result = await _controller.GetAll();
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(list, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenFound()
        {
            var response = new AircraftResponse { Id = 1 };
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(response);

            var result = await _controller.GetById(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenNull()
        {
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync((AircraftResponse)null);

            var result = await _controller.GetById(1);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Update_ReturnsOk_WhenUpdated()
        {
            var request = new UpdateAircraftRequest();
            var response = new AircraftResponse { Id = 1 };
            _mockService.Setup(s => s.UpdateAsync(1, request)).ReturnsAsync(response);

            var result = await _controller.Update(1, request);
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenNull()
        {
            var request = new UpdateAircraftRequest();
            _mockService.Setup(s => s.UpdateAsync(1, request)).ReturnsAsync((AircraftResponse)null);

            var result = await _controller.Update(1, request);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenDeleted()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenNotDeleted()
        {
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(false);

            var result = await _controller.Delete(1);
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
