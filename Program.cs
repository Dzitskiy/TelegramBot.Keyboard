// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.CalendarKit;
//using Telegram.Bot.Extensions.Polling;

namespace TelegramBot.Lesson
{
    internal class Program
    {
        private const string _botKey = "8304123361:AAFiBvEwF5P0ZLZhlF9XkHWzuVMpOgLrWjU";

        private static List<QuizGame> _games = new List<QuizGame>();

        private static Schedule _schedule;


        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception.ToString();

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await BotOnMessageReceived(botClient, update.Message);
                        break;

                    case UpdateType.CallbackQuery:

                        await BotOnCallbackQueryReceived(botClient, update.CallbackQuery);
                        break;
                }
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            var data = callbackQuery.Data;
            var message = callbackQuery.Message;

            var parts = data.Split(':');
            if (parts[0] == "calendar")
            {
                var action = parts[1];
                var param = parts[2];

                switch (action)
                {
                    case "day":
                        var selectedDate = DateTime.Parse(param);
                        await botClient.AnswerCallbackQuery(callbackQuery.Id, $"⏰ Вы выбрали дату: {selectedDate.ToShortDateString()}");
                        //TODO: Сохранить выбранную дату и продолжить процесс планирования
                        break;
                    case "next":
                    case "prev":

                        //TODO: Вычислить следующий или предыдущий месяц и обновить календарь
                        var yearMonth = param.Split('-');
                        var year = int.Parse(yearMonth[0]);
                        var month = int.Parse(yearMonth[1]);

                        var newDate  = new DateTime(year, month, 1);
                        if (action == "next")
                        {
                            newDate = newDate.AddMonths(1);
                        }
                        else if (action == "prev")
                        {
                            newDate = newDate.AddMonths(-1);
                        }

                        var newMarkup = new CalendarBuilder().GenerateCalendarButtons(
                            newDate.Year,
                            newDate.Month,
                            Telegram.CalendarKit.Models.Enums.CalendarViewType.Default);

                        await botClient.EditMessageReplyMarkup(
                            chatId: message.Chat.Id,
                            messageId: message.MessageId,
                            replyMarkup: newMarkup
                        );
                        break;
                }
            }

            // Если не от календаря, то передаем обработку в Schedule
            if (_schedule == null)
                return;
            await _schedule.OnAnswer(callbackQuery);
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");

            if (message.Type == MessageType.Poll)
            {
                var t = message.Poll;

                Console.WriteLine($"Создан опрос с вопросом: {t.Question}");


                //await botClient.SendTextMessageAsync(chatId: message.Chat.Id,
                //    text: $"Переданы координаты: {message.Venue.Location.Latitude}:{message.Venue.Location.Longitude}");
            }

            if (message.Type == MessageType.Location)
            {
                await botClient.SendMessage(chatId: message.Chat.Id, 
                    text: $"Переданы координаты: {message.Location.Latitude}:{message.Location.Longitude}");
            }
     
            if (message.Type != MessageType.Text)
                return;

            var action = message.Text!.Split(' ')[0];
            switch (action)
            {
                case "/start":
                    await StartMessage(botClient, message);
                    break;

                case "/startgame":
                    await StartGame(botClient, message);
                    break;

                case "/schedule":
                    _schedule = new Schedule(botClient, message.Chat);
                    await _schedule.StartAsync();
                    break;

                case "/calendar":
                    var _calendar = new Calendar(botClient, message.Chat);
                    await _calendar.ShowCalendarAsync();
                    break;

                default:
                    var chatId = message.Chat.Id;
                    var  game = _games.Find(x => x.ChatId == chatId);
                    if (game != null && !game.IsFinished)
                    {
                        await game.OnAnswer(message.Text);
                        return;
                    }

                    await Echo(botClient, message);
                    break;
            }
        }

        private static async Task Echo(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendMessage(chatId: message.Chat.Id, text: $"{message.Text}");
        }

        private static async Task Main()
        {
            using var cts = new CancellationTokenSource();

            var bot = new TelegramBotClient(_botKey);

            var me = await bot.GetMe();
            Console.Title = me.Username ?? "My awesome Bot";
            Console.WriteLine($"My bot: {me.Username}");

            // Создаем список команд
            var commands = new List<BotCommand>
            {
                new BotCommand { Command = "start", Description = "Запустить бота" },
                new BotCommand { Command = "startgame", Description = "Запустить квиз" },
                new BotCommand { Command = "schedule", Description = "Запланировать событие" },
                new BotCommand { Command = "calendar", Description = "Вызвать календарь" }
            };
            bot.SetMyCommands(commands);

            bot.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                cancellationToken: cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");

            

            Console.ReadLine();

            cts.Cancel();
        }

        private static async Task StartMessage(ITelegramBotClient botClient, Message message)
        {

            var userName = $"{message.From.LastName} {message.From.FirstName}";
            await botClient.SendMessage(chatId: message.Chat.Id, text: $"Hello {userName}, ChatId: {message.Chat.Id}");
        }

        private static async Task StartGame(ITelegramBotClient botClient, Message message)
        {
            var currentGame = _games.Find(x => x.ChatId == message.Chat.Id);
            if (currentGame != null)
            {
                _games.Remove(currentGame);
            }

            var game = new QuizGame(botClient, message.Chat);
            await game.StartAsync();
            _games.Add(game);
        }
    }
}