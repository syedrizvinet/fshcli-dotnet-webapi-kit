using DN.WebApi.Application.Common.FileStorage;
using DN.WebApi.Application.Common.Persistance;
using DN.WebApi.Domain.Catalog.Products;
using DN.WebApi.Domain.Common;
using MediatR;

namespace DN.WebApi.Application.Catalog.Products;

public class CreateProductRequest : IRequest<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Rate { get; set; }
    public Guid BrandId { get; set; }
    public FileUploadRequest? Image { get; set; }
}

public class CreateProductRequestHandler : IRequestHandler<CreateProductRequest, Guid>
{
    private readonly IRepositoryAsync _repository;
    private readonly IFileStorageService _file;

    public CreateProductRequestHandler(IRepositoryAsync repository, IFileStorageService file) =>
        (_repository, _file) = (repository, file);

    public async Task<Guid> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        string productImagePath = await _file.UploadAsync<Product>(request.Image, FileType.Image, cancellationToken);

        var product = new Product(request.Name, request.Description, request.Rate, request.BrandId, productImagePath);

        // Add Domain Events to be raised after the commit
        product.DomainEvents.Add(new ProductCreatedEvent(product));

        await _repository.CreateAsync(product, cancellationToken);

        await _repository.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}