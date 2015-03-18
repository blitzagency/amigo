using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Amigo.ORM.Utils
{
    public class Operator
    {
        private string _defaultTable;

        public virtual object Value { get; set; }
        public string DefaultTable { 
            get {
                return _defaultTable;
            } 
            set {
                if (value != null)
                    _defaultTable = value.ToLower();
            }
        }

        public Operator()
        {

        }

        public Operator(object value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return (string)Value;
        }
    }

    public class RelationalOperator : Operator
    {

        public RelationalOperator(object value) : base(value)
        {

        }

        public List<KeyValuePair<string, object>> ProcessValue()
        {
            var type = Value.GetType();
            var props = type.GetRuntimeProperties();
            var result = new List<KeyValuePair<string, object>>();

            foreach (var each in props)
            {
                string[] splitBy = new string[] { "__" };

                var parts = each.Name.Split(splitBy, StringSplitOptions.None);
                parts = parts.Select(x => x.ToLower()).ToArray();

                string key;

                if (parts.Length == 1)
                {
                    if (DefaultTable == null)
                    {
                        key = parts[0];
                    }
                    else
                    {
                        
                        key = string.Format("{0}.{1}", DefaultTable, parts[0]);
                    }
                }
                else
                {
                    key = string.Join(".", parts);
                }

                var value = each.GetValue(Value, null);
                result.Add(new KeyValuePair<string, object>(key, value));
            }

            return result;
        }

        public override string ToString()
        {
            return (string)Value;
        }
    }

    public class LogicalOperator: Operator
    {
        public new List<Operator> Value { get; set; }

        public LogicalOperator()
        {
            Value = new List<Operator>();
        }

        public LogicalOperator(params Operator[] values)
        {
            Value = new List<Operator>(values);
        }
    }

    public class Eq: RelationalOperator
    {
        public Eq(object value) : base(value)
        {

        }

        public override string ToString()
        {
            var eq = new Eq<And>(Value);
            eq.DefaultTable = DefaultTable;

            return eq.ToString();
        }
    }

    public class Eq<T> : RelationalOperator 
        where T : LogicalOperator
    {
        public Eq(object value) : base(value)
        {

        }

        public override string ToString()
        {
            var logicalOperator = (T)Activator.CreateInstance(typeof(T));
            logicalOperator.DefaultTable = DefaultTable;

            foreach (var kvp in  ProcessValue())
            {
                var value = Utils.EscapeSqlValue(kvp.Value);
                var v = string.Format("{0} = {1}", kvp.Key, value);

                logicalOperator.Value.Add(new Operator(v));
            }

            return logicalOperator.ToString();
        }

    }

    public class Neq: RelationalOperator
    {
        public Neq(object value) : base(value)
        {

        }

        public override string ToString()
        {
            var neq = new Neq<And>(Value);
            neq.DefaultTable = DefaultTable;

            return neq.ToString();
        }
    }

    public class Neq<T>: RelationalOperator
        where T : LogicalOperator
    {
        public Neq(object value) : base(value)
        {

        }

        public override string ToString()
        {
            var logicalOperator = (T)Activator.CreateInstance(typeof(T));
            logicalOperator.DefaultTable = DefaultTable;

            foreach (var kvp in  ProcessValue())
            {

                var value = Utils.EscapeSqlValue(kvp.Value);
                var v = string.Format("{0} != {1}", kvp.Key, value); 

                logicalOperator.Value.Add(new Operator(v));
            }

            return logicalOperator.ToString();
        }
    }


    public class Or: LogicalOperator
    {
        public Or()
        {
            Value = new List<Operator>();
        }

        public Or(params Operator[] values) : base(values)
        {

        }

        public override string ToString()
        {
            var sql = new List<string>();

            foreach (var v in Value)
            {
                v.DefaultTable = DefaultTable;

                if (v is LogicalOperator)
                {
                    sql.Add(string.Format("({0})", v));
                }
                else
                {
                    sql.Add(string.Format("{0}", v));
                }
            }

            return string.Join(" OR ", sql);
        }
    }

    public class And: LogicalOperator
    {
        public And()
        {
            Value = new List<Operator>();
        }

        public And(params Operator[] values) : base(values)
        {

        }

        public override string ToString()
        {
            var sql = new List<string>();

            foreach (var v in Value)
            {
                v.DefaultTable = DefaultTable;

                if (v is LogicalOperator)
                {
                    sql.Add(string.Format("({0})", v));
                }
                else
                {
                    sql.Add(string.Format("{0}", v));
                }
            }

            return string.Join(" AND ", sql);
        }
    }
}

