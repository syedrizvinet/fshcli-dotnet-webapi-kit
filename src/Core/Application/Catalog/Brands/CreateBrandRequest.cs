using DN.WebApi.Application.Common.Persistance;
using DN.WebApi.Domain.Catalog.Brands;
using MediatR;

namespace DN.WebApi.Application.Catalog.Brands;

public class CreateBrandRequest : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateBrandRequestHandler : IRequestHandler<CreateBrandRequest, Guid>
{
    private readonly IRepositoryAsync _repository;

    public CreateBrandRequestHandler(IRepositoryAsync repository) => _repository = repository;

    public async Task<Guid> Handle(CreateBrandRequest request, CancellationToken cancellationToken)
    {
        var brand = new Brand(request.Name, request.Description);

        brand.DomainEvents.Add(new BrandCreatedEvent(brand));

        await _repository.CreateAsync(brand, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return brand.Id;
    }
}