using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TwitterTrends.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TwitterTrends.Services
{
    public static class StateService
    {
        private const string STATES_FILE_NAME = "states.json";
        public const double ALL_NEUTRAL_TWEETS = -100;

        public static Dictionary<string, double> CalculateAverageSentiments()
        {
            var statesTweets = GroupTweetsByState();
            var stateFeelings = new Dictionary<string, double>();
            foreach (var stateTweets in statesTweets)
            {
                if (!stateTweets.Value.Any(tw => tw.Feeling.HasValue))
                {
                    continue;
                }
                int zeroCounter = 0;
                stateFeelings[stateTweets.Key] = stateTweets.Value.Select(tw => {
                    if (tw.Feeling.HasValue) return tw.Feeling.Value;
                    zeroCounter++;
                    return 0;
                }).Sum() / (stateTweets.Value.Count - zeroCounter);
            }
            return stateFeelings;
        }

        public static Dictionary<string, double[][][][]> GetStatesCoordinates()
        {
            using var states = new StreamReader(new FileStream(STATES_FILE_NAME, FileMode.Open));
            JObject jObject = JObject.Parse(states.ReadToEnd());
            var coordinates = new Dictionary<string, double[][][][]>();
            foreach (var pair in jObject.ToObject<Dictionary<string, JArray>>())
            {
                try
                {
                    // if state consists of only one polygon
                    var value = new double[][][][] { pair.Value.ToObject<double[][][]>() };
                    coordinates.Add(pair.Key, value);
                }
                catch (JsonReaderException)
                {
                    // if state consists of multiple polygons, thus json array has additional dimension
                    coordinates.Add(pair.Key, pair.Value.ToObject<double[][][][]>());
                }
            }
            return coordinates;
        }

        public static Dictionary<string, Coordinates> GetStatesCenters()
        {
            var statesCenters = new Dictionary<string, Coordinates>();
            foreach (var state in GetStatesCoordinates())
            {
                statesCenters.Add(state.Key, FindStateCenter(state.Value));
            }
            return statesCenters;
        }

        public static Coordinates FindStateCenter(double[][][][] state)
        {
            var centers = new WeightedCenter[state.Length];
            for (int i = 0; i < state.Length; i++)
            {
                centers[i] = FindPolygonCenter(state[i]);
            }
            var stateCenter = new Coordinates();
            double areaSum = centers.Sum(wc => wc.Area);
            stateCenter.Lat = centers.Select(wc => wc.Center.Lat * wc.Area)
                .Sum() / areaSum;
            stateCenter.Lon = centers.Select(wc => wc.Center.Lon * wc.Area)
                .Sum() / areaSum;
            return stateCenter;
        }

        private static WeightedCenter FindPolygonCenter(double[][][] polygon)
        {
            var center = new WeightedCenter();
            var area = FindPolygonArea(polygon);
            if (area == 0)
            {
                polygon[0][0][0] += 0.0000001;
                polygon[0][0][1] += 0.0000001;
                area = FindPolygonArea(polygon);
            }
            for (int i = 0; i < polygon[0].Length - 1; i++)
            {
                center.Center.Lat += (polygon[0][i][1] + polygon[0][i + 1][1]) * 
                    (polygon[0][i][1] * polygon[0][i + 1][0] - polygon[0][i + 1][1] * polygon[0][i][0]);
                center.Center.Lon += (polygon[0][i][0] + polygon[0][i + 1][0]) *
                    (polygon[0][i][1] * polygon[0][i + 1][0] - polygon[0][i + 1][1] * polygon[0][i][0]);
            }          
            center.Center.Lat /= 6 * area;
            center.Center.Lon /= 6 * area;
            center.Area = area;
            return center;
        }

        private static double FindPolygonArea(double[][][] polygon)
        {
            double area = 0;
            for (int i = 0; i < polygon[0].Length - 1; i++)
            {
                area += polygon[0][i][1] * polygon[0][i + 1][0] - polygon[0][i + 1][1] * polygon[0][i][0];
            }
            return area / 2;
        }

        private static string FindTweetClosestState(Dictionary<string, Coordinates> stateCenters, Coordinates tweetCoordinates)
        {
            return stateCenters
                    .Select(sc => (sc.Key, sc.Value - tweetCoordinates))
                    .OrderBy(e => e.Item2).First().Key;
        }

        private static Dictionary<string, List<Tweet>> GroupTweetsByState()
        {
            var stateCenters = GetStatesCenters();
            var tweets = TweetService.GetTweets();
            var statesTweets = new Dictionary<string, List<Tweet>>();
            foreach (var state in stateCenters)
            {
                statesTweets[state.Key] = new List<Tweet>();
            }
            foreach (var tweet in tweets)
            {
                string closestState = FindTweetClosestState(stateCenters, tweet.Coordinates);
                statesTweets[closestState].Add(tweet);
            }
            return statesTweets;
        }
    }
}
