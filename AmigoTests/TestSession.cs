using NUnit.Framework;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

namespace AmigoTests
{
    [TestFixture()]
    [Ignore]
    public class TestSession
    {
        [Test]
        public void TestSessionSingleAdd()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine();

            meta.RegisterModel<Author>();

            var session = new Session(meta, engine);

            var a1 = new Author {
                FirstName = "Foo",
                LastName = "Bar"
            };
                
//            session.Add<Author>(a1);
//
//            var sql = engine.CreateAllInsertSql(meta, session);
//            var expected = "INSERT INTO 'Author' ('FirstName','LastName') VALUES \n('Foo', 'Bar');";
//
//            StringAssert.AreEqualIgnoringCase(expected, sql[0]);
        }

        [Test]
        public void TestSessionMultiAdd()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine();

            meta.RegisterModel<Author>();

            var session = new Session(meta, engine);

            var a1 = new Author {
                FirstName = "Foo",
                LastName = "Bar"
            };

            var a2 = new Author {
                FirstName = "Baz",
                LastName = "Zap"
            };

            var a3 = new Author {
                FirstName = "Qux",
                LastName = "Bark"
            };

//            session.Add<Author>(a1);
//            session.Add<Author>(a2);
//            session.Add<Author>(a3);
//
//            var sql = engine.CreateAllInsertSql(meta, session);
//            var expected = "INSERT INTO 'Author' ('FirstName','LastName') VALUES \n('Foo', 'Bar'), \n('Baz', 'Zap'), \n('Qux', 'Bark');";
//
//            StringAssert.AreEqualIgnoringCase(expected, sql[0]);
        }

        [Test]
        public void TestSessionMixedTypeAdd()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine();

            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, engine);

            var o1 = new Post {
                Title = "Lorem",
            };

            var o2 = new Author {
                FirstName = "Foo",
                LastName = "Bar"
            };

            var o3 = new Author {
                FirstName = "Baz",
                LastName = "Zap"
            };

            var o4 = new Post {
                Title = "Ipsum",
            };

            var o5 = new Post {
                Title = "Dolor",
            };

//            session.Add<Post>(o1);
//            session.Add<Author>(o2);
//            session.Add<Author>(o3);
//            session.Add<Post>(o4);
//            session.Add<Post>(o5);

//            var sql = engine.CreateAllInsertSql(meta, session);
//            var expected = new System.Collections.Generic.List<string> {
//                "INSERT INTO 'Post' ('Title','Author') VALUES \n('Lorem', NULL), \n('Ipsum', NULL), \n('Dolor', NULL);",
//                "INSERT INTO 'Author' ('FirstName','LastName') VALUES \n('Foo', 'Bar'), \n('Baz', 'Zap');"
//            };
//
//            Assert.AreEqual(sql.Count, 2);
//            StringAssert.AreEqualIgnoringCase(expected[0], sql[0]);
//            StringAssert.AreEqualIgnoringCase(expected[1], sql[1]);
        }

        [Test]
        public void TestSessionAddForeignKeyNull()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine();

            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, engine);

            var o1 = new Post {
                Title = "Lorem",
            };
                
//            session.Add<Post>(o1);
//
//            var sql = engine.CreateAllInsertSql(meta, session);
//            var expected = "INSERT INTO 'Post' ('Title','Author') VALUES \n('Lorem', NULL);";
//
//            StringAssert.AreEqualIgnoringCase(expected, sql[0]);
        }

        [Test]
        public void TestSessionAddForeignKeyNotNull()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine();

            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, engine);

            var o1 = new Author {
                Id = 22,
                FirstName = "Foo",
                LastName = "Bar"
            };

            var o2 = new Post {
                Title = "Lorem",
                Author = o1
            };

//            session.Add<Post>(o2);
//
//            var sql = engine.CreateAllInsertSql(meta, session);
//            var expected = "INSERT INTO 'Post' ('Title','Author') VALUES \n('Lorem', 22);";
//
//            StringAssert.AreEqualIgnoringCase(expected, sql[0]);
        }
    }
}

