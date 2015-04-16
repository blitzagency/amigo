using NUnit.Framework;
using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;
using Amigo.ORM;

namespace AmigoTests
{
    [TestFixture()]
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

        //        [Test]
        //        public void TestLinqFilters()
        //        {
        //            var meta = new MetaData();
        //            meta.RegisterModel<Author>();
        //            meta.RegisterModel<Post>();
        //
        //            var engine = new SqliteEngine();
        //            var session = new Session(meta, engine);
        //
        //            var q = session.Query<Post>()
        //                           .SelectRelated("Author")
        //                           .Where<Author>((post, author) => post.Title == "Foo" && author.FirstName == "Lucy");
        //
        ////            var q = session.Query<Post>()
        ////                .SelectRelated("Author")
        ////                .Where("Foo");
        //
        //            var sql = q.LinqFilters.CompileExpression(meta);
        //            //Expression<Func<Author,bool>> tgt = x => x.FirstName == "BLITZ" && x.LastName == "Agency";
        //            //Expression<Func<Author,bool>> tgt = x => x.FirstName == "BLITZ" && x.Id == 5;
        //
        //            //Expression<Func<Post, Author, bool>> tgt = (post, author) => post.Title == "Foo" && author.FirstName == "Adam";
        //
        //            //var foo = tgt.Body.CompileExpression(meta);
        //            var stop = 1;
        ////            var exp = (LambdaExpression)tgt;
        ////            var pred = exp.Body;
        ////            var exprtype = exp.GetType();
        ////            var predType = pred.GetType();
        ////            var predNodeType = pred.NodeType;
        ////
        ////            if (pred is BinaryExpression)
        ////            {
        ////
        ////                var left = ((BinaryExpression)pred).Left;
        ////                var leftType = left.GetType();
        ////
        ////                if (left is BinaryExpression)
        ////                {
        ////                    var left2 = ((BinaryExpression)left).Left;
        ////                    var left2Type = left2.GetType();
        ////                    var stop = 1;
        ////
        ////                }
        ////
        ////
        ////
        ////            }
        //
        ////            var q = session.Query<Author>().LinqFilterBy(x => x.FirstName == "Adam");
        ////            q.CompileFilters();
        //
        //
        //        }
    }
}



//# create a configured "Session" class
//Session = sessionmaker(bind=some_engine)