using Intervu.Application.DTOs.PreparedQuestion;
using Intervu.Application.Exceptions;
using Intervu.Domain.Entities.Constants.PreparedQuestionConstants;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;
using InterviewRoomEntity = Intervu.Domain.Entities.InterviewRoom;
using NewtonsoftJson = Newtonsoft.Json.JsonConvert;
using SystemTextJson = System.Text.Json.JsonSerializer;

namespace Intervu.Application.UseCases.PreparedQuestion
{
    internal static class PreparedQuestionMapper
    {
        public static PreparedQuestionDto ToDto(Domain.Entities.PreparedQuestion pq)
        {
            return new PreparedQuestionDto
            {
                Id = pq.Id,
                InterviewRoomId = pq.InterviewRoomId,
                CreatedBy = pq.CreatedBy,
                SourceBankQuestionId = pq.SourceBankQuestionId,
                InteractionType = pq.InteractionType,
                DisplayCategoryLabel = pq.DisplayCategoryLabel,
                Title = pq.Title,
                Description = pq.Description,
                FunctionName = pq.FunctionName,
                TestCases = pq.TestCases,
                Status = pq.Status,
                AskedAt = pq.AskedAt,
                SortOrder = pq.SortOrder,
                CreatedAt = pq.CreatedAt,
                UpdatedAt = pq.UpdatedAt,
                IsReadyForEditor = pq.IsReadyForEditor()
            };
        }

        /// <summary>
        /// Maps a Question-bank Category/Round onto our binary interaction type.
        /// Anything not explicitly Coding is treated as NonCoding.
        /// </summary>
        public static PreparedQuestionInteractionType MapBankToInteractionType(
            QuestionCategory category,
            Domain.Entities.Constants.QuestionConstants.InterviewRound round)
        {
            if (category == QuestionCategory.Coding
                || round == Domain.Entities.Constants.QuestionConstants.InterviewRound.CodingChallenge
                || round == Domain.Entities.Constants.QuestionConstants.InterviewRound.LiveCoding)
            {
                return PreparedQuestionInteractionType.Coding;
            }

            return PreparedQuestionInteractionType.NonCoding;
        }

        /// <summary>
        /// Normalises a raw <c>object[]</c> test-case payload into a shape that the
        /// EF Core <see cref="System.Text.Json.JsonSerializer"/>-based value converter
        /// can persist cleanly.
        ///
        /// Why this exists: the API layer uses <c>AddNewtonsoftJson</c>, so request
        /// bodies deserialize each untyped <c>object</c> into a Newtonsoft
        /// <c>JObject</c>. When we then hand that straight to the entity property
        /// and EF serialises it with <c>System.Text.Json</c>, STJ has no converter
        /// for <c>JObject</c> and emits either empty objects or the CLR-property
        /// projection of <c>JObject</c> — which is how Coding test cases end up
        /// "disappearing" by the time the question is sent to the editor.
        ///
        /// We re-serialise through Newtonsoft (which preserves the wire JSON 1:1)
        /// and reparse with <c>System.Text.Json</c> so the entity holds
        /// <c>JsonElement[]</c>, which STJ serialises losslessly on save and on
        /// every SignalR broadcast afterwards.
        /// </summary>
        public static object[]? NormalizeTestCasesForPersistence(object[]? raw)
        {
            if (raw == null || raw.Length == 0)
            {
                return null;
            }

            var json = NewtonsoftJson.SerializeObject(raw);
            return SystemTextJson.Deserialize<object[]>(json);
        }

        /// <summary>
        /// Humanised label for the bank category. Preserves the original category string
        /// (e.g. "Behavioral", "Technical", "SystemDesign"), which the UI displays as a chip.
        /// </summary>
        public static string MapBankCategoryToLabel(QuestionCategory category)
        {
            return category switch
            {
                QuestionCategory.SystemDesign => "System Design",
                QuestionCategory.DataStructures => "Data Structures",
                QuestionCategory.CaseStudy => "Case Study",
                QuestionCategory.DistributedSystems => "Distributed Systems",
                _ => category.ToString()
            };
        }
    }

    internal static class PreparedQuestionAuthorization
    {
        /// <summary>
        /// Loads the room and validates that the caller is its assigned Coach.
        /// Throws NotFoundException if the room does not exist,
        /// or ForbiddenException if the caller isn't the room's coach.
        /// </summary>
        public static async Task<InterviewRoomEntity> EnsureRoomCoachAsync(
            IInterviewRoomRepository roomRepo,
            Guid interviewRoomId,
            Guid userId)
        {
            var room = await roomRepo.GetByIdAsync(interviewRoomId)
                ?? throw new NotFoundException("Interview room not found");

            if (!room.CoachId.HasValue || room.CoachId.Value != userId)
            {
                throw new ForbiddenException("Only the assigned coach can manage prepared questions for this room");
            }

            return room;
        }
    }
}
