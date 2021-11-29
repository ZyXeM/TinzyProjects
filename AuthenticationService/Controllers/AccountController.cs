using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IdentityServer4.Models;
using AuthenticationService.Models;

namespace AuthenticationService.Controllers
{
    public class AccountController : Controller
    {

            private readonly SignInManager<IdentityUser> signInManager;
            private readonly UserManager<IdentityUser> userManager;
            private readonly IIdentityServerInteractionService interaction;

            public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IIdentityServerInteractionService interaction)
            {
                this.signInManager = signInManager;
                this.userManager = userManager;
                this.interaction = interaction;
            }

            [HttpGet]
            public IActionResult Login(string ReturnUrl)
            {
                return View(new UserViewModel() { RedirectUrl = ReturnUrl });
            }

            [HttpPost]
            public async Task<IActionResult> Login(UserViewModel user)
            {
                /*    if(!ModelState.IsValid)
                        return View(user);*/
                var result = await signInManager.PasswordSignInAsync(user.UserName, user.Password, false, false);
                if (result.Succeeded)
                {
                    return Redirect(user.RedirectUrl);
                }
                else
                {
                    return new StatusCodeResult(StatusCodes.Status400BadRequest);
                }
            }

            [HttpGet]
            public IActionResult Register()
            {
                return View(new RegistrationViewModel());
            }

            [HttpPost]
            public async Task<IActionResult> Register(RegistrationViewModel userModel)
            {
                if (!ModelState.IsValid)
                    return View(userModel);
                var user = new IdentityUser(userModel.UserName);
                var result = await userManager.CreateAsync(user, userModel.Password);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);
                    return Redirect(userModel.ReturnUrl);
                }
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }


            public async Task<IActionResult> Logout(string logoutId)
            {
                await signInManager.SignOutAsync();
                var result = await interaction.GetLogoutContextAsync(logoutId);
                return Redirect(result.PostLogoutRedirectUri);

            }

        
    }
}
