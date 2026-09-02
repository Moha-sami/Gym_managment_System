using AutoMapper;
using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Class;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Class;
using GymMangment.BLL.ViewModels.PlansViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GymSystem.Tests
{
    public class PlanServiceTests : IDisposable
    {
        private readonly GymDbcontext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly PlanService _service;

        public PlanServiceTests()
        {
            var options = new DbContextOptionsBuilder<GymDbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbcontext(options);
            _context.Database.EnsureCreated();

            _mockMapper = new Mock<IMapper>();

            _mockMapper.Setup(m => m.Map<IEnumerable<PlanViewModel>>(It.IsAny<IEnumerable<Plans>>()))
                .Returns((IEnumerable<Plans> src) => src.Select(p => new PlanViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DurationInDays = p.DurationInDays,
                    IsActive = p.IsActive
                }));

            _mockMapper.Setup(m => m.Map<PlanViewModel>(It.IsAny<Plans>()))
                .Returns((Plans p) => new PlanViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DurationInDays = p.DurationInDays,
                    IsActive = p.IsActive
                });

            _mockMapper.Setup(m => m.Map(It.IsAny<EditPlanViewModel>(), It.IsAny<Plans>()))
                .Callback((EditPlanViewModel src, Plans dest) =>
                {
                    dest.Description = src.Description;
                    dest.Price = src.Price;
                    dest.DurationInDays = src.DurationInDays;
                });

            var mockSessionRepo = new Mock<ISessionRepository>();
            _unitOfWork = new UnitOfWork(_context, mockSessionRepo.Object);
            _service = new PlanService(_unitOfWork, _mockMapper.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllPlansAsync_ReturnsAllPlans()
        {
            // Arrange
            _context.Plans.AddRange(
                new Plans { Name = "Basic Plan", Description = "Basic access", Price = 50, DurationInDays = 30, IsActive = true },
                new Plans { Name = "Pro Plan", Description = "Pro access", Price = 100, DurationInDays = 60, IsActive = true }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllPlansAsync();

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPlanByIdAsync_WithValidId_ReturnsPlan()
        {
            // Arrange
            var plan = new Plans { Name = "VIP Plan", Description = "VIP access", Price = 200, DurationInDays = 90, IsActive = true };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetPlanByIdAsync(plan.Id);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("VIP Plan", result.Data.Name);
            Assert.Equal(200, result.Data.Price);
        }

        [Fact]
        public async Task GetPlanByIdAsync_WithInvalidId_ReturnsFailure()
        {
            // Act
            var result = await _service.GetPlanByIdAsync(999);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No plan found with this id", result.Error);
        }

        [Fact]
        public async Task EditPlanAsync_WithValidId_UpdatesPlanSuccessfully()
        {
            // Arrange
            var plan = new Plans { Name = "Original Plan", Description = "Original description", Price = 50, DurationInDays = 30, IsActive = true };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var editModel = new EditPlanViewModel
            {
                Description = "Updated description",
                Price = 75,
                DurationInDays = 45
            };

            // Act
            var result = await _service.EditPlanAsync(plan.Id, editModel);

            // Assert
            Assert.True(result.Succeeded);
            var updated = await _context.Plans.FindAsync(plan.Id);
            Assert.NotNull(updated);
            Assert.Equal("Updated description", updated.Description);
            Assert.Equal(75, updated.Price);
            Assert.Equal(45, updated.DurationInDays);
        }

        [Fact]
        public async Task EditPlanAsync_WithInvalidId_ReturnsFailure()
        {
            // Arrange
            var editModel = new EditPlanViewModel
            {
                Description = "Desc",
                Price = 100,
                DurationInDays = 30
            };

            // Act
            var result = await _service.EditPlanAsync(999, editModel);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No plan found with this id", result.Error);
        }

        [Fact]
        public async Task ToggleActivationAsync_WithActivePlan_TogglesToInactive()
        {
            // Arrange
            var plan = new Plans { Name = "Active Plan", Description = "Desc", Price = 50, DurationInDays = 30, IsActive = true };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleActivationAsync(plan.Id);

            // Assert
            Assert.True(result.Succeeded);
            var updated = await _context.Plans.FindAsync(plan.Id);
            Assert.NotNull(updated);
            Assert.False(updated.IsActive);
        }

        [Fact]
        public async Task ToggleActivationAsync_WithInactivePlan_TogglesToActive()
        {
            // Arrange
            var plan = new Plans { Name = "Inactive Plan", Description = "Desc", Price = 50, DurationInDays = 30, IsActive = false };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.ToggleActivationAsync(plan.Id);

            // Assert
            Assert.True(result.Succeeded);
            var updated = await _context.Plans.FindAsync(plan.Id);
            Assert.NotNull(updated);
            Assert.True(updated.IsActive);
        }

        [Fact]
        public async Task ToggleActivationAsync_WithInvalidId_ReturnsFailure()
        {
            // Act
            var result = await _service.ToggleActivationAsync(999);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No plan found with this id", result.Error);
        }
    }
}
