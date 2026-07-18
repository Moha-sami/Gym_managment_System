using GymManagment.DAL.DbContext;
using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace GymmanagmentSystem
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GymDbcontext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            try
            {
                await context.Database.MigrateAsync();
                await SeedPlansAsync(context, logger);
                await SeedCategoriesAsync(context, logger);
                await SeedTrainersAsync(context, logger);
                await SeedMembersAsync(context, logger);
                await SeedMemberMembershipsAsync(context, logger);
                await SeedWeightProgressRecordsAsync(context, logger);
                await SeedRolesAsync(roleManager, logger);
                await SeedAdminUserAsync(userManager, logger);
                await SeedManagerUsersAsync(userManager, logger);
                await SeedBadgeDefinitionsAsync(context, logger);
                await SeedExercisesAsync(context, logger);
                await SeedUpcomingSessionsAsync(context, logger); // must be LAST — needs Trainers + Categories
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            string[] roles = { "Admin", "Manager", "Member", "Trainer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Created role: {Role}", role);
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            var adminEmail = "admin@gymmanagement.com";
            var existing = await userManager.FindByEmailAsync(adminEmail);

            if (existing != null)
            {
                // Reset password every startup to ensure it's correct
                var token = await userManager.GeneratePasswordResetTokenAsync(existing);
                var resetResult = await userManager.ResetPasswordAsync(existing, token, "Admin@1234");

                if (resetResult.Succeeded)
                    logger.LogInformation("Admin password reset successfully.");
                else
                    logger.LogError("Failed to reset admin password: {Errors}",
                        string.Join(", ", resetResult.Errors.Select(e => e.Description)));

                // Ensure admin has Admin role
                if (!await userManager.IsInRoleAsync(existing, "Admin"))
                {
                    await userManager.AddToRoleAsync(existing, "Admin");
                    logger.LogInformation("Admin role assigned to existing user.");
                }

                return;
            }

            var admin = new AppUser
            {
                FullName = "System Admin",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@1234");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Admin user created: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        private static async Task SeedManagerUsersAsync(UserManager<AppUser> userManager, ILogger logger)
        {
            var managers = new[]
            {
                new { FullName = "Operations Manager", Email = "manager1@gymmanagement.com", Password = "Manager@1234" },
                new { FullName = "Front Desk Manager", Email = "manager2@gymmanagement.com", Password = "Manager@1234" }
            };

            foreach (var managerInfo in managers)
            {
                var existing = await userManager.FindByEmailAsync(managerInfo.Email);

                if (existing != null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(existing);
                    var resetResult = await userManager.ResetPasswordAsync(existing, token, managerInfo.Password);

                    if (resetResult.Succeeded)
                        logger.LogInformation("Manager password reset successfully for {Email}.", managerInfo.Email);
                    else
                        logger.LogError("Failed to reset manager password for {Email}: {Errors}",
                            managerInfo.Email,
                            string.Join(", ", resetResult.Errors.Select(e => e.Description)));

                    if (!await userManager.IsInRoleAsync(existing, "Manager"))
                    {
                        await userManager.AddToRoleAsync(existing, "Manager");
                        logger.LogInformation("Manager role assigned to existing user: {Email}", managerInfo.Email);
                    }

                    continue;
                }

                var manager = new AppUser
                {
                    FullName = managerInfo.FullName,
                    UserName = managerInfo.Email,
                    Email = managerInfo.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(manager, managerInfo.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(manager, "Manager");
                    logger.LogInformation("Manager user created: {Email}", managerInfo.Email);
                }
                else
                {
                    logger.LogError("Failed to create manager user {Email}: {Errors}",
                        managerInfo.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        private static async Task SeedPlansAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Plans.Any())
            {
                logger.LogInformation("Plans already seeded — skipping.");
                return;
            }

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "plans.json");
            if (!File.Exists(jsonPath))
            {
                logger.LogWarning("plans.json not found at {Path}", jsonPath);
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var planDtos = JsonSerializer.Deserialize<List<PlanSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (planDtos == null || !planDtos.Any())
            {
                logger.LogWarning("No plans found in plans.json");
                return;
            }

            var plans = planDtos.Select(p => new Plans
            {
                Name = p.Name,
                Description = p.Description,
                DurationInDays = p.DurationDays,
                Price = p.Price,
                IsActive = p.IsActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await context.Plans.AddRangeAsync(plans);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} plans.", plans.Count);
        }

        private static async Task SeedCategoriesAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Category.Any())
            {
                logger.LogInformation("Categories already seeded — skipping.");
                return;
            }

            var categories = new List<Category>
            {
                new() { CategoryName = Categories.Cardio,   CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { CategoryName = Categories.Strength, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { CategoryName = Categories.Training, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { CategoryName = Categories.Yoga,     CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };

            await context.Category.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} categories.", categories.Count);
        }

        private static async Task SeedUpcomingSessionsAsync(GymDbcontext context, ILogger logger)
        {
            // Count upcoming sessions
            var upcomingCount = context.Session
                .Count(s => s.StartDate > DateTime.Now);

            if (upcomingCount >= 7)
            {
                logger.LogInformation("7 upcoming sessions already exist — skipping.");
                return;
            }

            var trainers = context.Trainer.ToList();
            var categories = context.Category.ToList();

            if (!trainers.Any() || !categories.Any())
            {
                logger.LogWarning("No trainers or categories found — skipping session seeding.");
                return;
            }

            // Mix of session times
            var sessionTimes = new[]
            {
        (hour: 7,  duration: 1),  // 7 AM - 8 AM
        (hour: 9,  duration: 2),  // 9 AM - 11 AM
        (hour: 11, duration: 1),  // 11 AM - 12 PM
        (hour: 14, duration: 2),  // 2 PM - 4 PM
        (hour: 16, duration: 1),  // 4 PM - 5 PM
        (hour: 18, duration: 2),  // 6 PM - 8 PM
        (hour: 20, duration: 1),  // 8 PM - 9 PM
    };

            var random = new Random();
            var sessionsToAdd = new List<Session>();
            var startDate = DateTime.Now.Date.AddDays(1); // Start from tomorrow

            for (int i = 0; i < 7; i++)
            {
                // Pick random category
                var category = categories[random.Next(categories.Count)];

                // Find trainers that match this category via specialty mapping
                var matchingTrainers = trainers.Where(t =>
                    GetMatchingCategory(t.Specialty) == category.CategoryName).ToList();

                // Fallback to any trainer if no matching trainer found
                var trainer = matchingTrainers.Any()
                    ? matchingTrainers[random.Next(matchingTrainers.Count)]
                    : trainers[random.Next(trainers.Count)];

                var time = sessionTimes[i];
                var sessionDate = startDate.AddDays(i);
                var sessionStart = sessionDate.AddHours(time.hour);
                var sessionEnd = sessionStart.AddHours(time.duration);

                sessionsToAdd.Add(new Session
                {
                    Description = $"{category.CategoryName} training session",
                    Capacity = random.Next(10, 26), // 10 to 25 slots
                    StartDate = sessionStart,
                    EndDate = sessionEnd,
                    TrainerId = trainer.Id,
                    CategoryId = category.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }

            await context.Session.AddRangeAsync(sessionsToAdd);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} upcoming sessions.", sessionsToAdd.Count);
        }

        private static Categories GetMatchingCategory(Specialty specialty)
        {
            return specialty switch
            {
                Specialty.GeneralFitness => Categories.Training,
                Specialty.Yoga => Categories.Yoga,
                Specialty.Boxing => Categories.Cardio,
                Specialty.CrossFit => Categories.Strength,
                _ => Categories.Training
            };
        }
        private static async Task SeedTrainersAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Trainer.Any())
            {
                logger.LogInformation("Trainers already seeded — skipping.");
                return;
            }

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "trainers.json");
            if (!File.Exists(jsonPath))
            {
                logger.LogWarning("trainers.json not found at {Path}", jsonPath);
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var trainerDtos = JsonSerializer.Deserialize<List<TrainerSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (trainerDtos == null || !trainerDtos.Any())
            {
                logger.LogWarning("No trainers found in trainers.json");
                return;
            }

            var trainers = trainerDtos.Select(t => new Trainer
            {
                Name = t.Name,
                Email = t.Email,
                Phone = t.Phone,
                DateOFBirth = DateOnly.Parse(t.DateOfBirth),
                Gender = (Gender)t.Gender,
                Specialty = (Specialty)t.Specialty,
                Address = new Address
                {
                    BuildingNumber = t.BuildingNumber,
                    Street = t.Street,
                    City = t.City
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await context.Trainer.AddRangeAsync(trainers);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} trainers.", trainers.Count);
        }
        private static async Task SeedMembersAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Member.Any())
            {
                logger.LogInformation("Members already seeded — skipping.");
                return;
            }

            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "members.json");
            if (!File.Exists(jsonPath))
            {
                logger.LogWarning("members.json not found at {Path}", jsonPath);
                return;
            }

            var json = await File.ReadAllTextAsync(jsonPath);
            var memberDtos = JsonSerializer.Deserialize<List<MemberSeedDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (memberDtos == null || !memberDtos.Any())
            {
                logger.LogWarning("No members found in members.json");
                return;
            }

            var maleAvatars = new[] { "/images/avatars/male-avatar-1.png", "/images/avatars/male-avatar-2.png" };
            var femaleAvatar = "/images/avatars/female-avatar-1.png";
            int maleIndex = 0;

            var members = memberDtos.Select(m =>
            {
                var gender = (Gender)m.Gender;
                string photo;

                if (gender == Gender.Male)
                {
                    photo = maleAvatars[maleIndex % maleAvatars.Length];
                    maleIndex++;
                }
                else
                {
                    photo = femaleAvatar;
                }

                return new Member
                {
                    Name = m.Name,
                    Email = m.Email,
                    Phone = m.Phone,
                    DateOFBirth = DateOnly.Parse(m.DateOfBirth),
                    Gender = gender,
                    Photo = photo,
                    Address = new Address
                    {
                        BuildingNumber = m.BuildingNumber,
                        Street = m.Street,
                        City = m.City
                    },
                    HealthRecord = new HealthRecord
                    {
                        Height = m.Height,
                        Weight = m.Weight,
                        BloodType = m.BloodType,
                        Note = m.Note,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    },
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }).ToList();

            await context.Member.AddRangeAsync(members);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} members.", members.Count);
        }

        private static async Task SeedWeightProgressRecordsAsync(GymDbcontext context, ILogger logger)
        {
            if (context.WeightProgressRecords.Any())
            {
                logger.LogInformation("Weight progress records already seeded — skipping.");
                return;
            }

            var healthRecords = context.HealthRecord.ToList();
            if (!healthRecords.Any())
            {
                logger.LogWarning("No health records found — skipping weight progress seeding.");
                return;
            }

            var progressRecords = healthRecords.Select(record => new WeightProgressRecord
            {
                MemberId = record.MemberId,
                Weight = record.Weight,
                RecordedAt = record.CreatedAt == default ? DateTime.Now : record.CreatedAt,
                Note = "Imported from current health record",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await context.WeightProgressRecords.AddRangeAsync(progressRecords);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} weight progress records.", progressRecords.Count);
        }

        public class MemberSeedDto
        {
            public string Name { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Phone { get; set; } = default!;
            public string DateOfBirth { get; set; } = default!;
            public int Gender { get; set; }
            public int BuildingNumber { get; set; }
            public string Street { get; set; } = default!;
            public string City { get; set; } = default!;
            public decimal Height { get; set; }
            public decimal Weight { get; set; }
            public string BloodType { get; set; } = default!;
            public string? Note { get; set; }
        }

        public class PlanSeedDto
        {
            public string Name { get; set; } = default!;
            public string Description { get; set; } = default!;
            public int DurationDays { get; set; }
            public decimal Price { get; set; }
            public bool IsActive { get; set; }
        }
        public class TrainerSeedDto
        {
            public string Name { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Phone { get; set; } = default!;
            public string DateOfBirth { get; set; } = default!;
            public int Gender { get; set; }
            public int Specialty { get; set; }
            public int BuildingNumber { get; set; }
            public string Street { get; set; } = default!;
            public string City { get; set; } = default!;
        }
        private static async Task SeedMemberMembershipsAsync(GymDbcontext context, ILogger logger)
        {
            if (context.Membership.Any())
            {
                logger.LogInformation("Memberships already seeded — skipping.");
                return;
            }

            var members = context.Member.ToList();
            var basicPlan = context.Plans.FirstOrDefault(p => p.Name == "Basic Plan" && p.IsActive);

            if (basicPlan == null)
            {
                logger.LogWarning("Basic Plan not found — skipping membership seeding.");
                return;
            }

            if (!members.Any())
            {
                logger.LogWarning("No members found — skipping membership seeding.");
                return;
            }

            var memberships = members.Select(m => new Membership
            {
                MemberID = m.Id,
                PlansID = basicPlan.Id,
                EndDate = DateTime.Now.AddDays(basicPlan.DurationInDays),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            }).ToList();

            await context.Membership.AddRangeAsync(memberships);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} memberships for existing members.", memberships.Count);
        }

        private static async Task SeedBadgeDefinitionsAsync(GymDbcontext context, ILogger logger)
        {
            if (context.BadgeDefinitions.Any())
            {
                logger.LogInformation("Badge definitions already seeded — skipping.");
                return;
            }

            var badges = new List<BadgeDefinition>
            {
                // WorkoutCount
                new() { Name = "Iron Starter", Description = "Log your first workout.", IconPath = "/images/badges/bronze_badge.jpg", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Bronze, Threshold = 1, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Dedicated Lifter", Description = "Log 25 workouts.", IconPath = "/images/badges/silver_badge.jpg", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Silver, Threshold = 25, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Century Club", Description = "Log 100 workouts.", IconPath = "/images/badges/gold_badge.jpg", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Gold, Threshold = 100, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // TotalVolume
                new() { Name = "Ton Lifter", Description = "Lift 1,000 kg total volume.", IconPath = "/images/badges/bronze_badge.jpg", Category = BadgeCategory.TotalVolume, Tier = BadgeTier.Bronze, Threshold = 1000, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Heavy Hitter", Description = "Lift 10,000 kg total volume.", IconPath = "/images/badges/silver_badge.jpg", Category = BadgeCategory.TotalVolume, Tier = BadgeTier.Silver, Threshold = 10000, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Titan", Description = "Lift 100,000 kg total volume.", IconPath = "/images/badges/gold_badge.jpg", Category = BadgeCategory.TotalVolume, Tier = BadgeTier.Gold, Threshold = 100000, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // SessionAttendance
                new() { Name = "Regular", Description = "Attend 5 training sessions.", IconPath = "/images/badges/bronze_badge.jpg", Category = BadgeCategory.SessionAttendance, Tier = BadgeTier.Bronze, Threshold = 5, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Committed", Description = "Attend 25 training sessions.", IconPath = "/images/badges/silver_badge.jpg", Category = BadgeCategory.SessionAttendance, Tier = BadgeTier.Silver, Threshold = 25, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Ironclad", Description = "Attend 100 training sessions.", IconPath = "/images/badges/gold_badge.jpg", Category = BadgeCategory.SessionAttendance, Tier = BadgeTier.Gold, Threshold = 100, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // BookingCount
                new() { Name = "First Booking", Description = "Make your first session booking.", IconPath = "/images/badges/bronze_badge.jpg", Category = BadgeCategory.BookingCount, Tier = BadgeTier.Bronze, Threshold = 1, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Session Fan", Description = "Make 25 session bookings.", IconPath = "/images/badges/silver_badge.jpg", Category = BadgeCategory.BookingCount, Tier = BadgeTier.Silver, Threshold = 25, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Booking Machine", Description = "Make 100 session bookings.", IconPath = "/images/badges/gold_badge.jpg", Category = BadgeCategory.BookingCount, Tier = BadgeTier.Gold, Threshold = 100, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // PersonalRecord
                new() { Name = "PR Breaker", Description = "Lift a max weight of 50 kg on any set.", IconPath = "/images/badges/bronze_badge.jpg", Category = BadgeCategory.PersonalRecord, Tier = BadgeTier.Bronze, Threshold = 50, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Strength Surge", Description = "Lift a max weight of 100 kg on any set.", IconPath = "/images/badges/silver_badge.jpg", Category = BadgeCategory.PersonalRecord, Tier = BadgeTier.Silver, Threshold = 100, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Beast Mode", Description = "Lift a max weight of 200 kg on any set.", IconPath = "/images/badges/gold_badge.jpg", Category = BadgeCategory.PersonalRecord, Tier = BadgeTier.Gold, Threshold = 200, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // ConsistencyStreak
                new() { Name = "3-Day Streak", Description = "Log workouts 3 days in a row.", IconPath = "/images/badges/bronze_badge.jpg", Category = BadgeCategory.ConsistencyStreak, Tier = BadgeTier.Bronze, Threshold = 3, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Weekly Warrior", Description = "Log workouts 7 days in a row.", IconPath = "/images/badges/silver_badge.jpg", Category = BadgeCategory.ConsistencyStreak, Tier = BadgeTier.Silver, Threshold = 7, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Monthly Machine", Description = "Log workouts 30 days in a row.", IconPath = "/images/badges/gold_badge.jpg", Category = BadgeCategory.ConsistencyStreak, Tier = BadgeTier.Gold, Threshold = 30, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // WeightProgress
                new() { Name = "First Step", Description = "Change your body weight by 1 kg.", IconPath = "/images/badges/bronze_badge.jpg", Category = BadgeCategory.WeightProgress, Tier = BadgeTier.Bronze, Threshold = 1, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Transformation", Description = "Change your body weight by 5 kg.", IconPath = "/images/badges/silver_badge.jpg", Category = BadgeCategory.WeightProgress, Tier = BadgeTier.Silver, Threshold = 5, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Total Makeover", Description = "Change your body weight by 15 kg.", IconPath = "/images/badges/gold_badge.jpg", Category = BadgeCategory.WeightProgress, Tier = BadgeTier.Gold, Threshold = 15, IsAutomatic = true, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },

                // Manual Badges
                new() { Name = "Competition Winner", Description = "Awarded for winning a gym competition.", IconPath = "/images/badges/special_badge.jpg", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Gold, Threshold = null, IsAutomatic = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Member of the Month", Description = "Awarded for outstanding dedication and attitude.", IconPath = "/images/badges/special_badge.jpg", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Gold, Threshold = null, IsAutomatic = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Special Recognition", Description = "Awarded for special achievements or contributions.", IconPath = "/images/badges/special_badge.jpg", Category = BadgeCategory.WorkoutCount, Tier = BadgeTier.Gold, Threshold = null, IsAutomatic = false, CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };

            await context.BadgeDefinitions.AddRangeAsync(badges);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} badge definitions.", badges.Count);
        }

        private static async Task SeedExercisesAsync(GymDbcontext context, ILogger logger)
        {
            var exercises = new List<Exercise>
            {
                new() { Name = "Bench Press", Description = "Lies flat on a bench and press a barbell upwards to build chest, front shoulders, and triceps.", MuscleGroup = "Chest", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/rT7DgcrgW1U", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Barbell Back Squat", Description = "Squat down with a barbell resting on your upper back to build quads, glutes, and hamstrings.", MuscleGroup = "Legs", Difficulty = "Intermediate", VideoUrl = "https://www.youtube.com/embed/U3HlEF_E9fo", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Deadlift (Conventional)", Description = "Lift a barbell from the floor to hip height using a flat back to develop full posterior chain strength.", MuscleGroup = "Back", Difficulty = "Advanced", VideoUrl = "https://www.youtube.com/embed/ytGaGIn3SjY", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Lat Pulldown (Wide Grip)", Description = "Pull the bar down towards the upper chest while seated to isolate the lats and build back width.", MuscleGroup = "Back", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/CAwf7n6Luuc", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Overhead Dumbbell Press", Description = "Press dumbbells straight up from shoulder height to target the front and side deltoids.", MuscleGroup = "Shoulders", Difficulty = "Intermediate", VideoUrl = "https://www.youtube.com/embed/2yjwHeEwCy8", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Dumbbell Bicep Curl", Description = "Rotate and lift dumbbells towards the shoulders to isolate the bicep muscles.", MuscleGroup = "Arms", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/yTwoK3Kxs2E", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Decline Bench Sit-Ups", Description = "Lie on a decline bench and pull your torso up towards your knees to isolate and build strong abdominal muscles.", MuscleGroup = "Abs", Difficulty = "Intermediate", VideoUrl = "https://www.youtube.com/embed/CKQKsT4hlV4", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Decline Shoulder Taps", Description = "Hold a high plank with feet elevated on a bench and alternate tapping opposite shoulders to build core and shoulder stability.", MuscleGroup = "Shoulders", Difficulty = "Advanced", VideoUrl = "https://www.youtube.com/embed/iPRLXqNytOM", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Alternating Lunge with Rotation", Description = "Step forward into a lunge and rotate your torso toward your front leg to target quads, glutes, and core rotators.", MuscleGroup = "Legs", Difficulty = "Intermediate", VideoUrl = "https://www.youtube.com/embed/XO26sIQgam0", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Couch Dips with Leg Raise", Description = "Perform tricep dips off a bench/couch while raising one leg to engage triceps and hip flexors simultaneously.", MuscleGroup = "Arms", Difficulty = "Intermediate", VideoUrl = "https://www.youtube.com/embed/Y6Fag9ucMLU", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Cardio Side to Side Step", Description = "Step rapidly side-to-side to increase your heart rate, improve agility, and build functional lower body stamina.", MuscleGroup = "Cardio", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/V6C2Gv-3QvE", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Basic Stretching", Description = "Perform a series of basic muscle stretches to improve flexibility, reduce stiffness, and boost workout recovery.", MuscleGroup = "Stretching", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/UeGvAae_7E8", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Barbell Close Grip Press", Description = "Press a barbell from a flat bench using a narrow grip to shift the focus onto the triceps and inner chest.", MuscleGroup = "Chest", Difficulty = "Intermediate", VideoUrl = "https://www.youtube.com/embed/cnYuxIt7SCw", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Barbell Lying Triceps Extension", Description = "Lie on a flat bench and lower a barbell to your forehead before pushing it back up to isolate the triceps.", MuscleGroup = "Arms", Difficulty = "Advanced", VideoUrl = "https://www.youtube.com/embed/0cvKseBn22g", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Brazilian Crunches", Description = "Lie flat and perform twisting crunches to target the upper and lower abdominals as well as the obliques.", MuscleGroup = "Abs", Difficulty = "Intermediate", VideoUrl = "https://www.youtube.com/embed/EpqfxiLTumo", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Alternating Toe Tap", Description = "Lying flat, lift legs and alternate reaching up to tap your toes to build lower ab strength.", MuscleGroup = "Abs", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/kmIH3Yws_0o", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Alternating Plank Lunge", Description = "From a plank position, jump or step feet forward into alternating lunge positions for high-intensity core and leg engagement.", MuscleGroup = "Abs", Difficulty = "Advanced", VideoUrl = "https://www.youtube.com/embed/pAziCbaFt2I", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Alternating Oblique Sit-Ups", Description = "Alternate twisting elbows to opposite knees during sit-ups to target the side abdominal muscles.", MuscleGroup = "Abs", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/zNCkxWX_fZE", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Alternating Leg Lifts", Description = "Lie flat on your back and alternate lifting each leg to a 90-degree angle to target the lower core.", MuscleGroup = "Abs", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/xzdlNsXF-UU", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Alternating Arm Leg Plank", Description = "From a high plank position, lift the opposite arm and leg simultaneously to test and build extreme core stability.", MuscleGroup = "Abs", Difficulty = "Advanced", VideoUrl = "https://www.youtube.com/embed/SHTqSjdsmf4", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Decline Diamond Pike Push-Up", Description = "Elevate your feet and perform close-grip push-ups to heavily isolate the upper chest and triceps.", MuscleGroup = "Chest", Difficulty = "Advanced", VideoUrl = "https://www.youtube.com/embed/lhcdkc6X_uA", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now },
                new() { Name = "Upward Rotation Extend Arms", Description = "Extend and rotate arms upwards to build mobility and strength in the rotator cuff and rear shoulders.", MuscleGroup = "Shoulders", Difficulty = "Beginner", VideoUrl = "https://www.youtube.com/embed/OnYrI-4hCBk", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
            };

            foreach (var ex in exercises)
            {
                var existing = await context.Exercises.FirstOrDefaultAsync(x => x.Name == ex.Name);
                if (existing != null)
                {
                    existing.VideoUrl = ex.VideoUrl;
                    existing.Description = ex.Description;
                    existing.MuscleGroup = ex.MuscleGroup;
                    existing.Difficulty = ex.Difficulty;
                    existing.UpdatedAt = DateTime.Now;
                    context.Exercises.Update(existing);
                }
                else
                {
                    await context.Exercises.AddAsync(ex);
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Seeded and updated exercises with 3D animation videos.");
        }
    }
}
