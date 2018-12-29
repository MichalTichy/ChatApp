using System;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.WEB.DAL;
using ChatApp.WEB.Hub;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace ChatApp.WEB.Services
{
    public class MessageService
    {
        private readonly AppDbContext context;
        private readonly IHubContext<ChatHub> hubContext;
        private readonly GroupService groupService;
        private readonly UserService userService;

        public MessageService(AppDbContext context,IHubContext<ChatHub> hubContext,GroupService groupService,UserService userService)
        {
            this.context = context;
            this.hubContext = hubContext;
            this.groupService = groupService;
            this.userService = userService;
        }
        public IQueryable<Message> GetMessages(Guid recipient,Guid sender)
        {
            var messages = context.Set<Message>();
            var enumerable = messages.Where(t => t.Recipient == recipient && t.Sender==sender || t.Recipient==sender && t.Sender==recipient);
            return enumerable.OrderBy(t=>t.Date);
        }
        public IQueryable<Message> GetMessages(Guid groupId)
        {
            var messages = context.Set<Message>();
            var enumerable = messages.Where(t => t.Recipient == groupId);
            return enumerable.OrderBy(t=>t.Date);
        }

        public async Task Send(string text, Guid recipient,bool isGroup, Guid senderId,string senderName)
        {
            var message = new Message()
            {
                Data = text,
                Date = DateTime.Now,
                IsGroupMessage = isGroup, Recipient = recipient, SenderName = senderName, Sender = senderId
            };
            context.Set<Message>().Add(message);
            await context.SaveChangesAsync();

            var dateWithoutTimezone = new DateTime(message.Date.Year, message.Date.Month, message.Date.Day, message.Date.Hour, message.Date.Minute, message.Date.Second);
            
            if (message.IsGroupMessage)
            {
                var groupName = (await groupService.Get(recipient)).Name;
                await hubContext.Clients.GroupExcept(recipient.ToString(),ChatHub.ConnectedUsers[senderName]).SendAsync("ReceiveMessage", message.Recipient, message.Data, dateWithoutTimezone.ToString("O"), message.Sender, message.SenderName,groupName, message.IsGroupMessage, message.Id);
            }
            else
            {
                var user = await userService.Get(message.Recipient);
                if (ChatHub.IsUserConnected(user.UserName))
                {
                    var connectedUser = ChatHub.ConnectedUsers[user.UserName];
                    var recipientName = (await userService.Get(recipient)).UserName;
                    await hubContext.Clients.Client(connectedUser).SendAsync("ReceiveMessage", message.Sender, message.Data, dateWithoutTimezone.ToString("O"), message.Sender, message.SenderName,recipientName, message.IsGroupMessage,message.Id);
                }
            }
        }
    }
}