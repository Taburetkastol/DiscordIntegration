 namespace Terrasoft.Configuration.Omnichannel.Messaging
{
	using Newtonsoft.Json;
	using Newtonsoft.Json.Serialization;
	using OmnichannelProviders.Application.Http;
	using OmnichannelProviders.Domain.Entities;
	using OmnichannelProviders.MessageWorkers;
	using Terrasoft.Core;

	#region Class: DiscordOutcomeMessageWorker

	/// <summary>
	/// Class that send Test provider messages.
	/// </summary>
	public class DiscordOutcomeMessageWorker : IOutcomeMessageWorker
	{

		#region Properties: Protected 

		protected UserConnection UserConnection;
		private readonly string _discordProviderApiUrl = "https://creatio-bot.integration.local";
		#endregion

		#region Constructors: Public

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordOutcomeMessageWorker "/> class.
		/// <param name="userConnection">Instance of the <see cref="UserConnection"/>.</param>
		/// </summary>
		public DiscordOutcomeMessageWorker(UserConnection userConnection) {
			UserConnection = userConnection;
		}

		#endregion

		#region Methods: Public	

		/// <summary>
		/// Send message to Discord provider.
		/// </summary>
		/// <param name="message">UnifiedMessage message.</param>
		public string SendMessage(UnifiedMessage unifiedMessage) {
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			var json = JsonConvert.SerializeObject(unifiedMessage, serializerSettings);
			var requestUrl = string.Concat(_discordProviderApiUrl, json);
			var result = new HttpRequestSender().PostAsync(requestUrl, json).Result;
			return result;
		}

        public string SendMessage(UnifiedMessage unifiedMessage, out bool success)
        {
            throw new System.NotImplementedException();
        }

        public string PassControlToPrimaryReceiver(UnifiedMessage message)
        {
            throw new System.NotImplementedException();
        }

        #endregion
    }

	#endregion
}