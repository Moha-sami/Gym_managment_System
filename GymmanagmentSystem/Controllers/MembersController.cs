using AutoMapper;
using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Common;
using GymMangment.BLL.Services.Class;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.MemberViewModels;
using Microsoft.AspNetCore.Mvc;
namespace GymmanagmentSystem.PL.Controllers
{
    public class MembersController : Controller
    {
        private readonly ImemberService memberservice;
        

        public MembersController(ImemberService _memberservice )
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

            TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"]
                = result.Succeeded ? "Member created successfully!" : result.Error;

            return RedirectToAction(nameof(Index));
        }






        #endregion

        // Member Details(int id) - Shows member details localhost:port/Members/MemberDetails/{Id}(Get)
        public async Task<IActionResult> MemberDetails(int id, CancellationToken ct)
        {
            // Use the service here
            var result = await memberservice.GetMemberDetailsByIdAsync(id, ct);

            if (!result.Succeeded)
            {
                return NotFound(result.Error);
            }

            return View(result.Data);
        }

        // HealthRecord Details(int id) - Shows health record details localhost:port/Members/HealthRecordDetails/{Id}(Get)
        //public IActionResult HealthRecordDetails(int id , CancellationToken ct)
        //{
        //    //Get HealthRecord details using id and pass to view
        //    //Check if HealthRecord exists, if not return NotFound()
        //    // IF HealthRecord not Null Return view with HealthRecord details
        //}

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
