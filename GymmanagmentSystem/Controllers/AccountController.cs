using GymManagment.DAL.Models;
using GymManagment.DAL.Models.Enum;
using GymManagment.DAL.Repositories.Interfaces;
using GymMangment.BLL.Services.Interfaces;
using GymMangment.BLL.ViewModels.AccountViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GymmanagmentSystem.PL.Services;

namespace GymmanagmentSystem.PL.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,IUnitOfWork unitOfWork,IFileService fileService, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _emailService = emailService;
        }

        // GET: Account/Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
                return RedirectToAction("Index", "Home");

            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        // GET: Account/Register
        [HttpGet]
        public IActionResult Register() => View();

        // POST: Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var emailExists = await _unitOfWork.Members.AnyAsync(x => x.Email == model.Email);
            var phoneExists = await _unitOfWork.Members.AnyAsync(x => x.Phone == model.Phone);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "A member with this email already exists.");
                return View(model);
            }

            if (phoneExists)
            {
                ModelState.AddModelError("Phone", "A member with this phone number already exists.");
                return View(model);
            }

            var photoPath = await _fileService.SaveImageAsync(model.Photo, "uploads");
            if (photoPath == null)
            {
                ModelState.AddModelError("Photo", "Invalid photo. Please upload a JPG, PNG or WebP image under 2MB.");
                return View(model);
            }

            var member = new Member
            {
                Name = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                DateOFBirth = model.DateOfBirth,
                Gender = model.Gender,
                Photo = photoPath,
                Address = new Address
                {
                    BuildingNumber = model.BuildingNumber,
                    Street = model.Street,
                    City = model.City
                },
                HealthRecord = new HealthRecord
                {
                    Height = 0,
                    Weight = 0,
                    BloodType = "Unknown",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Members.AddAsync(member, default);

            // Auto-assign Basic Plan membership
            var basicPlan = (await _unitOfWork.Plans.GetAllAsync())
                .FirstOrDefault(p => p.Name == "Basic Plan" && p.IsActive);

            if (basicPlan != null)
            {
                var membership = new Membership
                {
                    MemberID = member.Id,
                    PlansID = basicPlan.Id,
                    EndDate = DateTime.Now.AddDays(basicPlan.DurationInDays),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _unitOfWork.Memberships.AddAsync(membership, default);
            }

            var user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                MemberId = member.Id
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Member");
                TempData["SuccessMessage"] = "Account created successfully! You can now login.";
                return RedirectToAction(nameof(Login));
            }

            await _unitOfWork.Members.DeleteAsync(member, default);

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider = "Google")
        {
            // هنا نجبر الرابط أن يبدأ بـ https بغض النظر عن بروتوكول الطلب القادم
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", null, "https");

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null) return RedirectToAction("Login");

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null) return RedirectToAction("Login");

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded) return LocalRedirect(returnUrl ?? "/");

            // لو المستخدم أول مرة يسجل دخول عن طريق جوجل
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var picture = info.Principal.FindFirstValue("urn:google:picture"); // ده الـ Claim الخاص بالصورة

            // هنا هتعمل Redirect لصفحة تكملة البيانات اللي فيها (الاسم، الفون، الصورة)
            return RedirectToAction(nameof(RegisterExternal));
        }
        // GET: Account/RegisterExternal
        [HttpGet]
        public async Task<IActionResult> RegisterExternal()
        {
            // Get the external login info from the cookie set during callback
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "External login info not found. Please try again.";
                return RedirectToAction(nameof(Login));
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            var fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

            var model = new RegisterExternalViewModel
            {
                Email = email,
                FullName = fullName,
                Provider = info.LoginProvider
            };

            return View(model);
        }

        // POST: Account/RegisterExternal
        [HttpPost]
        public async Task<IActionResult> RegisterExternal(RegisterExternalViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["ErrorMessage"] = "External login session expired. Please try again.";
                return RedirectToAction(nameof(Login));
            }

            // Check for duplicate email/phone
            var emailExists = await _unitOfWork.Members.AnyAsync(x => x.Email == model.Email);
            var phoneExists = await _unitOfWork.Members.AnyAsync(x => x.Phone == model.Phone);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "A member with this email already exists.");
                return View(model);
            }
            if (phoneExists)
            {
                ModelState.AddModelError("Phone", "A member with this phone number already exists.");
                return View(model);
            }

            // Create Member profile (no photo required for social login)
            var member = new Member
            {
                Name = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                DateOFBirth = model.DateOfBirth,
                Gender = model.Gender,
                Photo = "/images/avatars/male-avatar-1.png", // default avatar
                Address = new Address
                {
                    BuildingNumber = model.BuildingNumber,
                    Street = model.Street,
                    City = model.City
                },
                HealthRecord = new HealthRecord
                {
                    Height = 0,
                    Weight = 0,
                    BloodType = "Unknown",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                },
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _unitOfWork.Members.AddAsync(member, default);

            // Auto-assign Basic Plan
            var basicPlan = (await _unitOfWork.Plans.GetAllAsync())
                .FirstOrDefault(p => p.Name == "Basic Plan" && p.IsActive);

            if (basicPlan != null)
            {
                var membership = new Membership
                {
                    MemberID = member.Id,
                    PlansID = basicPlan.Id,
                    EndDate = DateTime.Now.AddDays(basicPlan.DurationInDays),
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _unitOfWork.Memberships.AddAsync(membership, default);
            }

            // Create Identity user
            var user = new AppUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                MemberId = member.Id
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                await _unitOfWork.Members.DeleteAsync(member, default);
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Link Google/Facebook login to this user
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                await _unitOfWork.Members.DeleteAsync(member, default);
                TempData["ErrorMessage"] = "Failed to link external login. Please try again.";
                return RedirectToAction(nameof(Login));
            }

            // Assign Member role
            await _userManager.AddToRoleAsync(user, "Member");

            // Sign in
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["SuccessMessage"] = $"Welcome {model.FullName}! Your account has been created.";
            return RedirectToAction("Index", "Home");
        }
        // POST: Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET: Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword() => View();

        // POST: Account/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var otp = await _userManager.GenerateUserTokenAsync(user, "EmailOtpProvider", "ResetPassword");
                
                System.Diagnostics.Debug.WriteLine($"====================================");
                System.Diagnostics.Debug.WriteLine($"PASSWORD RESET OTP FOR {model.Email}: {otp}");
                System.Diagnostics.Debug.WriteLine($"====================================");
                Console.WriteLine($"====================================");
                Console.WriteLine($"PASSWORD RESET OTP FOR {model.Email}: {otp}");
                Console.WriteLine($"====================================");

                var emailSubject = "Power Fitness - Password Reset OTP";
                var emailBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                        <h2 style='color: #0a1172; text-align: center;'>Power Fitness</h2>
                        <hr style='border: 0; border-top: 1px solid #eee;' />
                        <p>Hello,</p>
                        <p>We received a request to reset your password. Use the following 6-digit One-Time Password (OTP) to proceed:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <span style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #0a1172; border: 2px dashed #0a1172; padding: 10px 20px; border-radius: 5px; background-color: #f8f9fa;'>{otp}</span>
                        </div>
                        <p style='color: #666; font-size: 14px;'>This OTP is valid for 5 minutes. If you did not request this, please ignore this email.</p>
                        <hr style='border: 0; border-top: 1px solid #eee;' />
                        <p style='font-size: 12px; color: #999; text-align: center;'>Power Fitness Gym Management System</p>
                    </div>";

                await _emailService.SendEmailAsync(model.Email, emailSubject, emailBody);
            }

            TempData["SuccessMessage"] = "If the email is registered, a 6-digit OTP has been sent.";
            return RedirectToAction(nameof(VerifyOtp), new { email = model.Email });
        }

        // GET: Account/VerifyOtp
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email)) return RedirectToAction(nameof(ForgotPassword));
            return View(new VerifyOtpViewModel { Email = email });
        }

        // POST: Account/VerifyOtp
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid request.");
                return View(model);
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, "EmailOtpProvider", "ResetPassword", model.Otp);
            if (!isValid)
            {
                ModelState.AddModelError("Otp", "Invalid or expired OTP code.");
                return View(model);
            }

            TempData["SuccessMessage"] = "OTP verified successfully. Please set your new password.";
            return RedirectToAction(nameof(ResetPassword), new { email = model.Email, otp = model.Otp });
        }

        // GET: Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
                return RedirectToAction(nameof(ForgotPassword));

            return View(new ResetPasswordViewModel { Email = email, Otp = otp });
        }

        // POST: Account/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid request.");
                return View(model);
            }

            var isValid = await _userManager.VerifyUserTokenAsync(user, "EmailOtpProvider", "ResetPassword", model.Otp);
            if (!isValid)
            {
                ModelState.AddModelError("", "Your session has expired. Please request a new OTP.");
                return View(model);
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                TempData["SuccessMessage"] = "Password reset successful! You can now log in.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }
    }
}