using System;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.WEB.DAL
{
    public class GroupMembership :EntityBase
    {
        public Group Group { get; set; }
        public ApplicationUser User { get; set; }
        public bool IsManager { get; set; }
    }
}