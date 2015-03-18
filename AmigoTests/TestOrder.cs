using NUnit.Framework;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

namespace AmigoTests
{
    [TestFixture]
    public class TestOrder
    {
        [Test]
        public void TestQuerySetOrderByPlusMinus()
        {
            var meta = new MetaData();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, new SqliteEngine());

            var q = session.Query<Author>()
                .OrderBy(new {LastName = "-", FirstName = "+"});

            var sql = q.ToSql();
            var expected = "SELECT author.id, author.firstname, author.lastname FROM author AS author ORDER BY author.lastname DESC, author.firstname ASC;";

            Assert.AreEqual(expected, sql);
        }

        [Test]
        public void TestQuerySetOrderByAscDesc()
        {
            var meta = new MetaData();
            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();

            var session = new Session(meta, new SqliteEngine());

            var q = session.Query<Author>()
                .OrderBy(new {LastName = "DeSc", FirstName = "aSc"});

            var sql = q.ToSql();
            var expected = "SELECT author.id, author.firstname, author.lastname FROM author AS author ORDER BY author.lastname DESC, author.firstname ASC;";

            Assert.AreEqual(expected, sql);
        }
    }
}



//# create a configured "Session" class
//Session = sessionmaker(bind=some_engine)