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
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;
using System.Globalization;

namespace MediaBrowser.Channels.Twitch
{
    public class Channel : IChannel, ISupportsLatestMedia, IHasCacheKey, IHasChangeEvent
    { 
        private readonly IHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public event EventHandler ContentChanged;
        private Timer _updateTimer;

        private const string TWITCH_CLIENT_ID = "jexf81lm1alf3o79290nyzbulhc4dy";

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

        public void Dispose()
        {
            if (_updateTimer != null)
            {
                _updateTimer.Dispose();
                _updateTimer = null;
            }
        }

        public Channel(IHttpClient httpClient, ILogManager logManager, IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _logger = logManager.GetLogger(GetType().Name);
            _jsonSerializer = jsonSerializer; 
            
            var interval = TimeSpan.FromMinutes(5);
            _updateTimer = new Timer(OnUpdateTimerCallback, null, interval, interval);
        }

        // Increment as needed to invalidate all caches
        public string DataVersion => "10";

        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            return await this.GetChannelItemsInternal(query, cancellationToken).ConfigureAwait(false);
        }
          
        private async Task<ChannelItemResult> GetChannelItemsInternal(InternalChannelItemQuery query, CancellationToken cancellationToken)
        { 
            List<ChannelItemInfo> items = new List<ChannelItemInfo>();

            foreach (TwitchChannel i in Plugin.Instance.Configuration.TwitchChannels)
            {
                // get user id
                UserMetadata userMetadata = null;
                ChannelMetadata channelMetadata = null;

                if (String.IsNullOrWhiteSpace(i.ChannelId))
                {
                    userMetadata = await GetMetadataFromTwitchUserAsync(i.UserName);

                    if (userMetadata != null && userMetadata.Users.Length > 0)
                        i.ChannelId = userMetadata.Users[0]._id.ToString();
                    else
                        i.ChannelId = string.Empty;

                    Plugin.Instance.SaveConfiguration(); // keep user id in db
                }

                if (String.IsNullOrWhiteSpace(i.ChannelId) == false)
                    channelMetadata = await GetMetadataFromTwitchChannelAsync(i.ChannelId);

                string imageUrl = String.Empty;
                string overview = $"No description";
                
                if (channelMetadata != null)
                {
                    overview = channelMetadata.description;

                    if (string.IsNullOrWhiteSpace(channelMetadata.profile_banner) == false)
                        imageUrl = channelMetadata.profile_banner;

                    if (string.IsNullOrWhiteSpace(imageUrl) && string.IsNullOrWhiteSpace(channelMetadata.video_banner) == false) // Profile banner has priority
                        imageUrl = channelMetadata.video_banner;
                }

                items.Add(new ChannelItemInfo
                {
                    ContentType = ChannelMediaContentType.Clip,
                    ImageUrl = imageUrl,
                    IsLiveStream = true,
                    MediaType = ChannelMediaType.Video,
                    MediaSources = (List<MediaSourceInfo>)await GetMediaSources(i.UserName, cancellationToken),
                    Name = i.Name.ToLowerInvariant(),
                    Id = i.UserName.ToLowerInvariant(),
                    ExtraType = ExtraType.Clip,
                    Overview = overview,
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

        private async Task<ChannelMetadata> GetMetadataFromTwitchChannelAsync(string id)
        {
            try
            {
                HttpRequestOptions requestOptions = new HttpRequestOptions();
                requestOptions.Url = $"https://api.twitch.tv/kraken/channels/{id}";
                requestOptions.AcceptHeader = "application/vnd.twitchtv.v5+json";
                requestOptions.RequestHeaders.Add("Client-ID", TWITCH_CLIENT_ID);

                using (var data = await _httpClient.Get(requestOptions).ConfigureAwait(false))
                {
                    return _jsonSerializer.DeserializeFromStream<ChannelMetadata>(data);
                }
            }
            catch
            {
                return null;
            }
        }

        private async Task<UserMetadata> GetMetadataFromTwitchUserAsync(string userName)
        { 
            try
            {
                HttpRequestOptions requestOptions = new HttpRequestOptions();
                requestOptions.Url = $"https://api.twitch.tv/kraken/users?login={userName.ToLowerInvariant()}";
                requestOptions.AcceptHeader = "application/vnd.twitchtv.v5+json";
                requestOptions.RequestHeaders.Add("Client-ID", TWITCH_CLIENT_ID);

                using (var data = await _httpClient.Get(requestOptions).ConfigureAwait(false))
                {
                    return _jsonSerializer.DeserializeFromStream<UserMetadata>(data);
                }
            }
            catch
            {
                return null;
            }
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
                channel.Path += process.StandardOutput.ReadLine();
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

                SupportsSortOrderToggle = false,

                AutoRefreshLevels = 3 
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
        {
            var now = DateTimeOffset.UtcNow;

            var values = new List<string>();

            values.Add(now.DayOfYear.ToString(CultureInfo.InvariantCulture));
            values.Add(now.Hour.ToString(CultureInfo.InvariantCulture));

            double minute = now.Minute;
            minute /= 5;

            values.Add(Math.Floor(minute).ToString(CultureInfo.InvariantCulture)); 

            return string.Join("-", values.ToArray());
        }

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