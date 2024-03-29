﻿using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Bootstrap4;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ChatApp.WEB
{
    public class DotvvmStartup : IDotvvmStartup, IDotvvmServiceConfigurator
    {
        // For more information about this class, visit https://dotvvm.com/docs/tutorials/basics-project-structure
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
            ConfigureControls(config, applicationPath);
            ConfigureResources(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Default", "", "Views/default.dothtml");
            config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));
        }

        private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
        {
            // register code-only controls and markup controls
            config.AddBootstrap4Configuration();
            config.Markup.AddMarkupControl("cc", "NewConversation", "Controls/NewConversation.dotcontrol");
            config.Markup.AddMarkupControl("cc", "GroupManagement", "Controls/GroupManagement.dotcontrol");
        }

        private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
        {
            // register custom resources and adjust paths to the built-in resources
            config.Resources.Register("Styles", new StylesheetResource()
            {
                Location = new UrlResourceLocation("~/site.min.css")
            });

            config.Resources.Register("SignalR", new ScriptResource()
            {
                Location = new UrlResourceLocation(@"~/lib/signalR/dist/browser/signalR.min.js")
            });
            config.Resources.Register("Chat", new ScriptResource()
            {
                Location = new UrlResourceLocation("~/chat.js"),
                Dependencies = new[] { "SignalR" }
            });
        }
        public void ConfigureServices(IDotvvmServiceCollection options)
        {
            options.AddDefaultTempStorages("temp");
        }
    }
}
