using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ChatApp.WEB.Services;
using DotVVM.Framework.Runtime.Filters;

namespace ChatApp.WEB.ViewModels
{
    [Authorize]
    public class MasterPageViewModel : DotvvmViewModelBase
    {
        private readonly UserService userService;
        public string Username  => Context.GetAuthentication().Context.User.Identity.Name;
        public Guid UserId { get; set; }

        public MasterPageViewModel(UserService userService)
        {
            this.userService = userService;
        }

        public override Task Load()
        {
            if (UserId==Guid.Empty)
            {
                UserId = userService.Get(Username).Result.Id;
            }
            return base.Load();
        }

        public async Task SignOut()
        {
            await Context.GetAuthentication().SignOutAsync(IdentityConstants.ApplicationScheme);
            Context.RedirectToRoute("Default", null, false, false);
        }
    }
}
