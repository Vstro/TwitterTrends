﻿using TwitterTrends.Entities;

namespace TwitterTrends.Entities
{
    public class Tweet
    {
        public string Content { get; set; }
        public Coordinates Coordinates { get; set; } = new Coordinates();
        public double? Feeling { get; set; } = null;
    }
}
