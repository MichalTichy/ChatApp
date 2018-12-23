using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        public override Task Load()
        {
            if (!ChatRooms.Any())
            {
                NewConversationModalVisible = true;
            }

            return base.Load();
        }

        public override Task PreRender()
        {
            if (ActiveChatRoom!=null)
            {
                ActiveChatRoom.LoadMessages(messageService, UserId);
                ActiveChatRoom.Messages.Items = ActiveChatRoom.Messages.Items.Reverse().ToList();
            }
            return base.PreRender();
        }

        public void LoadMessages(ChatRoom chatRoom)
        {
            chatRoom.LoadMessages(messageService,UserId);
        }

        public void NewConversation(Guid recipientId, bool isGroup)
        {
            var chatRoom = ChatRooms.FirstOrDefault(t => t.Id == recipientId);
            if (chatRoom != null)
            {
                ActivateChatRoom(chatRoom);
                NewConversationModalVisible = false;
                return;
            }

            if (isGroup)
            {
                var group = groupService.Get(recipientId).Result;
                chatRoom = ChatRoom.From(group);
            }
            else
            {
                var user = userService.Get(recipientId).Result;
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
            return applicationUsers.Where(t => t.UserName != Username).Select(t => new UserInfo() { UserName = t.UserName, UserId = t.Id,Online = ChatHub.IsUserConnected(t.UserName)}).OrderBy(t => t.Online).ToList(); //TODO STATUS
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
        }

        public async Task SendMessage(ChatRoom chatRoom)
        {
            await chatRoom.SendMessage(messageService, UserId, Username);
        }

        public NewGroupModel NewGroup { get; set; } = new NewGroupModel();
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
        public string Name { get; set; }
        public Guid Id { get; set; }
        public GridViewDataSet<Message> Messages { get; set; } = new GridViewDataSet<Message>();
        public bool IsActive { get; set; }
        public bool IsOnline { get; set; }
        public BootstrapColor OnlineColor => BootstrapColor.Success;
        public BootstrapColor OfflineColor => BootstrapColor.Danger;
        public string NewMessageText { get; set; }
        public void LoadMessages(MessageService messageService,Guid userId)
        {
            Messages.PagingOptions.PageSize = 10;
            Messages.LoadFromQueryable(messageService.GetMessages(Id,userId).OrderByDescending(t=>t.Date));
        }

        public static ChatRoom From(Group group)
        {
            return new ChatRoom()
            {
                Id = group.Id,
                Name = group.Name,
                IsGroup = true,
                IsOnline = true
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

        public async Task SendMessage(MessageService messageService,Guid senderId,string senderName)
        {
            await messageService.Send(NewMessageText, Id,IsGroup,senderId,senderName);
            NewMessageText=String.Empty;
        }
    }
}
