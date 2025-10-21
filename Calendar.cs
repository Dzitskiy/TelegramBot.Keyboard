// See https://aka.ms/new-console-template for more information
using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.CalendarKit;

namespace TelegramBot.Lesson
{
    public class Calendar
    {
        private ITelegramBotClient _botClient;
        private Chat _chat;

        public Calendar(ITelegramBotClient botClient, Chat chat)
        {
            _botClient = botClient;
            _chat = chat;
        }

        public async Task ShowCalendarAsync() {

            await _botClient.SendMessage(_chat.Id,
              $"Выбор даты в календаре");

            var calendarBuilder = new CalendarBuilder();

            // Генерация календаря на октябрь 2025 года
            var currentDate = DateTime.UtcNow;
            var calendarMaerkup = calendarBuilder
                .GenerateCalendarButtons(currentDate.Year, currentDate.Month, Telegram.CalendarKit.Models.Enums.CalendarViewType.Default);

            // Отправка календаря в чат
            await _botClient.SendMessage(_chat.Id,
                    $"Пожалуйста, выберите дату:",
                    replyMarkup: calendarMaerkup);
        }
    }
}