using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq.Dynamic.Core;
using System.Reflection;
using TechWebSol.Extensions;

namespace TechWebSol.Extensions
{
    public static class ExtensionsMethods
    {
        public static string ToDateFormat(this DateTime Date)
        {
              string  FormattedDate = Date.ToString("dd/MM/yyyy");
            return FormattedDate;
        }
        public static string ToDateFormat(this DateTime? Date)
        {
            if (Date.HasValue)
            {
                return Date.Value.ToString("dd/MM/yyyy");
            }
            else
            {
                return string.Empty; 
            }
        }
    }

    public static class EntityExtensions
    {
        public static IQueryable<T> FilterRecords<T>(this IQueryable<T> query, JQDTParams Params, bool applySorting = true)
        {

            if (applySorting)
            {
                query = query.ApplyOrder(Params);
            }

            //Text search
            string queryExp = string.Empty;
            string searchExp = Params.search?.value?.Trim();
            if (!string.IsNullOrEmpty(searchExp))
            {
                foreach (var col in Params.columns)
                {
                    if (col.searchable)
                    {
                        PropertyInfo property = typeof(T).GetProperties().Where(p => p.Name.ToLower() == col.name.ToLower()).FirstOrDefault();

                        if (property != null)
                        {
                            if (property.PropertyType == typeof(string))
                            {
                                queryExp += string.Format("({0} != null and Convert.ToString({0}).Contains(@0))", col.name);
                            }
                            else if (property.PropertyType == typeof(short) || property.PropertyType == typeof(short?) || property.PropertyType == typeof(int) || property.PropertyType == typeof(int?) || property.PropertyType == typeof(long) || property.PropertyType == typeof(long?))
                            {
                                queryExp += string.Format("(Convert.ToString(Int64({0})).Contains(@0))", col.name);
                            }
                            else if (property.PropertyType == typeof(decimal) || property.PropertyType == typeof(decimal?) || property.PropertyType == typeof(float) || property.PropertyType == typeof(float?) || property.PropertyType == typeof(double) || property.PropertyType == typeof(double?))
                            {
                                queryExp += string.Format("(Convert.ToString(Decimal({0})).Contains(@0))", col.name);
                            }
                            else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                            {
                                DateTime reslut = DateTime.Today;
                                if (DateTime.TryParse(searchExp, out reslut))
                                {

                                    //queryExp += string.Format("(Convert.ToString(DateTime({0})).Contains(@0))", col.name);
                                    queryExp += string.Format("(DateTime({0}).Date == DateTime.Parse(@0))", col.name);
                                }
                                else
                                {
                                    queryExp += "(1 = 0)";
                                }
                            }

                            if (Params.columns.Where(x => x.searchable).Last() != col)
                                queryExp += " or ";
                        }
                    }
                }

                query = query.Where(queryExp, searchExp);
            }

            return query;
        }

        public static IQueryable<T> FilterRecordsForCompleteList<T>(this IQueryable<T> query, JQDTParams Params)
        {
            query = query.ApplyOrder(Params);
            //Text search
            string queryExp = string.Empty;
            if (!string.IsNullOrEmpty(Params.search?.value))
            {
                foreach (var col in Params.columns)
                {
                    if (col.searchable)
                    {
                        queryExp += string.Format("({0} != null and Convert.ToString({0}).Contains(@0, StringComparison.OrdinalIgnoreCase))", col.name);

                        if (Params.columns.Where(p => p.searchable).Last() != col)
                            queryExp += " or ";
                    }
                }
                try
                {
                    query = query.Where(queryExp, Params.search?.value);
                }
                catch
                { }
            }

            return query;
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, JQDTParams Params)
        {
            int totalRecords = query.Count();
            var list = query.Skip(Params.start).Take(Params.length);

            return list;
        }

        public static Tuple<int, List<T>> ApplyPagingQuerable<T>(this IQueryable<T> query, JQDTParams Params)
        {
            int totalRecords = query.Count();
            var list = query.Skip(Params.start).Take(Params.length).ToList();
            return new Tuple<int, List<T>>(totalRecords, list);
        }

        public static Tuple<int, List<T>> ApplyPaging<T>(this IEnumerable<T> query, JQDTParams Params)
        {
            int totalRecords = query.Count();
            var list = query.Skip(Params.start).Take(Params.length).ToList();

            return new Tuple<int, List<T>>(totalRecords, list);
        }

        private static IQueryable<T> ApplyOrder<T>(this IQueryable<T> query, JQDTParams Params)
        {
            string queryOrder = string.Empty;

            foreach (var orderCol in Params.order)
            {
                queryOrder += Params.columns.ElementAt(orderCol.column).name + " " + orderCol.dir;

                if (Params.order.Last() != orderCol)
                    queryOrder += ", ";
            }

            if (!string.IsNullOrEmpty(queryOrder))
                query = query.OrderBy(queryOrder);

            return query;
        }

    }
    public static class ModelStateExtensions
    {
        public static Dictionary<string, string> GetModalErrors(this ModelStateDictionary modelState)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in modelState)
            {
                string key = item.Key.ToLower().Trim();

                foreach (var subitem in item.Value.Errors)
                {
                    if (dict.ContainsKey(key))
                    {
                        dict[key] = dict[key] + "  " + subitem.ErrorMessage;
                    }
                    else
                    {
                        dict.Add(key, subitem.ErrorMessage);
                    }
                }
            }
            return dict;
        }
    }
}
