using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TwitterTrends.Entities;
using TwitterTrends.Services;

namespace TwitterTrends
{
    public class Tweet
    {
        public string Content { get; set; }
        public Coordinates Coordinates { get; set; } = new Coordinates();
        public double? Feeling { get; set; } = null;

        private static Dictionary<string, double> Sentiments { get; set; } = 
            TweetService.GetSentiments();

        private const int SYMBOLS_AMONG_COORDS_AND_CONTENT = 23;
        private const int TWITTER_REF_LENGTH = 22;


        public Tweet() { }
        public Tweet(string tweet)
        {
            int coordSeparator = tweet.IndexOf(',');
            int coordEnd = tweet.IndexOf(']');
            Coordinates.Lat = double.Parse(tweet[1..coordSeparator].Replace('.', ','));
            Coordinates.Lon = double.Parse(tweet[(coordSeparator + 2)..coordEnd].Replace('.', ','));
            Content = tweet[(coordEnd + SYMBOLS_AMONG_COORDS_AND_CONTENT + 1)..];
            CalculateFeeling();
        }

        public List<string> ParseContent()
        {
            var content = Content;
            int i = content.IndexOf("http://t.co/");
            while (i >= 0)
            {
                content = content.Remove(i, TWITTER_REF_LENGTH);
                i = content.IndexOf("http://t.co/");
            }

            i = content.IndexOf('@');
            while (i >= 0)
            {
                int j = 1;
                while (i + j < content.Length && IsLetter(content[i + j])) j++;
                int nickLength = j;

                content = content.Remove(i, nickLength);
                i = content.IndexOf("@");
            }

            int wordStart = -1;
            var parsedContent = new List<string>();
            for (i = 0; i < content.Length; i++)
            {
                if (IsLetter(content[i]) && wordStart == -1)
                {
                    wordStart = i;                   
                }
                else if (!IsLetter(content[i]) && wordStart != -1)
                {
                    parsedContent.Add(content[wordStart..i]);
                    wordStart = -1;
                }
            }
            if (wordStart != -1)
            {
                parsedContent.Add(content[wordStart..i]);
                wordStart = -1;
            }
            return parsedContent;
        }

        public static bool IsLetter(char ch)
        {
            if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9') || ch == '_' || ch == '-' || ch == '\'')
                return true;
            return false;
        }

        public void CalculateFeeling()
        {
            List<string> words = ParseContent();
            for (int i = words.Count; i > 0; i--)
            {
                for (int j = 0; j < words.Count - i + 1; j++)
                {
                    string checkWord = words.GetRange(j, i)
                        .Aggregate((s1, s2) => s1 + " " + s2).ToLower();
                    if (Sentiments.ContainsKey(checkWord))
                    {
                        words[j] = $"*{Sentiments[checkWord]}";
                        words.RemoveRange(j + 1, i - 1);
                    }
                }
            }

            bool isNone = true;
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i][0] == '*')
                {
                    isNone = false;
                    words[i] = words[i][1..];
                }
                else
                {
                    words[i] = "0";
                }
            }

            if (isNone)
            {
                Feeling = null;
                return;
            }
            Feeling = words.Select(s => double.Parse(s)).Sum() / words.Count;
        }
    }
}
