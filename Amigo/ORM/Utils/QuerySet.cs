using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Amigo.ORM.Engines;

namespace Amigo.ORM.Utils
{
    //    public static class LinqExtension
    //    {
    //        public static String CompileExpression(this Expression This, MetaData meta)
    //        {
    //            if (This is BinaryExpression)
    //            {
    //                var bin = (BinaryExpression)This;
    //
    //                var leftExpr = bin.Left.CompileExpression(meta);
    //                var rightExpr = bin.Right.CompileExpression(meta);
    //
    //                return leftExpr + " " + GetSqlName(bin) + " " + rightExpr;
    //            }
    //            else if (This is ParameterExpression)
    //            {
    //                var param = (ParameterExpression)This;
    //                return ":" + param.Name;
    //            }
    //            else if (This is ConstantExpression)
    //            {
    //                var value = This.EvaluateExpression(meta);
    //                return Utils.EscapeSqlValue(value).ToString();
    //            }
    //            else if (This is MemberExpression)
    //            {
    //                var member = (MemberExpression)This;
    //
    //                if (member.Expression != null && member.Expression.NodeType == ExpressionType.Parameter)
    //                {
    //                    // This is a column in the table, output the column name
    //                    var info = (PropertyInfo)member.Member;
    //                    var metaModel = meta.MetaModelForType(info.DeclaringType);
    //                    var column = metaModel.Columns.FirstOrDefault(x => x.PropertyName == info.Name);
    //                    if (column == null)
    //                        throw new Exception(string.Format("Unknown Column {0} on type {1}",
    //                            info.Name, metaModel.Table.TypeName));
    //
    //                    return string.Format("'{0}.{1}'", metaModel.Table.TableName, column.ColumnName);
    //                }
    //                else
    //                {
    //                    var x = member.EvaluateExpression(meta);
    //
    //                    return "";
    //                }
    //            }
    //
    //            throw new NotSupportedException("Cannot compile: " + This.NodeType.ToString());
    //        }
    //
    //        public static object EvaluateExpression(this Expression expr, MetaData meta)
    //        {
    //            if (expr is ConstantExpression)
    //            {
    //                var c = (ConstantExpression)expr;
    //                return c.Value;
    //            }
    //            else if (expr is MemberExpression)
    //            {
    //                var memberExpr = (MemberExpression)expr;
    //                var obj = EvaluateExpression(memberExpr.Expression, meta);
    //
    //                if (memberExpr.Member is PropertyInfo)
    //                {
    //                    var m = (PropertyInfo)memberExpr.Member;
    //                    return m.GetValue(obj, null);
    //                }
    //                else if (memberExpr.Member is FieldInfo)
    //                {
    //                    var m = (FieldInfo)memberExpr.Member;
    //                    return m.GetValue(obj);
    //                }
    //            }
    //            else if (expr is ParameterExpression)
    //            {
    //                return meta.MetaModelForType(expr.Type);
    //            }
    //
    //            throw new NotSupportedException("Cannot compile: " + expr.NodeType.ToString());
    //        }
    //
    //        public static string GetSqlName(Expression prop)
    //        {
    //            var n = prop.NodeType;
    //
    //            switch (n)
    //            {
    //            case ExpressionType.GreaterThan:
    //                return ">";
    //            case ExpressionType.GreaterThanOrEqual:
    //                return ">=";
    //            case ExpressionType.LessThan:
    //                return "<";
    //            case ExpressionType.LessThanOrEqual:
    //                return "<=";
    //            case ExpressionType.And:
    //                return "&";
    //            case ExpressionType.AndAlso:
    //                return "AND";
    //            case ExpressionType.Or:
    //                return "|";
    //            case ExpressionType.OrElse:
    //                return "OR";
    //            case ExpressionType.Equal:
    //                return "=";
    //            case ExpressionType.NotEqual:
    //                return "!=";
    //            default:
    //                throw new NotSupportedException("Cannot get SQL for: " + n);
    //            }
    //        }
    //    }
    //
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
        public Expression LinqFilters { get; set; }


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

        //        public QuerySet<T> Where(object foo)
        //        {
        //            return this;
        //        }
        //
        //        public QuerySet<T> Where(Expression<Func<T,bool>> predExpr)
        //        {
        //            var exp = (LambdaExpression)predExpr;
        //            var pred = exp.Body;
        //            LinqFilters = LinqFilters == null ? pred : Expression.AndAlso(LinqFilters, pred);
        //
        //            return this;
        //        }
        //
        //        public QuerySet<T> Where<U>(Expression<Func<T, U, bool>> predExpr)
        //        {
        //            var exp = (LambdaExpression)predExpr;
        //            var pred = exp.Body;
        //            LinqFilters = LinqFilters == null ? pred : Expression.AndAlso(LinqFilters, pred);
        //
        //            return this;
        //        }

        //        public void CompileFilters()
        //        {
        //            var filters = LinqFilters;
        //            var foo = 1;
        //        }


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

