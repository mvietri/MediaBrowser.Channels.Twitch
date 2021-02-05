using System;
using System.Collections.Generic;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Common.Extensions;

namespace MediaBrowser.Channels.Twitch
{
    public class ChannelMediaInfo
    {
        public TwitchChannel Channel { get; set; }

        public string Path { get; set; }

        public Dictionary<string, string> RequiredHttpHeaders { get; set; }

        public string Container { get; set; }
        public string AudioCodec { get; set; }
        public string VideoCodec { get; set; }

        public int? AudioBitrate { get; set; }
        public int? VideoBitrate { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? AudioChannels { get; set; }
        public int? AudioSampleRate { get; set; }

        public string VideoProfile { get; set; }
        public float? VideoLevel { get; set; }
        public float? Framerate { get; set; }

        public bool? IsAnamorphic { get; set; }

        public MediaProtocol Protocol { get; set; }

        public long? RunTimeTicks { get; set; }

        public string Id { get; set; }

        public bool ReadAtNativeFramerate { get; set; }
        public bool SupportsDirectPlay { get; set; }

        public ChannelMediaInfo(string path)
        {
            Path = path;
        }

        public MediaSourceInfo ToMediaSource()
        {
            Path = Channel.Path;

            if (Path.ToLowerInvariant().EndsWith("m3u8") == false)
                Path = ""; // no live stream

            var id = Path.GetMD5().ToString("N");

            var source = new MediaSourceInfo
            {
                Container = Container,
                Protocol = Protocol,
                Path = Path,
                RequiredHttpHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                RunTimeTicks = null,
                Name = id,
                Id = id, 
                SupportsDirectStream = true,
                SupportsDirectPlay = SupportsDirectPlay,
                IsRemote = true,
                IsInfiniteStream = true
            };

            source.InferTotalBitrate();

            return source;
        }
    }
}