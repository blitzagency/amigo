using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using SQLitePCL.pretty;
using Amigo.ORM.Utils;


namespace Amigo.ORM.Engines
{
    public partial class SqliteEngine
    {
        public async Task Insert(object model)
        {
            var metaModel = Meta.MetaModelForModel(model);

            // ensure any foreign keys on this model are saved prior 
            // to inserting the row.
            await InsertVerifyForeignKeys(model, metaModel);

            var sql = CreateInsertSql(model, metaModel);

            await Connection.ExecuteAsync(sql);
            var id = await LastInsertId();

            model.GetType().GetRuntimeProperty(metaModel.PrimaryKey.PropertyName).SetValue(model, id);
        }

        public async Task InsertManyToMany(SessionModelAction action)
        {
            var pivotTableName = string.Format("{0}_{1}",
                                     action.SourceMetaModel.Table.TableName, action.TargetMetaModel.Table.TableName);
            
            Func<MetaModel, object, object> getPrimaryKeyValue = delegate(MetaModel metaModel, object model) {
                return metaModel.ModelType
                                .GetRuntimeProperty(metaModel.PrimaryKey.PropertyName)
                                .GetValue(model, null);
            };

            var sourceTableName = action.SourceMetaModel.Table.TableName;
            var sourcePkValue = getPrimaryKeyValue(action.SourceMetaModel, action.SourceModel);
            var sourcePivotColumnName = string.Format("{0}_id", sourceTableName);
            
            var targetTableName = action.TargetMetaModel.Table.TableName;
            var targetPkValue = getPrimaryKeyValue(action.TargetMetaModel, action.TargetModel);
            var targetPivotColumnName = string.Format("{0}_id", targetTableName);

            // are the targe and source PKs null?
            // (handles nullable types here as well)
            if (((int)(sourcePkValue ?? 0)) == 0)
            {
                await Insert(action.SourceModel);
                sourcePkValue = getPrimaryKeyValue(action.SourceMetaModel, action.SourceModel);
            }
            
            if (((int)(targetPkValue ?? 0)) == 0)
            {
                await Insert(action.TargetModel);
                targetPkValue = getPrimaryKeyValue(action.TargetMetaModel, action.TargetModel);
            }
                
            var sql = string.Format("INSERT INTO '{0}' ({1}, {2}) VALUES ({3}, {4})",
                          pivotTableName, 
                          sourcePivotColumnName, targetPivotColumnName,
                          Utils.Utils.EscapeSqlValue(sourcePkValue), 
                          Utils.Utils.EscapeSqlValue(targetPkValue)
                      );
            await Connection.ExecuteAsync(sql);
        }

        public async Task InsertVerifyForeignKeys(object model, MetaModel metaModel = null)
        {
            if (metaModel == null)
                metaModel = Meta.MetaModelForModel(model);

            var modelType = model.GetType();

            foreach (var each in metaModel.Columns.Where(x => x.ForeignKey != null))
            {

                MetaModel fkModel;
                string propertyName = each.PropertyName;
                ForeignKeyAttribute fk = each.ForeignKey;
                string fkPropertyName;
                object value = modelType.GetRuntimeProperty(propertyName).GetValue(model, null);

                if (value == null)
                    continue;

                Meta.Tables.TryGetValue(fk.RelatedTypeName, out fkModel);

                if (fkModel == null)
                    continue;

                fkPropertyName = fkModel.PrimaryKey.PropertyName;

                var fkValue = value.GetType().GetRuntimeProperty(fkPropertyName).GetValue(value, null);

                // Does this object need to be saved first?
                // The fkValue could be `null` aka int? or some other nullable type
                // or it could be an int, in which case it's default empty value
                // will be `0`. Handle both cases. This will mess up the user
                // that chooses to intentionally use 0 as a manual foreign key value.
                // Don't know that anyone would ever do that, perhaps we are naive?
                if (((int)(fkValue ?? 0)) == 0)
                {
                    await Insert(value);
                }
            }
        }

        public string CreateInsertSql(object model, MetaModel metaModel = null)
        {
            if (metaModel == null)
                metaModel = Meta.MetaModelForModel(model);
            
            var tableName = metaModel.Table.TableName;
            var columnNames = metaModel.Columns
                .Where(x => 
                    // http://www.sqlite.org/faq.html#q1
                    // A column declared INTEGER PRIMARY KEY will autoincrement
                    !(x.PrimaryKey && x.PropertyType.Name.ToLower().StartsWith("int")))
                .Select(x => 
                    string.Format("'{0}'", x.ColumnName)).ToList();

            // SQLite 3.7.11+ (iOS 8 is 3.7.13)
            var preamble = string.Format("INSERT INTO '{0}' ({1}) VALUES ", 
                               tableName, string.Join(",", columnNames));

            var rows = new List<string>();

            var modelValues = new List<object>();
            var modelType = model.GetType();

            // begin columns
            foreach (var column in  metaModel.Columns)
            {
                if (column.PrimaryKey && column.PropertyType.Name.ToLower().StartsWith("int"))
                {
                    // http://www.sqlite.org/faq.html#q1
                    // A column declared INTEGER PRIMARY KEY will autoincrement
                    continue;
                }

                string propertyName = column.PropertyName;
                object value = modelType.GetRuntimeProperty(propertyName).GetValue(model, null);

                if (column.ForeignKey != null)
                {
                    var fk = column.ForeignKey;

                    if (value == null)
                    {
                        modelValues.Add(SqlTypeForPropertyType(null));
                        continue;
                    }


                    MetaModel fkModel;
                    Meta.Tables.TryGetValue(fk.RelatedTypeName, out fkModel);

                    if (fkModel == null)
                        continue;

                    var fkPropertyName = fkModel.PrimaryKey.PropertyName;

                    // Reach into the related model and get the value of w/e it's 
                    // primary key is.
                    value = value.GetType().GetRuntimeProperty(fkPropertyName).GetValue(value, null);
                }
                    
                modelValues.Add(Utils.Utils.EscapeSqlValue(value));
            }
            // end columns

            rows.Add(string.Format("({0})", string.Join(", ", modelValues)));
            var sql = string.Format("{0} {1};", preamble, string.Join(", ", rows));

            return sql;
        }
    }
}

