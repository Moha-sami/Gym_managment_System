using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MemberViewModels;

namespace GymMangment.BLL.Services.Class
{
    public class MemberService : ImemberService
    {
        private readonly IGenericRepository<Member> memberRepository;

        public MemberService(IGenericRepository<Member> memberRepository)
        {
            this.memberRepository = memberRepository;
        }
        //CreateMemberAsync - Accepts a CreateMemberViewModel, validates the input, and creates a new Member entity in the database.
        public async Task<bool> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            //Check if the email already exists in the database
            var EmailExists = await memberRepository.AnyAsync(x => x.Email == model.Email, ct: ct);
            var PhoneExists = await memberRepository.AnyAsync(x => x.Phone == model.Phone, ct: ct);
            //Check if the phone number already exists in the database
            if (EmailExists || PhoneExists)
            {
                return false;

            }
            else
            {
                var member = new Member
                {
                    Name = model.Name,
                    Email = model.Email,
                    Phone = model.Phone,
                    Gender = model.Gender,
                    DateOFBirth = model.DateOfBirth,
                    Address = new Address
                    {
                        BuildingNumber = model.BuildingNumber,
                        City = model.City,
                        Street = model.Street
                    },
                    HealthRecord = new HealthRecord
                    {
                        BloodType = model.HealthRecordViewModel.BloodType,
                        Weight = model.HealthRecordViewModel.Weight,
                        Height = model.HealthRecordViewModel.Height,
                        Note = model.HealthRecordViewModel.Note,
                    }


                };
                var result = await memberRepository.AddAsync(member, ct: ct);       
                return result > 0; 
            }
        }
        //GetAllMembersAsync - Retrieves a list of all members and maps them to MemberViewModel instances.
        public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct)
        {
            var members = await memberRepository.GetAllAsync(ct: ct);
            if (!members.Any()) return [];
            var MemberViewModels = members.Select(m => new MemberViewModel
            {
                
                name=m.Name,
                Email = m.Email,
                phone = m.Phone,
                gender = m.Gender.ToString(),
                photo = m.Photo,

            });
            return MemberViewModels;
        }
    }
}
