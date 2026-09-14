namespace Karmak.Integrations.Volvo.Inbox.Models.Dto;

public class PagingArguments
{
    public int PageSize { get; set; } = 50;
    public int? PageNumber { get; set; }
    public string? NextPageToken { get; set; }
    public string? Oem { get; set; }
    public bool ActiveOnly { get; set; } = false;
    public string? SortField { get; set; }
    public bool SortAscending { get; set; } = true;
}