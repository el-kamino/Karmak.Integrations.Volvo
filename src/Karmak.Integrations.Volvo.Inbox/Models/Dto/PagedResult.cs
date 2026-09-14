namespace Karmak.Integrations.Volvo.Inbox.Models.Dto;

public class PagedResult<TItem>
{
    public int? CurrentPage { get; set; }
    public int ItemsPerPage { get; set; }
    public bool HasMoreItems { get; set; }
    public string NextPageToken { get; set; }
    public List<TItem> Items { get; set; }
}