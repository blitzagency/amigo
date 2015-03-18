using System;

namespace Amigo.ORM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string TypeName { get; set; }

        public TableAttribute(string name = null)
        {
            TableName = name;
        }
    }
}

