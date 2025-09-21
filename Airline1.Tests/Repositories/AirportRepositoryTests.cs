using Xunit;
using Moq;
using Airline1.Repositories.Implementations;
using Airline1.Repositories.Interfaces;
using Airline1.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Airline1.Data;

namespace Airline1.Tests.Repositories
{
    public class AirportRepositoryTests
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public AirportRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "AirportRepoTestDb")
                .Options;
        }

        [Fact]
        public async Task AddAsync_AddsAirport()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var repo = new Airline1.Repositories.Implementations.AirportRepository(context);
                var airport = new Airport { Name = "Test Airport" };
                var result = await repo.AddAsync(airport);
                Assert.Equal("Test Airport", result.Name);
                Assert.True(context.Airports.Any(a => a.Name == "Test Airport"));
            }
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllAirports()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                context.Airports.Add(new Airport { Name = "A1" });
                context.Airports.Add(new Airport { Name = "A2" });
                context.SaveChanges();
                var repo = new Airline1.Repositories.Implementations.AirportRepository(context);
                var result = await repo.GetAllAsync();
                Assert.True(result.Count >= 2);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsCorrectAirport()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var airport = new Airport { Name = "FindMe" };
                context.Airports.Add(airport);
                context.SaveChanges();
                var repo = new Airline1.Repositories.Implementations.AirportRepository(context);
                var found = await repo.GetByIdAsync(airport.Id);
                Assert.NotNull(found);
                Assert.Equal("FindMe", found.Name);
            }
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAirport()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var airport = new Airport { Name = "OldName" };
                context.Airports.Add(airport);
                context.SaveChanges();
                var repo = new Airline1.Repositories.Implementations.AirportRepository(context);
                airport.Name = "NewName";
                await repo.UpdateAsync(airport);
                var updated = context.Airports.First(a => a.Id == airport.Id);
                Assert.Equal("NewName", updated.Name);
            }
        }

        [Fact]
        public async Task DeleteAsync_RemovesAirport()
        {
            using (var context = new AppDbContext(_dbOptions))
            {
                var airport = new Airport { Name = "ToDelete" };
                context.Airports.Add(airport);
                context.SaveChanges();
                var repo = new Airline1.Repositories.Implementations.AirportRepository(context);
                await repo.DeleteAsync(airport);
                Assert.False(context.Airports.Any(a => a.Id == airport.Id));
            }
        }
    }
}
