using GymManagment.DAL.Models;
using Microsoft.AspNetCore.Mvc;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MemberViewModels;
namespace GymmanagmentSystem.PL.Controllers
{
    public class MembersController : Controller
    {
        private readonly ImemberService memberservice;

        public MembersController(ImemberService _memberservice)
        {
            memberservice = _memberservice;
        }
        #region GetAllMembers
        //index list all members localhost:port/Members/Index(Get)
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var members = await memberservice.GetAllMembersAsync(ct);
            return View(members);
        }
        //details of member localhost:port/Members/MemberDetails/{Id}(Get)
        //Deatils of HealthRecord localhost:port/Members/HealthRecordDetails/{Id}(Get)
        #endregion

        #region MemberCreate
        // Create Shows member registration form localhost:port/Members/Create(Get)
        [HttpGet]
        public IActionResult Create() => View();
        //CreateMember Processes form submission localhost:port/Members/CreateMember(Post)
        [HttpPost]
        public async Task<IActionResult> CreateMember(CreateMemberViewModel model, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return View(nameof(Create), model);
            
            var result = await memberservice.CreateMemberAsync(model, ct);
            if(result)
                TempData["SuccessMessage"] = "Member created successfully!";
            else  
                TempData["ErrorMessage"] = "Failed to create member. Please try again.";
            return RedirectToAction(nameof(Index));
            



        }

        #endregion

        #region MemberEdit
        //MemberEdit(int id) - Displays edit form localhost:port/Members/MemberEdit/{Id}(Get)
        //MemberEdit() - Processes update submission localhost:port/Members/MemberEdit(Post)

        #endregion

        #region MemberDelete
        //Delete(int id) - Shows deletion confirmation page localhost:port/Members/Delete/{Id}(Get)
        //DeleteConfirmed(int id) - Processes deletion localhost:port/Members/DeleteConfirmed/{Id}(Post)

        #endregion

    }
}
