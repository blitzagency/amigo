using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amigo.ORM.Engines;

namespace Amigo.ORM.Utils
{
    public class MetaData
    {
        public Dictionary<string, MetaModel> Tables { get; set; }
        public IAlchemyEngine Engine { get; set; }

        public Dictionary<string, MetaModel>.ValueCollection Models { 
            get {
                return Tables.Values;
            } 
        }

        public MetaData()
        {
            Tables = new Dictionary<string, MetaModel>();
        }

        public virtual MetaModel RegisterModel<T>()
        {
            var meta = new MetaModel(typeof(T));
            Tables.Add(meta.Table.TypeName, meta);

            VerifyForeignKeys();
            VerifyManyToMany();

            return meta;
        }

        public void VerifyForeignKeys()
        {
            var modelsWithForeignKeys = (from m in Models
                                                  where m.ForeignKeys.Count > 0
                                                  select m).ToList();

            foreach (var model in modelsWithForeignKeys)
            {
                foreach (var fk in model.ForeignKeys)
                {
                    MetaModel relatedModel;
                    Tables.TryGetValue(fk.RelatedTypeName, out relatedModel);

                    if (relatedModel == null)
                        continue;

                    var pk = relatedModel.PrimaryKey;

                    var column = new ColumnAttribute {
                        PropertyType = pk.PropertyType,
                        PropertyName = fk.PropertyName,
                        AllowNull = fk.AllowNull,
                        Index = true,
                        ColumnName = string.Format("{0}_id", fk.ColumnName),
                        ForeignKey = fk
                    };

                    var existingColumn = (from m in model.Columns
                                                         where m.ColumnName == string.Format("{0}_id", fk.ColumnName)
                                                         select m).FirstOrDefault();

                    if (existingColumn == null)
                        model.Columns.Add(column);
                }
            }
        }

        public void VerifyManyToMany()
        {
            var modelsWithManyToMany = (from m in Models
                                                 where m.ManyToMany.Count > 0
                                                 select m).ToList();

            foreach (var model in modelsWithManyToMany)
            {
                foreach (var m2m in model.ManyToMany)
                {
                    MetaModel relatedModel1;
                    MetaModel relatedModel2;
                    MetaModel forModelMeta; // this may or may not be used below.
                    ColumnAttribute primaryKey;
                    ColumnAttribute column1;
                    ColumnAttribute column2;

                    Tables.TryGetValue(m2m.RelatedTypeName, out relatedModel1);
                    Tables.TryGetValue(m2m.PropertyName, out relatedModel2);

                    if (relatedModel1 == null || relatedModel2 == null)
                        continue;


                    var meta = new MetaModel(relatedModel1.ModelType, relatedModel2.ModelType);

                    primaryKey = new ColumnAttribute {
                        PrimaryKey = true,
                        PropertyType = typeof(int),
                        PropertyName = null,
                        ColumnName = "id",
                        Index = true,
                    };

                    column1 = new ColumnAttribute {
                        PropertyType = relatedModel1.PrimaryKey.PropertyType,
                        PropertyName = null,
                        ColumnName = string.Format("{0}_id", relatedModel1.Table.TableName),
                        Index = true,
                    };

                    column2 = new ColumnAttribute {
                        PropertyType = relatedModel2.PrimaryKey.PropertyType,
                        PropertyName = null,
                        ColumnName = string.Format("{0}_id", relatedModel2.Table.TableName),
                        Index = true,
                    };

                    if (m2m.ForModel != null)
                    {
                        
                        Tables.TryGetValue(m2m.ForModel.Name, out forModelMeta);

                        if (forModelMeta == null)
                            continue;
                        

                        if (m2m.ThroughFields == null)
                        {
                            
                            var data = (from x in relatedModel2.ForeignKeys
                                                             where x.PropertyType == m2m.ForModel
                                                             select x).ToList();
                            
                            m2m.ThroughFields = new ThroughFieldsAttribute(data.Select(x => x.ColumnName).ToArray());
                        }
                    }

                    meta.Columns.Add(primaryKey);
                    meta.Columns.Add(column1);
                    meta.Columns.Add(column2);


                    if (Tables.ContainsKey(meta.Table.TableName) == false)
                        Tables.Add(meta.Table.TableName, meta);
                }
            }
        }

