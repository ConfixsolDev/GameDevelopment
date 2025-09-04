
namespace TechWebSol.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public class Datatable<T>
    {

        private IQueryable<T> _query;

        public Datatable(IQueryable<T> query)
        {
            _query = query;
        }

        public IQueryable<T> ApplyDatatablesFilters(JQDTParams jqParams, bool applyPaging = false)
        {
            foreach (var item in jqParams.columns)
            {
                if (!string.IsNullOrEmpty(item.search.value))
                {
                    var predicate = BuildContainsPredicate(item.name, item.search.value);
                    _query = _query.Where(predicate);
                }
            }

            if (!string.IsNullOrEmpty(jqParams.search.value))
            {
                foreach (var item in jqParams.columns.Where(c => c.searchable))
                {
                    if (typeof(T).GetProperty(item.name)?.PropertyType == typeof(string))
                    {
                        var predicate = BuildContainsPredicate(item.name, jqParams.search.value);
                        _query = _query.Where(predicate);
                    }
                }
            }

            foreach (var item in jqParams.order)
            {
                var propertyName = jqParams.columns[item.column].name;
                if (item.dir == JQDTColumnOrderDirection.asc)
                {
                    _query = _query.OrderByPropertyName(propertyName);
                }
                else
                {
                    _query = _query.OrderByDescendingPropertyName(propertyName);
                }
            }

            if (applyPaging)
                _query = _query.Skip(jqParams.start).Take(jqParams.length);

            return _query;
        }

        public Tuple<int, List<T>> FilterRecords(JQDTParams jqParams)
        {
            int totalRecords = 0;
            //foreach (var item in jqParams.columns)
            //{
            //    if (!string.IsNullOrEmpty(item.search.value))
            //    {
            //        var predicate = BuildContainsPredicate(item.name, item.search.value);
            //        _query = _query.Where(predicate);
            //    }
            //}
            //if (!string.IsNullOrEmpty(jqParams.search.value))
            //{
            //    foreach (var item in jqParams.columns.Where(c => c.searchable))
            //    {
            //        if (typeof(T).GetProperty(item.name)?.PropertyType == typeof(string))
            //        {
            //            var predicate = BuildContainsPredicate(item.name, jqParams.search.value);
            //            _query = _query.Where(predicate);
            //        }
            //    }
            //}

            foreach (var item in jqParams.order)
            {
                var propertyName = jqParams.columns[item.column].name;
                if (item.dir == JQDTColumnOrderDirection.asc)
                {
                    _query = _query.OrderByPropertyName(propertyName);
                }
                else
                {
                    _query = _query.OrderByDescendingPropertyName(propertyName);
                }
            }

            totalRecords = _query.Count();
            var list = _query.Skip(jqParams.start).Take(jqParams.length).ToList();

            return new Tuple<int, List<T>>(totalRecords, list);
        }

        private Expression<Func<T, bool>> BuildContainsPredicate(string propertyName, string value)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var propertyToLower = Expression.Call(property, "ToLower", null);
            var constant = Expression.Constant(value.ToLower());

            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsExpression = Expression.Call(propertyToLower, containsMethod, constant);

            return Expression.Lambda<Func<T, bool>>(containsExpression, parameter);
        }
    }

    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> OrderByPropertyName<T>(this IQueryable<T> source, string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var keySelector = Expression.Lambda(property, parameter);

            return Queryable.OrderBy(source, (dynamic)keySelector);
        }

        public static IOrderedQueryable<T> OrderByDescendingPropertyName<T>(this IQueryable<T> source, string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, propertyName);
            var keySelector = Expression.Lambda(property, parameter);

            return Queryable.OrderByDescending(source, (dynamic)keySelector);
        }
    }

}
