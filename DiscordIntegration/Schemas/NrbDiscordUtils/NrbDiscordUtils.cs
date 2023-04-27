namespace Terrasoft.Configuration.Omnichannel.Messaging
{
    using global::Common.Logging;
    using Discord.WebSocket;
    using OmnichannelProviders.Domain.Entities;
    using System;
    using Terrasoft.Core.DB;
    using System.Threading.Tasks;
    using Terrasoft.Core;
    using System.Collections.Generic;
    using Terrasoft.Common;

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
        public DiscordIncomingMessage convertToDiscordMessage(SocketMessage msg, UserConnection userConnection)
        {
            var contact = new Dictionary<ContactIdentificationType, string>();
            var channelId = new Guid();
            contact[ContactIdentificationType.Name] = msg.Author.Username;
            contact[ContactIdentificationType.Phone] = null;
            Select channelSelect = new Select(userConnection)
                .Top(1).Column("Id")
                .From("Channel")
                .Where("Source").IsEqual(Column.Parameter("Discord"))
                .And("IsActive").IsEqual(Column.Parameter(true)) as Select;
            channelSelect.ExecuteReader(reader => {
                channelId = reader.GetColumnValue<Guid>("Id");
            });
            var result = new DiscordIncomingMessage()
            {
                Id = Guid.NewGuid(),
                Sender = msg.Author.Username,
                Recipient = msg.Channel.Id.ToString(),
                Message = msg.Content,
                ContactIdentification = contact,
                ChannelId = channelId.ToString(),
                Timestamp = msg.Timestamp.ToUnixTimeSeconds(),
                Source = ChannelType.Discord,
                MessageType = OmnichannelProviders.Domain.Entities.MessageType.Text,
                MessageDirection = msg.Author.Username == "Creatio Integration"
                                    ? OmnichannelProviders.Domain.Entities.MessageDirection.Outcoming
                                    : OmnichannelProviders.Domain.Entities.MessageDirection.Incoming,
                IsEcho = msg.Author.Username == "Creatio Integration"
                                    ? true
                                    : false,
                //Attachments = msg.Attachments.,
            };
            Log.Debug($"{result.Id} {result.Sender} {result.Recipient} {result.Message} {result.Timestamp} {result.Source}");
            Log.Info($"Lock for message: Omni_ChatLock_{msg.Channel.Id}_{msg.Author.Id}");
            if (userConnection.ApplicationCache[$"Omni_ChatLock_{msg.Channel.Id}_{msg.Author.Username}"] != null)
            {
                Log.Info("Message is cached in App");
                userConnection.ApplicationCache.Remove($"Omni_ChatLock_{msg.Channel.Id}_{msg.Author.Username}");
            }

            return result;
        }
    }
}