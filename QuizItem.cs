// See https://aka.ms/new-console-template for more information
using System.Collections.Generic;

namespace TelegramBot.Lesson
{
    internal class QuizItem
    {
        public string Question { get; set; }
        public List<string> Answers { get; set; }
        public string CorrectAnswer { get; set; }
    }
}