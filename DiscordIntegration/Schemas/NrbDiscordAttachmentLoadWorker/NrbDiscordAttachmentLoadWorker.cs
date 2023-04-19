namespace Terrasoft.Configuration.Omnichannel.Messaging
{
	using OmnichannelProviders;
	using OmnichannelProviders.Domain.Entities;
	using OmnichannelProviders.MessageConverters;
	using Terrasoft.Core;

	/// <summary>
	/// Class that load attachments from Discord provider.
	/// </summary>
	public class DiscordAttachmentLoadWorker : IAttachmentsLoadWorker
	{

		#region Fields: Protected 

		protected UserConnection UserConnection;
		protected AttachmentsDownloader AttachmentsDownloader;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// Initializes a new instance of the <see cref="DiscordAttachmentLoadWorker "/> class.
		/// <param name="userConnection">Instance of the <see cref="UserConnection"/>.</param>
		/// </summary>
		public DiscordAttachmentLoadWorker(UserConnection userConnection) {
			UserConnection = userConnection;
			AttachmentsDownloader = new AttachmentsDownloader(userConnection);
		}

		#endregion

		#region Methods: Public	

		/// <summary>
		/// Load attachments.
		/// </summary>
		/// <param name="incomeAttachment">Attachment from messenger.</param>
		/// <param name="message">Source message.</param>
		public void Load(MessageAttachment incomeAttachment, UnifiedMessage message) {
			incomeAttachment.FileName = FileUtilities.GetFileNameFromUrl(incomeAttachment.UploadUrl);
			incomeAttachment.FileId = AttachmentsDownloader.Load(incomeAttachment);
		}

		#endregion
	}
}