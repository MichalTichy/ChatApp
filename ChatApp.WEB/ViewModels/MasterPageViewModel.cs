using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Filters;

namespace ChatApp.WEB.ViewModels
{
    [Authorize]
    public class MasterPageViewModel : DotvvmViewModelBase
    {
        public string Username  => Context.GetAuthentication().Context.User.Identity.Name;
		public async Task SignOut()
        {
            await Context.GetAuthentication().SignOutAsync(IdentityConstants.ApplicationScheme);
            Context.RedirectToRoute("Default", null, false, false);
        }
    }
}
