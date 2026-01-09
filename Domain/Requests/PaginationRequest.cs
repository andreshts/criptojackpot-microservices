using System.ComponentModel.DataAnnotations;

namespace CryptoJackpot.Domain.Core.Requests;

/// <summary>
/// Pagination request parameters for API endpoints.
/// Provides validation attributes and sensible defaults.
/// </summary>
public class PaginationRequest
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    private int _pageNumber = 1;

    [Range(1, int.MaxValue)]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    [Range(1, 50)]
    public int PageSize
    {
        get => _pageSize;
        set
        {
            if (value > MaxPageSize)
                _pageSize = MaxPageSize;
            else if (value < 1)
                _pageSize = 1;
            else
                _pageSize = value;
        }
    }
}

