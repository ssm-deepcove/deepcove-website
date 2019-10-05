﻿using Deepcove_Trust_Website.Data;
using Deepcove_Trust_Website.Helpers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Deepcove_Trust_Website.Models
{
    public enum MediaCategory
    {
        Image, Audio, General
    }

    public class MediaType
    {
        public int Index { get; private set; }
        public string Value { get; private set; }
        public string Mime { get; set; }
        public MediaCategory Category { get; private set; }

        public static MediaType[] All => new[] { Jpg, Png, Wav, Mp3, Doc, Docx, Pdf, Xls, Xlsx, Jpeg, Mpeg };

        public static MediaType Jpg = new MediaType { Index = 0, Value = "Jpg", Mime = "image/jpg", Category = MediaCategory.Image };
        public static MediaType Png = new MediaType { Index = 1, Value = "Png", Mime = "image/png", Category = MediaCategory.Image };
        public static MediaType Wav = new MediaType { Index = 2, Value = "Wav", Mime = "audio/wav", Category = MediaCategory.Audio };
        public static MediaType Mp3 = new MediaType { Index = 3, Value = "Mp3", Mime = "audio/mp3", Category = MediaCategory.Audio }; // Chrome uses audio/mp3
        public static MediaType Doc = new MediaType { Index = 4, Value = "Doc", Mime = "application/msword", Category = MediaCategory.General };
        public static MediaType Docx = new MediaType { Index = 5, Value = "Docx", Mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", Category = MediaCategory.General };
        public static MediaType Pdf = new MediaType { Index = 6, Value = "Pdf", Mime = "application/pdf", Category = MediaCategory.General };
        public static MediaType Xls = new MediaType { Index = 7, Value = "Xls", Mime = "application/vnd.ms-excel", Category = MediaCategory.General };
        public static MediaType Xlsx = new MediaType { Index = 8, Value = "Xlsx", Mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Category = MediaCategory.General };
        public static MediaType Jpeg = new MediaType { Index = 9, Value = "Jpeg", Mime = "image/jpeg", Category = MediaCategory.Image };
        public static MediaType Mpeg = new MediaType { Index = 3, Value = "Mp3", Mime = "audio/mpeg", Category = MediaCategory.Audio }; // Non-chrome browsers use audio/mpeg

        public static explicit operator MediaType(int i) => All[i];

        /// <summary>
        /// Returns a list of the mime type strings for the selected category
        /// </summary>
        public static List<string> MimesForCategory(MediaCategory category) => All.Where(t => t.Category == category).Select(t => t.Mime).ToList();

        public static MediaType FromString(string s) => All.Where(w => w.Mime.EqualsIgnoreCase(s)).FirstOrDefault();
    }

    abstract public class BaseMedia : BaseEntity
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mediaType")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MediaType MediaType { get; set; }
        [JsonProperty("path")]
        public string FilePath { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
        [JsonProperty("showCopyright")]
        public bool ShowCopyright { get; set; }
        [JsonProperty("isPublic")]
        public bool IsPublic { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }

        [NotMapped]
        [JsonProperty("filename")]
        public string Filename { get => System.IO.Path.GetFileName(FilePath); }

        public List<string> Tags()
        {
            return new List<string>(); // Todo
            // ["Website", "Education", "Fact Files", "App Activities", "App Quiz"]
        }

        public MediaCategory GetCategory() => MediaType.Category;

        public Dictionary<String, HashSet<int>> GetUsages(WebsiteDataContext db)
        {
            Dictionary<String, HashSet<int>> usages = new Dictionary<string, HashSet<int>>();

            // Mobile App Checks ---------------------------------------------------

            // Activities
            HashSet<int> activities = db.Activities.Where(a => a.ImageId == Id).Select(a => a.Id).ToHashSet();
            activities.Concat(db.ActivityImages.Where(ai => ai.ImageId == Id).Select(ai => ai.ActivityId).ToHashSet());
            if(activities.Count > 0)
                usages.Add("Activities", activities);

            // Fact Files
            HashSet<int> factfiles = db.FactFileEntries.Where(e => e.MainImageId == Id || e.PronounceAudioId == Id || e.ListenAudioId == Id).Select(e => e.Id).ToHashSet();
            factfiles.Concat(db.FactFileEntryImages.Where(ei => ei.MediaFileId == Id).Select(ei => ei.FactFileEntryId).ToHashSet());
            factfiles.Concat(db.FactFileNuggets.Where(n => n.ImageId == Id).Select(n => n.FactFileEntryId)).ToHashSet();
            if (factfiles.Count > 0)
                usages.Add("FactFileEntries", factfiles);

            // Quizzes
            HashSet<int> quizzes = db.Quizzes.Where(q => q.ImageId == Id).Select(q => q.Id).ToHashSet();
            quizzes.Concat(db.QuizQuestions.Where(qq => qq.ImageId == Id || qq.AudioId == Id).Select(qq => qq.QuizId).ToHashSet());
            quizzes.Concat(db.QuizAnswers.Where(qa => qa.ImageId == Id).Select(qa => qa.QuizQuestion.QuizId).ToHashSet());
            if (quizzes.Count > 0)
                usages.Add("Quizzes", quizzes);

            // End Mobile App Checks -----------------------------------------------

            // Media components
            // Todo : Bit trickier with revisions...

            // Button links
            // Todo: Also a bit tricky since these aren't linked in the database, only via HREF properties

            return usages;
        }
    }
}