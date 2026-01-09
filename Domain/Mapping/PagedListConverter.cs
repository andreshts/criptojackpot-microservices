using AutoMapper;
using CryptoJackpot.Domain.Core.Models;

namespace CryptoJackpot.Domain.Core.Mapping;

/// <summary>
/// AutoMapper type converter for mapping PagedList of one type to PagedList of another type.
/// This converter preserves pagination metadata while converting the items collection.
/// </summary>
public class PagedListConverter<TSource, TDestination> : ITypeConverter<PagedList<TSource>, PagedList<TDestination>>
{
    public PagedList<TDestination> Convert(
        PagedList<TSource> source,
        PagedList<TDestination> destination,
        ResolutionContext context)
    {
        return new PagedList<TDestination>
        {
            Items = context.Mapper.Map<IEnumerable<TDestination>>(source.Items),
            TotalItems = source.TotalItems,
            PageNumber = source.PageNumber,
            PageSize = source.PageSize
        };
    }
}

