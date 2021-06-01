<img src="https://i.imgur.com/r0ilni7.png" align="right" style="height: 8em"/>

# Linq2Db Fluent Mappings
## Why and how?  
  
[Linq2Db](https://github.com/linq2db/linq2db) is one of my favourite libraries. It's definitely my favourite micro-ORM and the reasons why are pretty clear - it's portable, it's thread-safe, it outperforms anything except raw ADO.NET (by latest metrics), it supports all my sneaky toolbox of SQL tricks like `CROSS APPLY` and `MERGE` and CTEs and of course, it's LINQ-based, making for clear, easy to read code that plays nice with [System.Reactive](http://reactivex.io/) and [LanguageExt](https://github.com/louthy/language-ext), which I rely on to get anything done.

Where it falls down is the documentation and this is very common - OSS projects with great docs are rare. Follow the examples and you'd think mapping a simple class to the database has to look like this.

```csharp
[Table(Name="Artists")]
public class Artist
    {
        [PrmaryKey, Identity]
        public long Id { get; set; }
        [Column(Name="Name")]
        public string Name { get; set; }
    }
```

And mapping a complex class is even worse.  
  
```csharp
[Table(Name="Albums")]
public class Album
    {
        [PrimaryKey, Identity]
        public long Id { get; set; }
        [Column(Name="Name")]
        public string Name { get; set; }
        [Column(Name="ArtistId")]
        public long ArtistId { get; set; }
        [Column(Name="LabelId")]
        public long LabelId { get; set; }
        [Attribute(ThisKey="nameof(Album.ArtistId)", OtherKey="nameof(Artist.Id)")]
        public virtual Artist Artist { get; set; } = new Artist();
        [Attribute(ThisKey="nameof(Album.LabelId)", OtherKey="nameof(Label.Id)")]
        public virtual Label Label { get; set; } = new Label();
    }
```

And that's okay. Really. There's nothing wrong with attributes _except_ that they restrict the portability of code. If my models are used at both back-end (for data access) and front-end (for actual use by actual users) then the front-end now needs Linq2Db installed just to compile _even though it will never actually be used_. This bothers me because I hate writing the same code twice.
  
## Fluent Mappings

So what's the answer? Well, it's Fluent Mappings and it needs a little bit of plumbing.  Let's define ourselves a Mappings file.  
  
```csharp
public static class Mappings
{
    private static MappingSchema schema = null;

    public static MappingSchema GetSchema()
    {
        if (schema == null)
        {
            schema = MappingSchema.Default;
            var mapper = schema.GetFluentMappingBuilder();

            mapper.Entity<Album>()
                .HasTableName("Albums")
                .Property(x => x.Id)
                    .HasColumnName(nameof(Album.Id))
                    .IsIdentity()
                    .IsPrimaryKey()
                .Property(x => x.Name)
                    .HasColumnName(nameof(Album.Name))
                .Property(x => x.ArtistId)
                    .HasColumnName("Artist")
                .Property(x => x.LabelId)
                    .HasColumnName("Label")
                .Property(x => x.ImageSource)
                    .SkipOnEntityFetch()
                    .HasSkipOnInsert()
                    .HasSkipOnUpdate()
                .Property(x => x.Artist)
                    .HasAttribute(new AssociationAttribute { ThisKey = nameof(Album.ArtistId), OtherKey = nameof(Artist.Id) })
                .Property(x => x.Label)
                    .HasAttribute(new AssociationAttribute { ThisKey = nameof(Album.LabelId), OtherKey = nameof(Label.Id) });
        }

        return schema;
    }
}
```

As you can see, it's a static class and we define the schema to match the Default schema (others can be added but that's the advanced manual) then we instantiate a `FluentMappingBuilder` and suddenly, all the world is ours.  The table is defined in a FLuent fashion, attributes are added as extension method calls. Each field is defined exactly the same way.  
  
On its own, this doesn't actually do anything. It needs to be called, so here's how.  

Your basic Linq2Db `DataConnection` class looks like this - 

```csharp
public class MusicalogData : DataConnection
{
    public MusicalogData(LinqToDbConnectionOptions<MusicalogData> options) : base(options)
    { }

    public ITable<Album> Albums => GetTable<Album>();
    public ITable<Artist> Artists => GetTable<Artist>();
    public ITable<Label> Labels => GetTable<Label>();
}
```  
  
What we do is add initializing in the logical place, the constructor.  
  
```csharp
public class MusicalogData : DataConnection
{
    private static MappingSchema mappingSchema = null;

    public MusicalogData(LinqToDbConnectionOptions<MusicalogData> options) : base(options)
    {
        if (mappingSchema != null)
        {
            return;
        }
        mappingSchema = Mappings.GetSchema();
    }

    public ITable<Album> Albums => GetTable<Album>();
    public ITable<Artist> Artists => GetTable<Artist>();
    public ITable<Label> Labels => GetTable<Label>();

}
```

Your fluent mappings are applied every time the constructor is initialized.    
  
There are loads of attributes and options you can apply in this fashion, `Association` especially being very powerful since you could use it with  - in this case -   
  
```csharp
var connection = new MusicalogData(options)
var album = await connection.Albums.LoadWith(x => x.Artist)
                                   .LoadWith(x => x.Label)
                                   .ToListAsync();
```

and retrieve a complete list of Albums with the Artist and Label object properties populated correctly.  
Happy coding!



