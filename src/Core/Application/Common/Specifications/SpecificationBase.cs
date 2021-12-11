using System.Linq.Expressions;
using DN.WebApi.Domain.Common.Contracts;
using Microsoft.EntityFrameworkCore.Query;

namespace DN.WebApi.Application.Common.Specifications;

public class BaseSpecification<T> : ISpecification<T>
where T : BaseEntity
{
    /*public Expression<Func<T, bool>>? Criteria { get; set; }
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public List<string> IncludeStrings { get; } = new();

    public Expression<Func<T, bool>> And(Expression<Func<T, bool>> query)
    {
        return Criteria = Criteria == null ? query : Criteria.And(query);
    }

    public Expression<Func<T, bool>> Or(Expression<Func<T, bool>> query)
    {
        return Criteria = Criteria == null ? query : Criteria.Or(query);
    }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }*/

    public List<Expression<Func<T, bool>>> Conditions { get; set; } = new List<Expression<Func<T, bool>>>();

    public Func<IQueryable<T>, IIncludableQueryable<T, object>> Includes { get; set; } = default!;

    // public List<string> IncludeStrings { get; } = new();

    public Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy { get; set; } = default!;

    public string[]? OrderByStrings { get; set; }
}