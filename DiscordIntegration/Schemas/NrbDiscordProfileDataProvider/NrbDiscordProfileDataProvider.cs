 namespace Terrasoft.Configuration.Omnichannel.Messaging
{
	using Newtonsoft.Json;
	using OmnichannelProviders.Domain.Entities;
	using OmnichannelProviders.Interfaces;
	using System.IO;
	using System.Net;
	using Terrasoft.Core;

	public class DiscordProfileData {
		public string userName { get; set; }
		public string discriminator { get; set; }
	}

	#region Class: DiscordProfileDataProvider

	/// <summary>
	/// Get profile data from Discord provider.
	/// </summary>
	public class DiscordProfileDataProvider : IProfileDataProvider
	{
		#region Properties: Private 

		// Discord endpoint to get user profile.
		private readonly string _discordProviderApiUrl = "https://discord.com/api/v8/users/";

		#endregion

		#region Constructors: Public 

		/// <summary>
		/// Initializes new instance of <see cref="DiscordProfileDataProvider"/>
		/// </summary>
		/// <param name="userConnection">User connection.</param>
		public DiscordProfileDataProvider(UserConnection userConnection) {
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Get profile data from discord.
		/// <param name="profileId">Discord profile identifier.</param>
		/// <param name="channelId">Channel from which we will send request.</param>
		/// <returns>Contact identifier.</returns>
		/// </summary>
		public ProfileData GetProfileDataByProfileId(string profileId, string channelId)	{
			var requestUrl = string.Concat(_discordProviderApiUrl, profileId);
			WebRequest request = WebRequest.Create(requestUrl);
			try	{
				using (var response = request.GetResponse()) {
					using (Stream stream = response.GetResponseStream()) {
						using (StreamReader sr = new StreamReader(stream)) {
							var discordProfile = JsonConvert.DeserializeObject<DiscordProfileData>(sr.ReadToEnd());
							return new ProfileData {
								FirstName = discordProfile.userName,
								LastName = discordProfile.discriminator,
							};
						}
					}
				}
			}
			catch {
				return new ProfileData();
			}
		}

		#endregion

	}

	#endregion
}