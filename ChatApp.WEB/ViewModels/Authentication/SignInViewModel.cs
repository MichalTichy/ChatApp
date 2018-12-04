using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.AspNetCore.Identity;
using ChatApp.WEB.Services;

namespace ChatApp.WEB.ViewModels.Authentication
{
    public class SignInViewModel : DotvvmViewModelBase
    {
        public string ErrorMessage { get; set; }

        private readonly UserService userService;

        public SignInViewModel(UserService userService)
        {
            this.userService = userService;
        }

        [Required]
        public string UserName { get; set; }
        public string Password { get; set; }

        public async Task SignIn()
        {
            var identity = await userService.SignInAsync(UserName, Password);
            if (identity == null)
            {
                ErrorMessage = "Incorrect login";
                return;
            }
			else
            {
                await Context.GetAuthentication().SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(identity));
				Context.RedirectToRoute("Default", allowSpaRedirect: false);
			} 
        }
    }
}
