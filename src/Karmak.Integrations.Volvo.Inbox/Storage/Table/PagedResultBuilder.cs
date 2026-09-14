using Karmak.Integrations.Volvo.Inbox.Models;
using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Karmak.Integrations.Volvo.Inbox.Storage.Table;

namespace Karmak.Integrations.Volvo.Inbox.Storage.Table;

public class PagedResultBuilder<T>
    where T : class, IMetaDataEntity, new()
{
    private readonly List<T> _unPagedResults;

    public PagedResultBuilder(List<T> unPagedResults)
    {
        _unPagedResults = unPagedResults;
    }

    public PagedResult<T> Build(QueryArguments queryArgs)
    {
        queryArgs.PageNumber = queryArgs.PageNumber > 1 ? queryArgs.PageNumber : 1;
        var startIndex = queryArgs.PageNumber < 2 ? 0 : (queryArgs.PageNumber - 1) * queryArgs.ItemsPerPage;
        var endIndex = startIndex + queryArgs.ItemsPerPage;

        var pagedResult = new PagedResult<T>
        {
            CurrentPage = queryArgs.PageNumber,
            ItemsPerPage = queryArgs.ItemsPerPage,
            HasMoreItems = endIndex < _unPagedResults.Count,
            Items = new List<T>()
        };

        if (_unPagedResults.Any())
        {
            var sortedResults = ApplySortingIfNeeded(queryArgs);

            pagedResult.Items = sortedResults
                .Skip(startIndex)
                .Take(queryArgs.ItemsPerPage)
                .ToList();

            BuildNextPageToken(queryArgs, pagedResult);
        }

        return pagedResult;
    }

    private List<T> ApplySortingIfNeeded(QueryArguments queryArgs)
    {
        if (!string.IsNullOrWhiteSpace(queryArgs.SortField))
        {
            var queryablePagedResults = _unPagedResults.AsQueryable();
            return queryArgs.SortAscending
                ? queryablePagedResults.OrderBy(queryArgs.SortField).ThenBy(i => i.Id) .ToList()
                : queryablePagedResults.OrderByDescending(queryArgs.SortField).ThenByDescending(i => i.Id).ToList();
        }

        return _unPagedResults;
    }
    
    private static void BuildNextPageToken(QueryArguments queryArgs, PagedResult<T> pagedResult)
    {
        var sortKey = !string.IsNullOrWhiteSpace(queryArgs.SortField)
            ? $":{queryArgs.SortField}:{queryArgs.SortAscending}"
            : "";

        pagedResult.NextPageToken = pagedResult.HasMoreItems
            ? $"{pagedResult.CurrentPage + 1}{sortKey}"
            : null;
    }
}