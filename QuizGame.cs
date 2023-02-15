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
    internal class QuizGame
    {
        private ITelegramBotClient _botClient;
        private Chat _chat;

        private int _step = 0;
        private int _correct;

        private readonly List<QuizItem> _questions = new List<QuizItem>() {
            new QuizItem()
            {
                Question = "2+2=",
                Answers = new List<string>{"1","2","3","4", "5" },
                CorrectAnswer = "D"
            },
            new QuizItem()
            {
                Question = "2+2=4?",
                Answers = new List<string> { "Да", "Нет", "Не знаю" },
                CorrectAnswer = "A"
            }
        };

        public QuizGame(ITelegramBotClient botClient, Chat chat)
        {
            this._botClient = botClient;
            this._chat = chat;
        }

        public long ChatId => _chat.Id;
        public bool IsFinished { get; set; }

        internal async Task StartAsync()
        {
            await _botClient.SendTextMessageAsync(_chat.Id, "начинаем игру");
            await NextQuestion();
        }

        private async Task NextQuestion()
        {
            await Task.Delay(300);
            await _botClient.SendTextMessageAsync(_chat.Id, $"Вопрос №{_step + 1}");
            ReplyKeyboardMarkup keyboard = new ReplyKeyboardMarkup(
                new[]{
                GetKeyboardButtons(_step)
            }
                )
            { 
            ResizeKeyboard = true,
            };

            await _botClient.SendTextMessageAsync(_chat.Id,
                CreatQuestion(_step),
                replyMarkup: keyboard
                );
        }

        private string CreatQuestion(int step)
        {
            var builder = new StringBuilder();
            builder.AppendLine(_questions[step].Question);
            char c = 'A';
            var answers = _questions[step].Answers;
            for (int i = 0; i < answers.Count; i++)
            {
                builder.AppendLine($"{(char)(c+i)}: {answers[i]}");
            }
            return builder.ToString();
        }

        private KeyboardButton[] GetKeyboardButtons(int step)
        {
            int l = _questions[step].Answers.Count;
            var кeyboardButtons = new KeyboardButton[l];

            char c = 'A';
            for (int i = 0; i < l; i++)
            {
                кeyboardButtons[i] = new KeyboardButton(((char)(c + i)).ToString());
            }

            return кeyboardButtons;
        }

        internal async Task OnAnswer(string text)
        {
            if (_questions[_step].CorrectAnswer ==text)
            {
                await _botClient.SendTextMessageAsync(_chat.Id, "Верно!");
                _correct++;
            }
            else
            {
                await _botClient.SendTextMessageAsync(_chat.Id, "Не верно");
            }

            _step++;
            if (_step<_questions.Count)
            {
                await NextQuestion();
            }
            else
            {
                await _botClient.SendTextMessageAsync(_chat.Id, $"Игра завершена. Результат: {_correct}/{_questions.Count}");
                IsFinished= true;   
            }
        }
    }
}