namespace Terrasoft.Configuration.Omnichannel.Messaging
{
    using global::Common.Logging;
    using Common;
    using Core;
    using Discord.WebSocket;
    using OmnichannelProviders;
    using OmnichannelProviders.Interfaces;
    using OmnichannelProviders.MessageWorkers;
    using Terrasoft.Core.Factories;
    using Web.Common;
    using System.Collections.Generic;
    using Terrasoft.Core.Configuration;
    using Terrasoft.Core.Scheduler;

    #region Class : DiscordAppEventListener

    /// <summary>
    /// Class, that run all what OmnichannelMessaging need on app start.
    /// </summary>
    public class DiscordAppEventListener : AppEventListenerBase
	{
		#region Fields : Private

		private const int RunAndCheckDiscordPeriod = 5;
		private const string JobGroupName = "OmnichannelMessagingGroup";
		private UserConnection _userConnection;
		private List<string> _channelsCache;
		private DiscordSocketClient _client;
		private ILog _log;

        #endregion

        #region Fields : Protected
        protected ILog Log
		{
			get
			{
				return _log ?? (_log = LogManager.GetLogger("OmnichannelMessageHandler"));
			}
		}
		protected UserConnection UserConnection	{
			get;
			private set;
		}

		protected static IIncomingMessageNotifier Notifier;

		#endregion

		#region Methods : Protected

		/// <summary>
		/// Create notifiers of Creatio.
		/// </summary>
		protected void CreateNotifiers()
		{
			Notifier = ClassFactory.Get<IIncomingMessageNotifier>();
			Notifier.AddListener(new SendToUserIncomingMessageListener());
		}

		/// <summary>
		/// Setup Quartz.NET jobs.
		/// </summary>
		/// <typeparam name="TJob"></typeparam>
		/// <param name="periodInMinutes"></param>
		protected virtual void ScheduleJob<TJob>(int periodInMinutes)
			where TJob : IJobExecutor
		{
			SysUserInfo currentUser = UserConnection.CurrentUser;
			string className = typeof(TJob).AssemblyQualifiedName;
			if (!AppScheduler.DoesJobExist(className, JobGroupName))
			{
				AppScheduler.ScheduleMinutelyJob<TJob>(JobGroupName, UserConnection.Workspace.Name,
					currentUser.Name, periodInMinutes, null, true);
			}
		}

		/// <summary>
		/// Schedule Discord jobs via Quartz.NET jobs.
		/// </summary>
		protected virtual void SetupDiscordMessagingJobs()
		{
			ScheduleJob<RunDiscordChannelsJob>(RunAndCheckDiscordPeriod);
		}

		/// <summary>
		/// Gets user connection from application event context.
		/// </summary>
		/// <param name="context">Application event context.</param>
		/// <returns>User connection.</returns>
		protected UserConnection GetUserConnection(AppEventContext context)	
		{
			var appConnection = context.Application["AppConnection"] as AppConnection;
			if (appConnection == null) {
				throw new ArgumentNullOrEmptyException("AppConnection");
			}
			return appConnection.SystemUserConnection;
		}

		/// <summary>
		/// Bind Discord interfaces.
		/// </summary>
		protected void BindInterfaces() {
			ClassFactory.Bind<IAttachmentsLoadWorker, DiscordAttachmentLoadWorker>("Discord");
			ClassFactory.Bind<IProfileDataProvider, DiscordProfileDataProvider>("Discord");
			ClassFactory.Bind<IOutcomeMessageWorker, DiscordOutcomeMessageWorker>("Discord");
		}

		#endregion

		#region Methods : Public

		/// <summary>
		/// Handles application start.
		/// </summary>
		/// <param name="context">Application event context.</param>
		public override void OnAppStart(AppEventContext context) 
		{
			base.OnAppStart(context);
			UserConnection = GetUserConnection(context);
			new RunDiscordChannelsJob().Execute(UserConnection, null);
			SetupDiscordMessagingJobs();
			CreateNotifiers();
			BindInterfaces();
		}

		#endregion
	}
	#endregion
}