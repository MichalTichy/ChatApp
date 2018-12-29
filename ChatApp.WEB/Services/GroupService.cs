using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.WEB.DAL;

namespace ChatApp.WEB.Services
{
    public class GroupService
    {
        private readonly AppDbContext context;

        public GroupService(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<Group> CreateGroup(string name)
        {
            var entity = new Group()
            {
                Name = name
            };
            var entityEntry = context.Set<Group>().Add(entity);

            await context.SaveChangesAsync();

            return entityEntry.Entity;
        }

        public async Task AddMemberToGroup(Guid groupId, Guid userId, bool asManager=false)
        {
            var groupMembership = new GroupMembership()
            {
                UserId = userId, IsManager = asManager, GroupId = groupId
            };
            await context.Set<GroupMembership>().AddAsync(groupMembership);

            await context.SaveChangesAsync();

        }

        public async Task RemoveUserFromGroup(Guid groupId, Guid userId)
        {
            var groupMemberships = context.Set<GroupMembership>();

            var groupMembership = groupMemberships.First(t => t.UserId == userId);

            context.Remove(groupMembership);

            var count = groupMemberships.Count(t => t.GroupId == groupId);

            if (count == 0)
            {
                await DeleteGroup(groupId);
            }

            await context.SaveChangesAsync();

        }

        public async Task DeleteGroup(Guid groupId)
        {
            var group = await context.Set<Group>().FindAsync(groupId);
            await DeleteGroup(group);
        }

        public async Task DeleteGroup(Group group)
        {

            var groupMemberships = context.Set<GroupMembership>().Where(t => t.GroupId == @group.Id);

            context.Set<GroupMembership>().RemoveRange(groupMemberships);

            context.Set<Group>().Remove(group);

            await context.SaveChangesAsync();
        }

        public async Task<Group> Get(Guid id)
        {
            return await context.FindAsync<Group>(id);
        }

        public ICollection<Group> GetUsersGroups(Guid userId)
        {
            var groups = new List<Group>();
            var groupsIds = context.Memberships.Where(t => t.UserId == userId).Select(t => t.GroupId);
            foreach (var groupId in groupsIds)
            {
                groups.Add(context.Find<Group>(groupId));
            }

            return groups;
        }
        public ICollection<Group> GetUsersGroups(string username)
        {
            var userId = context.Users.Single(t=>t.UserName==username).Id;
            var groups = new List<Group>();
            var groupsIds = context.Memberships.Where(t => t.UserId == userId).Select(t => t.GroupId);
            foreach (var groupId in groupsIds)
            {
                groups.Add(context.Find<Group>(groupId));
            }

            return groups;
        }

        public ICollection<ApplicationUser> GetUsersNotInGroup(Guid groupId)
        {
            var usersInGroup = context.Memberships.Where(t => t.GroupId == groupId).Select(t => t.UserId)
                .ToArray();
            var usersNotInGroup = context.Users.Where(t => !usersInGroup.Contains(t.Id)).ToArray();
            return usersNotInGroup;
        }

        public async Task<ICollection<ApplicationUser>> GetAllUsersInGroup(Guid groupId)
        {
            var usersInGroup = context.Memberships.Where(t => t.GroupId == groupId).Select(t => t.UserId);
            var users=new List<ApplicationUser>();
            foreach (var userId in usersInGroup)
            {
                users.Add(await context.FindAsync<ApplicationUser>(userId));
            }

            return users;
        }
        public async Task<ICollection<ApplicationUser>> GetNormalUsersInGroup(Guid groupId)
        {
            var usersInGroup = context.Memberships.Where(t => t.GroupId == groupId && !t.IsManager).Select(t => t.UserId);
            var users = new List<ApplicationUser>();
            foreach (var userId in usersInGroup)
            {
                users.Add(await context.FindAsync<ApplicationUser>(userId));
            }

            return users;
        }
        public async Task<ICollection<ApplicationUser>> GetManagersInGroup(Guid groupId)
        {
            var usersInGroup = context.Memberships.Where(t => t.GroupId == groupId && t.IsManager).Select(t => t.UserId);
            var users = new List<ApplicationUser>();
            foreach (var userId in usersInGroup)
            {
                users.Add(await context.FindAsync<ApplicationUser>(userId));
            }

            return users;
        }



        public bool IsManager(Guid groupId, Guid userId)
        {
            return context.Memberships.Single(t => t.UserId == userId && t.GroupId == groupId).IsManager;
        }

        public async Task PromoteUser(Guid groupId, Guid userId)
        {
            context.Set<GroupMembership>().First(t => t.UserId == userId && t.GroupId == groupId).IsManager = true;
            await context.SaveChangesAsync();
        }
    }
}
