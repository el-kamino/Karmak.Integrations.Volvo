using Karmak.Integrations.Volvo.Inbox.Models.Dto;
using Karmak.Integrations.Volvo.Inbox.Storage.Table.Filtering;

namespace Karmak.Integrations.Volvo.Inbox.Storage.Table;

public class QueryArguments
{
    private readonly PagingArguments _pagingArguments;
    public QueryArguments() { }
    
    public QueryArguments(PagingArguments pagingArguments)
    {
        _pagingArguments = pagingArguments;
        
        var (sortField, sortAscending) = DetermineSorting();
        PageNumber = DeterminePageNumber();
        SortField = sortField?.ToCapitalized();
        SortAscending = sortAscending;
        ItemsPerPage = _pagingArguments.PageSize;
    }

    public string AccountNumber { get; set; }

    public string BranchId { get; set; }

    public string StartIdRange { get; set; }

    public string EndIdRange { get; set; }

    public int PageNumber { get; set; }

    public int ItemsPerPage { get; set; }
    
    public string SortField { get; set; }

    public bool SortAscending { get; set; } = true;

    public List<FilterCondition> FilterConditions { get; set; }
    
    private int DeterminePageNumber()
    {
        if (!string.IsNullOrWhiteSpace(_pagingArguments.NextPageToken))
        {
            if (!_pagingArguments.NextPageToken.Contains(":") && int.TryParse(_pagingArguments.NextPageToken, out var nextPage))
                return nextPage;

            if (_pagingArguments.NextPageToken.Contains(":") && int.TryParse(_pagingArguments.NextPageToken.Split(':')[0], out var nextPage1))
                return nextPage1;
        }

        return _pagingArguments.PageNumber ?? 1;
    }

    private (string, bool) DetermineSorting()
    {
        if (!string.IsNullOrWhiteSpace(_pagingArguments.SortField))
            return (_pagingArguments.SortField, _pagingArguments.SortAscending);
        
        if (!string.IsNullOrWhiteSpace(_pagingArguments.NextPageToken)
            && _pagingArguments.NextPageToken.Contains(":"))
        {
            var tokens = _pagingArguments.NextPageToken.Split(':');

            if (tokens.Length > 1)
            {
                if (tokens.Length > 2 && bool.TryParse(tokens[2], out var sortDirection))
                    return (tokens[1], sortDirection);

                return (tokens[1], _pagingArguments.SortAscending);
            }
        }

        return (null, _pagingArguments.SortAscending);
    }
}