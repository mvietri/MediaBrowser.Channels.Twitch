using MediaBrowser.Model.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Channels.Twitch.Api
{
    class ServerApiEndpoints : IService
    {
        public ServerApiEndpoints()
        {

        }

        public void Post(VideoSend request)
        {
            if (String.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Name cannot be empty.");
            }
            if (String.IsNullOrWhiteSpace(request.Path))
            {
                throw new ArgumentException("Path cannot be empty.");
            }
            if (String.IsNullOrWhiteSpace(request.UserId))
            {
                throw new ArgumentException("UserId cannot be empty.");
            }

            List<TwitchChannel> list = Plugin.Instance.Configuration.TwitchChannels.ToList<TwitchChannel>();

            TwitchChannel channel = new TwitchChannel()
            {
                UserId = request.UserId,
                Name = request.Name,
                UserName = request.UserName,
                Image = "",
                Protocol = Model.MediaInfo.MediaProtocol.Http
            }; 

            list.Add(channel);

            Plugin.Instance.Configuration.TwitchChannels = list.ToArray();

            Plugin.Instance.SaveConfiguration();
        }
    }
}
