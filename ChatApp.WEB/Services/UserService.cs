using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.WEB.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChatApp.WEB.Services
{
    public class UserService
    {
        private readonly UserManager<ApplicationUser> userManager;
        

        public UserService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<ApplicationUser> Get(Guid id)
        {
            return await userManager.FindByIdAsync(id.ToString());
        }
        public async Task<ApplicationUser> Get(string username)
        {
            return await userManager.FindByNameAsync(username);
        }

        public async Task<ClaimsIdentity> SignInAsync(string userName, string password)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var result = await userManager.CheckPasswordAsync(user, password);
                if (result)
                {
                    return CreateIdentity(user);
                }
            }
            return null;
        }

        public async Task<IdentityResult> RegisterAsync(string userName, string password)
        {
            var user = new ApplicationUser(userName);
            return await userManager.CreateAsync(user, password);
        }

        private ClaimsIdentity CreateIdentity(ApplicationUser user)
        {
            var identity = new ClaimsIdentity(
                new []
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), 
                    new Claim(ClaimTypes.Role, "administrator"), 
                },"Cookie");
            return identity;
        }

        public List<ApplicationUser> GetAll()
        {
            return userManager.Users.ToList();
        }
    }
}
