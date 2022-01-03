﻿using Ardalis.ApiEndpoints;
using DN.WebApi.Application.Common.Interfaces;
using DN.WebApi.Application.Wrapper;
using DN.WebApi.Domain.Catalog;
using DN.WebApi.Domain.Constants;
using DN.WebApi.Host.Controllers;
using DN.WebApi.Infrastructure.Identity.Permissions;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace DN.WebApi.Host.Endpoints.Catalog.Products;

[ApiConventionType(typeof(FSHApiConventions))]
public class GetDapper : EndpointBaseAsync
    .WithRequest<GetProductRequest>
    .WithResult<Result<ProductDto>>
{
    private readonly IRepositoryAsync _repository;

    public GetDapper(IRepositoryAsync repository) => _repository = repository;

    [HttpGet("dapper/{id:guid}")]
    [MustHavePermission(PermissionConstants.Products.View)]
    [OpenApiOperation("Get product details via dapper.", "")]
    public override async Task<Result<ProductDto>> HandleAsync([FromRoute] GetProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _repository.QueryFirstOrDefaultAsync<Product>(
            $"SELECT * FROM public.\"Products\" WHERE \"Id\"  = '{request.Id}' AND \"Tenant\" = '@tenant'");
        var mappedProduct = product.Adapt<ProductDto>();
        return await Result<ProductDto>.SuccessAsync(mappedProduct);
    }
}