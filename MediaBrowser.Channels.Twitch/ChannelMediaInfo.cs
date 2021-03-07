using MediaBrowser.Common.Extensions;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.MediaInfo;
using System;
using System.Collections.Generic;

namespace MediaBrowser.Channels.Twitch
{
    public class ChannelMediaInfo
    {
        public ChannelMediaInfo(string path)
        {
            Path = path;
        }

        public int? AudioBitrate { get; set; }
        public int? AudioChannels { get; set; }
        public string AudioCodec { get; set; }
        public int? AudioSampleRate { get; set; }
        public TwitchChannel Channel { get; set; }

        public string Container { get; set; }
        public float? Framerate { get; set; }
        public int? Height { get; set; }
        public string Id { get; set; }
        public bool? IsAnamorphic { get; set; }
        public string Path { get; set; }

        public MediaProtocol Protocol { get; set; }
        public bool ReadAtNativeFramerate { get; set; }
        public Dictionary<string, string> RequiredHttpHeaders { get; set; }
        public long? RunTimeTicks { get; set; }
        public bool SupportsDirectPlay { get; set; }
        public int? VideoBitrate { get; set; }
        public string VideoCodec { get; set; }
        public float? VideoLevel { get; set; }
        public string VideoProfile { get; set; }
        public int? Width { get; set; }
        public MediaSourceInfo ToMediaSource()
        {
            Path = Channel.Path;

            if (!Path.EndsWith("m3u8", StringComparison.InvariantCultureIgnoreCase))
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