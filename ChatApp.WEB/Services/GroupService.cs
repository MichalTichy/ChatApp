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

        public async Task AddMemberToGroup(Guid groupId,Guid userId,bool isManager=false)
        {
            var groupMembership = new GroupMembership()
            {
                UserId = userId, IsManager = isManager, GroupId = groupId
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
            var groups=new List<Group>();
            var groupsIds = context.Memberships.Where(t=>t.UserId==userId).Select(t=>t.GroupId);
            foreach (var groupId in groupsIds)
            {
                groups.Add(context.Find<Group>(groupId));
            }

            return groups;
        }
    }
}
