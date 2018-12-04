using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.WEB.ViewModels
{
    public class DefaultViewModel : MasterPageViewModel
    {
        public List<Message> Messages { get; set; } = new List<Message>(){new Message(){test = "1",test2 = "1"},new Message(){test = "2",test2 = "2"}};
        public bool NewConversationModalVisible { get; set; }
    }

    public class Message
    {
        public bool IsNew { get; set; }
        public string test { get; set; }
        public string test2 { get; set; }
    }
}
