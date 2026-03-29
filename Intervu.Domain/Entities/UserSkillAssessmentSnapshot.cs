using Intervu.Domain.Abstractions.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Intervu.Domain.Entities
{
    public class UserSkillAssessmentSnapshot : EntityBase<Guid>
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        public Guid UserId { get; set; }

        public string TargetJson { get; set; } = "{}";
        public string CurrentJson { get; set; } = "{}";
        public string GapJson { get; set; } = "{}";

        [NotMapped]
        public Target? Target
        {
            get => Deserialize<Target>(TargetJson);
            set => TargetJson = Serialize(value);
        }

        [NotMapped]
        public Current? Current
        {
            get => Deserialize<Current>(CurrentJson);
            set => CurrentJson = Serialize(value);
        }

        [NotMapped]
        public Gap? Gap
        {
            get => Deserialize<Gap>(GapJson);
            set => GapJson = Serialize(value);
        }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }

        public void EnsureJsonPayloads()
        {
            TargetJson = Normalize(TargetJson);
            CurrentJson = Normalize(CurrentJson);
            GapJson = Normalize(GapJson);
        }

        private static T? Deserialize<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        private static string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, JsonOptions);
        }

        private static string Normalize(string? json)
        {
            return string.IsNullOrWhiteSpace(json) ? "{}" : json;
        }
    }

    public class Target
    {
        public List<string> Roles { get; set; } = new();
        public string Level { get; set; } = string.Empty;
        public List<string> SkillsTarget { get; set; } = new();
    }

    public class SkillLevel
    {
        public string Skill { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public int SfiaLevel { get; set; }
    }

    public class Current
    {
        public List<SkillLevel> Skills { get; set; } = new();
    }

    public class Gap
    {
        public List<string> Missing { get; set; } = new();
        public List<string> Weak { get; set; } = new();
    }
}
