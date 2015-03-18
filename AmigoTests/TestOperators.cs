using NUnit.Framework;
using Amigo.ORM.Utils;

namespace AmigoTests
{
    [TestFixture]
    public class TestOperators
    {

        [Test]
        public void TestQueryEqual()
        {
            var value = new Eq(new {Author__FirstName = "Foo's"}).ToString();
            var expected = "author.firstname = 'Foo''s'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryEqualMulti()
        {
            var value = new Eq(new {Author__FirstName = "Foo", Author__LastName = 6}).ToString();
            var expected = "author.firstname = 'Foo' AND author.lastname = 6";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryNotEqual()
        {
            var value = new Neq(new {Author__FirstName = "Foo"}).ToString();
            var expected = "author.firstname != 'Foo'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryNotEqualMulti()
        {
            var value = new Neq(new {Author__FirstName = "Foo", Author__LastName = 6}).ToString();
            var expected = "author.firstname != 'Foo' AND author.lastname != 6";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryAnd()
        {
            var value = new And(
                            new Eq(new {Name = "foo"}),
                            new Neq(new {Label = "bar"})
                        ).ToString();

            var expected = "name = 'foo' AND label != 'bar'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryOr()
        {
            var value = new Or(
                            new Eq(new {Name = "foo"}),
                            new Neq(new {Label = "bar"})
                        ).ToString();

            var expected = "name = 'foo' OR label != 'bar'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryOrWithNestedAnd()
        {
            var value = new Or(
                            new Eq(new {Name = "foo"}),
                            new And(
                                new Eq(new {Label = "bar"}),
                                new Neq(new {Description = "baz"})
                            ),
                            new Eq(new {Zap = "qux"})
                        ).ToString();

            var expected = "name = 'foo' OR (label = 'bar' AND description != 'baz') OR zap = 'qux'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryAndWithNestedOr()
        {
            var value = new And(
                            new Eq(new {Name = "foo"}),
                            new Or(
                                new Eq(new {Label = "bar"}),
                                new Neq(new {Description = "baz"})
                            ),
                            new Eq(new {Zap = "qux"})
                        ).ToString();

            var expected = "name = 'foo' AND (label = 'bar' OR description != 'baz') AND zap = 'qux'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryAndWithNestedAnd()
        {
            var value = new And(
                            new Eq(new {Name = "foo"}),
                            new And(
                                new Eq(new {Label = "bar"}),
                                new Neq(new {Description = "baz"})
                            ),
                            new Eq(new {Zap = "qux"})
                        ).ToString();

            var expected = "name = 'foo' AND (label = 'bar' AND description != 'baz') AND zap = 'qux'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryOrWithNestedOr()
        {
            var value = new Or(
                            new Eq(new {Name = "foo"}),
                            new Or(
                                new Eq(new {Label = "bar"}),
                                new Neq(new {Description = "baz"})
                            ),
                            new Eq(new {Zap = "qux"})
                        ).ToString();

            var expected = "name = 'foo' OR (label = 'bar' OR description != 'baz') OR zap = 'qux'";

            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestQueryWithDefaultTable()
        {
            var op = new Or(
                         new Eq(new {Name = "foo"}),
                         new Or(
                             new Eq(new {Label = "bar"}),
                             new Neq(new {Description = "baz"})
                         ),
                         new Eq(new {Zap = "qux"})
                     );

            op.DefaultTable = "Post";

            var value = op.ToString();
            var expected = "post.name = 'foo' OR (post.label = 'bar' OR post.description != 'baz') OR post.zap = 'qux'";

            Assert.AreEqual(expected, value);
        }
    }
}



//# create a configured "Session" class
//Session = sessionmaker(bind=some_engine)