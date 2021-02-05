﻿using MediaBrowser.Model.MediaInfo;

namespace MediaBrowser.Channels.Twitch
{
    public class TwitchChannel
    {
        public TwitchChannel() { }

        public string Name { get; set; }
        public string UserName { get; set; }
        public string Quality { get; set; }
        public string UserId { get; set; }
        public string Path { get; set; }
        public string Image { get; set; }
        public MediaProtocol Protocol { get; set; }
    }
}
