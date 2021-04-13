using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwitterTrends.Entities;

namespace TwitterTrends.Services
{
    public static class StateService
    {
        private const string STATES_FILE_NAME = "states.json";

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
                stateFeelings[stateTweets.Key] = stateTweets.Value.Select(tw => {
                    if (tw.Feeling.HasValue) return tw.Feeling.Value;
                    return 0;
                }).Sum() / (stateTweets.Value.Count);
            }
            return stateFeelings;
        }

        public static Dictionary<string, Coordinates[][]> GetStatesCoordinates()
        {
            using var states = new StreamReader(new FileStream(STATES_FILE_NAME, FileMode.Open));
            string jsonStates = states.ReadToEnd();
            var namedCoordinates = new Dictionary<string, Coordinates[][]>();
            int nameStart = jsonStates.IndexOf('\"');
            while (nameStart != -1)
            {
                string stateName = jsonStates.Substring(nameStart + 1, 2);
                int coordsStart = jsonStates.IndexOf("[[[");
                int coordsEnd;
                // if state consists of few polygons
                if (jsonStates[coordsStart + 3] == '[')
                {
                    var polygon = new List<Coordinates[]>();
                    int polygonStart = coordsStart + 3;
                    int polygonEnd;
                    do
                    {
                        polygonEnd = jsonStates.IndexOf("]]]");
                        polygon.Add(ParsePolygon(jsonStates[polygonStart..polygonEnd]));
                        jsonStates = jsonStates[(polygonEnd + 3)..];
                        polygonStart = jsonStates.IndexOf("[[[") + 2;
                    } while (jsonStates[0] != ']');
                    coordsEnd = polygonEnd;
                    namedCoordinates.Add(stateName, polygon.ToArray());
                }
                else // if state consists of one polygon
                {
                    coordsEnd = jsonStates.IndexOf("]]]");
                    namedCoordinates.Add(stateName, new Coordinates[1][]
                        { ParsePolygon(jsonStates[(coordsStart + 2)..coordsEnd]) });
                    jsonStates = jsonStates[(coordsEnd + 3)..];
                }          
                nameStart = jsonStates.IndexOf('\"');
            }
            return namedCoordinates;
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

        private static Coordinates FindStateCenter(Coordinates[][] state)
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

        private static WeightedCenter FindPolygonCenter(Coordinates[] polygon)
        {
            var center = new WeightedCenter();
            var area = FindPolygonArea(polygon);
            if (area == 0)
            {
                polygon[0].Lon += 0.0000001;
                polygon[0].Lat += 0.0000001;
                area = FindPolygonArea(polygon);
            }
            for (int i = 0; i < polygon.Length - 1; i++)
            {
                center.Center.Lat += (polygon[i].Lat + polygon[i + 1].Lat) * 
                    (polygon[i].Lat * polygon[i + 1].Lon - polygon[i + 1].Lat * polygon[i].Lon);
                center.Center.Lon += (polygon[i].Lon + polygon[i + 1].Lon) *
                    (polygon[i].Lat * polygon[i + 1].Lon - polygon[i + 1].Lat * polygon[i].Lon);
            }
            center.Center.Lat += (polygon[^1].Lat + polygon[0].Lat) *
                    (polygon[^1].Lat * polygon[0].Lon - polygon[0].Lat * polygon[^1].Lon);
            center.Center.Lon += (polygon[^1].Lon + polygon[0].Lon) *
                (polygon[^1].Lat * polygon[0].Lon - polygon[0].Lat * polygon[^1].Lon);

            center.Center.Lat /= 6 * area;
            center.Center.Lon /= 6 * area;
            center.Area = area;
            return center;
        }

        private static double FindPolygonArea(Coordinates[] polygon)
        {
            double area = 0;
            for (int i = 0; i < polygon.Length - 1; i++)
            {
                area += polygon[i].Lat * polygon[i + 1].Lon - polygon[i + 1].Lat * polygon[i].Lon;
            }
            area += polygon[^1].Lat * polygon[0].Lon - polygon[0].Lat * polygon[^1].Lon;
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

        private static Coordinates[] ParsePolygon(string poly)
        {
            string[] splittedCoords = poly.Split("],");
            var parsedCoords = new List<Coordinates>();
            foreach (var coords in splittedCoords)
            {
                parsedCoords.Add(CoordinatesService.ParseCoordinates(coords + "]"));
            }
            return parsedCoords.ToArray();
        }
    }
}
