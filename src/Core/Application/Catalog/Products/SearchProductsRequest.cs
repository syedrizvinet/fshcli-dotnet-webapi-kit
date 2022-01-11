using DN.WebApi.Application.Common.Models;
using DN.WebApi.Application.Common.Persistence;
using DN.WebApi.Domain.Catalog.Products;
using Mapster;
using MediatR;

namespace DN.WebApi.Application.Catalog.Products;

public class SearchProductsRequest : PaginationFilter, IRequest<PaginationResponse<ProductDto>>
{
    public Guid? BrandId { get; set; }
    public decimal? MinimumRate { get; set; }
    public decimal? MaximumRate { get; set; }
}

public class SearchProductsRequestHandler : IRequestHandler<SearchProductsRequest, PaginationResponse<ProductDto>>
{
    private readonly IReadRepository<Product> _repository;

    public SearchProductsRequestHandler(IReadRepository<Product> repository) => _repository = repository;

    public async Task<PaginationResponse<ProductDto>> Handle(SearchProductsRequest request, CancellationToken cancellationToken)
    {
        var spec = new ProductsWithBrandsBySearchRequestSpec(request);

        var list = await _repository.ListAsync(spec, cancellationToken);
        int count = await _repository.CountAsync(spec, cancellationToken);

        var dtoList = list.Adapt<List<ProductDto>>();

        return PaginationResponse<ProductDto>.Create(dtoList, count, request.PageNumber, request.PageSize);
    }
}