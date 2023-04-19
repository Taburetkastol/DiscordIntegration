 namespace Terrasoft.Configuration.Omnichannel.Messaging
{
	using System;
    using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;
	using Terrasoft.Web.Common.ServiceRouting;

	/// <summary>
	/// Service for sending and receiving messages from messaging integration api.
	/// </summary>
	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	[DefaultServiceRoute]
	public class DiscordOmnichannelMessagingService : OmnichannelMessagingService
	{
		#region Fields: Private

		private UserConnection _userConnection;

        #endregion

        #region Constructors: Public

        public DiscordOmnichannelMessagingService() : base() {
		}

		/// <summary>
		/// Initializes new instance of <see cref="DiscordOmnichannelMessagingService"/>.
		/// </summary>
		public DiscordOmnichannelMessagingService(UserConnection userConnection) : base(userConnection) {
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Set channel queue fro incoming message.
		/// </summary>
		/// <param name="message"></param>
		private void GetChannelAndQueueBySource(MessagingMessage message) {
			Select channelSelect = new Select(UserConnection)
				.Top(1).Column("Id")
				.Column("ChatQueueId")
				.From("Channel")
				.Where("Source").IsEqual(Column.Parameter(message.Recipient))
				.And("IsActive").IsEqual(Column.Parameter(true)) as Select;               
			channelSelect.ExecuteReader(reader => {
				message.ChannelId = reader.GetColumnValue<Guid>("Id").ToString();
				message.ChannelQueueId = reader.GetColumnValue<Guid>("ChatQueueId");
			});
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Receives messages from integration api.
		/// </summary>
		/// <param name="message">Discord provider message.</param>
		[OperationContract]
		[WebInvoke(UriTemplate = "receive", Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		  ResponseFormat = WebMessageFormat.Json)]
		public void ReceiveMessage(DiscordIncomingMessage message) {
			MessagingMessage messagingMessage = new MessagingMessage(DiscordIncomingMessageConverter.Convert(message));
			GetChannelAndQueueBySource(messagingMessage);
			InternalReceive(messagingMessage);
		}

		/// <summary>
		/// Test method.
		/// </summary>
		/// <returns>Pong.</returns>
		[OperationContract]
		[WebInvoke(UriTemplate = "ping", Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare,
		  ResponseFormat = WebMessageFormat.Json)]
		public string Ping()
		{
			return "Pong";
		}

		#endregion

	}
}