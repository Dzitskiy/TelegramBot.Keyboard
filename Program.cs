// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramBot.Lesson
{
    internal class Program
    {
        private const string _botKey = "6860575489:AAFlSPLCJouQPA_4rWRjpEHbP2hvmFmqRsM";

        private static List<QuizGame> _games = new List<QuizGame>();

        private static Schedule _schedule;

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception.ToString();

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static void HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                         BotOnMessageReceived(botClient, update.Message);
                        break;

                    case UpdateType.CallbackQuery:
                        if (_schedule == null)
                            return;
                         _schedule.OnAnswer(update.CallbackQuery);
                        break;
                }
            }
            catch (Exception exception)
            {
                 HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static void BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");

            if (message.Type != MessageType.Text)
                return;

            var action = message.Text!.Split(' ')[0];
            switch (action)
            {
                case "/start":
                     StartMessage(botClient, message);
                    break;

                case "/startgame":
                     StartGame(botClient, message);
                    break;

                case "/schedule":
                    _schedule = new Schedule(botClient, message.Chat);
                     _schedule.StartAsync();
                    break;

                default:
                    var chatId = message.Chat.Id;
                    var  game = _games.Find(x => x.ChatId == chatId);
                    if (game != null && !game.IsFinished)
                    {
                         game.OnAnswer(message.Text);
                        return;
                    }

                     Echo(botClient, message);
                    break;
            }
        }

        private static void Echo(ITelegramBotClient botClient, Message message)
        {
             botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"{message.Text}");
        }

        private static void Main()
        {
            using var cts = new CancellationTokenSource();

            var bot = new TelegramBotClient(_botKey);

            var me =  bot.GetMeAsync();
            Console.Title = me.Username ?? "My awesome Bot";
            Console.WriteLine($"My bot: {me.Username}");

            bot.StartReceiving(updateHandler: HandleUpdateAsync,
                   errorHandler: HandleErrorAsync,
                   receiverOptions: new ReceiverOptions()
                   {
                       AllowedUpdates = Array.Empty<UpdateType>()
                   },
                   cancellationToken: cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");

            Console.ReadLine();

            cts.Cancel();
        }

        private static void StartMessage(ITelegramBotClient botClient, Message message)
        {
            var userName = $"{message.From.LastName} {message.From.FirstName}";
             botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Hello {userName}");
        }

        private static void StartGame(ITelegramBotClient botClient, Message message)
        {
            var currentGame = _games.Find(x => x.ChatId == message.Chat.Id);
            if (currentGame != null)
            {
                _games.Remove(currentGame);
            }

            var game = new QuizGame(botClient, message.Chat);
             game.StartAsync();
            _games.Add(game);
        }
    }
}