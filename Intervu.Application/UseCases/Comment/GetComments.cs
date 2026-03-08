using Intervu.Application.DTOs.Comment;
using Intervu.Application.DTOs.Common;
using Intervu.Application.Interfaces.UseCases.Comment;
using Intervu.Domain.Entities.Constants.QuestionConstants;
using Intervu.Domain.Repositories;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Comment
{
    public class GetComments(ICommentRepository commentRepository, IUserRepository userRepository, IUserCommentLikeRepository commentLikeRepository)
        : IGetComments
    {
        public async Task<PagedResult<CommentDetailDto>> ExecuteAsync(Guid questionId, int page, int pageSize, SortOption? sortBy = null, Guid? userId = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var (items, total) = await commentRepository.GetPagedByQuestionIdAsync(questionId, page, pageSize, sortBy);

            var authorIds = items.Select(c => c.CreateBy).Distinct().ToList();
            var authorMap = new Dictionary<Guid, Domain.Entities.User>();
            foreach (var uid in authorIds)
            {
                var u = await userRepository.GetByIdAsync(uid);
                if (u != null) authorMap[uid] = u;
            }

            var commentIds = items.Select(c => c.Id).ToList();
            var likedCommentIds = userId.HasValue
                ? await commentLikeRepository.GetLikedCommentIdsAsync(userId.Value, commentIds)
                : new HashSet<Guid>();

            var dtos = items.Select(c =>
            {
                authorMap.TryGetValue(c.CreateBy, out var author);
                return new CommentDetailDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    Vote = c.Vote,
                    IsAnswer = c.IsAnswer,
                    CreatedAt = c.CreatedAt,
                    CreatedBy = c.CreateBy,
                    AuthorName = author?.FullName ?? "Anonymous",
                    AuthorProfilePicture = author?.ProfilePicture,
                    IsLikedByUser = likedCommentIds.Contains(c.Id)
                };
            }).ToList();

            return new PagedResult<CommentDetailDto>(dtos, total, pageSize, page);
        }
    }
}
