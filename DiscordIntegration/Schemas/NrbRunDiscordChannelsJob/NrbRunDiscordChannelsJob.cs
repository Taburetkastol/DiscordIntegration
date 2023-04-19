namespace Terrasoft.Configuration.Omnichannel.Messaging
{
	using global::Common.Logging;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Discord.WebSocket;
	using Terrasoft.Common;
	using Terrasoft.Core;
	using Terrasoft.Core.DB;

	public class RunDiscordChannelsJob : IJobExecutor
    {
		#region Fields: Private

		private UserConnection _userConnection;
		private List<string> _channelsCache;
		private DiscordSocketClient _client;
		private ILog _log;

		#endregion

		#region Fields: Public

		public static string CacheChannelsName = "DiscordChannelsList";

		#endregion

		#region Properties: Protected

		protected ILog Log
		{
			get
			{
				return _log ?? (_log = LogManager.GetLogger("OmnichannelMessageHandler"));
			}
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Get a list of all Discord Channels that are active.
		/// </summary>
		/// <returns></returns>
		private List<string> GetAllActiveDiscordChannels()
		{
			List<string> list = new List<string>();
			Select channelSelect = new Select(_userConnection)
				.Column("Id")
				.From("Channel")
				.Where("ProviderId").IsEqual(Column.Parameter(new Guid("485B5CA7-D878-4BEE-BFBD-30732BF82CE4")))
					.And("IsActive").IsEqual(Column.Parameter(true)) as Select;
			channelSelect.ExecuteReader(reader => {
				list.Add(reader.GetColumnValue<Guid>("Id").ToString());
			});
			return list;
		}

		/// <summary>
		/// Get a list of MsgSettingsIds of Discord Channels.
		/// </summary>
		/// <returns></returns>
		private List<string> GetAllActiveDiscordChannelMsgSettingsIds()
		{
			List<string> list = new List<string>();
			Select channelSelect = new Select(_userConnection)
				.Column("MsgSettingsId")
				.From("Channel")
				.Where("ProviderId").IsEqual(Column.Parameter(new Guid("485B5CA7-D878-4BEE-BFBD-30732BF82CE4")))
				.And("IsActive").IsEqual(Column.Parameter(true)) as Select;
			channelSelect.ExecuteReader(reader => {
				list.Add(reader.GetColumnValue<Guid>("MsgSettingsId").ToString());
			});
			return list;
		}

		/// <summary>
		/// Cache channels info.
		/// </summary>
		/// <param name="newChannels"></param>
		private void AddToCache(List<string> newChannels)
		{
			_channelsCache.AddRangeIfNotExists(newChannels);
			_userConnection.ApplicationCache[CacheChannelsName] = _channelsCache;
		}

		/// <summary>
		/// Start channels that haven't been run yet.
		/// </summary>
		private void RunDiscordChannels()
		{
			var channels = GetAllActiveDiscordChannels();
			_channelsCache = _userConnection.ApplicationCache[CacheChannelsName] as List<string> ?? new List<string>();
			var notRanChannels = channels.Where(c => !_channelsCache.Contains(c)).ToList();
			if (notRanChannels.Count > 0)
			{
				AddToCache(notRanChannels);
				var activatedChannels = RunChannels(notRanChannels);
				if (activatedChannels.ToList().Count != notRanChannels.Count)
				{
					Log.ErrorFormat($"Tried to run by channel IDs [{string.Join(", ", notRanChannels)}]," +
						$" but ran only bot IDs [{string.Join(", ", activatedChannels.Select(c => c.Id))}]");
				}
			}
		}

		/// <summary>
		/// Check connection to channels.
		/// </summary>
		private void CheckDiscordChannels()
		{
			var msgSettingsIds = GetAllActiveDiscordChannelMsgSettingsIds();
			throw new NotImplementedException();
		}

		/// <summary>
		/// Start channels connection.
		/// </summary>
		/// <param name="notRanChannels"></param>
		/// <returns></returns>
		private IEnumerable<DiscordChannelInfo> RunChannels(List<string> notRanChannels)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Methods: Public

		/// <summary>
		/// Execute Discord Jobs.
		/// </summary>
		/// <param name="userConnection"></param>
		/// <param name="parameters"></param>
		public virtual void Execute(UserConnection userConnection, IDictionary<string, object> parameters)
		{
			Log.DebugFormat("RunDiscordChannelsJob have been started");
			_userConnection = userConnection;
			RunDiscordChannels();
			CheckDiscordChannels();
			Log.DebugFormat("RunDiscordChannelsJob finished");
		}

        #endregion
    }
}