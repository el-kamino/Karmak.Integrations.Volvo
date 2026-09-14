namespace Karmak.Integrations.Volvo.Inbox.Storage.Table.Filtering;

public class FilterCondition
{
    public FilterOperators Operator { get; set; }

    public string PropertyName { get; set; }

    public FilterComparisons Comparison { get; set; }

    public string ComparedValue { get; set; }

    public bool ComparedValueIsString { get; set; } = true;

    public string BuildFilterString()
    {
        var value = ComparedValueIsString ? $"'{ComparedValue}'" : ComparedValue;

        return $" {OperatorString} {PropertyName} {ComparisonString} {value}";
    }

    private string OperatorString
    {
        get
        {
            switch (Operator)
            {
                case FilterOperators.And: return "and";
                case FilterOperators.Not: return "not";
                case FilterOperators.Or: return "or";
                default: return "and";
            }
        }
    }

    private string ComparisonString
    {
        get
        {
            switch (Comparison)
            {
                case FilterComparisons.Equal: return "eq";
                case FilterComparisons.NotEqual: return "ne";
                case FilterComparisons.GreaterThan: return "gt";
                case FilterComparisons.GreaterThanOrEqual: return "ge";
                case FilterComparisons.LessThan: return "lt";
                case FilterComparisons.LessThanOrEqual: return "le";
                default: return "eq";
            }
        }
    }
}