using AutoMapper;
using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Class;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Class;
using GymMangment.BLL.ViewModels.BadgeViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GymSystem.Tests
{
    public class BadgeServiceTests : IDisposable
    {
        private readonly GymDbcontext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly BadgeService _service;

        public BadgeServiceTests()
        {
            var options = new DbContextOptionsBuilder<GymDbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbcontext(options);
            _context.Database.EnsureCreated();

            _mockMapper = new Mock<IMapper>();

            // Setup mapping from BadgeDefinition to BadgeDefinitionViewModel
            _mockMapper.Setup(m => m.Map<BadgeDefinitionViewModel>(It.IsAny<BadgeDefinition>()))
                .Returns((BadgeDefinition src) => new BadgeDefinitionViewModel
                {
                    Id = src.Id,
                    Name = src.Name,
                    Description = src.Description,
                    IconPath = src.IconPath,
                    Category = src.Category.ToString(),
                    Tier = src.Tier.ToString(),
                    Threshold = src.Threshold,
                    IsAutomatic = src.IsAutomatic
                });

            // Mock session repo for UnitOfWork constructor
            var mockSessionRepo = new Mock<ISessionRepository>();

            _unitOfWork = new UnitOfWork(_context, mockSessionRepo.Object);
            _service = new BadgeService(_unitOfWork, _mockMapper.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task EvaluateAndAwardBadgesAsync_ShouldAwardBadge_WhenWorkoutCountThresholdMet()
        {
            // Arrange
            var member = new Member { Name = "John Doe", Email = "john@example.com", Phone = "123456" };
            await _context.Member.AddAsync(member);

            var badge = new BadgeDefinition
            {
                Name = "Iron Starter",
                Description = "Log your first workout.",
                IconPath = "/images/badges/bronze_badge.jpg",
                Category = BadgeCategory.WorkoutCount,
                Tier = BadgeTier.Bronze,
                Threshold = 1,
                IsAutomatic = true
            };
            await _context.BadgeDefinitions.AddAsync(badge);

            var workout = new WorkoutLog
            {
                Name = "Test Workout",
                Date = DateTime.UtcNow,
                Member = member
            };
            await _context.WorkoutLogs.AddAsync(workout);
            await _context.SaveChangesAsync();

            // Act
            var newlyAwarded = await _service.EvaluateAndAwardBadgesAsync(member.Id);

            // Assert
            Assert.Single(newlyAwarded);
            Assert.Equal("Iron Starter", newlyAwarded.First());

            var earnedBadges = await _context.MemberBadges.Where(mb => mb.MemberId == member.Id).ToListAsync();
            Assert.Single(earnedBadges);
            Assert.Equal(badge.Id, earnedBadges.First().BadgeDefinitionId);
        }

        [Fact]
        public async Task EvaluateAndAwardBadgesAsync_ShouldNotDoubleAward_AlreadyEarnedBadge()
        {
            // Arrange
            var member = new Member { Name = "John Doe", Email = "john@example.com", Phone = "123456" };
            await _context.Member.AddAsync(member);

            var badge = new BadgeDefinition
            {
                Name = "Iron Starter",
                Description = "Log your first workout.",
                IconPath = "/images/badges/bronze_badge.jpg",
                Category = BadgeCategory.WorkoutCount,
                Tier = BadgeTier.Bronze,
                Threshold = 1,
                IsAutomatic = true
            };
            await _context.BadgeDefinitions.AddAsync(badge);

            var workout = new WorkoutLog
            {
                Name = "Test Workout",
                Date = DateTime.UtcNow,
                Member = member
            };
            await _context.WorkoutLogs.AddAsync(workout);
            await _context.SaveChangesAsync();

            // Pre-award the badge
            var existingAward = new MemberBadge
            {
                MemberId = member.Id,
                BadgeDefinitionId = badge.Id,
                EarnedAtUtc = DateTime.UtcNow
            };
            await _context.MemberBadges.AddAsync(existingAward);
            await _context.SaveChangesAsync();

            // Act
            var newlyAwarded = await _service.EvaluateAndAwardBadgesAsync(member.Id);

            // Assert
            Assert.Empty(newlyAwarded); // Should not award again

            var allAwards = await _context.MemberBadges.Where(mb => mb.MemberId == member.Id).ToListAsync();
            Assert.Single(allAwards); // Still just one award
        }

        [Fact]
        public async Task AwardManualBadgeAsync_ShouldAwardBadgeSuccessfully_ForAdmin()
        {
            // Arrange
            var member = new Member { Name = "John Doe", Email = "john@example.com", Phone = "123456" };
            await _context.Member.AddAsync(member);

            var badge = new BadgeDefinition
            {
                Name = "Member of the Month",
                Description = "Awarded manually.",
                IconPath = "/images/badges/special_badge.jpg",
                Category = BadgeCategory.WorkoutCount,
                Tier = BadgeTier.Gold,
                Threshold = null,
                IsAutomatic = false
            };
            await _context.BadgeDefinitions.AddAsync(badge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.AwardManualBadgeAsync(member.Id, badge.Id, "admin-user-id");

            // Assert
            Assert.True(result.Succeeded);

            var earned = await _context.MemberBadges.FirstOrDefaultAsync(mb => mb.MemberId == member.Id);
            Assert.NotNull(earned);
            Assert.Equal(badge.Id, earned.BadgeDefinitionId);
            Assert.Equal("admin-user-id", earned.AwardedByUserId);
        }

        [Fact]
        public async Task GetLeaderboardAsync_ShouldReturnMembersOrderedByBadgeCount()
        {
            // Arrange
            var member1 = new Member { Name = "User One", Email = "user1@example.com", Phone = "111" };
            var member2 = new Member { Name = "User Two", Email = "user2@example.com", Phone = "222" };
            await _context.Member.AddRangeAsync(member1, member2);

            var badge1 = new BadgeDefinition { Name = "Badge A", Description = "A", IconPath = "A", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Bronze, IsAutomatic = true };
            var badge2 = new BadgeDefinition { Name = "Badge B", Description = "B", IconPath = "B", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Bronze, IsAutomatic = true };
            await _context.BadgeDefinitions.AddRangeAsync(badge1, badge2);
            await _context.SaveChangesAsync();

            // Member 1 gets 2 badges
            await _context.MemberBadges.AddRangeAsync(
                new MemberBadge { MemberId = member1.Id, BadgeDefinitionId = badge1.Id, EarnedAtUtc = DateTime.UtcNow },
                new MemberBadge { MemberId = member1.Id, BadgeDefinitionId = badge2.Id, EarnedAtUtc = DateTime.UtcNow }
            );

            // Member 2 gets 1 badge
            await _context.MemberBadges.AddAsync(
                new MemberBadge { MemberId = member2.Id, BadgeDefinitionId = badge1.Id, EarnedAtUtc = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetLeaderboardAsync(10);

            // Assert
            Assert.True(result.Succeeded);
            var leaderboard = result.Data!.ToList();
            Assert.Equal(2, leaderboard.Count);
            
            // Member 1 should be rank 1
            Assert.Equal("User One", leaderboard[0].MemberName);
            Assert.Equal(1, leaderboard[0].Rank);
            Assert.Equal(2, leaderboard[0].BadgeCount);

            // Member 2 should be rank 2
            Assert.Equal("User Two", leaderboard[1].MemberName);
            Assert.Equal(2, leaderboard[1].Rank);
            Assert.Equal(1, leaderboard[1].BadgeCount);
        }
    }
}
