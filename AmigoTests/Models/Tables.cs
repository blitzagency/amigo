using System;
using System.Collections.Generic;
using Amigo.ORM;

namespace AmigoTests
{
    [Table]
    public class TestItemImplicit
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column(name: "AltLabel")]
        public string Label { get; set; }

        public string IgnoredProperty { get; set; }
        public string AnotherIgnoredProperty { get; set; }
    }


    [Table("Foo")]
    public class TestItemExplicit : TestItemImplicit
    {
        
    }

    [Table]
    public class TestUnique
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column(name: "Label", unique: true)]
        public string Label { get; set; }
    }

    [Table]
    public class TestUniqueNull
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column(name: "Label", unique: true, allowNull: true)]
        public string Label { get; set; }
    }

    [Table]
    public class TestIndex
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column(name: "Label", index: true)]
        public string Label { get; set; }
    }


    [Table]
    public class Publication
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column]
        public string Label { get; set; }
    }

    [Table]
    public class Author
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column]
        public string FirstName { get; set; }

        [Column]
        public string LastName { get; set; }
    }

    [Table]
    public class PublicationMeta
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column]
        public int Publication_Order { get; set; }

        [ForeignKey]
        public Publication Publication { get; set; }
    }

    [Table]
    public class Post
    {
        [Column(primaryKey: true)]
        public int Id { get; set; }

        [Column]
        public string Title { get; set; }

        [ForeignKey]
        public Author Author { get; set; }

        [ManyToMany(forModel: typeof(Publication))]
        public List<PublicationMeta> Publication { get; set; }
    }



}

