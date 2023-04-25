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
    using System.Threading.Tasks;
    using Discord;
    using Terrasoft.Core.Entities;

    public class RunDiscordChannelsJob : IJobExecutor
    {
		#region Fields: Private

		private UserConnection _userConnection;
		private List<string> _channelsCache;
		private DiscordSocketClient _client;
		private ILog _log;
		private ILog _discordLog;

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

		protected ILog DiscordLog
        {
            get
            {
				return _discordLog ?? (_discordLog = LogManager.GetLogger("DiscordMessageHandler"));
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
			var list = new List<string>();
			var channelSelect = new Select(_userConnection)
				.Column("MsgSettingsId")
				.From("Channel")
				.Where("ProviderId").IsEqual(Column.Parameter(new Guid("485B5CA7-D878-4BEE-BFBD-30732BF82CE4")))
				.And("IsActive").IsEqual(Column.Parameter(true)) as Select;
			channelSelect.ExecuteReader(reader => {
				list.Add(reader.GetColumnValue<Guid>("MsgSettingsId").ToString());
			});

			return list;
		}

		private List<string> GetAllActiveDiscordChannelTokens()
        {
			var list = new List<string>();
			var channelSelect = new Select(_userConnection)
				.Column("Token")
				.From("Channel")
				.Join(JoinType.Inner, "DiscordMsgSettings").On("DiscordMsgSettings", "Id").IsEqual("Channel", "MsgSettingsId")
				.Where("ProviderId").IsEqual(Column.Parameter(new Guid("485B5CA7-D878-4BEE-BFBD-30732BF82CE4")))
				.And("IsActive").IsEqual(Column.Parameter(true)) as Select;
			channelSelect.ExecuteReader(reader => {
				list.Add(reader.GetColumnValue<string>("Token").ToString());
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

		private void LogChannels(IEnumerable<string> list)
        {
			var i = 1;
			foreach(var item in list)
            {
				Log.Debug($"Channel {i}: {item}");
            }
        }

		/// <summary>
		/// Log discord to DiscordHandler LogManager.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		private Task LogDiscord(LogMessage msg)
        {
			DiscordLog.Info(msg);

			return Task.CompletedTask;
        }

		/// <summary>
		/// Start channels that haven't been run yet.
		/// </summary>
		private void RunDiscordChannels()
		{
			Log.Debug("Start RunDiscordChannels.");
			var channels = GetAllActiveDiscordChannels();
			_channelsCache = _userConnection.ApplicationCache[CacheChannelsName] as List<string> ?? new List<string>();
			LogChannels(channels);
			var notRanChannels = channels.Where(c => !_channelsCache.Contains(c)).ToList();
			LogChannels(notRanChannels);
			if (notRanChannels.Count > 0)
			{
				AddToCache(notRanChannels);
				Log.Debug("Cache added.");
				var activatedChannels = RunChannels(notRanChannels);
				if (activatedChannels.ToList().Count != notRanChannels.Count)
				{
					Log.ErrorFormat($"Tried to run by channel IDs [{string.Join(", ", notRanChannels)}]," +
						$" but ran only bot IDs [{string.Join(", ", activatedChannels.Select(c => c.Id))}]");
				}
			}
            else
            {
				var activatedChannels = RunChannels(notRanChannels);
				if (activatedChannels.ToList().Count != notRanChannels.Count)
				{
					Log.ErrorFormat($"Tried to run by channel IDs [{string.Join(", ", notRanChannels)}]," +
						$" but ran only bot IDs [{string.Join(", ", activatedChannels.Select(c => c.Id))}]");
				}
			}
			Log.Debug("Ended RunDiscordChannels.");
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
		/// Start channels connection that haven't been ran yet.
		/// </summary>
		/// <param name="notRanChannels"></param>
		/// <returns></returns>
		private IEnumerable<DiscordChannelInfo> RunChannels(List<string> notRanChannels)
		{
			Log.Debug("Started RunChannels.");
			var list = GetAllActiveDiscordChannelTokens();
			LogChannels(list);
			var infoList = new List<DiscordChannelInfo>();
			foreach(var token in list)
            {
				infoList.Add(RunDiscordChannel(token).Result);
				Log.Debug("Added bot to list.");
            }
			Log.Debug($"Ended RunChannels. {infoList[0].Id} {infoList[0].UserName}");

			return infoList;
		}

		/// <summary>
		/// Run one Discord channel with a given token.
		/// </summary>
		/// <param name="token">Discord bot token</param>
		/// <returns></returns>
		private async Task<DiscordChannelInfo> RunDiscordChannel(string token)
        {
			Log.Debug("Started RunDiscordChannel.");
			var config = new DiscordSocketConfig()
			{
				GatewayIntents = GatewayIntents.AllUnprivileged
			};
			_client = new DiscordSocketClient(config);
            try
            {
				Log.Info("Login Discord WebSocket client.");
				await _client.LoginAsync(TokenType.Bot, token);
				await _client.StartAsync();
				_client.Log += LogDiscord;
				Log.Debug($"DiscordChannel {_client.ConnectionState}");
                _client.MessageReceived += ReceiveMessage;
			}
            catch(Exception e)
            {
				Log.Error($"{e.Message} {e.StackTrace}");
            }
			//Log.Debug("Before info collected");
			//var id = _client.CurrentUser.Id;
			//Log.Debug($"Bot's id: {id}");
			//var userName = _client.GetUser(id).Username;
			//Log.Debug($"Bot's name: {userName}");
			Log.Debug("Ended RunDiscordChannel.");

			return new DiscordChannelInfo(1231241.ToString(), "Creatio Integration");
		}

		private async Task ReceiveMessage(SocketMessage msg)
        {
			Log.Debug("Received message.");
			if (msg.Content == "!hi")
			{
				await msg.Channel.SendMessageAsync("Hello!");
				Log.Debug("Answered Hello.");
			}
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
			//CheckDiscordChannels();
			Log.DebugFormat("RunDiscordChannelsJob finished");
		}

        #endregion
    }
}