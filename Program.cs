using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace HtmlParser
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            int index = 0;
            var cultureInfo = new CultureInfo("ru-RU");
            Console.WriteLine("Начинаю парсинг...");
            string dirName = "D:\\Ляпцев И.А\\Учеба\\Магистерская\\messages";
            //Проходимся по всем папкам
            // если папка существует
            if (Directory.Exists(dirName))
            {
                var dirs = Directory.GetDirectories(dirName).ToList();
                foreach (string s in dirs)
                {
                    //Если первый символ - скип
                    if (s.Contains("-"))
                        continue;

                    //Для каждой папки- новый txt
                    using (StreamWriter file = new StreamWriter("D:\\Ляпцев И.А\\Учеба\\Магистерская\\Result\\" + index + ".txt"))
                    {
                        //Проходимся по каждому html-документу, начиная с наибольшего числа
                        var files = Directory.GetFiles(s).ToList();
                        List<Tuple<string, string, DateTime>> data =
                            new List<Tuple<string, string, DateTime>>();
                        foreach (var m in files)
                        {
                            //message__header- имя, если Вы- меняю на Илья Ляпцев
                            //После </a>- дата и время, преобразуем в нужный формат
                            //kludges- сообщение
                            HtmlDocument doc = new HtmlDocument();
                            doc.Load(m, Encoding.GetEncoding(1251));

                            //Selecting all the nodes with tagname `span` having "id=ctl00_ContentBody_CacheName".
                            var nodes = doc.DocumentNode.SelectNodes("//div")
                                .Where(d => d.Attributes.Contains("class"))
                                .Where(d => d.Attributes["class"].Value == "item").ToList();

                            foreach (HtmlNode node in nodes)
                            {
                                var text = node.InnerText;

                                var name = text.Substring(0, text.IndexOf(@", ", StringComparison.Ordinal));
                                text = text.Remove(0, name.Length + 2);
                                name = name.Remove(0, 6);
                                if (name.Contains("Вы"))
                                    name = "Илья Ляпцев";

                                var date = text.Substring(0, text.IndexOf("\n  ", StringComparison.Ordinal));

                                text = text.Remove(0, date.Length + 2);
                                text = text.Substring(0, text.Length - 8);

                                date = date.Replace(" в ", ", ");

                                date = date.Replace(" (ред.)", "");

                                var datetime = DateTime.Parse(date, cultureInfo);

                                data.Add(new Tuple<string, string, DateTime>(name, text, datetime));

                                Console.WriteLine($"Сообщение от {name} в {datetime}: {text}\n");
                            }
                        }

                        data = data.OrderBy(o => o.Item3).ToList();

                        foreach (var message in data)
                        {
                            file.WriteLine(
                                $"[{message.Item3.ToString("MM/dd/yyyy, HH:mm::ss")}] {message.Item1}: {message.Item2}");
                        }
                    }
                    index++;
                }
            }
        }
    }
}