﻿using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Channels.Twitch.Configuration;
using System.IO;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Controller.Channels;

namespace MediaBrowser.Channels.Twitch
{
    /// <summary>
    /// Class Plugin
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage
    {
        private IChannelManager ChannelManager;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, IChannelManager channelManager)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this; 
            ChannelManager = channelManager;
        }

        public static string PluginName = "Twitch";

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "twitch",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }

        private Guid _id = new Guid("40C12641-317D-48FC-ABFD-9F608FA0386A");

        public override Guid Id
        {
            get { return _id; }
        }

        /// <summary>
        /// Gets the name of the plugin
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get { return PluginName; }
        }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public override string Description
        {
            get
            {
                return "Live streams from your favourites Twitch channels.";
            }
        }

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".Images.plugin.png");
        }

        public ImageFormat ThumbImageFormat
        {
            get
            {
                return ImageFormat.Png;
            }
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Plugin Instance { get; private set; }

        public override void UpdateConfiguration(BasePluginConfiguration configuration)
        {
            base.UpdateConfiguration(configuration);
            this.ChannelManager.GetChannel<Channel>().OnContentChanged();
        }

        public event EventHandler ContentChanged;

        public void OnContentChanged()
        {
            if (ContentChanged != null)
            {
                ContentChanged(this, EventArgs.Empty);
            }
        }

        private void OnUpdateTimerCallback(object state)
        {
            OnContentChanged();
        }

    }
}