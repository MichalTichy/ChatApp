using System;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.WEB.DAL
{
    public class GroupMembership :EntityBase
    {
        public Guid GroupId { get; set; }
        public Guid UserId { get; set; }
        public bool IsManager { get; set; }
    }
}