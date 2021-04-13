using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TwitterTrends.Services
{
    public static class TweetService
    {
        private static Dictionary<string, double> Sentiments { get; set; } = GetSentiments();

        private const string TWEETS_FILE_NAME = "shopping_tweets2014.txt";
        private const string SENTIMENTS_FILE_NAME = "sentiments.csv";
        private const int SYMBOLS_AMONG_COORDS_AND_CONTENT = 23;
        private const int TWITTER_REF_LENGTH = 22;

        public static Dictionary<string, double> GetSentiments()
        {
            using var sentimentsFile = new StreamReader(new FileStream(SENTIMENTS_FILE_NAME, FileMode.Open));
            var sentiments = new Dictionary<string, double>();
            while (!sentimentsFile.EndOfStream)
            {
                string[] sentiment = sentimentsFile.ReadLine().Split(',');
                sentiments.Add(sentiment[0], double.Parse(sentiment[1].Replace('.', ',')));
            }
            return sentiments;
        }

        public static List<Tweet> GetTweets()
        {
            using var tweetsFile = new StreamReader(new FileStream(TWEETS_FILE_NAME, FileMode.Open));
            var tweets = new List<Tweet>();
            while (!tweetsFile.EndOfStream)
            {
                tweets.Add(ParseTweet(tweetsFile.ReadLine()));
            }
            return tweets;
        }

        public static Tweet ParseTweet(string tweetText)
        {
            int coordSeparator = tweetText.IndexOf(',');
            int coordEnd = tweetText.IndexOf(']');
            Tweet tweet = new Tweet();
            tweet.Coordinates.Lat = double.Parse(tweetText[1..coordSeparator].Replace('.', ','));
            tweet.Coordinates.Lon = double.Parse(tweetText[(coordSeparator + 2)..coordEnd].Replace('.', ','));
            tweet.Content = tweetText[(coordEnd + SYMBOLS_AMONG_COORDS_AND_CONTENT + 1)..];
            tweet.Feeling = CalculateFeeling(tweet.Content);
            return tweet;
        }

        private static double? CalculateFeeling(string content)
        {
            List<string> words = ParseContent(content);
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
                return null;
            }
            return words.Select(s => double.Parse(s)).Sum() / words.Count;
        }

        private static List<string> ParseContent(string content)
        {
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

        private static bool IsLetter(char ch)
        {
            if (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || ('0' <= ch && ch <= '9') || ch == '_' || ch == '-' || ch == '\'')
                return true;
            return false;
        }   
    }
}
