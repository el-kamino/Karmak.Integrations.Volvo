using System.Linq.Expressions;

namespace Karmak.Integrations.Volvo.Inbox.Storage.Table;

public static class DynamicLinqSortingExtensions
{
    public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string property)
    {
        return ApplyOrder(source, property, "OrderBy");
    }

    public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string property)
    {
        return ApplyOrder(source, property, "OrderByDescending");
    }

    public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string property)
    {
        return ApplyOrder(source, property, "ThenBy");
    }

    public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> source, string property)
    {
        return ApplyOrder(source, property, "ThenByDescending");
    }

    private static IOrderedQueryable<T> ApplyOrder<T>(IQueryable<T> source, string property, string methodName) 
    {
        var type = typeof(T);
        var arg = Expression.Parameter(type, "x");
        var propertyInfo = type.GetProperty(property);
        if (propertyInfo == null)
            throw new InboxDataException($"There is no field with the name {property} to sort on");
        
        var expr = Expression.Property(arg, propertyInfo);
        
        var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), propertyInfo.PropertyType);
        var lambda = Expression.Lambda(delegateType, expr, arg);
        
        var result = typeof(Queryable).GetMethods()
            .Single(method => 
                method.Name == methodName
                    && method.IsGenericMethodDefinition
                    && method.GetGenericArguments().Length == 2
                    && method.GetParameters().Length == 2
            )
            .MakeGenericMethod(typeof(T), propertyInfo.PropertyType)
            .Invoke(null, new object[] {source, lambda});
        return (IOrderedQueryable<T>)result;
    }
}