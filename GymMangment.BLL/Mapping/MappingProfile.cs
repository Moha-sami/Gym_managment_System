using AutoMapper;
using GymManagment.DAL.Models;
using GymMangment.BLL.ViewModels.MemberViewModels;
using GymMangment.BLL.ViewModels.HealthRecordsViewModels;

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
                .ForMember(dest => dest.photo, opt => opt.MapFrom(src => src.Photo));

            // CreateMemberViewModel -> Member (creation)
            CreateMap<CreateMemberViewModel, Member>()
                .ForMember(dest => dest.DateOFBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => new Address
                {
                    BuildingNumber = src.BuildingNumber,
                    City = src.City,
                    Street = src.Street
                }))
                .ForMember(dest => dest.HealthRecord, opt => opt.MapFrom(src => src.HealthRecordViewModel));
            // Member-> MemberDetails
            CreateMap<Member, MemberViewModel>()
     .ForMember(dest => dest.Address, opt => opt.MapFrom(src => $"{src.Address.Street} - {src.Address.City}- {src.Address.BuildingNumber}"))
     .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.Gender.ToString()))
     .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOFBirth.ToString()));
     
    
     

            // HealthRecordViewModel -> HealthRecord
            CreateMap<HealthRecordViewModel, HealthRecord>();

            // HealthRecord -> HealthRecordViewModel (for details/edit later)
            CreateMap<HealthRecord, HealthRecordViewModel>();
        }
    }
}