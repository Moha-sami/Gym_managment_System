using AutoMapper;
using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Class;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Class;
using GymMangment.BLL.ViewModels.HealthRecordsViewModels;
using GymMangment.BLL.ViewModels.MemberViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GymSystem.Tests
{
    public class MemberServiceTests : IDisposable
    {
        private readonly GymDbcontext _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly UnitOfWork _unitOfWork;
        private readonly MemberService _service;

        public MemberServiceTests()
        {
            var options = new DbContextOptionsBuilder<GymDbcontext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new GymDbcontext(options);
            _context.Database.EnsureCreated();

            _mockMapper = new Mock<IMapper>();

            _mockMapper.Setup(m => m.Map<Member>(It.IsAny<CreateMemberViewModel>()))
                .Returns((CreateMemberViewModel src) => new Member
                {
                    Name = src.Name,
                    Email = src.Email,
                    Phone = src.Phone,
                    DateOFBirth = src.DateOfBirth,
                    Gender = src.Gender,
                    Photo = src.PhotoPath,
                    Address = new Address { City = src.City, Street = src.Street, BuildingNumber = src.BuildingNumber }
                });

            _mockMapper.Setup(m => m.Map<IEnumerable<MemberViewModel>>(It.IsAny<IEnumerable<Member>>()))
                .Returns((IEnumerable<Member> src) => src.Select(m => new MemberViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Phone = m.Phone
                }));

            _mockMapper.Setup(m => m.Map<MemberViewModel>(It.IsAny<Member>()))
                .Returns((Member m) => new MemberViewModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Email = m.Email,
                    Phone = m.Phone
                });

            _mockMapper.Setup(m => m.Map<HealthRecordViewModel>(It.IsAny<HealthRecord>()))
                .Returns((HealthRecord h) => new HealthRecordViewModel
                {
                    Height = h.Height,
                    Weight = h.Weight,
                    BloodType = h.BloodType ?? "O+",
                    Note = h.Note
                });

            _mockMapper.Setup(m => m.Map<MemberToUpdateViewModel>(It.IsAny<Member>()))
                .Returns((Member m) => new MemberToUpdateViewModel
                {
                    Id = m.Id,
                    Email = m.Email,
                    Phone = m.Phone
                });

            _mockMapper.Setup(m => m.Map(It.IsAny<MemberToUpdateViewModel>(), It.IsAny<Member>()))
                .Callback((MemberToUpdateViewModel src, Member dest) =>
                {
                    dest.Email = src.Email;
                    dest.Phone = src.Phone;
                    dest.Address = new Address { City = src.City, Street = src.Street, BuildingNumber = src.BuildingNumber };
                });

            var mockSessionRepo = new Mock<ISessionRepository>();
            _unitOfWork = new UnitOfWork(_context, mockSessionRepo.Object);
            _service = new MemberService(_unitOfWork, _mockMapper.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateMemberAsync_WithValidData_ReturnsSuccessAndAddsProgressRecord()
        {
            // Arrange
            var model = new CreateMemberViewModel
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                DateOfBirth = new DateOnly(1995, 5, 20),
                Gender = Gender.Male,
                BuildingNumber = 12,
                Street = "Main St",
                City = "Metropolis",
                PhotoPath = "/images/default.png",
                HealthRecordViewModel = new HealthRecordViewModel
                {
                    Height = 180,
                    Weight = 80,
                    BloodType = "O+",
                    Note = "Healthy"
                }
            };

            // Act
            var result = await _service.CreateMemberAsync(model);

            // Assert
            Assert.True(result.Succeeded);
            var member = await _context.Member.FirstOrDefaultAsync(m => m.Email == "john@example.com");
            Assert.NotNull(member);
            Assert.Equal("John Doe", member.Name);
            Assert.Equal("/images/default.png", member.Photo);

            var progressRecords = await _context.WeightProgressRecords.Where(w => w.MemberId == member.Id).ToListAsync();
            Assert.Single(progressRecords);
            Assert.Equal(80m, progressRecords[0].Weight);
        }

        [Fact]
        public async Task CreateMemberAsync_WithDuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var existingMember = new Member
            {
                Name = "Alice",
                Email = "duplicate@example.com",
                Phone = "111222333",
                Address = new Address { City = "City", Street = "Street", BuildingNumber = 1 }
            };
            _context.Member.Add(existingMember);
            await _context.SaveChangesAsync();

            var model = new CreateMemberViewModel
            {
                Name = "Bob",
                Email = "duplicate@example.com",
                Phone = "999888777",
                HealthRecordViewModel = new HealthRecordViewModel { Weight = 75 }
            };

            // Act
            var result = await _service.CreateMemberAsync(model);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("A member with this email already exists.", result.Error);
        }

        [Fact]
        public async Task CreateMemberAsync_WithDuplicatePhone_ReturnsFailure()
        {
            // Arrange
            var existingMember = new Member
            {
                Name = "Alice",
                Email = "alice@example.com",
                Phone = "111222333",
                Address = new Address { City = "City", Street = "Street", BuildingNumber = 1 }
            };
            _context.Member.Add(existingMember);
            await _context.SaveChangesAsync();

            var model = new CreateMemberViewModel
            {
                Name = "Bob",
                Email = "bob@example.com",
                Phone = "111222333",
                HealthRecordViewModel = new HealthRecordViewModel { Weight = 75 }
            };

            // Act
            var result = await _service.CreateMemberAsync(model);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("A member with this phone number already exists.", result.Error);
        }

        [Fact]
        public async Task GetAllMembersAsync_ReturnsAllMembers()
        {
            // Arrange
            _context.Member.AddRange(
                new Member { Name = "User 1", Email = "u1@test.com", Phone = "111", Address = new Address { City = "C1", Street = "S1", BuildingNumber = 1 } },
                new Member { Name = "User 2", Email = "u2@test.com", Phone = "222", Address = new Address { City = "C2", Street = "S2", BuildingNumber = 2 } }
            );
            await _context.SaveChangesAsync();

            // Act
            var members = await _service.GetAllMembersAsync();

            // Assert
            Assert.Equal(2, members.Count());
        }

        [Fact]
        public async Task GetMemberDetailsByIdAsync_WithValidIdAndActiveMembership_ReturnsDetailsWithPlan()
        {
            // Arrange
            var plan = new Plans { Name = "Gold Plan", Description = "Gold access", Price = 100, DurationInDays = 30, IsActive = true };
            _context.Plans.Add(plan);
            await _context.SaveChangesAsync();

            var member = new Member
            {
                Name = "Charlie",
                Email = "charlie@test.com",
                Phone = "333444",
                Address = new Address { City = "City", Street = "Street", BuildingNumber = 5 },
                Memberships = new List<Membership>
                {
                    new Membership
                    {
                        PlansID = plan.Id,
                        CreatedAt = DateTime.Now.AddDays(-5),
                        EndDate = DateTime.Now.AddDays(25)
                    }
                }
            };
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMemberDetailsByIdAsync(member.Id);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal("Charlie", result.Data.Name);
            Assert.Equal("Gold Plan", result.Data.PlanName);
        }

        [Fact]
        public async Task GetMemberDetailsByIdAsync_WithInvalidId_ReturnsFailure()
        {
            // Act
            var result = await _service.GetMemberDetailsByIdAsync(999);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Invalid Member", result.Error);
        }

        [Fact]
        public async Task GetMemberHealthRecordAsync_WithExistingRecord_ReturnsSuccess()
        {
            // Arrange
            var member = new Member
            {
                Name = "Health Test",
                Email = "health@test.com",
                Phone = "555",
                Address = new Address { City = "City", Street = "Street", BuildingNumber = 1 }
            };
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            var record = new HealthRecord
            {
                MemberId = member.Id,
                Height = 175,
                Weight = 70,
                BloodType = "A+",
                Note = "None",
                CreatedAt = DateTime.Now
            };
            _context.HealthRecord.Add(record);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetMemberHealthRecordAsync(member.Id);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(175, result.Data.Height);
            Assert.Equal(70, result.Data.Weight);
        }

        [Fact]
        public async Task GetMemberHealthRecordAsync_WithNoRecord_ReturnsFailure()
        {
            // Act
            var result = await _service.GetMemberHealthRecordAsync(999);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No record for This member", result.Error);
        }

        [Fact]
        public async Task UpdateMemberAsync_WithValidData_UpdatesMemberSuccessfully()
        {
            // Arrange
            var member = new Member
            {
                Name = "Original Name",
                Email = "original@test.com",
                Phone = "000111",
                Address = new Address { City = "City", Street = "Street", BuildingNumber = 1 },
                HealthRecord = new HealthRecord { Height = 170, Weight = 65, BloodType = "B+" }
            };
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            var updateModel = new MemberToUpdateViewModel
            {
                Id = member.Id,
                Email = "updated@test.com",
                Phone = "000222",
                BuildingNumber = 20,
                Street = "Updated St",
                City = "Updated City",
                Height = 172,
                Weight = 68,
                BloodType = "B+",
                Note = "Gained weight",
                PhotoPath = "/images/updated.png"
            };

            // Act
            var result = await _service.UpdateMemberAsync(member.Id, updateModel);

            // Assert
            Assert.True(result.Succeeded);
            var updatedMember = await _context.Member.Include(m => m.HealthRecord).FirstOrDefaultAsync(m => m.Id == member.Id);
            Assert.NotNull(updatedMember);
            Assert.Equal("updated@test.com", updatedMember.Email);
            Assert.Equal("000222", updatedMember.Phone);
            Assert.Equal("/images/updated.png", updatedMember.Photo);
            Assert.NotNull(updatedMember.HealthRecord);
            Assert.Equal(172, updatedMember.HealthRecord.Height);
            Assert.Equal(68, updatedMember.HealthRecord.Weight);
        }

        [Fact]
        public async Task UpdateMemberAsync_WithDuplicateEmail_ReturnsFailure()
        {
            // Arrange
            var member1 = new Member { Name = "M1", Email = "m1@test.com", Phone = "111", Address = new Address { City = "C", Street = "S", BuildingNumber = 1 } };
            var member2 = new Member { Name = "M2", Email = "m2@test.com", Phone = "222", Address = new Address { City = "C", Street = "S", BuildingNumber = 1 } };
            _context.Member.AddRange(member1, member2);
            await _context.SaveChangesAsync();

            var updateModel = new MemberToUpdateViewModel
            {
                Id = member2.Id,
                Email = "m1@test.com",
                Phone = "222"
            };

            // Act
            var result = await _service.UpdateMemberAsync(member2.Id, updateModel);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("Email or Phone already exists", result.Error);
        }

        [Fact]
        public async Task DeleteMemberAsync_WithValidId_DeletesSuccessfully()
        {
            // Arrange
            var member = new Member
            {
                Name = "Delete Me",
                Email = "delete@test.com",
                Phone = "999",
                Address = new Address { City = "City", Street = "Street", BuildingNumber = 1 }
            };
            _context.Member.Add(member);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.DeleteMemberAsync(member.Id);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(await _context.Member.FindAsync(member.Id));
        }

        [Fact]
        public async Task DeleteMemberAsync_WithInvalidId_ReturnsFailure()
        {
            // Act
            var result = await _service.DeleteMemberAsync(999);

            // Assert
            Assert.False(result.Succeeded);
            Assert.Equal("No Member Available", result.Error);
        }
    }
}
