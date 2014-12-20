using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimilarityTextChecker
{
    class Program
    {

        static void Main(string[] args)
        {
            string text1 = GetText("1.txt");
            string text2 = GetText("2.txt");
            int length = 3;
             
            var nGramma1 = GetNGrammas(text1, length);
            var nGramma2 = GetNGrammas(text2, length);

            Console.Write(String.Concat(nGramma1.Count(), "\r\n"));
            Console.Write(String.Concat(nGramma2.Count(), "\r\n"));
            Console.Write(CalcJk(nGramma1,  nGramma2));
            Console.ReadKey();
        }

        static private string GetText(string path)
        {
            var reader = new StreamReader(path, System.Text.Encoding.UTF8);

            return reader.ReadToEnd().ToLower();
        }

        static private IEnumerable<string> ToGrammasFormat(string text)
        {
            var grammInput = text
                .Split(new char[] {'.', '!', '?', '(', ')', '[', ']', '{', '}'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => Regex.Replace(x, @"\P{L}+", " ", RegexOptions.Compiled).Trim());    
                                       
            return grammInput;
        }

        static private IEnumerable<Tuple<string, int>> GetNGrammas(string text, int length)
        {
            var grammInput = ToGrammasFormat(text);

            var nGramma = grammInput                                            //Создаем массив N-грамм
                .Where(line => line.Split(' ').Length >= length)                //Не обрабатываем заведомо маленькие предложения
                .SelectMany(sentence => sentence
                    .Split(' ')
                    .Select((word, index) =>
                        sentence
                            .Split(' ').Skip(index))
                            .Where(subline => subline.Count() >= length)        //Не обрабатываем заведомо маленькие под-предложения
                            .Select(subline => String.Join(" ",subline.Take(length)).Trim()))
                .GroupBy(gramma => gramma)
                .Select(group => Tuple.Create(group.Key, group.Count()));

            return nGramma;
        }

        static private double CalcJk(IEnumerable<Tuple<string, int>> grammas1, IEnumerable<Tuple<string, int>> grammas2)
        {
            var totalGrammas = grammas1.Select(gramma => Tuple.Create(gramma.Item1, gramma.Item2, 0))
                .Concat(grammas2.Select(gramma => Tuple.Create(gramma.Item1, 0, gramma.Item2)))
                .GroupBy(gramma => gramma.Item1)
                .Select(group => Tuple.Create(group.First().Item1, group.First().Item2, group.Last().Item3));

            double common = 0, total = 0;
            foreach (Tuple<string, int, int> tGramma in totalGrammas)
            {
                common += Math.Min(tGramma.Item2, tGramma.Item3);
                total += Math.Max(tGramma.Item2, tGramma.Item3);
            }

            return common/total;
        }

    }
}