        public MetaModel MetaModelForModel(object model)
        {
            MetaModel metaModel;
            var modelTypeName = model.GetType().Name;
            Tables.TryGetValue(modelTypeName, out metaModel);

            if (metaModel == null)
                throw new Exception(string.Format("Unable to locate registered model for type '{0}'", modelTypeName));

            return metaModel;
        }

        public async Task CreateAllAsync(IAlchemyEngine engine)
        {
            // the engine can do nothing until it's Metadata is set.
            // ensure it is.

            if (engine.Meta == null)
                engine.Meta = this;

            await engine.CreateAllAsync();
        }
    }

    public class MetaModel
    {
        public TableAttribute Table { get; set; }
        public List<ColumnAttribute> Columns { get; set; }
        public List<ForeignKeyAttribute> ForeignKeys { get; set; }
        public List<ManyToManyAttribute> ManyToMany { get; set; }
        public ColumnAttribute PrimaryKey { get; set; }
        public List<Type> ModelTypes { get; set; }

        public Type ModelType { 
            get {
                if (ModelTypes.Count > 0)
                    return ModelTypes[0];

                return null;
            }

            set {
                if (ModelTypes.Count > 0)
                    ModelTypes[0] = value;
                else
                    ModelTypes.Add(value);
                        
            }
        }
            
        public MetaModel(Type modelType)
        {
            ModelTypes = new List<Type>();

            modelType = EnsureType(modelType);
            ModelTypes.Add(modelType);

            Initialize();
        }

        public MetaModel(Type modelType1, Type modelType2)
        {
            // This case is likely a ManyToMany situation
            // we don't do any default initialization 
            // of columns as they columns and table will should be
            // provided manually. The Table initialization can proceed
            // as it knows how to deal with multi types via 
            // GetMultiTableAttribute

            modelType1 = EnsureType(modelType1);
            modelType2 = EnsureType(modelType2);

            ModelTypes = new List<Type>();
            ModelTypes.Add(modelType1);
            ModelTypes.Add(modelType2);

            Initialize();
        }

        public Type EnsureType(Type type)
        {
            var nullableType = Nullable.GetUnderlyingType(type);

            if (nullableType != null)
                return nullableType;

            return type;
        }

        public void Initialize()
        {
            InitializeTable();
            InitializeColumns();
        }

        public void InitializeTable()
        {
            if (ModelTypes.Count == 1)
                Table = GetTableAttribute();
            else
                Table = GetMultiTableAttribute();
        }

        public void InitializeColumns()
        {
            if (ModelTypes.Count == 1)
            {
                Columns = GetColumnAttributes();
                ForeignKeys = GetForeignKeyAttributes();
                ManyToMany = GetManyToManyAttributes();
            }
            else
            {
                Columns = new List<ColumnAttribute>();
                ForeignKeys = new List<ForeignKeyAttribute>();
                ManyToMany = new List<ManyToManyAttribute>();
            }
        }

        public TableAttribute GetTableAttribute()
        {
            var attr = (TableAttribute)System.Reflection.CustomAttributeExtensions
                .GetCustomAttribute(ModelType.GetTypeInfo(), typeof(TableAttribute), true);

            attr.TypeName = ModelType.GetTypeInfo().Name;
            attr.TableName = attr.TableName ?? attr.TypeName;
            attr.TableName = attr.TableName.ToLower();

            return attr;
        }

