using System;

namespace ChatApp.WEB.DAL
{
    public class Message : EntityBase
    {
        public string Data { get; set; }
        public DateTime Date { get; set; }
        public Guid Sender { get; set; }
        public string SenderName { get; set; }
        public Guid Recipient { get; set; }
        public bool IsGroupMessage { get; set; }
    }
}