using System;
using System.Collections;
using System.Collections.Generic;
using Amigo.ORM.Utils;
using System.Threading.Tasks;

namespace Amigo.ORM.Engines
{
    public interface IAlchemyEngine
    {
        // it's not a party until someone sets the Meta on the engine.
        // A session requires the meta and engine to be passed and will
        // set the Meta on the engine for you. Otherwise you need to be
        // sure the engine has it's Metadata set by your own means.

        MetaData Meta { get; set; }

        Task<List<object>> QueryAsync(string sql, Func<object, object> parseRow = null);
        Task<List<T>> QueryAsync<T>(string sql, Func<object, T> parseRow = null);
        Task<T> ExecuteAsync<T>(QuerySet<T>query);
        Task<List<T>> ExecuteListAsync<T>(QuerySet<T>query);

        // we might be able to kill these.
        string CreateQuerySetSql<T>(QuerySet<T>query);
        void Commit(Session session);
        Task CreateAllAsync();
        Task Begin();
        Task Commit();
        Task Rollback();
        Task Insert(object model);
        Task Delete(object model);
        Task Update(object model);
        Task InsertManyToMany(SessionModelAction action);
        Task DeleteManyToMany(SessionModelAction action);
    }
}

