﻿using System.Collections.Generic;
using Deepcove_Trust_Website.Models;
using Newtonsoft.Json;

namespace Deepcove_Trust_Website.DiscoverDeepCove
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        [JsonProperty("TrueFalseAnswer")]
        public bool? TrueFalseAnswer { get; set; }

        public string Text { get; set; }

        // Navigation Properties
        public AudioMedia Audio { get; set; }

        public List<QuizAnswer> Answers { get; set; }
                
        [JsonProperty("correct_answer")]
        public QuizAnswer CorrectAnswer { get; set; }

        public ImageMedia Image { get; set; }

        public Quiz Quiz { get; set; }
        // End Navigation Properties
    }
}
