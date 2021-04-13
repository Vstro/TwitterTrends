using System;
using System.Collections.Generic;
using System.IO;

namespace TwitterTrends.Services
{
    public static class TweetService
    {
        private const string tweetsFileName = "weekend_tweets2014.txt";
        private const string sentimentsFileName = "sentiments.csv";

        public static Dictionary<string, double> GetSentiments()
        {
            using var sentimentsFile = new StreamReader(new FileStream(sentimentsFileName, FileMode.Open));
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
            using var tweetsFile = new StreamReader(new FileStream(tweetsFileName, FileMode.Open));
            var tweets = new List<Tweet>();
            while (!tweetsFile.EndOfStream)
            {
                tweets.Add(new Tweet(tweetsFile.ReadLine()));
            }
            return tweets;
        }
    }
}
