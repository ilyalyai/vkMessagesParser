using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace htmlparser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var index = 0;
            var chatCounter = 0;
            var messageCounter = 0;
            var cultureInfo = new CultureInfo("ru-RU");
            Console.WriteLine("Начинаю парсинг...");
            var dirName = "E:\\Документы и прочее\\Политех\\5\\5-2\\Магистерская\\messages";
            //Проходимся по всем папкам
            // если папка существует
            if (Directory.Exists(dirName))
            {
                var dirs = Directory.GetDirectories(dirName).ToList();
                int convCounter = 0;
                foreach (var s in dirs)
                {
                    //Проходимся по каждому html-документу, начиная с наибольшего числа
                    var files = Directory.GetFiles(s).ToList();
                    List<Tuple<string, string, DateTime>> data =
                        new();
                    var chatName = "";
                    var isLocalchat = false;
                    foreach (var m in files)
                    {
                        //message__header- имя, если Вы- меняю на Илья Ляпцев
                        //После </a>- дата и время, преобразуем в нужный формат
                        //kludges- сообщение
                        HtmlDocument doc = new();
                        doc.Load(m, Encoding.GetEncoding(1251));

                        var nodes = doc.DocumentNode.SelectNodes("//div")
                            .Where(d => d.Attributes.Contains("class"))
                            .Where(d => d.Attributes["class"].Value == "item").ToList();

                        foreach (var node in nodes)
                        {
                            var text = node.InnerText;
                            var name = text.Substring(0, text.IndexOf(@", ", StringComparison.Ordinal));
                            text = text.Remove(0, name.Length + 2);
                            name = name.Remove(0, 6);
                            if (name.Contains("Вы"))
                            {
                                name = "Илья Ляпцев";
                            }
                            else
                            {
                                if (chatName == "")
                                {
                                    chatName = name;
                                }
                                else if (!chatName.Equals(name) && !isLocalchat && !chatName.Contains("Беседа"))
                                {
                                    isLocalchat = true;
                                    chatName = "Беседа" + convCounter;
                                    convCounter++;
                                }
                            }

                            var date = text.Substring(0, text.IndexOf("\n  ", StringComparison.Ordinal));

                            text = text.Remove(0, date.Length + 3);
                            text = text[..^8];

                            text = text.Replace("\n", ". ");
                            text = text.Replace("Запись на стене", ". ");
                            text = text.Replace("Подарок", ". ");
                            text = text.Replace("прикреплённое сообщение.", "");
                            text = text.Replace("Стикер.", "");
                            

                            if (text.Contains("https:") || text.Contains("http://"))
                                continue;

                            date = date.Replace(" в ", ", ");

                            date = date.Replace(" (ред.)", "");

                            Dictionary<string, string> months = new Dictionary<string, string>()
                            {
                                { "янв", "01"},
                                { "фев", "02"},
                                { "мар", "03"},
                                { "апр", "04"},
                                { "мая", "05"},
                                { "июн", "06"},
                                { "июл", "07"},
                                { "авг", "08"},
                                { "сен", "09"},
                                { "окт", "10"},
                                { "ноя", "11"},
                                { "дек", "12"},
                            };
                            foreach (var month in months)
                            {
                                if (date.Contains(month.Key))
                                {
                                    date = date.Replace(month.Key, month.Value);
                                }
                            }

                            var datetime = DateTime.Parse(date, cultureInfo);
                            //var datetime = DateTime.ParseExact(date, "dd MM yyyy, HH:mm:ss", cultureInfo);
                            data.Add(new Tuple<string, string, DateTime>(name, text, datetime));
                        }
                    }

                    data = data.OrderBy(o => o.Item3).ToList();

                    //Для каждой папки- новый txt
                    using (StreamWriter file = new("E:\\Документы и прочее\\Политех\\5\\5-2\\Магистерская\\Result\\" +
                                                   chatName + ".txt"))
                    {
                        foreach (var message in data)
                        {
                            messageCounter++;
                            file.WriteLine(
                                $"[{message.Item3:dd.MM.yy, HH:mm:ss}] {message.Item1}: {message.Item2}");
                        }

                        chatCounter++;
                    }

                    index++;
                }
            }

            Console.WriteLine($"Всего запарсено {chatCounter} бесед и {messageCounter} сообщений!");
        }
    }
}
