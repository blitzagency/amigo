using NUnit.Framework;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

namespace AmigoTests
{
    [TestFixture]
    public class TestTables
    {
        [Test]
        public void TestTableAttributeNameImplicit()
        {
            var meta = new MetaData();
            var model = meta.RegisterModel<TestItemImplicit>();
        
            Assert.IsNotNull(model);
            Assert.AreEqual("testitemimplicit", model.Table.TableName);
        }
    
        [Test]
        public void TestTableAttributeNameExplicit()
        {
            var meta = new MetaData();
            var model = meta.RegisterModel<TestItemExplicit>();
        
            Assert.IsNotNull(model);
            Assert.AreEqual("foo", model.Table.TableName);
        }
    }
}

