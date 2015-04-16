using NUnit.Framework;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;


namespace AmigoTests
{
    [TestFixture()]
    public class TestSqliteEngine
    {
        [Test]
        public void TestCreateTable()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<TestItemImplicit>();
            var value = engine.CreateAllTablesSql();
            var expected = "CREATE TABLE IF NOT EXISTS testitemimplicit (id INTEGER PRIMARY KEY NOT NULL,altlabel TEXT NOT NULL);";
    
            Assert.AreEqual(expected, value);
        }
    
        [Test]
        public void TestCreateTableWithUnique()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };
            meta.RegisterModel<TestUnique>();
            var value = engine.CreateAllTablesSql();
            var expected = "CREATE TABLE IF NOT EXISTS testunique (id INTEGER PRIMARY KEY NOT NULL,label TEXT NOT NULL);";
    
            Assert.AreEqual(expected, value);
        }
    
        [Test]
        public void TestCreateTableWithNull()
        {
            
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<TestUniqueNull>();
            var value = engine.CreateAllTablesSql();
            var expected = "CREATE TABLE IF NOT EXISTS testuniquenull (id INTEGER PRIMARY KEY NOT NULL,label TEXT NULL);";
    
            Assert.AreEqual(expected, value);
        }
    
        [Test]
        public void TestCreateMultipleTables()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<TestUniqueNull>();
            meta.RegisterModel<TestUnique>();
            meta.RegisterModel<TestItemExplicit>();
            var value = engine.CreateAllTablesSql();
            var expected = "CREATE TABLE IF NOT EXISTS testuniquenull (id INTEGER PRIMARY KEY NOT NULL,label TEXT NULL);CREATE TABLE IF NOT EXISTS testunique (id INTEGER PRIMARY KEY NOT NULL,label TEXT NOT NULL);CREATE TABLE IF NOT EXISTS foo (id INTEGER PRIMARY KEY NOT NULL,altlabel TEXT NOT NULL);";
    
            Assert.AreEqual(expected, value);
        }
    
        [Test]
        public void TestCreateIndexWithUnique()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<TestUnique>();
            var value = engine.CreateAllIndexesSql();
            var expected = "CREATE UNIQUE INDEX IF NOT EXISTS testunique_label_idx ON testunique (label ASC);";
    
            Assert.AreEqual(expected, value);
        }
    
        [Test]
        public void TestCreateIndex()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<TestIndex>();
            var value = engine.CreateAllIndexesSql();
            var expected = "CREATE INDEX IF NOT EXISTS testindex_label_idx ON testindex (label ASC);";
    
            Assert.AreEqual(expected, value);
        }
    
        [Test]
        public void TestCreateAllSql()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<TestIndex>();
            meta.RegisterModel<TestUnique>();
            var value = engine.CreateAllSql();
            var expected = "CREATE TABLE IF NOT EXISTS testindex (id INTEGER PRIMARY KEY NOT NULL,label TEXT NOT NULL);CREATE TABLE IF NOT EXISTS testunique (id INTEGER PRIMARY KEY NOT NULL,label TEXT NOT NULL);CREATE INDEX IF NOT EXISTS testindex_label_idx ON testindex (label ASC);CREATE UNIQUE INDEX IF NOT EXISTS testunique_label_idx ON testunique (label ASC);";
    
            Assert.AreEqual(expected, value);
        }
    
        [Test]
        public void TestCreateForeignKey()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();
            var value = engine.CreateAllSql();
            var expected = "CREATE TABLE IF NOT EXISTS author (id INTEGER PRIMARY KEY NOT NULL,firstname TEXT NOT NULL,lastname TEXT NOT NULL);CREATE TABLE IF NOT EXISTS post (id INTEGER PRIMARY KEY NOT NULL,title TEXT NOT NULL,author_id INTEGER NOT NULL);CREATE INDEX IF NOT EXISTS post_author_id_idx ON post (author_id ASC);";
    
            Assert.AreEqual(expected, value);
        }

        [Test]
        public void TestCreateManyToMany()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<Author>();
            meta.RegisterModel<Post>();
            meta.RegisterModel<Publication>();

            var value = engine.CreateAllSql();
            var expected = "CREATE TABLE IF NOT EXISTS author (id INTEGER PRIMARY KEY NOT NULL,firstname TEXT NOT NULL,lastname TEXT NOT NULL);CREATE TABLE IF NOT EXISTS post (id INTEGER PRIMARY KEY NOT NULL,title TEXT NOT NULL,author_id INTEGER NOT NULL);CREATE TABLE IF NOT EXISTS publication (id INTEGER PRIMARY KEY NOT NULL,label TEXT NOT NULL);CREATE TABLE IF NOT EXISTS post_publication (id INTEGER PRIMARY KEY NOT NULL,post_id INTEGER NOT NULL,publication_id INTEGER NOT NULL);CREATE INDEX IF NOT EXISTS post_author_id_idx ON post (author_id ASC);CREATE INDEX IF NOT EXISTS post_publication_id_idx ON post_publication (id ASC); CREATE INDEX IF NOT EXISTS post_publication_post_id_idx ON post_publication (post_id ASC); CREATE INDEX IF NOT EXISTS post_publication_publication_id_idx ON post_publication (publication_id ASC);";

            Assert.AreEqual(value, expected);
        }

        [Test]
        public void TestCreateInsertSql()
        {
            var meta = new MetaData();
            var engine = new SqliteEngine() {
                Meta = meta
            };

            meta.RegisterModel<Author>();

            var o1 = new Author {
                FirstName = "Adam",
                LastName = "Venturella"
            };

            var value = engine.CreateInsertSql(o1);
            var expected = "INSERT INTO 'author' ('firstname','lastname') VALUES \n('Adam', 'Venturella');";

            StringAssert.AreEqualIgnoringCase(value, expected);
        }
    }
}

