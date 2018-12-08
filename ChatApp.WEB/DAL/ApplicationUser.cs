using System;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.WEB.DAL
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            
        }

        public ApplicationUser(string userName) : base(userName)
        {
            
        }
    }
}