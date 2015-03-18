using System;
using System.Reflection;

namespace Amigo.ORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute: Attribute
    {
        public Type PropertyType { get; set; }
        public string PropertyName { get; set; }
        public bool PrimaryKey { get; set; }
        public bool Unique { get; set; }
        public bool Index { get; set; }
        public bool AllowNull { get; set; }
        public string ColumnName { get; set; }
        public ForeignKeyAttribute ForeignKey { get; set; }


        public ColumnAttribute(string name = null, bool primaryKey = false, bool unique = false, bool index = false, bool allowNull = false)
        {
            PrimaryKey = primaryKey;
            ColumnName = name;
            Unique = unique;
            AllowNull = allowNull;
            Index = index;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ForeignKeyAttribute: Attribute
    {
        public Type PropertyType { get; set; }
        public string PropertyName { get; set; }
        public bool AllowNull { get; set; }
        public string ColumnName { get; set; }
        public Type RelatedType { get; set; }
        public string RelatedTypeName { get; set; }


        public ForeignKeyAttribute(string name = null, bool allowNull = false)
        {
            AllowNull = allowNull;
            ColumnName = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ManyToManyAttribute: Attribute
    {
        public Type PropertyType { get; set; }
        public string PropertyName { get; set; }
        public Type RelatedType { get; set; }
        public Type ThroughModel { get; set; }
        public Type ForModel { get; set; }
        public string RelatedTypeName { get; set; }
        public bool AllowNull { get; set; }

        // dunno if we will copy this idea yet from django.
        // it's here more as a note to self.. maybe we use it?
        public ThroughFieldsAttribute ThroughFields { get; set; }


        public ManyToManyAttribute(string name = null, bool allowNull = false, Type forModel = null)
        {
            AllowNull = allowNull;
            ForModel = forModel;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ThroughFieldsAttribute: Attribute
    {
        public string[] Fields { get; set; }

        public ThroughFieldsAttribute(params string[] fields)
        {
            Fields = fields;
        }
    }


}

