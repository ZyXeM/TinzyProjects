using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationServer.Logic.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<IdentityUser> userManager;

        public ProfileService(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        public  Task GetProfileDataAsync(ProfileDataRequestContext context)
        {

            context.IssuedClaims.AddRange(context.Subject.Claims);
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
