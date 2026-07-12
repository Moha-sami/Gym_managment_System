using AutoMapper;
using GymManagment.DAL.Models;
using GymMangment.BLL.ViewModels.BookingViewModels;
using GymMangment.BLL.ViewModels.HealthRecordsViewModels;
using GymMangment.BLL.ViewModels.MembershipViewModels;
using GymMangment.BLL.ViewModels.MemberViewModels;
using GymMangment.BLL.ViewModels.PlansViewModels;
using GymMangment.BLL.ViewModels.SessionsViewModels;
using GymMangment.BLL.ViewModels.TrainerViewModels;
using GymMangment.BLL.ViewModels.WorkoutViewModels;
using GymMangment.BLL.ViewModels.BadgeViewModels;

namespace GymMangment.BLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Member -> MemberViewModel (list display)
            CreateMap<Member, MemberViewModel>()
    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
    .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone))
    .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
    .ForMember(dest => dest.photo, opt => opt.MapFrom(src => src.Photo))
    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => $"{src.Address.Street} - {src.Address.City} - {src.Address.BuildingNumber}"))
    .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOFBirth.ToString()));

            // CreateMemberViewModel -> Member (creation)
            CreateMap<CreateMemberViewModel, Member>()
       .ForMember(dest => dest.DateOFBirth, opt => opt.MapFrom(src => src.DateOfBirth))
       .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
       {
           BuildingNumber = src.BuildingNumber,
           City = src.City,
           Street = src.Street
       }))
       .ForMember(dest => dest.HealthRecord, opt => opt.MapFrom(src => src.HealthRecordViewModel))
       .ForMember(dest => dest.Photo, opt => opt.Ignore()) 
                .ForMember(dest => dest.HealthRecord, opt => opt.MapFrom(src => src.HealthRecordViewModel));

           
    
     

            // HealthRecordViewModel -> HealthRecord
            CreateMap<HealthRecordViewModel, HealthRecord>();

            // HealthRecord -> HealthRecordViewModel (for details/edit later)
            CreateMap<HealthRecord, HealthRecordViewModel>();


            CreateMap<Member, MemberToUpdateViewModel>()
    .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOFBirth))
    .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
    .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.Address.BuildingNumber))
    .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
    .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street))
    .ForMember(dest => dest.CurrentPhoto, opt => opt.MapFrom(src => src.Photo))
    .ForMember(dest => dest.Photo, opt => opt.Ignore())
    .ForMember(dest => dest.PhotoPath, opt => opt.Ignore())
    .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.HealthRecord != null ? src.HealthRecord.Height : 0))
    .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.HealthRecord != null ? src.HealthRecord.Weight : 0))
    .ForMember(dest => dest.BloodType, opt => opt.MapFrom(src => src.HealthRecord != null ? src.HealthRecord.BloodType : null))
    .ForMember(dest => dest.Note, opt => opt.MapFrom(src => src.HealthRecord != null ? src.HealthRecord.Note : null));

            CreateMap<MemberToUpdateViewModel, Member>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }))
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.DateOFBirth, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.Photo, opt => opt.Ignore())
                .ForMember(dest => dest.HealthRecord, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));
            // Plans -> PlanViewModel (list & details)
            CreateMap<Plans, PlanViewModel>();

             // Plans -> EditPlanViewModel (pre-fill edit form)
             CreateMap<Plans, EditPlanViewModel>();

            // EditPlanViewModel -> Plans (saving changes, Name is locked)
            CreateMap<EditPlanViewModel, Plans>()
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));

            // Trainer -> TrainerViewModel (Index + Details)
            CreateMap<Trainer, TrainerViewModel>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src =>
                    $"{src.Address.BuildingNumber} - {src.Address.Street} - {src.Address.City}"))
                .ForMember(dest => dest.Specialty, opt => opt.MapFrom(src => src.Specialty.ToString()));

            // CreateTrainerViewModel -> Trainer (creation)
            CreateMap<CreateTrainerViewModel, Trainer>()
                .ForMember(dest => dest.DateOFBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }));

            // Trainer -> TrainerToUpdateViewModel (pre-fill edit form)
            CreateMap<Trainer, TrainerToUpdateViewModel>()
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOFBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
                .ForMember(dest => dest.BuildingNumber, opt => opt.MapFrom(src => src.Address.BuildingNumber))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.Street, opt => opt.MapFrom(src => src.Address.Street));

            // TrainerToUpdateViewModel -> Trainer (saving edits, Name/DOB/Gender locked)
            CreateMap<TrainerToUpdateViewModel, Trainer>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }))
                .ForMember(dest => dest.Name, opt => opt.Ignore())
                .ForMember(dest => dest.DateOFBirth, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));

            // Session -> SessionViewModel
            CreateMap<Session, SessionViewModel>()
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.Trainer.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName.ToString()))
                .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.Capacity - src.SessionMembers.Count));

            // CreateSessionViewModel -> Session
            CreateMap<CreateSessionViewModel, Session>()
                .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.SessionMembers, opt => opt.Ignore());

            // Session -> SessionToUpdateViewModel
            CreateMap<Session, SessionToUpdateViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName.ToString()));

            // SessionToUpdateViewModel -> Session
            CreateMap<SessionToUpdateViewModel, Session>()
                .ForMember(dest => dest.Trainer, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ForMember(dest => dest.Capacity, opt => opt.Ignore())
                .ForMember(dest => dest.SessionMembers, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now));

            // Membership -> MembershipViewModel
            CreateMap<Membership, MembershipViewModel>()
                .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name))
                .ForMember(dest => dest.PlanName, opt => opt.MapFrom(src => src.Plans.Name))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // CreateMembershipViewModel -> Membership
            CreateMap<CreateMembershipViewModel, Membership>()
                .ForMember(dest => dest.Member, opt => opt.Ignore())
                .ForMember(dest => dest.Plans, opt => opt.Ignore());

            // Session -> SessionScheduleViewModel
            CreateMap<Session, SessionScheduleViewModel>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName.ToString()))
                .ForMember(dest => dest.TrainerName, opt => opt.MapFrom(src => src.Trainer.Name))
                .ForMember(dest => dest.DateDisplay, opt => opt.MapFrom(src => src.StartDate.ToString("MMM dd, yyyy")))
                .ForMember(dest => dest.TimeRangeDisplay, opt => opt.MapFrom(src => $"{src.StartDate:hh:mm tt} - {src.EndDate:hh:mm tt}"))
                .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.Capacity - src.SessionMembers.Count))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.StartDate > DateTime.Now ? "Upcoming" :
                    src.EndDate >= DateTime.Now ? "Ongoing" : "Completed"));

            // Booking -> BookingViewModel
            CreateMap<Booking, BookingViewModel>()
    .ForMember(dest => dest.MemberId, opt => opt.MapFrom(src => src.MemberId))
    .ForMember(dest => dest.SessionId, opt => opt.MapFrom(src => src.SessionId))
    .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member.Name))
    .ForMember(dest => dest.SessionCategory, opt => opt.MapFrom(src => src.Session.Category.CategoryName.ToString()))
    .ForMember(dest => dest.SessionDate, opt => opt.MapFrom(src => src.Session.StartDate.ToString("MMM dd, yyyy")))
    .ForMember(dest => dest.SessionTime, opt => opt.MapFrom(src => $"{src.Session.StartDate:hh:mm tt} - {src.Session.EndDate:hh:mm tt}"))
    .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.CreatedAt));

            // Workout Mapping
            CreateMap<WorkoutLog, WorkoutLogViewModel>();
            CreateMap<WorkoutExerciseLog, WorkoutExerciseViewModel>();
            CreateMap<WorkoutSetLog, WorkoutSetViewModel>();

            CreateMap<CreateWorkoutLogViewModel, WorkoutLog>()
                .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises))
                .ForMember(dest => dest.Member, opt => opt.Ignore());

            CreateMap<CreateWorkoutExerciseViewModel, WorkoutExerciseLog>()
                .ForMember(dest => dest.Sets, opt => opt.MapFrom(src => src.Sets))
                .ForMember(dest => dest.WorkoutLog, opt => opt.Ignore());

            CreateMap<CreateWorkoutSetViewModel, WorkoutSetLog>()
                .ForMember(dest => dest.WorkoutExerciseLog, opt => opt.Ignore());

            // Badge Mapping
            CreateMap<BadgeDefinition, BadgeDefinitionViewModel>()
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
                .ForMember(dest => dest.Tier, opt => opt.MapFrom(src => src.Tier.ToString()));

            CreateMap<MemberBadge, MemberBadgeViewModel>()
                .ForMember(dest => dest.BadgeName, opt => opt.MapFrom(src => src.BadgeDefinition.Name))
                .ForMember(dest => dest.BadgeDescription, opt => opt.MapFrom(src => src.BadgeDefinition.Description))
                .ForMember(dest => dest.BadgeIconPath, opt => opt.MapFrom(src => src.BadgeDefinition.IconPath))
                .ForMember(dest => dest.BadgeTier, opt => opt.MapFrom(src => src.BadgeDefinition.Tier.ToString()))
                .ForMember(dest => dest.BadgeCategory, opt => opt.MapFrom(src => src.BadgeDefinition.Category.ToString()))
                .ForMember(dest => dest.AwardedByUserName, opt => opt.MapFrom(src => src.AwardedByUser != null ? src.AwardedByUser.FullName : null));
        }
    }
}