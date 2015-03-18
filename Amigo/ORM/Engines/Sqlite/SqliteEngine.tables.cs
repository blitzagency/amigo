using System;
using System.Collections.Generic;
using Amigo.ORM.Utils;

namespace Amigo.ORM.Engines
{
    public partial class SqliteEngine
    {
        public string CreateAllSql()
        {
            var sections = new List<string>();
            sections.Add(CreateAllTablesSql());
            sections.Add(CreateAllIndexesSql());

            var sql = string.Join("", sections);
            return sql;
        }

        public string CreateAllIndexesSql()
        {
            var indexesSql = new List<string>();

            foreach (var model in Meta.Models)
            {
                indexesSql.Add(CreateIndexSql(model.Table, model.Columns));
            }

            var sql = string.Join("", indexesSql);
            return sql;
        }

        public string CreateAllTablesSql()
        {
            var tablesSql = new List<string>();

            foreach (var model in Meta.Models)
            {
                tablesSql.Add(CreateTableSql(model.Table, model.Columns));
                //tablesSql.Add(CreateManyToManyTableSql(model));
            }
                

            var sql = string.Join("", tablesSql);
            return sql;
        }

        public string CreateIndexSql(TableAttribute table, List<ColumnAttribute> columns)
        {
            var indexSql = new List<string>();

            var on_sql = string.Format("ON {0}", table.TableName);
            string preamble_sql;
            string sql;

            foreach (var each in columns)
            {
                if (!each.Unique && !each.Index)
                    continue;

                preamble_sql = each.Unique ? "CREATE UNIQUE INDEX IF NOT EXISTS" :
                    "CREATE INDEX IF NOT EXISTS";

                var indexName = string.Format(
                                    "{0}_{1}_idx", 
                                    table.TableName.ToLower(), 
                                    each.ColumnName.ToLower()
                                );

                sql = string.Format(
                    "{0} {1} {2} ({3} ASC);",
                    preamble_sql,
                    indexName,
                    on_sql,
                    each.ColumnName
                );

                indexSql.Add(sql);
            }

            sql = string.Join(" ", indexSql);
            return sql;
        }

        public string CreateTableSql(TableAttribute table, List<ColumnAttribute> columns)
        {
            var preamble = String.Format("CREATE TABLE IF NOT EXISTS {0}", table.TableName);
            var columnsSql = new List<string>();

            foreach (var each in columns)
            {
                columnsSql.Add(CreateColumnSql(each));
            }

            var sql = string.Format("{0} ({1});", preamble, string.Join(",", columnsSql));
            return sql;
        }

        public string CreateColumnSql(ColumnAttribute column)
        {
            //ID INT PRIMARY KEY      NOT NULL,
            var columnName = column.ColumnName;
            var columnType = SqlTypeForPropertyType(column.PropertyType);
            var options = new List<string>();

            if (column.PrimaryKey)
            {
                options.Add("PRIMARY KEY");
                options.Add("NOT NULL");
            }
            else
            {
                options.Add(column.AllowNull ? "NULL" : "NOT NULL");
            }

            var sql = String.Format("{0} {1} {2}", columnName, columnType, string.Join(" ", options));
            return sql;
        }
    }
}

