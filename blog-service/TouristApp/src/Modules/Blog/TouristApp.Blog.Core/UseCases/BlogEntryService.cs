using AutoMapper;
using System.Linq;
using TouristApp.Blog.API.Dtos;
using TouristApp.Blog.API.Public;
using TouristApp.Blog.Core.Domain;
using TouristApp.Blog.Core.Domain.RepositoryInterfaces;
using TouristApp.BuildingBlocks.Core.UseCases;

namespace TouristApp.Blog.Core.UseCases;

public class BlogEntryService : IBlogEntryService
{
    private readonly IBlogEntryRepository _blogRepository;
    private readonly IMapper _mapper;

    public BlogEntryService(IBlogEntryRepository blogRepository, IMapper mapper)
    {
        _blogRepository = blogRepository;
        _mapper = mapper;
    }

    public BlogEntryDto Create(BlogEntryDto blogDto)
    {
        var blog = _mapper.Map<Domain.Blog>(blogDto);
        var result = _blogRepository.Create(blog);
        return _mapper.Map<BlogEntryDto>(result);
    }

    public PagedResult<BlogEntryDto> GetPaged(int page, int pageSize)
    {
        var result = _blogRepository.GetPaged(page, pageSize);
        return new PagedResult<BlogEntryDto>(result.Results.Select(b => _mapper.Map<BlogEntryDto>(b)).ToList(), result.TotalCount);
    }
}