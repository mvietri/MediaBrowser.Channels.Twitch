using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Services;

namespace MediaBrowser.Channels.Twitch.Api
{
    [Route("/Twitch/TwitchChannels", "POST", Summary = "Bookmarks a channel")]
    public class VideoSend : IReturnVoid, IReturn
    {
        [ApiMember(Name = "ImagePath", Description = "ImagePath", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string ImagePath
        {
            get;
            set;
        }

        [ApiMember(Name = "Name", Description = "Name", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Name
        {
            get;
            set;
        }

        [ApiMember(Name = "UserName", Description = "UserName", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string UserName
        {
            get;
            set;
        }

        [ApiMember(Name = "Path", Description = "Path", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Path
        {
            get;
            set;
        }

        [ApiMember(Name = "Protocol", Description = "Protocol", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public MediaProtocol Protocol
        {
            get;
            set;
        }

        [ApiMember(Name = "UserId", Description = "UserId", IsRequired = true, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string UserId
        {
            get;
            set;
        }

        [ApiMember(Name = "Quality", Description = "Quality", IsRequired = false, DataType = "string", ParameterType = "path", Verb = "POST")]
        public string Quality
        {
            get;
            set;
        }

        public VideoSend()
        {

        }
    }
}
