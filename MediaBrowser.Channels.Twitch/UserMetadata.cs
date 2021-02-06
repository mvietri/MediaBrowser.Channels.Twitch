using System;
using System.Collections.Generic;
using System.Text;

namespace MediaBrowser.Channels.Twitch
{
    public class UserMetadata
    {
        public int _total { get; set; }
        public UserMetadataItem[] Users { get; set; }
    }

    public class UserMetadataItem
    {
        public long _id { get; set; }
    }
}
