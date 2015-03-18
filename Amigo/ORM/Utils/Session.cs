using System;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using Amigo.ORM.Engines;

namespace Amigo.ORM.Utils
{
    public class SessionModelAction
    {
        public object SourceModel { get; set; }
        public object TargetModel { get; set; }
        public MetaModel TargetMetaModel { get; set; }
        public MetaModel SourceMetaModel { get; set; }
        public Type SourceModelType { get; set; }
        public Type TargetModelType { get; set; }
        public ManyToManyAttribute ManyToMany { get; set; }
        public Session Session { get; set; }

        public SessionModelAction(object model, Session session)
        {
            SourceModel = model;
            Session = session;
        }

        public async Task Add(object model)
        {
            if (TargetModel == null)
                TargetModel = model;

            if (SourceMetaModel == null || TargetMetaModel == null)
                InitializeMetaModels();
            
            var targetType = model.GetType();

            var m2m = SourceMetaModel.ManyToMany.FirstOrDefault(x => x.PropertyType == targetType);
            await Add(m2m, model);
        }

        public async Task Add(string PropertyName, object model)
        {
            if (TargetModel == null)
                TargetModel = model;

            if (SourceMetaModel == null || TargetMetaModel == null)
                InitializeMetaModels();
            
            var m2m = SourceMetaModel.ManyToMany.FirstOrDefault(x => x.PropertyName == PropertyName);
           
            await Add(m2m, model);
        }

        public async Task Add(ManyToManyAttribute m2m, object model)
        {
            if (TargetModel == null)
                TargetModel = model;

            if (SourceMetaModel == null || TargetMetaModel == null)
                InitializeMetaModels();

            ManyToMany = m2m;
            await Session.Engine.InsertManyToMany(this);
        }


        public async Task Remove(object model)
        {
            if (TargetModel == null)
                TargetModel = model;

            if (SourceMetaModel == null || TargetMetaModel == null)
                InitializeMetaModels();

            var targetType = model.GetType();

            var m2m = SourceMetaModel.ManyToMany.FirstOrDefault(x => x.PropertyType == targetType);
            await Remove(m2m, model);
        }

        public async Task Remove(string PropertyName, object model)
        {
            if (TargetModel == null)
                TargetModel = model;

            if (SourceMetaModel == null || TargetMetaModel == null)
                InitializeMetaModels();

            var m2m = SourceMetaModel.ManyToMany.FirstOrDefault(x => x.PropertyName == PropertyName);

            await Remove(m2m, model);
        }

        public async Task Remove(ManyToManyAttribute m2m, object model)
        {
            if (TargetModel == null)
                TargetModel = model;

            if (SourceMetaModel == null || TargetMetaModel == null)
                InitializeMetaModels();

            ManyToMany = m2m;
            await Session.Engine.DeleteManyToMany(this);
        }

        public void InitializeMetaModels()
        {
            TargetMetaModel = Session.Meta.MetaModelForModel(TargetModel);
            SourceMetaModel = Session.Meta.MetaModelForModel(SourceModel);

            SourceModelType = SourceModel.GetType();
            TargetModelType = TargetModel.GetType();
        }
            
    }

    public class Session
    {
        public MetaData Meta { get; set; }
        public IAlchemyEngine Engine { get; set; }
        public Session(MetaData meta, IAlchemyEngine engine)
        {
            // we force the use of a session in order to use the engine.
            // Good idea? Bad idea? And engine MUST have it's Meta set.
            // you could also manually set it on the engine as well.
            // It's not a party until the engine knows it's metadata.

            Engine = engine;
            Meta = Engine.Meta = meta;
        }

        public SessionModelAction FromModel(object model)
        {
            return new SessionModelAction(model, this);
        }

        public async Task Add(object model)
        {
            await Engine.Insert(model);
        }

        public async Task Remove(object model)
        {
            await Engine.Delete(model);
        }

        public async Task Update(object model)
        {
            await Engine.Update(model);
        }
            
        public async Task Begin()
        {
            await Engine.Begin();
        }

        public async Task Rollback()
        {
            await Engine.Rollback();
        }

        public async Task Commit()
        {
            await Engine.Commit();
        }

        public QuerySet<T> Query<T>()
        {
            return new QuerySet<T>(Engine);
        }
    }
}

