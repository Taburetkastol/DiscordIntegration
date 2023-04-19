namespace Terrasoft.Configuration.Omnichannel.Messaging
{
    using System;
    /// <summary>
    /// Class that describes Discord channel info.
    /// </summary>
    public class DiscordChannelInfo
    {
        #region Constructors: Public

        /// <summary>
        /// Creates an instance of <see cref="DiscordChannelInfo"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        public DiscordChannelInfo(string id, string userName)
        {
            Id = id;
            UserName = userName;
        }

        /// <summary>
        /// Creates an instance of <see cref="DiscordChannelInfo"/>
        /// </summary>
        /// <param name="settingsId"></param>
        /// <param name="id"></param>
        /// <param name="userName"></param>
        public DiscordChannelInfo(Guid settingsId, string id, string userName)
        {
            SettingsId = settingsId;
            Id = id;
            UserName = userName;
        }

        #endregion

        #region Fields: Public

        public string Id { get; }
        public string UserName { get; }
        public Guid SettingsId { get; set; }

        #endregion
    }
}