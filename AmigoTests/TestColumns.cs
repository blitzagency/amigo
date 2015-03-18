using NUnit.Framework;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;


namespace AmigoTests
{
    [TestFixture]
    public class TestColumns
    {
        [Test]
        public void TestColumnAttributesCount()
        {
            var meta = new MetaData();
            var model = meta.RegisterModel<TestItemImplicit>();
    
            Assert.AreEqual(model.Columns.Count, 2);
        }
    
        [Test]
        public void TestColumnAttributesNames()
        {
            var meta = new MetaData();
            var model = meta.RegisterModel<TestItemImplicit>();
    
            Assert.AreEqual(model.Columns[0].ColumnName, "id");
            Assert.AreEqual(model.Columns[1].ColumnName, "altlabel");
        }
    
        [Test]
        public void TestColumnAttributesTypes()
        {
            var meta = new MetaData();
            var model = meta.RegisterModel<TestItemImplicit>();
    
            Assert.AreEqual(model.Columns[0].PropertyType, typeof(int));
            Assert.AreEqual(model.Columns[1].PropertyType, typeof(string));
        }
    
        [Test]
        public void TestColumnPrimaryKey()
        {
            var meta = new MetaData();
            var model = meta.RegisterModel<TestItemImplicit>();
    
            Assert.AreEqual(model.Columns[0], model.PrimaryKey);
        }
    }
}

