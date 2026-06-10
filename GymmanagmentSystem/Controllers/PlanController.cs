using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymmanagmentSystem.Controllers
{
    public class PlanController : Controller
    {
        //Data base connection with dependency injection
        private readonly IGenericRepository<Plans> PlanRepository;
        public PlanController(IGenericRepository<Plans> planRepository)
        {
            PlanRepository=planRepository;
        }


        //get all plans localhost:port/Plan/Index
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var plans = await PlanRepository.GetAllAsync(ct: ct);//Pass by Name
            return View(plans);
        }
        //get plan details localhost:port/Plan/Details/1
        public   async Task<IActionResult> Details(int Id,CancellationToken ct)
        {
            var plan = await PlanRepository.GetByIdAsync(Id, ct);

            if (plan == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }
    }
}
