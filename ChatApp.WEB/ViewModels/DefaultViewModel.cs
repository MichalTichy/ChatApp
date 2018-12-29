using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ChatApp.WEB.DAL;
using ChatApp.WEB.Hub;
using ChatApp.WEB.Services;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Bootstrap4;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Identity;
using DotVVM.Framework.Binding.Expressions;

namespace ChatApp.WEB.ViewModels
{
    public class DefaultViewModel : MasterPageViewModel
    {
        private readonly UserService userService;
        private readonly GroupService groupService;
        private readonly MessageService messageService;

        public BootstrapButtonColor OnlineColor => BootstrapButtonColor.Success;
        public BootstrapButtonColor OfflineColor => BootstrapButtonColor.Danger;


        public DefaultViewModel(UserService userService, GroupService groupService, MessageService messageService) : base(userService)
        {
            this.userService = userService;
            this.groupService = groupService;
            this.messageService = messageService;
        }
        public bool NewConversationModalVisible { get; set; }

        [Bind(Direction.ServerToClient)]
        public ICollection<UserInfo> AvailableUsers => GetAvailableUsers();

        [Bind(Direction.ServerToClient)]
        public ICollection<Group> AvailableGroups => GetAvailableGroups();

        public List<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
        public ChatRoom ActiveChatRoom { get; set; }
        public GroupManagementModel GroupManagementModel { get; set; }
        public override Task PreRender()
        {

            if (ActiveChatRoom != null)
            {
                ActiveChatRoom.LoadMessages(messageService, UserId);
                ActiveChatRoom.Messages.Items = ActiveChatRoom.Messages.Items.Reverse().ToList();
            }
            else
            {
                ActiveChatRoom = ChatRooms.LastOrDefault();
                if (ActiveChatRoom != null)
                {
                    ActivateChatRoom(ActiveChatRoom);
                }
            }

            if (!ChatRooms.Any())
            {
                NewConversationModalVisible = true;
            }
            return base.PreRender();
        }

        public void LoadMessages(ChatRoom chatRoom)
        {
            chatRoom.LoadMessages(messageService, UserId);
        }

        public async Task LoadGroupManagementModalData(Guid groupId)
        {
            GroupManagementModel = new GroupManagementModel()
            {
                GroupId = groupId,
                AvailableUsers = groupService.GetUsersNotInGroup(groupId),
                NormalUsers = await groupService.GetNormalUsersInGroup(groupId),
                Managers = await groupService.GetManagersInGroup(groupId),
                SelectedUserId = AvailableUsers.FirstOrDefault()?.UserId
            };
        }

        public async Task AddUserToGroup()
        {
            await groupService.AddMemberToGroup(GroupManagementModel.GroupId, GroupManagementModel.SelectedUserId.Value);
            await LoadGroupManagementModalData(GroupManagementModel.GroupId);
        }
        public async Task RemoveUserFromGroup(Guid userId)
        {
            await groupService.RemoveUserFromGroup(GroupManagementModel.GroupId, userId);
            await LoadGroupManagementModalData(GroupManagementModel.GroupId);
        }
        public async Task PromoteUser(Guid userId)
        {
            await groupService.PromoteUser(GroupManagementModel.GroupId, userId);
            await LoadGroupManagementModalData(GroupManagementModel.GroupId);
        }

        public void NewConversation(Guid id, bool isGroup)
        {
            var chatRoom = ChatRooms.FirstOrDefault(t => t.Id == id);
            if (chatRoom != null)
            {
                ActivateChatRoom(chatRoom);
                NewConversationModalVisible = false;
                return;
            }

            if (isGroup)
            {
                var group = groupService.Get(id).Result;
                var isManager = groupService.IsManager(id, UserId);
                chatRoom = ChatRoom.From(group, isManager);
            }
            else
            {
                var user = userService.Get(id).Result;
                chatRoom = ChatRoom.From(user);
            }

            if (chatRoom == null)
                return;

            ChatRooms.Add(chatRoom);
            ActivateChatRoom(chatRoom);
            NewConversationModalVisible = false;
        }

