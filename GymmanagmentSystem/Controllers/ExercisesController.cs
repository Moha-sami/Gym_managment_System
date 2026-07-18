using GymManagment.DAL.Models;
using GymManagment.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GymmanagmentSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Member,Trainer")]
    public class ExercisesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExercisesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: Exercises
        public async Task<IActionResult> Index(string search, string muscleGroup)
        {
            var exercises = await _unitOfWork.Exercises.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
            {
                exercises = exercises.Where(e => e.Name.Contains(search, System.StringComparison.OrdinalIgnoreCase) || 
                                                 e.Description.Contains(search, System.StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(muscleGroup) && muscleGroup != "All Muscles")
            {
                exercises = exercises.Where(e => e.MuscleGroup.Equals(muscleGroup, System.StringComparison.OrdinalIgnoreCase));
            }

            ViewData["CurrentSearch"] = search;
            ViewData["CurrentMuscleGroup"] = muscleGroup ?? "All Muscles";

            return View(exercises.ToList());
        }

        // GET: Exercises/GetDetailsByName
        [HttpGet]
        public async Task<IActionResult> GetDetailsByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return BadRequest("Name is required");

            var exercises = await _unitOfWork.Exercises.GetAllAsync();
            var exercise = exercises.FirstOrDefault(e => e.Name.Equals(name.Trim(), System.StringComparison.OrdinalIgnoreCase));

            if (exercise == null) return NotFound();

            return Json(new
            {
                name = exercise.Name,
                description = exercise.Description,
                muscleGroup = exercise.MuscleGroup,
                difficulty = exercise.Difficulty,
                videoUrl = exercise.VideoUrl
            });
        }
    }
}
