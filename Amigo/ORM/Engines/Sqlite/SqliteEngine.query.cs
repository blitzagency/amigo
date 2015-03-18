using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Amigo.ORM.Utils;

namespace Amigo.ORM.Engines
{
    public partial class SqliteEngine
    {
        
        public string CreateQuerySetSql<T>(QuerySet<T>query)
        {
            if (Meta == null)
                throw new Exception("Unable to proceed, no MetaData has been set.");

            var typeName = typeof(T).GetTypeInfo().Name;
            MetaModel model;

            Meta.Tables.TryGetValue(typeName, out model);

            if (model == null)
            {
                var error = string.Format("No registered model for type '{0}'", typeName);
                throw new Exception(error);
            }

            var sqlFields = CreateQueryFields<T>(model, query);
            var sqlTables = CreateQueryTables<T>(model, query);
            var sqlJoins = CreateQueryJoins<T>(model, query);
            var sqlFilters = CreateQueryFilters<T>(model, query);
            var sqlOrder = CreateQueryOrder<T>(model, query);

            var sql = string.Format("SELECT {0} FROM {1}", 
                          string.Join(", ", sqlFields),
                          string.Join(", ", sqlTables)
                      );

            if (sqlJoins.Count > 0)
            {
                sql = string.Format("{0} {1}",
                    sql,
                    string.Join(" ", sqlJoins)
                );
            }

            if (sqlFilters != null)
            {
                sql = string.Format("{0} WHERE {1}",
                    sql,
                    sqlFilters
                );
            }

            if (sqlOrder != null)
            {
                sql = string.Format("{0} ORDER BY {1}",
                    sql,
                    sqlOrder
                );
            }

            return sql + ";";
        }

        public string CreateQueryOrder<T>(MetaModel model, QuerySet<T> query)
        {
            if (query.Orders == null)
                return null;

            query.Orders.DefaultTable = model.Table.TableName;
            return query.Orders.ToString();
        }

        public string CreateQueryFilters<T>(MetaModel model, QuerySet<T> query)
        {
            if (query.Filters == null)
                return null;

            query.Filters.DefaultTable = model.Table.TableName;
            return query.Filters.ToString();
        }

        public List<string> CreateQueryJoins<T>(MetaModel model, QuerySet<T> query)
        {
            // LEFT JOIN Reservations ON Customers.CustomerId = Reservations.CustomerId;
            // 
            var sqlJoins = new List<string>();

            foreach (var c in GetForeignKeyColumns(model, query))
            {
                MetaModel relatedModel;
                var pk = model.PrimaryKey;
                var pkTable = model.Table.TableName;
                var pkColumn = c.ColumnName; // the name of the column that represents the id of the FK Table

                Meta.Tables.TryGetValue(c.ForeignKey.RelatedTypeName, out relatedModel);

                if (relatedModel == null)
                    continue;

                var fkTable = relatedModel.Table.TableName;
                var fkColumn = relatedModel.PrimaryKey.ColumnName;

                // if we are using "Fields" we will likely need to change this
                // to a LEFT JOIN since we have the possibilities of NULL values?
                sqlJoins.Add(string.Format(
                    "INNER JOIN {0} AS {0} ON {1}.{2} = {0}.{3}", 
                    fkTable, pkTable, pkColumn, fkColumn)
                );
            }

            if (query.PivotModel != null)
            {
                var m2m_joins = CreateManyToManyJoins<T>(model, query);
                sqlJoins.AddRange(m2m_joins);
            }
                
            return sqlJoins;
        }

