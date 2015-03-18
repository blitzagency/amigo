using NUnit.Framework;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;


namespace AmigoTests
{
    [TestFixture()]
    [Ignore]
    public class TestMeta
    {
        [Test()]
        public void TestMetaModels()
        {
            var meta = new MetaData();
            meta.RegisterModel<TestItemImplicit>();
            meta.RegisterModel<TestItemExplicit>();
    
            Assert.Contains("TestItemImplicit", meta.Tables.Keys);
            Assert.Contains("TestItemExplicit", meta.Tables.Keys);
        }
    
        [Test()]
        public void TestMetaModelsList()
        {
            var meta = new MetaData();
            meta.RegisterModel<TestItemImplicit>();
            meta.RegisterModel<TestItemExplicit>();
    
            Assert.AreEqual(meta.Models.Count, 2);
        }
    }
}

