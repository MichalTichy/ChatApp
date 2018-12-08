using System;

namespace ChatApp.WEB.DAL
{
    public class Message : EntityBase
    {
        public DateTime Date { get; set; }
        public Guid Sender { get; set; }
        public Guid Recipient { get; set; }
        public bool IsGroupMessage { get; set; }
    }
}