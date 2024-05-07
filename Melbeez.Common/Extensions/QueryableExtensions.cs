using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melbeez.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Melbeez.Common.Extensions
{
    public static class QueryableExtensions
    {
        public static async Task<PagedList<T>> ToPagedListAsync<T>(this IQueryable<T> queryable, PagedListCriteria criteria, IDictionary<string, string>? orderByTranslations)
        {
            queryable = OrderQuery(queryable, criteria, orderByTranslations);

            List<T> items;
            int totalCount;

            if (criteria.Take > 0)
            {
                var pageQuery = queryable.Skip(criteria.Skip).Take(criteria.Take);

                items = await pageQuery.ToListAsync();

                totalCount = queryable.Count();
            }
            else
            {
                items = await queryable.ToListAsync();

                totalCount = items.Count;
            }

            return new PagedList<T>(items, totalCount);
        }

        public static IQueryable<T> ToPagedQuery<T>(this IQueryable<T> queryable, PagedListCriteria criteria, IDictionary<string, string>? orderByTranslations)
        {
            queryable = OrderQuery(queryable, criteria, orderByTranslations);

            if (criteria.Take > 0)
            {
                return queryable.Skip(criteria.Skip).Take(criteria.Take);
            }

            return queryable;
        }

        public static IQueryable<T> OrderQuery<T>(this IQueryable<T> queryable, PagedListCriteria criteria, IDictionary<string, string>? OrderByTranslations)
        {
            if (criteria.OrderBy != null)
            {
                foreach (var orderByField in criteria.OrderBy.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    var orderByFieldComponents = orderByField.Split(' ').ToList();
                    if (orderByFieldComponents.Any())
                    {
                        var fieldName = orderByFieldComponents[0];

                        if (OrderByTranslations != null && OrderByTranslations.ContainsKey(fieldName))
                        {
                            fieldName = OrderByTranslations[fieldName];
                        }

                        if (orderByFieldComponents.Count == 1 || orderByFieldComponents[1].StartsWith("asc"))
                        {
                            queryable = queryable is IOrderedQueryable<T> ordered && queryable.Expression.Type == typeof(IOrderedQueryable<T>)
                                ? ordered.ThenBy(fieldName)
                                : queryable.OrderBy(fieldName);
                        }
                        else
                        {
                            queryable = queryable is IOrderedQueryable<T> ordered && queryable.Expression.Type == typeof(IOrderedQueryable<T>)
                                ? ordered.ThenByDescending(fieldName)
                                : queryable.OrderByDescending(fieldName);
                        }
                    }
                }
            }
            return queryable;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "OrderBy", propertyName, comparer);
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "OrderByDescending", propertyName, comparer);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "ThenBy", propertyName, comparer);
        }

        public static IOrderedQueryable<T> ThenByDescending<T>(this IOrderedQueryable<T> query, string propertyName, IComparer<object> comparer = null)
        {
            return CallOrderedQueryable(query, "ThenByDescending", propertyName, comparer);
        }

        public static IOrderedQueryable<T> CallOrderedQueryable<T>(this IQueryable<T> query, string methodName, string propertyName, IComparer<object> comparer = null)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var body = BuildBodyExpression(propertyName, parameter);

            return comparer != null
                ? (IOrderedQueryable<T>)query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), body.Type },
                        query.Expression,
                        Expression.Lambda(body, parameter),
                        Expression.Constant(comparer)
                    )
                )
                : (IOrderedQueryable<T>)query.Provider.CreateQuery(
                    Expression.Call(
                        typeof(Queryable),
                        methodName,
                        new[] { typeof(T), body.Type },
                        query.Expression,
                        Expression.Lambda(body, parameter)
                    )
                );
        }

        public static IQueryable<T> IncludeIf<T, TProperty>(this IQueryable<T> query, bool condition, Expression<Func<T, TProperty>> path) where T : class
        {
            if (condition)
            {
                return query.Include(path);
            }

            return query;
        }

        public static IQueryable<TQuery> WhereIn<TKey, TQuery>(this IQueryable<TQuery> queryable, Expression<Func<TQuery, TKey>> keySelector, ICollection<TKey> values)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (values == null || !values.Any())
            {
                return queryable;
            }

            var distinctValues = Bucketize(values);

            if (distinctValues.Length > 2048)
            {
                throw new ArgumentException("Too many parameters for SQL Server, reduce the number of parameters", nameof(keySelector));
            }

            var predicates = distinctValues
                .Select(v =>
                {
                    // Create an expression that captures the variable so EF can turn this into a parameterized SQL query
                    Expression<Func<TKey>> valueAsExpression = () => v;

                    return Expression.Equal(keySelector.Body, valueAsExpression.Body);
                })
                .ToList();

            var body = CombinePredicates(predicates, Expression.OrElse);
            var clause = Expression.Lambda<Func<TQuery, bool>>(body, keySelector.Parameters);

            return queryable.Where(clause);
        }

        public static IQueryable<TQuery> WhereNotIn<TKey, TQuery>(this IQueryable<TQuery> queryable, Expression<Func<TQuery, TKey>> keySelector, ICollection<TKey> values)
        {
            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }

            if (values == null || !values.Any())
            {
                return queryable;
            }

            var distinctValues = Bucketize(values);

            if (distinctValues.Length > 2048)
            {
                throw new ArgumentException("Too many parameters for SQL Server, reduce the number of parameters", nameof(keySelector));
            }

            var predicates = distinctValues
                .Select(v =>
                {
                    // Create an expression that captures the variable so EF can turn this into a parameterized SQL query
                    Expression<Func<TKey>> valueAsExpression = () => v;

                    return Expression.NotEqual(keySelector.Body, valueAsExpression.Body);
                })
                .ToList();

            var body = CombinePredicates(predicates, Expression.AndAlso);
            var clause = Expression.Lambda<Func<TQuery, bool>>(body, keySelector.Parameters);

            return queryable.Where(clause);
        }

        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
        {
            return condition
                ? query.Where(predicate)
                : query;
        }

        public static IQueryable<T> WhereIfElse<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate, Expression<Func<T, bool>> elsePredicate)
        {
            return condition
                ? query.Where(predicate)
                : query.Where(elsePredicate);
        }

        public static IQueryable<T> IgnoreQueryFiltersIf<T>(this IQueryable<T> query, bool condition) where T : class
        {
            if (condition)
            {
                return query.IgnoreQueryFilters();
            }

            return query;
        }

        private static Expression BuildBodyExpression(string propertyName, Expression parameter)
        {
            var bodyComponents = propertyName.Trim().Split(' ');
            if (bodyComponents.Length == 0)
            {
                throw new ArgumentException("Property name was empty", nameof(propertyName));
            }

            if (bodyComponents.Length == 1)
            {
                return BuildPropertyExpression(propertyName, parameter);
            }

            const int binaryExpressionLength = 3;

            if (bodyComponents.Length == binaryExpressionLength)
            {
                // Three components means the string is a binary expression (e.g. Field1 == Field2)
                var firstPropertyExpression = BuildPropertyExpression(bodyComponents[0], parameter);
                var secondPropertyExpression = BuildPropertyExpression(bodyComponents[2], parameter);

                var operatorString = bodyComponents[1].Trim();

                return BuildBinaryExpression(firstPropertyExpression, secondPropertyExpression, operatorString);
            }

            throw new ArgumentException("Could not translate property name into body expression", nameof(propertyName));
        }

        private static Expression BuildBinaryExpression(Expression left, Expression right, string operatorString)
        {
            switch (operatorString)
            {
                case "==": return Expression.Equal(left, right);
                case "!=": return Expression.NotEqual(left, right);
                case ">": return Expression.GreaterThan(left, right);
                case ">=": return Expression.GreaterThanOrEqual(left, right);
                case "<": return Expression.LessThan(left, right);
                case "<=": return Expression.LessThanOrEqual(left, right);
                case "??": return Expression.Coalesce(left, right);

                default: throw new NotSupportedException($"Unable to interpret operator {operatorString}");
            }
        }

        private static Expression BuildPropertyExpression(string propertyName, Expression parameter)
        {
            return propertyName.Split('.').Aggregate(parameter, Expression.PropertyOrField);
        }

        private static TKey[] Bucketize<TKey>(IEnumerable<TKey> values)
        {
            var distinctValueList = values.Distinct().ToList();

            // Calculate bucket size as 1,2,4,8,16,32,64,...
            var bucket = 1;

            while (distinctValueList.Count > bucket)
            {
                bucket *= 2;
            }

            // Fill all slots.
            var lastValue = distinctValueList.Last();

            for (var index = distinctValueList.Count; index < bucket; index++)
            {
                distinctValueList.Add(lastValue);
            }

            var distinctValues = distinctValueList.ToArray();

            return distinctValues;
        }

        private static Expression CombinePredicates(IReadOnlyList<Expression> parts, Func<Expression, Expression, Expression> fn)
        {
            if (parts.Count == 0)
            {
                throw new ArgumentException("At least one part is required.", nameof(parts));
            }

            if (parts.Count == 1)
            {
                return parts[0];
            }

            var segment = new HalfList<Expression>(parts);

            return CombineCore(segment.Split(), fn);

            static Expression CombineCore((HalfList<Expression> left, HalfList<Expression> right) x, Func<Expression, Expression, Expression> fn)
            {
                var left = x.left.Count == 1 ? x.left.Item : CombineCore(x.left.Split(), fn);
                var right = x.right.Count == 1 ? x.right.Item : CombineCore(x.right.Split(), fn);
                return fn(left, right);
            }
        }

        private readonly struct HalfList<T>
        {
            private readonly IReadOnlyList<T> _list;
            private readonly int _startIndex;

            private HalfList(IReadOnlyList<T> list, int startIndex, int count)
            {
                _list = list ?? throw new ArgumentNullException(nameof(list));
                _startIndex = startIndex;
                Count = count;
            }

            public HalfList(IReadOnlyList<T> list) : this(list, 0, list.Count) { }

            public int Count { get; }

            public T Item => Count == 1 ? _list[_startIndex] : throw new InvalidOperationException();

            public (HalfList<T> left, HalfList<T> right) Split()
            {
                if (Count < 2)
                {
                    throw new InvalidOperationException();
                }

                var pivot = Count >> 1;
                var left = new HalfList<T>(_list, _startIndex, pivot);
                var right = new HalfList<T>(_list, _startIndex + pivot, Count - pivot);

                return (left, right);
            }
        }
    }
}
