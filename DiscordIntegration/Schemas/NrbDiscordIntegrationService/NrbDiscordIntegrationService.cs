namespace Terrasoft.Configuration
{
    using global::Common.Logging;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Web;
    using System.Threading.Tasks;
    using Terrasoft.Core.Entities;
    using OmnichannelMessaging;
    using Terrasoft.Configuration.Omnichannel.Messaging;


    /// <summary>
    /// Web-service to 
    /// </summary>
    [ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class NrbDiscordIntegrationService : OmnichannelMessagingService
    {
        private ILog _log;
        protected ILog Log
        {
            get
            {
                return _log ?? (_log = LogManager.GetLogger("OmnichannelMessageHandler"));
            }
        }
        private DiscordSocketClient _client;
        private CommandService _commands;

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Ignore messages that are not from users
            if (!(messageParam is SocketUserMessage message))
                return;

            // Ignore messages sent by the bot itself
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            // Process the message
            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos))
            {
                var context = new SocketCommandContext(_client, message);
                var result = await _commands.ExecuteAsync(context, argPos, null);

                if (!result.IsSuccess)
                    await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        private Task LogAsync(LogMessage log)
        {
            _log.Info(log.ToString());
            return Task.CompletedTask;
        }
        
        private bool InsertDiscordChannel(string name, string token, string language)
        {
            /* Создание запроса к схеме Order, добавление в запрос всех колонок схемы. */
            var esqResult = new EntitySchemaQuery(UserConnection.EntitySchemaManager, "Channel");
            esqResult.AddAllSchemaColumns();
            /* Выполнение запроса к базе данных и получение объекта с заданным идентификатором. UId объекта можно получить из строки навигации браузера с открытой страницей записи раздела. */
            var entity = esqResult.GetEntity(UserConnection, new Guid("58be5223-715d-4b16-a5c4-e3d4ec0412d9"));
            /* Создание объекта строки данных схемы OrderStatus. */
            var statusSchema = UserConnection.EntitySchemaManager.GetInstanceByName("OrderStatus");
            var newStatus = statusSchema.CreateEntity(UserConnection);
            /* Получение из базы данных объекта c заданным названием. */
            newStatus.FetchFromDB("Name", "4. Completed");
            /* Присваивает колонке StatusId новое значение. */
            entity.SetColumnValue("StatusId", newStatus.GetTypedColumnValue<Guid>("Id"));
            /* Сохранение измененного объекта в базе данных. */
            var result = entity.Save();
            return result;
        }

        [OperationContract]
        [WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public int Sum()
        {
            return 5 + 6;
        }

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
        ResponseFormat = WebMessageFormat.Json)]
        public async Task RegisterBot()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            var messageManager = new MessageManager(UserConnection);

            string token = "";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;

            await Task.Delay(-1);
        }
    }
}