        public List<string> CreateManyToManyJoins<T>(MetaModel model, QuerySet<T> query)
        {
            List<string> sqlJoins = new List<string>();
            
            // the tables for many to many are constructed as {{ model_type_table_name }}_{{ m2m_type_table_name}}
            // the model_type is first, in this case {{ query.PivotModel_table_name }}_{{ T_table_name }}

            /* -- FOR MODEL M2M Query
             * SELECT publicationmeta.id, publicationmeta.publication_order, publication.id, publication.label 
             * FROM publicationmeta AS publicationmeta
             * INNER JOIN publication AS publication ON publication.id = publicationmeta.publication_id
             * INNER JOIN post_publicationmeta AS post_publicationmeta ON post_publicationmeta.publicationmeta_id = publicationmeta.id
             * WHERE post_publicationmeta.post_id = 1
             */

            MetaModel metaTable1;
            MetaModel metaPivotTable;
            string pivotTableName;


            Meta.Tables.TryGetValue(query.PivotModel.GetType().Name, out metaTable1);

            if (metaTable1 == null)
                return sqlJoins;

            var m2m = metaTable1.ManyToMany.FirstOrDefault(x => x.PropertyType == model.ModelType);

            if (m2m == null)
                return sqlJoins;

            pivotTableName = string.Format("{0}_{1}", metaTable1.Table.TableName, model.Table.TableName);
            Meta.Tables.TryGetValue(pivotTableName, out metaPivotTable);

            if (metaPivotTable == null)
                return sqlJoins;
           
            var modelTableName = model.Table.TableName;
            var modelTableColumn = model.PrimaryKey.ColumnName;
            var pivotTableColumn = string.Format("{0}_id", modelTableName);
            var metaTable1TableName = metaTable1.Table.TableName;
            var metaTable1PkName = metaTable1.PrimaryKey.ColumnName;

            // INNER JOIN post_publication AS post_publication ON post_publication.publication_id = publication.id
            sqlJoins.Add(string.Format(
                "INNER JOIN {0} AS {0} ON {0}.{1} = {2}.{3}", 
                pivotTableName, pivotTableColumn, modelTableName, modelTableColumn)
            );

            if (m2m.ForModel != null)
            {
                // INNER JOIN publication AS publication ON publication.id = publicationmeta.publication_id
                MetaModel metaForModel;
                Meta.Tables.TryGetValue(m2m.ForModel.Name, out metaForModel);

                if (metaForModel != null)
                {   
                    var forModelTableName = metaForModel.Table.TableName;
                    var forModelPkName = metaForModel.PrimaryKey.ColumnName;

                    modelTableColumn = string.Format("{0}_id", forModelTableName);

                    // if we used ThroughFieldAttributes, this is where we would need em.
                    // loop though em and add inner joins based on whatever they were.
                    // right now we don't need them, so... there you have it.
                    // See Meta.VerifyManyToMany for where Default ThroughFields would
                    // be initialized

                    sqlJoins.Add(string.Format(
                        "INNER JOIN {0} AS {0} ON {0}.{1} = {2}.{3}", 
                        forModelTableName, forModelPkName, modelTableName, modelTableColumn)
                    );

                    // we can safely call this here as the field generation has already happened.
                    // it might be worth looking into just using this mechanism during the query
                    // field generation as well.
                    // we are doing it for the model hydration: 
                    // SqliteEngine.cs.ParseSqlRow (~114 there is a check for 
                    // `query.RelatedColumns.Contains`
                    // 
                    // this addition satisfies that check so it will hydrate properly.
                    query.RelatedColumns.Add(forModelTableName);
                }
            }
             
            var pivotModel = query.PivotModel;
            var pkValue = pivotModel.GetType()
                .GetRuntimeProperty(metaTable1.PrimaryKey.PropertyName)
                .GetValue(pivotModel, null);
            
            // WHERE post_publication.post_id = 2
            var op = new Operator {
                Value = string.Format("{0}.{1}_{2} = {3}", 
                    pivotTableName, metaTable1TableName, metaTable1PkName, Utils.Utils.EscapeSqlValue(pkValue)
                )
            };

            query.FilterBy(op);
            return sqlJoins;
        }



        public List<string> CreateQueryTables<T>(MetaModel model, QuerySet<T> query)
        {
            List<string> sqlTables = new List<string> {
                string.Format("{0} AS {0}", model.Table.TableName)
            };

            return sqlTables;
        }

        public List<string> CreateQueryFields<T>(MetaModel model, QuerySet<T> query)
        {
            var fields = new List<string>();
            List<string> sqlFields;

            if (query.Fields.Count == 0)
            {
                fields = (from c in model.Columns
                                      where c.ForeignKey == null
                                      select c.ColumnName).ToList<string>();
            }
            else
            {
                fields = query.Fields;
            }

            sqlFields = QueryFieldsForModel(model, fields);

            if (query.RelatedColumns.Count > 0)
            {
                var validForeignKeys = GetForeignKeyColumns(model, query);

                foreach (var c in validForeignKeys)
                {
                    MetaModel relatedModel;
                    Meta.Tables.TryGetValue(c.ForeignKey.RelatedTypeName, out relatedModel);

                    if (relatedModel == null)
                        continue;
                    sqlFields.AddRange(QueryFieldsForModel(relatedModel));
                }
            }

            if (query.PivotModel != null)
            {
                var m2mForModelFields = CreateManyToManyForModelQueryFields<T>(model, query);
                sqlFields.AddRange(m2mForModelFields);
            }

            return sqlFields;
        }

        public List<string> CreateManyToManyForModelQueryFields<T>(MetaModel model, QuerySet<T> query)
        {
            var sqlFields = new List<string>();

            MetaModel metaPivotModel;
            MetaModel metaForModel;
            ManyToManyAttribute m2m;

            Meta.Tables.TryGetValue(query.PivotModel.GetType().Name, out metaPivotModel);

            if (metaPivotModel == null)
                return sqlFields;
            
            m2m = metaPivotModel.ManyToMany.FirstOrDefault(x => x.PropertyType == model.ModelType);

            if (m2m == null)
                return sqlFields;

            if (m2m.ForModel == null)
                return sqlFields;

            Meta.Tables.TryGetValue(m2m.ForModel.Name, out metaForModel);

            if (metaForModel == null)
                return sqlFields;

            var tableName = metaForModel.Table.TableName;

            sqlFields.AddRange(metaForModel.Columns.Select(x => string.Format("{0}.{1}", tableName, x.ColumnName)));
            return sqlFields;
        }
    }
}

