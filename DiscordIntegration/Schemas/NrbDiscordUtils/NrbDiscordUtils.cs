namespace Terrasoft.Configuration.Omnichannel.Messaging
{
    using global::Common.Logging;
    using Discord.WebSocket;
    using OmnichannelProviders.Domain.Entities;
    using System;

    public class DiscordUtils
    {
        private ILog _log;
        protected ILog Log
        {
            get
            {
                return _log ?? (_log = LogManager.GetLogger("OmnichannelMessageHandler"));
            }
        }
        public DiscordIncomingMessage convertToDiscordMessage(SocketMessage msg)
        {
            var result = new DiscordIncomingMessage()
            {
                Id = Guid.NewGuid(),
                Sender = msg.Author.Username,
                Recipient = msg.Channel.Id.ToString(),
                Message = msg.Content,
                Timestamp = msg.Timestamp.ToUnixTimeSeconds(),
                Source = ChannelType.Discord,
                MessageType = MessageType.Text,
                MessageDirection = msg.Author.Username == "Creatio Integration"
                                    ? MessageDirection.Outcoming
                                    : MessageDirection.Incoming,
                //Attachments = msg.Attachments.,
            };
            Log.Debug($"{result.Id} {result.Sender} {result.Recipient} {result.Message} {result.Timestamp} {result.Source}");


            return result;
        }
    }
}