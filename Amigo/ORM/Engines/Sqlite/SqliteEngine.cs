using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using SQLitePCL.pretty;

using Amigo.ORM;
using Amigo.ORM.Utils;

namespace Amigo.ORM.Engines
{
    public partial class SqliteEngine : IAlchemyEngine
    {

        public MetaData Meta { get; set; }
        public string Path { get; set; }

        public IAsyncDatabaseConnection Connection { get; set; }
        public bool InTransaction { get; private set; }


        public SqliteEngine(string path = ":memory:")
        {
            Path = path;
            //var connection = SQLite3.Open(path);
        }

        public void Commit(Session session)
        {
            //var adds = CreateAllInsertSql(meta, session);
            //session.Adds;
            //session.Removes;
            //session.Updates;
        }

        public async Task Begin()
        {
            InTransaction = true;

            if (Connection == null)
                Connect();

            await Connection.ExecuteAsync("BEGIN TRANSACTION;");
        }

        public async Task<int> LastInsertId()
        {
            var result = await Connection.Query("SELECT last_insert_rowid();").Select(x => x[0].ToInt()).ToList();
            return result[0];
        }

        public async Task Commit()
        {
            await Connection.ExecuteAsync("COMMIT TRANSACTION;");
        }

        public async Task Rollback()
        {
            await Connection.ExecuteAsync("ROLLBACK TRANSACTION;");
        }

        public MetaModel MetaModelForModel(object model)
        {
            MetaModel metaModel;
            var modelTypeName = model.GetType().Name;
            Meta.Tables.TryGetValue(modelTypeName, out metaModel);

            if (metaModel == null)
                throw new Exception(string.Format("Unable to locate registered model for type '{0}'", modelTypeName));

            return metaModel;
        }

        public object ConvertSQLiteType(IResultSetValue value)
        {
            object result = null;

            switch (value.SQLiteType)
            {
            case SQLiteType.Integer:
                result = value.ToInt();
                break;

            case SQLiteType.Float:
                result = value.ToDouble();
                break;
               
            case SQLiteType.Null:
                result = null;
                break;
               
            case SQLiteType.Text:
                result = value.ToString();
                break;
               
            case SQLiteType.Blob:
                result = value.ToBlob();
                break;
            }

            return result;
        }

        public T ParseSqlRow<T>(IReadOnlyList<IResultSetValue> row, MetaModel tableModel, QuerySet<T> query)
        {
            var model = (T)Activator.CreateInstance(typeof(T));
            var modelType = model.GetType();

            var contractedColumns = tableModel.Columns.Count;
            var resultColumns = row.Count;
            var currentRowColumn = 0;

            for (var i = 0; i < contractedColumns; i++)
            {
                
                var metaColumn = tableModel.Columns[i];
                var propertyName = metaColumn.PropertyName;
                var data = row[currentRowColumn];

                if (metaColumn.ForeignKey != null)
                {
                    var fkMeta = metaColumn.ForeignKey;

                    MetaModel fkMetaTable;
                    string fkRelatedName = fkMeta.ColumnName;
                    Meta.Tables.TryGetValue(fkMeta.RelatedTypeName, out fkMetaTable);

                    if (fkMetaTable == null || query.RelatedColumns.Contains(fkRelatedName) == false)
                        continue;

                    var fkColumnsCount = fkMetaTable.Columns.Count;
                    var fkModelType = fkMeta.RelatedType;
                    var fkModel = Activator.CreateInstance(fkModelType);

                    for (var fkIndex = 0; fkIndex < fkColumnsCount; fkIndex++)
                    {
                        var fkColumn = fkMetaTable.Columns[fkIndex];
                        var fkPropertyName = fkColumn.PropertyName;
                        var fkData = row[currentRowColumn];

                        fkModelType.GetRuntimeProperty(fkPropertyName).SetValue(fkModel, ConvertSQLiteType(fkData));
                        currentRowColumn++;
                    }

                    modelType.GetRuntimeProperty(propertyName).SetValue(model, fkModel);
                }
                else
                {
                    modelType.GetRuntimeProperty(propertyName).SetValue(model, ConvertSQLiteType(data));
                }

                currentRowColumn++;
            }
                
            return model;
        }
           

