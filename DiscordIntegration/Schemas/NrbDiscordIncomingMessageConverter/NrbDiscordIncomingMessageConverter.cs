namespace Terrasoft.Configuration.Omnichannel.Messaging
{
	using OmnichannelProviders.Domain.Entities;
	using System;
	using System.Collections.Generic;

	public class DiscordIncomingMessage : UnifiedMessage
	{ }

	public static class DiscordIncomingMessageConverter
	{
		#region Methods: Public	

		/// <summary>
		/// Converts DiscordIncomingMessage to MessagingMessage.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public static MessagingMessage Convert(DiscordIncomingMessage message)	{
			var messageType = MessageType.Text;
			var messageId = Guid.NewGuid();
			var result = new MessagingMessage {
				Id = messageId,
				Message = message.Message,
				Recipient = message.Recipient,
				Sender = message.Sender, 
				Timestamp = message.Timestamp,
				ChannelId = message.ChannelId, 
				MessageDirection = MessageDirection.Incoming,
				MessageType = messageType,
				Source = ChannelType.Discord, 
				ChannelName = "Discord" 
			};
			if (messageType != MessageType.Text) {
				result.Attachments = new List<MessageAttachment>();
				foreach (var attachment in message.Attachments){
					result.Attachments.Add(new MessageAttachment {
						MessageId = messageId,
						UploadUrl = attachment.UploadUrl,
						FileType = attachment.FileType
					});
				}
			}
			return result;
		}

		#endregion

	}
}