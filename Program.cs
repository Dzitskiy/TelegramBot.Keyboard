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
        //private static List<Photo> _photos = new List<Photo>();
        //private static string _fileName = "photos.json";

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

            if (message.Type == MessageType.Photo)
            {
                await StorePhoto(botClient, message);
                return;
            }

            if (message.Type != MessageType.Text)
                return;

            var action = message.Text!.Split(' ')[0];
            switch (action)
            {
                case "/start":
                    await StartMessage(botClient, message);
                    break;

                case "найди":
                case "Найди":
                    await FindPhoto(botClient, message);
                    break;

                default:
                    await Echo(botClient, message);
                    break;
            }
        }

        private static async Task FindPhoto(ITelegramBotClient botClient, Message message)
        {
            var caption = message.Text.Replace("найди ", "").Replace("Найди ", "").Trim();
            //var photo = _photos.Find(x => x.Caption == caption);
            //if (photo == null)
            //{
            //    await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"не удалось найти {caption}");
            //    return;
            //}

            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);
            //await botClient.SendPhotoAsync(chatId: message.Chat.Id, photo: new InputOnlineFile(photo.FileId));
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

            //if (System.IO.File.Exists(_fileName))
            //{
            //    var json = System.IO.File.ReadAllText(_fileName);
            //    _photos = JsonSerializer.Deserialize<List<Photo>>(json);
            //}
            //else System.IO.File.Create(_fileName);

            Console.ReadLine();

            cts.Cancel();
        }

        private static async Task StartMessage(ITelegramBotClient botClient, Message message)
        {
            var userName = $"{message.From.LastName} {message.From.FirstName}";
            await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"Hello {userName}");
        }

        private static async Task StorePhoto(ITelegramBotClient botClient, Message message)
        {
            if (String.IsNullOrEmpty(message.Caption))
            {
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"не указан заголовок картинки");
                return;
            }
            if (message.Photo.Length == 0)
            {
                await botClient.SendTextMessageAsync(chatId: message.Chat.Id, text: $"ошибка получения картинки");
                return;
            }

            //_photos.Add(new Photo() { Caption = message.Caption, FileId = message.Photo[0].FileId });
            //string jsonString = JsonSerializer.Serialize(_photos);
            //System.IO.File.WriteAllText(_fileName, jsonString);
        }
    }
}