        public TableAttribute GetMultiTableAttribute()
        {
            List<TableAttribute> attrs = new List<TableAttribute>();

            foreach (var each in ModelTypes)
            {
                var attr = (TableAttribute)System.Reflection.CustomAttributeExtensions
                    .GetCustomAttribute(each.GetTypeInfo(), typeof(TableAttribute), true);

                attr.TypeName = each.GetTypeInfo().Name;
                attr.TableName = attr.TableName ?? attr.TypeName;
                attr.TableName = attr.TableName.ToLower();

                attrs.Add(attr);
            }

            var table = new TableAttribute {
                TableName = string.Join("_", attrs.Select(x => x.TableName))
            };
           
            return table;
        }

        public List<ColumnAttribute> GetColumnAttributes()
        {
            var results = new List<ColumnAttribute>();
            var props = from p in ModelType.GetRuntimeProperties()
                                 where ((p.GetMethod != null && p.GetMethod.IsPublic) || (p.SetMethod != null && p.SetMethod.IsPublic) || (p.GetMethod != null && p.GetMethod.IsStatic) || (p.SetMethod != null && p.SetMethod.IsStatic))
                                 select p;


            foreach (var each in props)
            {
                var cols = each.GetCustomAttributes(typeof(ColumnAttribute), true).ToList();
                var use = cols.Count() == 1;

                if (use)
                {
                    var attr = (ColumnAttribute)cols[0];
                    attr.PropertyType = EnsureType(each.PropertyType);
                    attr.PropertyName = each.Name;
                    attr.ColumnName = attr.ColumnName ?? attr.PropertyName;
                    attr.ColumnName = attr.ColumnName.ToLower();

                    if (attr.PrimaryKey)
                    {
                        PrimaryKey = attr;
                    }

                    results.Add(attr);
                }
            }
            return results;

        }

        public List<ForeignKeyAttribute> GetForeignKeyAttributes()
        {
            var results = new List<ForeignKeyAttribute>();
            var props = from p in ModelType.GetRuntimeProperties()
                                 where ((p.GetMethod != null && p.GetMethod.IsPublic) || (p.SetMethod != null && p.SetMethod.IsPublic) || (p.GetMethod != null && p.GetMethod.IsStatic) || (p.SetMethod != null && p.SetMethod.IsStatic))
                                 select p;


            foreach (var each in props)
            {
                var cols = each.GetCustomAttributes(typeof(ForeignKeyAttribute), true).ToList();
                var use = cols.Count() > 0;

                if (use)
                {
                    var attr = (ForeignKeyAttribute)cols[0];
                    attr.PropertyType = each.PropertyType;
                    attr.PropertyName = each.Name;
                    attr.RelatedType = each.PropertyType;
                    attr.RelatedTypeName = attr.RelatedType.GetTypeInfo().Name;
                    attr.ColumnName = attr.ColumnName ?? attr.PropertyName;
                    attr.ColumnName = attr.ColumnName.ToLower();

                    results.Add(attr);
                }
            }
            return results;

        }

        public List<ManyToManyAttribute> GetManyToManyAttributes()
        {
            // TODO : capture related
            var results = new List<ManyToManyAttribute>();
            var props = from p in ModelType.GetRuntimeProperties()
                                 where ((p.GetMethod != null && p.GetMethod.IsPublic) || (p.SetMethod != null && p.SetMethod.IsPublic) || (p.GetMethod != null && p.GetMethod.IsStatic) || (p.SetMethod != null && p.SetMethod.IsStatic))
                                 select p;

            var name = ModelType.Name;

            foreach (var each in props)
            {
                var cols = each.GetCustomAttributes(typeof(ManyToManyAttribute), true).ToList();
                var use = cols.Count > 0;
                var type = each.PropertyType.GetTypeInfo();
                var isGenericList = (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>)));

                if (use && isGenericList)
                {
                    var attr = (ManyToManyAttribute)cols[0];
                    attr.PropertyType = type.GenericTypeArguments.Single();
                    attr.PropertyName = attr.PropertyType.GetTypeInfo().Name;
                    attr.RelatedType = ModelType;
                    attr.RelatedTypeName = Table.TypeName;

                    results.Add(attr);
                }
            }
            return results;

        }
    }
}

