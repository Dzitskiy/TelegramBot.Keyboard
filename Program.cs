// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace TelegramBot.Lesson
{
    internal class Program
    {
        private const string _botKey = "5997563686:AAEjaMzA3Ni_jL2UZGJ9x6BQeLrHeN9pjKQ";

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
                        await BotOnMessageReceived(botClient, update.Message!);
                        break;
                }
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            Console.WriteLine($"Receive message type: {message.Type}");

            if (message.Type != MessageType.Text)
                return;

            var action = message.Text!.Split(' ')[0];
            switch (action)
            {
                case "/start":
                    await StartMessage(botClient, message);
                    break;

                default:
                    await Echo(botClient, message);
                    break;
            }
        }


        private static async Task Echo(ITelegramBotClient botClient, Message message)
        {
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"{message.Text}");
        }

        private static async Task Main()
        {
            using var cts = new CancellationTokenSource();

            var bot = new TelegramBotClient(_botKey);

            var me = await bot.GetMeAsync();
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

        private static async Task StartMessage(ITelegramBotClient botClient, Message message)
        {
            var userName = $"{message.From.LastName} {message.From.FirstName}";
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Hello {userName}");
        }
    }
}