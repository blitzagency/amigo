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
        public async Task Update(object model)
        {
            var metaModel = Meta.MetaModelForModel(model);
            var sql = CreateUpdateSql(model, metaModel);

            await Connection.ExecuteAsync(sql);
        }

        public string CreateUpdateSql(object model, MetaModel metaModel = null)
        {
            if (metaModel == null)
                metaModel = Meta.MetaModelForModel(model);

            var tableName = metaModel.Table.TableName;
            var columns = metaModel.Columns
                .Where(x => 
                    // http://www.sqlite.org/faq.html#q1
                    // A column declared INTEGER PRIMARY KEY will autoincrement
                    !(x.PrimaryKey && x.PropertyType.Name.ToLower().StartsWith("int")))
                .ToList();

            var modelType = model.GetType();
            var pkColumnName = metaModel.PrimaryKey.ColumnName;
            var pkPropertyName = metaModel.PrimaryKey.PropertyName;
            var pkValue = Utils.Utils.EscapeSqlValue(modelType.GetRuntimeProperty(pkPropertyName).GetValue(model, null));

            var preamble = string.Format("UPDATE '{0}' SET", tableName);
            var values = new List<string>();

            foreach (var column in  columns)
            {
                var value = modelType.GetRuntimeProperty(column.PropertyName).GetValue(model, null);
                var sqlString = string.Format("{0} = {1}", column.ColumnName, Utils.Utils.EscapeSqlValue(value));
                values.Add(sqlString);
            }

            var sql = string.Format("{0} {1} WHERE {2} = {3}", 
                          preamble, string.Join(", ", values), pkColumnName, pkValue);
            

            return sql;
        }
    }
}