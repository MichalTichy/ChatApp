﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Validation;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.Cookies;
using ChatApp.WEB.Services;

namespace ChatApp.WEB.ViewModels.Authentication
{
   public class RegisterViewModel : DotvvmViewModelBase, IValidatableObject
    {
        private readonly UserService userService;

        public string ErrorMessage { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }

        public RegisterViewModel(UserService userService)
        {
            this.userService = userService;
        }


        public async Task Register()
        {

            var identityResult = await userService.RegisterAsync(UserName, Password);
            if (identityResult.Succeeded)
            {
                await SignIn();
            }
            else
            {
                var modelErrors = ConvertIdentityErrorsToModelErrors(identityResult);
                Context.ModelState.Errors.AddRange(modelErrors);
                Context.FailOnInvalidModelState();
            }

            Context.RedirectToRoute("Default", allowSpaRedirect: false);
        }
        
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Password != ConfirmPassword)
            {
                yield return new ValidationResult("Passwords does not match.", new[] { nameof(ConfirmPassword) });
            }
        }

        private async Task SignIn()
        {
            var claimsIdentity = await userService.SignInAsync(UserName, Password);
			await Context.GetAuthentication().SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        private IEnumerable<ViewModelValidationError> ConvertIdentityErrorsToModelErrors(IdentityResult identityResult)
        {
            return identityResult.Errors.Select(identityError => new ViewModelValidationError
            {
                ErrorMessage = identityError.Description,
                PropertyPath = nameof(UserName)
            });
        }
    }
}
