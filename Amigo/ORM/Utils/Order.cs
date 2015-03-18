using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Amigo.ORM.Utils
{
    public class Order
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

        public Order(object value)
        {
            Value = value;
        }

        public List<KeyValuePair<string, string>> ProcessValue()
        {
            var type = Value.GetType();
            var props = type.GetRuntimeProperties();
            var result = new List<KeyValuePair<string, string>>();

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

                var value = each.GetValue(Value, null).ToString().ToLower();
                result.Add(new KeyValuePair<string, string>(key, value));
            }

            return result;
        }

        public string TranslateOrderToSqlOrder(string value)
        {
            string result;

            switch (value)
            {
            case "desc":
            case "-":
                result = "DESC";
                break;
               
            default:
                result = "ASC";
                break;
            }

            return result;
        }

        public override string ToString()
        {
            var result = new List<string>();

            foreach (var kvp in  ProcessValue())
            {
                var k = kvp.Key;
                var v = TranslateOrderToSqlOrder(kvp.Value);

                result.Add(string.Format("{0} {1}", k, v));
            }

            return string.Join(", ", result);
        }
    }
}

