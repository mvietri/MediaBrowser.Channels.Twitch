using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Logging;
using System.Diagnostics;
using MediaBrowser.Model.MediaInfo;

namespace MediaBrowser.Channels.Twitch
{
    public class Channel : IChannel, IHasCacheKey, IHasChangeEvent
    {
        private readonly ILogger _logger;

        public event EventHandler ContentChanged;

        public void OnContentChanged()
        {
            if (this.ContentChanged != null)
            {
                this.ContentChanged(this, EventArgs.Empty);
            }
        }

        public Channel(ILogManager logManager)
        {
            _logger = logManager.GetLogger(GetType().Name);
        }

        // Increment as needed to invalidate all caches
        public string DataVersion => "3";

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            return await this.GetChannelItemsInternal(query, cancellationToken).ConfigureAwait(false);
        }
          
        private async Task<ChannelItemResult> GetChannelItemsInternal(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            List<ChannelItemInfo> items = new List<ChannelItemInfo>();

            foreach (TwitchChannel i in Plugin.Instance.Configuration.TwitchChannels)
            {
                items.Add(new ChannelItemInfo
                {
                    ContentType = ChannelMediaContentType.Clip,
                    ImageUrl = "",
                    IsLiveStream = true,
                    MediaType = ChannelMediaType.Video,
                    MediaSources = (List<MediaSourceInfo>)await GetMediaSources(i.UserName, cancellationToken),
                    Name = i.Name.ToLowerInvariant(),
                    Id = i.UserName.ToLowerInvariant(),
                    ExtraType = ExtraType.Clip,
                    Overview = $"{i.UserName}'s Twitch Channel",
                    Type = ChannelItemType.Media,
                });
            }

            ChannelItemResult channelItemResult = new ChannelItemResult
            {
                Items = items,
                TotalRecordCount = items.Count,
            };
            return await Task.FromResult(channelItemResult);
        }

        private void GetVodFromTwitch(ref TwitchChannel channel)
        {
            channel.Path = "";

            var processInfo = new ProcessStartInfo
            {
                FileName = "streamlink",
                Arguments = $"--stream-url https://www.twitch.tv/{channel.UserName.ToLowerInvariant()} {channel.Quality.ToLowerInvariant()}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };

            var process = new Process()
            {
                StartInfo = processInfo,
            };

            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                channel.Path = channel.Path + process.StandardOutput.ReadLine();
            }

            channel.Path = channel.Path.Replace(System.Environment.NewLine, "").Trim();
            process.WaitForExit();
        }

        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new List<ImageType>
            {
                ImageType.Thumb,
                ImageType.Primary,
                ImageType.Backdrop
            };
        }

        public string HomePageUrl => "http://www.twitch.tv";

        public string Name => "Twitch";

        public ChannelParentalRating ParentalRating
        {
            get { return ChannelParentalRating.GeneralAudience; }
        }

        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Clip
                },

                MediaTypes = new List<ChannelMediaType>
                {
                    ChannelMediaType.Video
                },

                SupportsContentDownloading = true, 

                SupportsSortOrderToggle = false
            };
        }

        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Thumb:
                case ImageType.Backdrop:
                    {
                        var path = GetType().Namespace + ".Images." + type.ToString().ToLowerInvariant() + ".png";

                        return Task.FromResult(new DynamicImageResponse
                        {
                            Format = ImageFormat.Png,
                            HasImage = true,
                            Stream = typeof(Channel).Assembly.GetManifestResourceStream(path)
                        });
                    }
                default:
                    throw new ArgumentException("Unsupported image type: " + type);
            }
        }

        public bool IsEnabledFor(string userId)
        {
            return true;
        }
         
        public string GetCacheKey(string userId)
            => Guid.NewGuid().ToString("N");

        Task<IEnumerable<MediaSourceInfo>> GetMediaSources(string id, CancellationToken cancellationToken)
        { 
            TwitchChannel tChannel = null;

            foreach (TwitchChannel i in Plugin.Instance.Configuration.TwitchChannels)
            {
                if (i.UserName.ToLowerInvariant().Equals(id.ToLowerInvariant()))
                {
                    tChannel = i;
                    break;
                }
            }

            if (tChannel != null)
            {
                GetVodFromTwitch(ref tChannel);

                return Task.FromResult<IEnumerable<MediaSourceInfo>>(new List<MediaSourceInfo>
                {
                    new MediaSourceInfo()
                    {
                        DirectStreamUrl  = tChannel.Path,
                        Path = tChannel.Path,
                        SupportsDirectPlay = true,
                        SupportsDirectStream = true,
                        SupportsTranscoding = false,
                        IsInfiniteStream  = true,
                        Name = $"{tChannel.UserName}_{tChannel.Path.ToLowerInvariant().Substring(tChannel.Path.Length - 5)}",
                        Id = $"stream_{tChannel.Path.ToLowerInvariant().Substring(tChannel.Path.Length - 5)}",
                        Type = MediaSourceType.Default,
                        Protocol = MediaProtocol.Http,
                       
                    }
                });
            }

            return Task.FromResult<IEnumerable<MediaSourceInfo>>(Enumerable.Empty<MediaSourceInfo>());
        }

        public string Description => string.Empty;
         
    }
}