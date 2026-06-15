using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MemberViewModels;

namespace GymMangment.BLL.Services.Class
{
    public class MemberService : ImemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MemberService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Result> CreateMemberAsync(CreateMemberViewModel model, CancellationToken ct = default)
        {
            var emailExists = await _unitOfWork.Members.AnyAsync(x => x.Email == model.Email, ct);
            var phoneExists = await _unitOfWork.Members.AnyAsync(x => x.Phone == model.Phone, ct);

            if (emailExists)
                return Result.Failure("A member with this email already exists.");

            if (phoneExists)
                return Result.Failure("A member with this phone number already exists.");

            var member = _mapper.Map<Member>(model);

            var rows = await _unitOfWork.Members.AddAsync(member, ct);

            return rows > 0
                ? Result.Success()
                : Result.Failure("Failed to create member. Please try again.");
        }

        public async Task<IEnumerable<MemberViewModel>> GetAllMembersAsync(CancellationToken ct = default)
        {
            var members = await _unitOfWork.Members.GetAllAsync(ct: ct);
            return _mapper.Map<IEnumerable<MemberViewModel>>(members);
        }

        public async Task<Result<MemberViewModel?>> GetMemberDetailsByIdAsync(int id, CancellationToken ct = default)
        {
            // 1. جلب العضو مع الـ Navigation Properties الخاصة به (Membership و Plan)
            // لاحظ استخدام الـ Includes عشان نتجنب الـ Lazy Loading
            var member = await _unitOfWork.Members.GetByIdAsync(
                id,
                ct,
                m => m.Memberships
               
            );

            if (member == null)
            {
                return Result<MemberViewModel?>.Failure("عفواً، لا يوجد عضو بهذا الكود.");
            }

            // 2. التحويل الأساسي للـ ViewModel
            var model = _mapper.Map<MemberViewModel>(member);

            // 3. تطبيق الـ Logic الخاص بالاشتراك (Membership)
            // بنشوف هل فيه اشتراك نشط (EndDate أكبر من تاريخ النهاردة)
            // 1. طلع الاشتراك النشط من القائمة أولاً
          

            var activeMembership = member.Memberships
     .FirstOrDefault(m => m.EndDate >= DateTime.Now);


            // 2. دلوقتي استخدم activeMembership (اللي هو عنصر واحد من نوع Membership)
            if (activeMembership != null)
            {
                // هنا تقدر توصل للـ Plan والـ EndDate والـ CreatedAt
                model.PlanName = activeMembership.Plans?.Name ?? "No Plan";
                model.MembershipStartDate = activeMembership.CreatedAt.ToString("yyyy-MM-dd"); 
                model.MembershipEndDate = activeMembership.EndDate.ToString("yyyy-MM-dd");
            }
            else
            {
                model.PlanName = "No Plan";
            }

            // 4. إرجاع النتيجة الناجحة
            return Result<MemberViewModel?>.Success(model);
        }


    }
}