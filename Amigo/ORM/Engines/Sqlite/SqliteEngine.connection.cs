using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using Amigo.ORM.Utils;
using SQLitePCL.pretty;


namespace Amigo.ORM.Engines
{
    public partial class SqliteEngine
    {
        public IAsyncDatabaseConnection Connect()
        {
            var conn = SQLite3.Open(Path);
            Connection = conn.AsAsyncDatabaseConnection();

            return Connection;
        }

    }
}

