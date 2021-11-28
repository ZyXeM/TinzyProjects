using AuthenticationServer.Logic.Enum;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AuthenticationServer.Logic
{
    public static class Configuration
    {
        public static IEnumerable<ApiResource> GetApis() =>
            new List<ApiResource> { new ApiResource("Infrastructure") };

        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", new[] { "role" })
            };

        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            { new Client
            {
                ClientName = "Infrastructure Client",
                ClientId ="Infrastructure",
                ClientSecrets = { new Secret("infra_secret")},
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowedScopes = 
                { 
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "roles",
                    "offline_access"

                },
                AllowOfflineAccess = true,
                RequireConsent = false,
                AllowAccessTokensViaBrowser = true,
                RedirectUris =  { "https://localhost:44306/signin-oidc" },
                PostLogoutRedirectUris = { "https://localhost:44306" },
                AlwaysIncludeUserClaimsInIdToken = true,
                AlwaysSendClientClaims = true,
                }
            };

        public static ConfigurationDbContext LoadMockData(this ConfigurationDbContext context)
        {
            Seed(context,GetClients().Select(s => s.ToEntity()));
            Seed(context, GetApis().Select(s => s.ToEntity()));
            Seed(context, GetIdentityResources().Select(s => s.ToEntity()));

            context.SaveChanges();
            return context;
        }

        private static void Seed<T>(ConfigurationDbContext context,IEnumerable<T> collection) where T : class
        {
            context.Set<T>().RemoveRange(context.Set<T>());
            foreach (var item in collection)
            {
                context.Set<T>().Add(item);
            }
        }

        public static async Task CreateUsersAndRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = new IdentityUser("Admin");
            userManager.CreateAsync(user, "password").GetAwaiter().GetResult();
            IdentityResult adminRoleResult;
            bool adminRoleExists = await roleManager.RoleExistsAsync(InfraRoles.Superuser.ToString());


            if (!adminRoleExists)
            {
                adminRoleResult = await roleManager.CreateAsync(new IdentityRole(InfraRoles.Superuser.ToString()));
            }

            IdentityUser userToMakeAdmin = await userManager.FindByNameAsync("Admin");
            //await userManager.AddClaimAsync(userToMakeAdmin, new Claim("role", "Superuser"));
            await userManager.AddToRoleAsync(userToMakeAdmin, InfraRoles.Superuser.ToString());
        }
    }
}
