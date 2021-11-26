using DN.WebApi.Application.Wrapper;
using DN.WebApi.Shared.DTOs.General.Responses;

namespace DN.WebApi.Application.Abstractions.Services.General;

public interface IAuditService : ITransientService
{
    Task<IResult<IEnumerable<AuditResponse>>> GetUserTrailsAsync(Guid userId);
}