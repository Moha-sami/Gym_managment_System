using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Class;
using GymManagment.DAL.Repositories.Interfaces;
using GymmanagmentSystem.PL.Controllers;
using GymMangment.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GymSystem.Tests
{
    public class DeleteRequestsControllerTests : IDisposable
    {
        private readonly GymDbcontext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly Mock<ImemberService> _mockMemberService;
        private readonly Mock<ITrainerService> _mockTrainerService;
        private readonly Mock<ISessionService> _mockSessionService;
        private readonly DeleteRequestsController _controller;

        public DeleteRequestsControllerTests()
        {
            var options = new DbContextOptionsBuilder<GymDbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbcontext(options);
            _context.Database.EnsureCreated();

            var mockSessionRepo = new Mock<ISessionRepository>();
            _unitOfWork = new UnitOfWork(_context, mockSessionRepo.Object);

            _mockMemberService = new Mock<ImemberService>();
            _mockTrainerService = new Mock<ITrainerService>();
            _mockSessionService = new Mock<ISessionService>();

            _controller = new DeleteRequestsController(
                _unitOfWork,
                _mockMemberService.Object,
                _mockTrainerService.Object,
                _mockSessionService.Object
            );

            // Mock User Identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "manager-123"),
                new Claim(ClaimTypes.Name, "manager@gym.com"),
                new Claim(ClaimTypes.Role, "Manager")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Mock TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task SubmitRequest_WithReason_ShouldSaveToDatabaseSuccessfully()
        {
            // Arrange
            var member = new Member { Name = "John Doe", Email = "john@example.com", Phone = "123456" };
            await _context.Member.AddAsync(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.SubmitRequest(
                DeleteTargetType.Member,
                member.Id,
                "Member requested profile termination due to relocation",
                CancellationToken.None
            );

            // Assert
            var savedRequest = await _context.DeleteRequests.FirstOrDefaultAsync();
            Assert.NotNull(savedRequest);
            Assert.Equal(DeleteTargetType.Member, savedRequest.TargetType);
            Assert.Equal(member.Id, savedRequest.TargetId);
            Assert.Equal("Member requested profile termination due to relocation", savedRequest.Reason);
            Assert.Equal(DeleteRequestStatus.Pending, savedRequest.Status);
            Assert.Equal("John Doe", savedRequest.TargetName);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Members", redirectResult.ControllerName);
        }

        [Fact]
        public async Task ApproveRequest_ShouldMarkApprovedAndInvokeDeleteService()
        {
            // Arrange
            var member = new Member { Name = "John Doe", Email = "john@example.com", Phone = "123456" };
            await _context.Member.AddAsync(member);
            await _context.SaveChangesAsync();

            var request = new DeleteRequest
            {
                TargetType = DeleteTargetType.Member,
                TargetId = member.Id,
                TargetName = member.Name,
                Status = DeleteRequestStatus.Pending,
                RequestedByUserId = "manager-123",
                RequestedByName = "manager@gym.com"
            };
            await _context.DeleteRequests.AddAsync(request);
            await _context.SaveChangesAsync();

            _mockMemberService.Setup(s => s.DeleteMemberAsync(member.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(GymMangment.BLL.Common.Result.Success());

            // Act
            var result = await _controller.Approve(request.Id, "Reason validated. Relocation confirmed.", CancellationToken.None);

            // Assert
            var updatedRequest = await _context.DeleteRequests.FindAsync(request.Id);
            Assert.NotNull(updatedRequest);
            Assert.Equal(DeleteRequestStatus.Approved, updatedRequest.Status);
            Assert.Equal("Reason validated. Relocation confirmed.", updatedRequest.AdminNote);

            _mockMemberService.Verify(s => s.DeleteMemberAsync(member.Id, It.IsAny<CancellationToken>()), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }
    }
}