        public async Task<List<object>> QueryAsync(string sql, Func<object, object> parseRow = null)
        {
            var db = Connection;
            var rows = await db.Query(sql).Select(parseRow).ToList();
            return (List<object>)rows;
        }

        public async Task<List<T>> QueryAsync<T>(string sql, Func<object, T> parseRow = null)
        {
            var db = Connection;
            var rows = await db.Query(sql).Select(parseRow).ToList();
            return (List<T>)rows;
        }

        public async Task<T> ExecuteAsync<T>(QuerySet<T>query)
        {
            var rows = await ExecuteListAsync<T>(query);
            return rows.FirstOrDefault();
        }

        public async Task<List<T>> ExecuteListAsync<T>(QuerySet<T>query)
        {
            var sql = CreateQuerySetSql(query);

            string tableModelName = typeof(T).GetTypeInfo().Name;
            MetaModel tableMetaModel;
            Meta.Tables.TryGetValue(tableModelName, out tableMetaModel);

            if (tableMetaModel == null)
                throw new Exception(string.Format("Unable to locate registered table for '{0}'", tableModelName));


            Func<object, T> parseRow = delegate(object result) {
                var row = (IReadOnlyList<IResultSetValue>)result;
                return this.ParseSqlRow<T>(row, tableMetaModel, query);
            };

            var rows = await QueryAsync<T>(sql, parseRow);
            return rows;
        }

        public List<ColumnAttribute>GetForeignKeyColumns<T>(MetaModel model, QuerySet<T> query)
        {
            var columns = new List<ColumnAttribute>();

            if (query.RelatedColumns.Count > 0)
            {
                var foreignKeys = (from c in model.Columns
                                               where c.ForeignKey != null
                                               select c).ToList();


                var validForeignKeys = (from c in foreignKeys
                                                    where query.RelatedColumns.Contains(c.ForeignKey.ColumnName)
                                                    select c).ToList();

                columns = validForeignKeys;
            }

            return columns;
        }

        public List<string> QueryFieldsForModel(MetaModel model)
        {
            // returns all columns not foreignKey fields.
            var tableName = model.Table.TableName;
            var columns = (from c in model.Columns
                                    where c.ForeignKey == null
                                    select c);

            var sqlFields = columns.Select((value, index) => 
                string.Format("{0}.{1}", tableName, value.ColumnName)
                            );

            return sqlFields.ToList();
        }

        public List<string> QueryFieldsForModel(MetaModel model, List<String>fields)
        {
            // returns all columns not foreignKey fields.
            var tableName = model.Table.TableName;

            var sqlFields = fields.Select((value, index) => 
                string.Format("{0}.{1}", tableName, value)
                            );

            return sqlFields.ToList();
        }

        public async Task CreateAllAsync()
        {
            var sql = CreateAllSql().ToString();

            using (var db = Connect())
            {
                await db.ExecuteAllAsync(sql);
            }
        }
            
        public string SqlTypeForPropertyType(Type type)
        {
            /*
             *
             * NULL. The value is a NULL value.
             * INTEGER. The value is a signed integer, stored in 1, 2, 3, 4, 6, or 8 bytes depending on the magnitude of the value.
             * REAL. The value is a floating point value, stored as an 8-byte IEEE floating point number.
             * TEXT. The value is a text string, stored using the database encoding (UTF-8, UTF-16BE or UTF-16LE).
             * BLOB. The value is a blob of data, stored exactly as it was input.
             */

            if (type == null)
            {
                return "NULL";
            }

            string result;


            switch (type.Name.ToLower())
            {

            case "string":
                result = "TEXT";
                break;
               
            case "int":
            case "int32":
            case "int64":
                result = "INTEGER";
                break;
               
            case "float":
            case "double":
                result = "REAL";
                break;
            
            default:
                result = "BLOB";
                break;
            }

            return result;
        }
    }
}

