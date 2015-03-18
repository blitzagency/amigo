using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amigo.ORM.Utils;


namespace Amigo.ORM.Engines
{
    public class NullEngine
    {
        public void CreateAll(MetaData meta)
        {
            throw new NotImplementedException();
        }

        public MetaData Meta { get; set; }

        public NullEngine()
        {

        }

        public void CreateAll()
        {
            
        }

        public T Execute<T>(QuerySet<T> query)
        {
            throw new NotImplementedException();
        }

        public List<T> ExecuteList<T>(QuerySet<T> query)
        {
            throw new NotImplementedException();
        }

        public string CreateQuerySetSql<T>(QuerySet<T> query)
        {
            throw new NotImplementedException();
        }

        public void Commit(Session session)
        {
            throw new NotImplementedException();
        }
    }
}

