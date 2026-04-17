using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Tag;
using Intervu.Application.Interfaces.UseCases.Tag;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Tag
{
    public class GetAllTags : IGetAllTags
    {
        private readonly ITagRepository _tagRepository;
        
        public GetAllTags(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<PagedResult<TagDto>> ExecuteAsync(int page, int pageSize)
        {
            var tags = await _tagRepository.GetAllAsync();
            var total = tags.Count;
            
            var pagedTags = tags
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TagDto { Id = t.Id, Name = t.Name })
                .ToList();

            return new PagedResult<TagDto>(
                pagedTags,
                total,
                pageSize,
                page
            );
        }
    }
}
