using System;
using System.Collections.Generic;
using System.Text;

namespace MediaBrowser.Channels.Twitch
{
    public class ChannelMetadata
    { 
        public string status { get; set; }
        public string logo { get; set; }
        public string video_banner { get; set; }
        public string profile_banner { get; set; }
        public string description { get; set; }
    }
}
