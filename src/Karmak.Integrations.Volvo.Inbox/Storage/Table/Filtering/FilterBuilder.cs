using System.Text;

namespace Karmak.Integrations.Volvo.Inbox.Storage.Table.Filtering
{
    internal static class FilterBuilder
    {
        public static string BuildFilter(QueryArguments queryArgs, bool inclusiveRange = false)
        {
            queryArgs.FilterConditions ??= new List<FilterCondition>();

            AddBranchIdFilter(queryArgs);
            AddStartIdRangeFilterIfNeeded(queryArgs, inclusiveRange);
            AddEndIdRangeFilterIfNeeded(queryArgs, inclusiveRange);

            var filterBuilder = new StringBuilder($"PartitionKey eq '{queryArgs.AccountNumber}'");

            foreach (var condition in queryArgs.FilterConditions)
            {
                filterBuilder.Append(condition.BuildFilterString());
            }

            var filter = filterBuilder.ToString();
            return filter;
        }

        private static void AddBranchIdFilter(QueryArguments queryArgs)
        {
            queryArgs.FilterConditions.Add(new FilterCondition
            {
                Operator = FilterOperators.And,
                PropertyName = "BranchId",
                Comparison = FilterComparisons.Equal,
                ComparedValue = queryArgs.BranchId,
                ComparedValueIsString = true
            });
        }

        private static void AddStartIdRangeFilterIfNeeded(QueryArguments queryArgs, bool inclusiveRange)
        {
            if (!string.IsNullOrWhiteSpace(queryArgs.StartIdRange))
            {
                queryArgs.FilterConditions.Add(new FilterCondition
                {
                    Operator = FilterOperators.And,
                    PropertyName = "RowKey",
                    Comparison = inclusiveRange ? FilterComparisons.GreaterThanOrEqual : FilterComparisons.GreaterThan,
                    ComparedValue = queryArgs.StartIdRange,
                    ComparedValueIsString = true
                });
            }
        }

        private static void AddEndIdRangeFilterIfNeeded(QueryArguments queryArgs, bool inclusiveRange)
        {
            if (!string.IsNullOrWhiteSpace(queryArgs.EndIdRange))
            {
                queryArgs.FilterConditions.Add(new FilterCondition
                {
                    Operator = FilterOperators.And,
                    PropertyName = "RowKey",
                    Comparison = inclusiveRange ? FilterComparisons.LessThanOrEqual : FilterComparisons.LessThan,
                    ComparedValue = queryArgs.EndIdRange,
                    ComparedValueIsString = true
                });
            }
        }
    }
}
