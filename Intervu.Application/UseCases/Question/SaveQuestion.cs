using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class SaveQuestion(
        IUserRepository userRepository,
        IQuestionRepository questionRepository,
        ICandidateProfileRepository candidateProfileRepository,
        ICoachProfileRepository coachProfileRepository) : ISaveQuestion
    {
        public async Task<bool> ExecuteAsync(Guid questionId, bool isSaveQuestion, Guid userId)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new Exception("User not found");

            var question = await questionRepository.GetByIdAsync(questionId)
                ?? throw new Exception("Question not found");


            if (user.Role == UserRole.Candidate)
            {
                var profile = await candidateProfileRepository.GetProfileByIdAsync(userId)
                    ?? throw new Exception("Candidate profile not found");

                profile.SavedQuestions ??= new List<QuestionSnapshot>();

                bool exists = profile.SavedQuestions.Any(s => s.Id == questionId);

                if (isSaveQuestion)
                {
                    if (!exists)
                    {
                        var snapshot = await BuildSnapshotAsync(questionId);
                        profile.SavedQuestions.Add(snapshot);
                        question.SaveCount++;
                    }
                }
                else
                {
                    if (exists)
                    {
                        profile.SavedQuestions.RemoveAll(s => s.Id == questionId);
                        question.SaveCount--;
                    }
                }

                await candidateProfileRepository.UpdateCandidateProfileAsync(profile);
                return isSaveQuestion;
            }
            else
            {
                var profile = await coachProfileRepository.GetProfileByIdAsync(userId)
                    ?? throw new Exception("Coach profile not found");

                profile.SavedQuestions ??= new List<QuestionSnapshot>();

                bool exists = profile.SavedQuestions.Any(s => s.Id == questionId);

                if (isSaveQuestion)
                {
                    if (!exists)
                    {
                        var snapshot = await BuildSnapshotAsync(questionId);
                        profile.SavedQuestions.Add(snapshot);
                        question.SaveCount++;
                    }
                }
                else
                {
                    if (exists)
                    {
                        profile.SavedQuestions.RemoveAll(s => s.Id == questionId);
                        question.SaveCount--;
                    }
                }

                await coachProfileRepository.UpdateCoachProfileAsync(profile);
                return isSaveQuestion;
            }
        }

        private async Task<QuestionSnapshot> BuildSnapshotAsync(Guid questionId)
        {
            var question = await questionRepository.GetDetailAsync(questionId)
                ?? throw new Exception("Question not found");

            return new QuestionSnapshot
            {
                Id = question.Id,
                Title = question.Title,
                Content = question.Content ?? string.Empty,
                Level = question.Level.ToString(),
                Round = question.Round.ToString(),
                Status = question.Status.ToString(),
                Category = question.Category.ToString(),
                ViewCount = question.ViewCount,
                SaveCount = question.SaveCount,
                Vote = question.Vote,
                IsHot = question.IsHot,
                CreatedAt = question.CreatedAt,
                AuthorName = question.Author?.FullName,
                AuthorProfilePicture = question.Author?.ProfilePicture,
                AuthorSlug = question.Author?.SlugProfileUrl,
                CompanyNames = question.QuestionCompanies?.Select(qc => qc.Company?.Name ?? string.Empty).ToList() ?? new(),
                Roles = question.QuestionRoles?.Select(qr => qr.Role.ToString()).ToList() ?? new(),
                Tags = question.QuestionTags?.Select(qt => qt.Tag?.Name ?? string.Empty).ToList() ?? new()
            };
        }
    }
}


