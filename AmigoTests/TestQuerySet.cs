using NUnit.Framework;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

namespace AmigoTests
{
    [TestFixture()]
    [Ignore]
    public class TestQuerySet
    {
        [Test()]
        public void TestQuerySetSimple()
        {
            var meta = new MetaData();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, new SqliteEngine());

            var q = session.Query<Author>();
            var sql = q.ToSql();
            var expected = "SELECT Author.Id, Author.FirstName, Author.LastName FROM Author AS Author;";

            Assert.AreEqual(expected, sql);
        }

        [Test()]
        public void TestQuerySetOrderByPlusMinus()
        {
            var meta = new MetaData();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, new SqliteEngine());

            var q = session.Query<Author>()
                .OrderBy(new {LastName = "-", FirstName = "+"});
            
            var sql = q.ToSql();
            var expected = "SELECT Author.Id, Author.FirstName, Author.LastName FROM Author AS Author ORDER BY Author.LastName DESC, Author.FirstName ASC;";

            Assert.AreEqual(expected, sql);
        }

        [Test()]
        public void TestQuerySetOrderByAscDesc()
        {
            var meta = new MetaData();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, new SqliteEngine());

            var q = session.Query<Author>()
                .OrderBy(new {LastName = "DeSc", FirstName = "aSc"});

            var sql = q.ToSql();
            var expected = "SELECT Author.Id, Author.FirstName, Author.LastName FROM Author AS Author ORDER BY Author.LastName DESC, Author.FirstName ASC;";

            Assert.AreEqual(expected, sql);
        }

        //        [Test()]
        //        public void TestSessionQuery()
        //        {
        //            var meta = new MetaData();
        //            meta.RegisterModel<Author>();
        //            meta.RegisterModel<Post>();
        //
        //            var session = new Session(meta, new SqliteEngine());
        //
        //            var q = session.Query<Post>().SelectRelated("Author", "Banana");
        //            var sql = q.ToSql();
        //            Assert.AreEqual(1, 2);
        //        }

        [Test()]
        public void TestQuerySetFilter()
        {
            var meta = new MetaData();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();
        
            var session = new Session(meta, new SqliteEngine());
        
            var q = session.Query<Post>()
                .SelectRelated("Author")
                .FilterBy(new And(
                        new Eq(new {Title = "Foo"}),
                        new Eq<Or>(new {Author__FirstName = "Adam", Author__LastName = "Venturella"}),
                        new Neq<And>(new {Author__FirstName = "Dino", Author__LastName = "Petrone"})
                    ));
            
            var sql = q.ToSql();
            //Assert.AreEqual(1, 2);
        }

        [Test()]
        public void TestQuerySetFilterAndOrder()
        {
            var meta = new MetaData();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, new SqliteEngine());

            var q = session.Query<Post>()
                .SelectRelated("Author")
                .FilterBy(new And(
                        new Eq(new {Title = "Foo"}),
                        new Eq<Or>(new {Author__FirstName = "Adam", Author__LastName = "Venturella"}),
                        new Neq<And>(new {Author__FirstName = "Dino", Author__LastName = "Petrone"})
                    ))
                .OrderBy(new {Title = "-", Author__FirstName = "+"});

            var sql = q.ToSql();
            //Assert.AreEqual(1, 2);
        }
    }
}



//# create a configured "Session" class
//Session = sessionmaker(bind=some_engine)