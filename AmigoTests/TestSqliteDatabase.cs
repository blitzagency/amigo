using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SQLitePCL.pretty;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

namespace AmigoTests
{
    [TestFixture()]
    public class TestSqliteDatabase
    {
        public Expression _where { get; set; }
        public string Foo { get; set; }

        [Test]
        public async Task TestCreateAll()
        {
            var meta = new MetaData();
            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            await meta.CreateAllAsync(engine);
        }

        [Test]
        public async Task TestExecuteSql()
        {

            var meta = new MetaData();
            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");

            engine.Connect();
            List<Author> rows;

            using (engine.Connect())
            {
                Func<object, Author> parse = delegate(object result) { 
                    var row = (IReadOnlyList<IResultSetValue>)result;
                    var id = row[0].ToInt();
                    var firstName = row[1].ToString();
                    var lastName = row[2].ToString();

                    return new Author {
                        Id = id,
                        FirstName = firstName,
                        LastName = lastName
                    };
                }; 

                rows = await engine.QueryAsync<Author>("SELECT id, firstName, lastname FROM author", parse);
            }

            Assert.AreEqual(3, rows.Count());
        }

        [Test]
        public async Task TestExecuteGet()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            engine.Connect();

            var q = session.Query<Author>();
            var model = await q.Get(new {Id = 2 });

            Assert.AreEqual(model.Id, 2);
        }

        [Test]
        public async Task TestExecuteAll()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            engine.Connect();

            var q = session.Query<Author>().OrderBy(new {FirstName = "-"});
            var models = await q.All();

            Assert.AreEqual(3, models.Count);
        }

        [Test]
        public async Task TestExecuteForeignKeyGet()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            engine.Connect();

            var q = session.Query<Post>().SelectRelated("Author");
            var model = await q.Get(new {Id = 1});

            Assert.IsNotNull(model.Author);
        }

        [Test]
        public async Task TestExecuteForeignKeyAll()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            engine.Connect();

            var q = session.Query<Post>().SelectRelated("Author").OrderBy(new {Author__FirstName = "-"});
            var models = await q.All();

            Assert.AreEqual(models.Count, 2);
        }

        [Test]
        public async Task TestExecuteManyToMany()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            engine.Connect();

            var model = new Post {
                Id = 1,
                Title = "Foo",
            };

            var publications = await session
                .Query<PublicationMeta>()
                .FromModel(model)
                .OrderBy(new {publicationmeta__publication_order = "-"})
                .All();

            Assert.AreEqual(publications.Count, 4);
        }

        [Test]
        public async Task TestTransactionBeginCommit()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            await session.Begin();
            await session.Commit();
        }

        [Test]
        public async Task TestTransactionBeginRollback()
        {
            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            await session.Begin();
            await session.Rollback();
        }

        [Test]
        public async Task TestTransactionLastInsertId()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            var model = new Author {
                FirstName = "Lucy",
                LastName = "Venturella"
            };

            await session.Begin();
            await session.Add(model);
            await session.Commit();
        }

        [Test]
        public async Task TestTransactionInsertForeignKey()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            var o1 = new Author {
                FirstName = "Lucy",
                LastName = "Venturella"
            };

            var o2 = new Post {
                Title = "Lorem Ipsum Dolor",
                Author = o1
            };

            await session.Begin();
            await session.Add(o2);
            await session.Commit();

            Assert.AreNotEqual(0, o1.Id);
            Assert.AreNotEqual(0, o2.Id);
        }

        [Test]
        public async Task TestTransactionRemove()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            var model = new Author {
                FirstName = "Lucy",
                LastName = "Venturella"
            };

            await session.Begin();
            await session.Add(model);
            await session.Commit();

            var modelOut = await session.Query<Author>()
                                        .Get(new {Id = model.Id});

            Assert.IsNotNull(modelOut);
            Assert.AreNotEqual(0, modelOut.Id);

            await session.Begin();
            await session.Remove(modelOut);
            await session.Commit();

            modelOut = await session.Query<Author>()
                                    .Get(new {Id = model.Id});

            Assert.IsNull(modelOut);
        }

        [Test]
        public async Task TestTransactionUpdate()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            var model = new Author {
                FirstName = "Lucy",
                LastName = "Venturella"
            };

            await session.Begin();
            await session.Add(model);
            await session.Commit();

            model.FirstName = "Ollie";
            model.LastName = "Gato";
                
            await session.Begin();
            await session.Update(model);
            await session.Commit();

            model = await session.Query<Author>()
                                 .Get(new {Id = model.Id});

            StringAssert.AreEqualIgnoringCase(model.FirstName, "ollie");
            StringAssert.AreEqualIgnoringCase(model.LastName, "gato");
        }

        [Test]
        public async Task TestTransactionMulti()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            var o1 = new Author {
                FirstName = "Lucy",
                LastName = "Venturella"
            };

            var o2 = new Author {
                FirstName = "Ollie",
                LastName = "Venturella"
            };

            int t1Id, t2Id;

            await session.Begin();
            // ----
            await session.Add(o1);
            await session.Add(o2);

            t1Id = o1.Id;
            t2Id = o2.Id;

            o1.FirstName = "Ollie";
            o1.LastName = "Gato";

            await session.Update(o1);
            await session.Remove(o2);
            // ----
            await session.Commit();

            var t1 = await session.Query<Author>()
                .Get(new {Id = t1Id});

            var t2 = await session.Query<Author>()
                .Get(new {Id = t2Id});

            StringAssert.AreEqualIgnoringCase(t1.FirstName, "ollie");
            StringAssert.AreEqualIgnoringCase(t1.LastName, "gato");
            Assert.IsNull(t2);
        }

        [Test]
        public async Task TestManyToManyAdd()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            var a1 = new Author {
                Id = 2,
                FirstName = "Lucy",
                LastName = "Venturella"
            };
                
            var p1 = new Post {
                Id = 1,
                Title = "Steelheart",
                Author = a1
            };
                    
            var o3 = new PublicationMeta {
                Publication_Order = 5,

                Publication = new Publication {
                    Label = "Lorem Ipsum Weekly"
                }
            };

            await session.Begin();
            await session.FromModel(p1).Add(o3);
            await session.Commit();

        }

        [Test]
        public async Task TestManyToManyRemove()
        {

            var meta = new MetaData();

            meta.RegisterModel<Post>();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Publication>();
            meta.RegisterModel<PublicationMeta>();

            var engine = new SqliteEngine("/Users/aventurella/Desktop/test.db");
            var session = new Session(meta, engine);

            var a1 = new Author {
                Id = 2,
                FirstName = "Lucy",
                LastName = "Venturella"
            };

            var p1 = new Post {
                Id = 1,
                Title = "Steelheart",
                Author = a1
            };

            var o3 = new PublicationMeta {
                Publication_Order = 5,

                Publication = new Publication {
                    Id = 4, 
                    Label = "Lorem Ipsum Weekly"
                }
            };

            List<PublicationMeta> publications;

            await session.Begin();
            await session.FromModel(p1).Add(o3);
            await session.Commit();

            publications = await session.Query<PublicationMeta>()
                                        .FromModel(p1)
                                        .OrderBy(new {Publication_Order = "-"})
                                        .All();


            await session.Begin();
            await session.FromModel(p1).Remove(o3);
            await session.Commit();

            publications = await session.Query<PublicationMeta>()
                                        .FromModel(p1)
                                        .OrderBy(new {Publication_Order = "-"})
                                        .All();

            Assert.AreEqual(0, publications.Count);
        }
    }
}

