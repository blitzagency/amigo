# Amigo

A PCL ORM for SQLite. Used in Xamarin or anything that supports PCLs.


### Lets do this!

Couple things, this was built in about 3.5 days. It could likely be better
in every way, but we needed a tool, so we built on. Probably most of
the filtering could be done with Linq Expressions. Again, we needed this
sooner rather than later, so the time was not spent to fully grasp how to
compile those. Maybe later.

This is likely not idomatic C#, coming from a Python background mostly
and this is a first go at it. Just needed a tool. and SQLite.NET + the
Extension wasn't doing it for us.

There is a LAUGHABLE excuse for SQL Escaping values. It handles the quotes
and makes sure numbers are numbers. SQLitePCL.pretty (what the sqllite engine
runs on) supports prepared statements, could have used those. Again time.
In the end, this is for a DB the user controls. If they REALLY want to they
can SQL inject themselves by whatever means they want. It's their data.
That said, you shoudl not consdider this safe for a multiuser environment.
Not that you dear reader would be using sqlite for a multi-user application
to begin with.



#### Create your tables:

```csharp
using System;
using System.Collection.Generics;
using Amigo.ORM;

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

```

#### Build the database:

```csharp
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

var meta = new MetaData();
meta.RegisterModel<Post>();
meta.RegisterModel<Author>();
meta.RegisterModel<Publication>();
meta.RegisterModel<PublicationMeta>();

var engine = new SqliteEngine("/path/to/your/sqlite.db");
await meta.CreateAllAsync(engine);
```


#### Start getting nuts:

```csharp
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

var meta = new MetaData();
meta.RegisterModel<Post>();
meta.RegisterModel<Author>();
meta.RegisterModel<Publication>();
meta.RegisterModel<PublicationMeta>();

var engine = new SqliteEngine("/path/to/your/sqlite.db");

// You do all your operations though the session.
// Initialize this and pass it around to anyone interested
// in doing DB ops.

var session = new Session(meta, engine);

var lucy = new Author {
    FirstName = "Lucy",
    LastName = "TheDog"
};


// start a transaction:
await session.Begin();

// bananas time:
await session.Add(lucy); // .Add | .Remove | .Update

// write it
await session.Commit();
```

#### Get your models back!

```csharp
using System;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

var meta = new MetaData();
meta.RegisterModel<Post>();
meta.RegisterModel<Author>();
meta.RegisterModel<Publication>();
meta.RegisterModel<PublicationMeta>();

var engine = new SqliteEngine("/path/to/your/sqlite.db");

// You do all your operations though the session.
// Initialize this and pass it around to anyone interested
// in doing DB ops.

var session = new Session(meta, engine);

var ollie = new Author {
    FirstName = "Ollie",
    LastName = "Gato"
};

await session.Begin();

// bananas time:
await session.Add(ollie);

// write it
await session.Commit();


// want one?
Author test1 = await session.Query<Author>()
                            .Get(new {Id = ollie.Id});

// I need all the authors!
List<Author> test1 = await session.Query<Author>()
                                  .All();

// I only need some of the authors!
List<Author> test1 = await session.Query<Author>()
                                  .FilterBy(new And(new {FirstName = "Ollie", LastName = "Gato"}));
```


#### Foreign Keys too!:

```csharp
using System;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

var meta = new MetaData();
meta.RegisterModel<Post>();
meta.RegisterModel<Author>();
meta.RegisterModel<Publication>();
meta.RegisterModel<PublicationMeta>();

var engine = new SqliteEngine("/path/to/your/sqlite.db");

// You do all your operations though the session.
// Initialize this and pass it around to anyone interested
// in doing DB ops.

var session = new Session(meta, engine);

var ollie = new Author {
    FirstName = "Ollie",
    LastName = "Gato"
};

var post = new Post {
    Title = "MeowTowne",
    Author = ollie
};


await session.Begin();

// bananas time:
await session.Add(post);

// write it
await session.Commit();

// Wait did it just save the foreign key too?

var verify = await session.Query<Post>()
                          .SelectRelated("Author")
                          .Get( new { Id = post.Id });


Console.WriteLine(string.Format(
    "Awe ya dog: FK ID:'{0}' '{1}'",
    verify.Author.Id, verify.Author.FirstName);
);

// don't want to select the Foreign Key? No Problem, just
// don't add the `.SelectRelated(ColumnName)`

// I only want Posts where the ForeignKey meets some criteria!

var verify2 = await session.Query<Post>()
                           .SelectRelated("Author")
                           .Get( new { Author__Id = ollie.Id });

```



#### Many to Many as well!

```csharp
using System;
using Amigo.ORM.Engines;
using Amigo.ORM.Utils;

var meta = new MetaData();

meta.RegisterModel<Post>();
meta.RegisterModel<Author>();
meta.RegisterModel<Publication>();
meta.RegisterModel<PublicationMeta>();

var engine = new SqliteEngine("/path/to/your/sqlite.db");
var session = new Session(meta, engine);

var a1 = new Author {
    FirstName = "Lucy",
    LastName = "TheDog"
};

var p1 = new Post {
    Title = "Chasing squirrels is a blast!",
    Author = a1
};

var o3 = new PublicationMeta {
    Publication_Order = 5,

    Publication = new Publication {
        Label = "Lorem Ipsum Weekly"
    }
};

await session.Begin();
await session.add(p1);
await session.FromModel(p1).Add(o3);
await session.Commit();

```

Lets look back at this Many To Many example real quick.
Check the model out:

```csharp

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

```

What's the `forModel` stuff? We are telling Amigo:

> hey! I need to add additional data to this many to many relationship.
> `PublicationMeta` is basically a wrapper for what I really want
> which is a `Publication` object.

This is akin to Django's `Through Tables`. Note that `PublicationMeta`
hosts a `ForeignKey` to a Publication. That `ForeignKey` is required
at the moment when using a `forModel` ManyToMany

You don't need to specify a `forModel` on a Many To many relationship,
only if you want extra info for the relationship beyond the model.
When you remove a Many To Many Model that has a `forModel` Amigo
will kill the wrapper model automatically and leave the underlying
`Publication` model (in this case) untouched.

If you don't want/need that additional info you would define the Many to Many
like this:

```csharp
[Table]
public class Post
{
    [Column(primaryKey: true)]
    public int Id { get; set; }

    [Column]
    public string Title { get; set; }

    [ForeignKey]
    public Author Author { get; set; }

    [ManyToMany]
    public List<Publication> Publication { get; set; }
}
```







