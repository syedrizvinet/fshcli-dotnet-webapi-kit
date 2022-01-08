﻿using DN.WebApi.Application.Common.Exceptions;
using DN.WebApi.Application.Common.Persistance;
using DN.WebApi.Domain.Catalog.Brands;
using DN.WebApi.Domain.Catalog.Products;
using MediatR;
using Microsoft.Extensions.Localization;

namespace DN.WebApi.Application.Catalog.Brands;

public class DeleteBrandRequest : IRequest<Guid>
{
    public Guid Id { get; set; }

    public DeleteBrandRequest(Guid id) => Id = id;
}

public class DeleteBrandRequestHandler : IRequestHandler<DeleteBrandRequest, Guid>
{
    private readonly IRepositoryAsync _repository;
    private readonly IStringLocalizer<DeleteBrandRequestHandler> _localizer;

    public DeleteBrandRequestHandler(IRepositoryAsync repository, IStringLocalizer<DeleteBrandRequestHandler> localizer) =>
        (_repository, _localizer) = (repository, localizer);

    public async Task<Guid> Handle(DeleteBrandRequest request, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsAsync<Product>(p => p.BrandId == request.Id, cancellationToken))
        {
            throw new ConflictException(_localizer["brand.cannotbedeleted"]);
        }

        var brandToDelete = await _repository.RemoveByIdAsync<Brand>(request.Id, cancellationToken);

        brandToDelete.DomainEvents.Add(new BrandDeletedEvent(brandToDelete));

        await _repository.SaveChangesAsync(cancellationToken);

        return request.Id;
    }
}