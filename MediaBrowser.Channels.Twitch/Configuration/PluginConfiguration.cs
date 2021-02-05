using MediaBrowser.Model.Plugins;
using System;

namespace MediaBrowser.Channels.Twitch.Configuration
{
    /// <summary>
    /// Class PluginConfiguration
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        public TwitchChannel[] TwitchChannels { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration" /> class.
        /// </summary>
        public PluginConfiguration()
        {
            TwitchChannels = Array.Empty<TwitchChannel>();
        }
    }
}