        public void ActivateChatRoom(ChatRoom chatRoom)
        {
            ChatRooms.ForEach(room => room.IsActive = false);
            chatRoom.IsActive = true;
            ActiveChatRoom = chatRoom;
            LoadMessages(ActiveChatRoom);
        }

        protected ICollection<UserInfo> GetAvailableUsers()
        {
            var applicationUsers = userService.GetAll();
            return applicationUsers.Where(t => t.UserName != Username).Select(t => new UserInfo() { UserName = t.UserName, UserId = t.Id, Online = ChatHub.IsUserConnected(t.UserName) }).OrderBy(t => t.Online).ToList(); //TODO STATUS
        }

        protected ICollection<Group> GetAvailableGroups()
        {
            return groupService.GetUsersGroups(UserId);
        }

        public async Task CreateGroup()
        {
            var group = await groupService.CreateGroup(NewGroup.Name);
            await groupService.AddMemberToGroup(@group.Id, UserId, true);
            NewGroup = new NewGroupModel();
            NewConversation(group.Id, true);
        }

        public async Task SendMessage(ChatRoom chatRoom)
        {
            await chatRoom.SendMessage(messageService, UserId, Username);
        }

        public async Task LeaveGroup(ChatRoom chatRoom)
        {
            CloseChatRoom(chatRoom);
            await groupService.RemoveUserFromGroup(chatRoom.Id, UserId);
        }

        public void CloseChatRoom(ChatRoom chatRoom)
        {

            ChatRooms.Remove(chatRoom);
            ActiveChatRoom = null;
        }
        public NewGroupModel NewGroup { get; set; } = new NewGroupModel();
        public bool NotificationDismissed { get; set; } = true;
        public string NotificationText { get; set; }
    }

    public class GroupManagementModel
    {
        public ICollection<ApplicationUser> AvailableUsers { get; set; }
        public ICollection<ApplicationUser> NormalUsers { get; set; }
        public ICollection<ApplicationUser> Managers { get; set; }
        public Guid? SelectedUserId { get; set; }
        public Guid GroupId { get; set; }
    }

    public class NewGroupModel
    {
        [Required]
        public string Name { get; set; }
    }
    public class UserInfo
    {
        public string UserName { get; set; }
        public Guid UserId { get; set; }
        public bool Online { get; set; }
    }
    public class ChatRoom
    {
        public bool IsGroup { get; set; }
        public bool IsManager { get; set; }
        public string Name { get; set; }
        public Guid Id { get; set; }
        public GridViewDataSet<Message> Messages { get; set; } = new GridViewDataSet<Message>();
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public BootstrapColor OnlineColor => BootstrapColor.Success;
        public BootstrapColor OfflineColor => BootstrapColor.Warning;
        public string NewMessageText { get; set; }

        public void LoadMessages(MessageService messageService, Guid userId)
        {
            Messages.PagingOptions.PageSize = 5;
            if (IsGroup)
            {
                Messages.LoadFromQueryable(messageService.GetMessages(Id).OrderByDescending(t => t.Date));
            }
            else
            {
                Messages.LoadFromQueryable(messageService.GetMessages(Id, userId).OrderByDescending(t => t.Date));
            }
        }

        public AlertType CurrentUserColor => AlertType.Success;

        public AlertType OtherUsersColor => AlertType.Primary;

        public static ChatRoom From(Group group, bool isManager)
        {
            return new ChatRoom()
            {
                Id = group.Id,
                Name = group.Name,
                IsGroup = true,
                IsOnline = true,
                IsManager = isManager
            };
        }

        public static ChatRoom From(ApplicationUser user)
        {
            return new ChatRoom()
            {
                Id = user.Id,
                Name = user.UserName,
                IsOnline = ChatHub.IsUserConnected(user.UserName)
            };
        }

        public async Task SendMessage(MessageService messageService, Guid senderId, string senderName)
        {
            Messages.GoToFirstPage();
            await messageService.Send(NewMessageText, Id, IsGroup, senderId, senderName);
            NewMessageText = String.Empty;
        }

        protected bool Equals(ChatRoom other)
        {
            return IsGroup == other.IsGroup && Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ChatRoom)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IsGroup.GetHashCode() * 397) ^ Id.GetHashCode();
            }
        }
    }
}
