using System;
using System.Threading.Tasks;
using System.Reflection;
using SQLitePCL.pretty;
using Amigo.ORM.Utils;


namespace Amigo.ORM.Engines
{
    public partial class SqliteEngine
    {
        public async Task Delete(object model)
        {
            var metaModel = Meta.MetaModelForModel(model);
            var sql = CreateDeleteSql(model, metaModel);

            await Connection.ExecuteAsync(sql);
        }

        public string CreateDeleteSql(object model, MetaModel metaModel = null)
        {
            if (metaModel == null)
                metaModel = Meta.MetaModelForModel(model);
            
            var modelType = model.GetType();
            var tableName = metaModel.Table.TableName;
            var pkColumnName = metaModel.PrimaryKey.ColumnName;
            var pkPropertyName = metaModel.PrimaryKey.PropertyName;
            var pkValue = modelType.GetRuntimeProperty(pkPropertyName).GetValue(model, null);

            var sql = string.Format("DELETE FROM '{0}' WHERE {1} = {2}", 
                          tableName, pkColumnName, Utils.Utils.EscapeSqlValue(pkValue));

            return sql;
        }

        public async Task DeleteManyToMany(SessionModelAction action)
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
            // no source pk? bail.
            if (((int)(sourcePkValue ?? 0)) == 0)
                return;

            // no target pk? bail.
            if (((int)(targetPkValue ?? 0)) == 0)
                return;

            if (action.ManyToMany.ForModel != null)
            {
                // this is just wrapping `something else`.
                // we will kill the wrapper
                // and leave the `something else` in tact.
                await Delete(action.TargetModel);
            }

            var sql = string.Format("DELETE FROM '{0}' WHERE {1} = {2} AND {3} = {4};",
                          pivotTableName, 
                          sourcePivotColumnName, Utils.Utils.EscapeSqlValue(sourcePkValue), 
                          targetPivotColumnName, Utils.Utils.EscapeSqlValue(targetPkValue)
                      );

            await Connection.ExecuteAsync(sql);
        }
    }
}

