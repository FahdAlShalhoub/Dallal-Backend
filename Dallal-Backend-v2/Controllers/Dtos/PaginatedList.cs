namespace Dallal_Backend_v2.Controllers.Dtos;

public class PaginatedList<T>(List<T> items, int currentPage, int count, int pageSize)
    where T : class
{
    public List<T> Items { get; } = items;
    public int CurrentPage { get; } = currentPage;
    public int TotalPages { get; } = count > 0 ? (int)Math.Ceiling((double)count / pageSize) : 0;
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}
