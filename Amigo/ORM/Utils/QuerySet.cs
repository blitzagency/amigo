using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Amigo.ORM.Engines;

namespace Amigo.ORM.Utils
{
    public static class Utils
    {
        public static Regex Quotes = new Regex("\'", RegexOptions.Multiline);

        public static string EscapeSqlValue(object value)
        {
            // This is super WEAK, but it's just in the flow for now.
            string result = "";

            if (value is int ||
                value is Int16 ||
                value is Int32 ||
                value is Int64)
            {
                result = value.ToString();
            }
            else if (value is string)
            {
                // this won't stop much of anything...
                // handle's the quote escape at least...
                // TODO Find something not laughable.
                var tmp = value.ToString();
                tmp = Quotes.Replace(tmp, "''");

                result = string.Format("'{0}'", tmp);
            }

            return result;
        }
    }

    public class QuerySet<T>
    {
        public Type ModelType { get; set; }
        public Operator Filters { get; set; }
        public Order Orders { get; set; }
        public List<string> RelatedColumns { get; set; }
        public List<string> Fields { get; set; }
        public bool WantsRelated { get; set; }
        public IAlchemyEngine Engine { get; set; }
        public MetaData Meta { get; set; }
        public object PivotModel { get; set; }


        public QuerySet(IAlchemyEngine engine)
        {
            ModelType = typeof(T);
            RelatedColumns = new List<string>();
            Fields = new List<string>();
            WantsRelated = false;

            Engine = engine;
            Meta = engine.Meta;

        }

        public async Task<T> Get(object keys)
        {
            FilterBy(keys);
            return await Engine.ExecuteAsync<T>(this);
        }

        public async Task<List<T>> All()
        {
            return await Engine.ExecuteListAsync<T>(this);
        }

        public int Count()
        {
            return 1;
        }

        public QuerySet<T> FilterBy(object kwargs)
        {
            var type = kwargs.GetType();
               
            if (type.Name.StartsWith("<>") &&
                type.Name.Contains("AnonType"))
            {
                Filters = new Eq<And>(kwargs);
            }
            else if (kwargs is Operator)
            {
                Filters = kwargs as Operator;
            }

            return this;
        }

        public QuerySet<T> FilterBy(IDictionary<string, object> kwargs)
        {
            var type = kwargs.GetType();

            if (type.Name.StartsWith("<>") &&
                type.Name.Contains("AnonType"))
            {
                Filters = new Eq<And>(kwargs);
            }
            else if (kwargs is Operator)
            {
                Filters = kwargs as Operator;
            }

            return this;
        }

        public QuerySet<T> SelectFields(params string[] list)
        {
            Fields = new List<string>(list);
            return this;
        }

        public QuerySet<T> FromModel(object model)
        {
            PivotModel = model;
            return this;
        }

        public QuerySet<T> OrderBy(object kwargs)
        {
            var type = kwargs.GetType();

            if (type.Name.StartsWith("<>") &&
                type.Name.Contains("AnonType"))
            {
                Orders = new Order(kwargs);
            }
            else if (kwargs is Operator)
            {
                Orders = kwargs as Order;
            }

            return this;
        }

        public QuerySet<T> SelectRelated(params string[] list)
        {
            WantsRelated = true;
            RelatedColumns = new List<string>(list.Select(x => x.ToLower()));

            return this;
        }

        public string ToSql()
        {
            return Engine.CreateQuerySetSql(this);
        }
    }
}

