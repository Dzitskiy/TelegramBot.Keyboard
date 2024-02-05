// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Lesson
{
    internal class Schedule
    {
        private ITelegramBotClient _botClient;
        private Chat _chat;

        public Schedule(ITelegramBotClient botClient, Chat chat)
        {
            this._botClient = botClient;
            this._chat = chat;
        }

        public long ChatId => _chat.Id;
        public bool IsFinished { get; set; }

        public void StartAsync()
        {
            InlineKeyboardMarkup inlineKeyboard = new(
                new[] {
                    new[]
                    {
                        InlineKeyboardButton.WithUrl("Перейти на сайт", "otus.ru")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Выбрать день", "step1")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Отменить", "stop")
                    }
                }
                );

             _botClient.SendTextMessageAsync(_chat.Id,
                    $"Inline кнопки",
                    replyMarkup: inlineKeyboard);
        }

        internal void OnAnswer(CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Data)
            {
                case "step1":
                    InlineKeyboardMarkup inlineKeyboard = new(
                        new[] {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Завтра", "step2_day1"),
                                InlineKeyboardButton.WithCallbackData("Послезавтра", "step2_day2")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Отменить", "stop")
                            }
                        }
                    );

                     _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "");
                     _botClient.SendTextMessageAsync(_chat.Id,
                            $"Выберите день",
                            replyMarkup: inlineKeyboard);
                    break;

                case "step2_day1":
                case "step2_day2":
                    InlineKeyboardMarkup keysStep2 = new(
                        new[] {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Утро", "complete"),
                                InlineKeyboardButton.WithCallbackData("День", "complete"),
                                InlineKeyboardButton.WithCallbackData("Вечер", "complete")
                            },
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Отменить", "stop")
                            }
                        }
                    );

                     _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "");
                     _botClient.EditMessageTextAsync(chatId: _chat.Id,
                            messageId: callbackQuery.Message.MessageId,
                            $"Когда вам удобно",
                            replyMarkup: keysStep2);
                    break;

                case "complete":
                    IsFinished = true;
                     _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Отлично! До встречи!");
                     _botClient.EditMessageTextAsync(chatId: _chat.Id,
                            messageId: callbackQuery.Message.MessageId,
                            $"Выбор сделан");
                    break;

                case "stop":
                    IsFinished = true;
                     _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Вы прервали выбор. Ждем вас снова", true);
                    break;
            }
        }
    }
}