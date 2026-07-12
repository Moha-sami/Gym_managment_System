using GymManagment.DAL.Models;
using Microsoft.AspNetCore.Identity;

namespace GymmanagmentSystem.PL.Services
{
    public class EmailOtpTokenProvider<TUser> : TotpSecurityStampBasedTokenProvider<TUser> where TUser : class
    {
        public override Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(true);
        }

        public override async Task<string> GetUserModifierAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            var email = await manager.GetEmailAsync(user);
            return $"EmailOTP:{purpose}:{email}";
        }
    }
}
