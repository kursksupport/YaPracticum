using EventManagementService.Models;

namespace EventManagementService.DTOs;

public class PaginatedResult
{
    public int TotalCount { get; set; }

    public List<Event> Items { get; set; } = new();

    public int Page { get; set; }

    public int PageSize { get; set; }
}