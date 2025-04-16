![Tests](https://github.com/BitzArt/XDoc.NET/actions/workflows/Tests.yml/badge.svg)

[![NuGet version](https://img.shields.io/nuget/v/BitzArt.XDoc.svg)](https://www.nuget.org/packages/BitzArt.XDoc/)
[![NuGet downloads](https://img.shields.io/nuget/dt/BitzArt.XDoc.svg)](https://www.nuget.org/packages/BitzArt.XDoc/)

A lightweight and efficient tool for parsing and managing C# XML documentation comments.


# BitzArt.XDoc

BitzArt.XDoc is a lightweight .NET library for parsing and accessing XML documentation comments from your C# code. It provides an intuitive API to retrieve documentation for:
* Types/classes
* Properties
* Methods
* Fields


```csharp

using BitzArt.XDoc;
...

var xdoc = new XDoc();

var typeDocs = xdoc.Get(typeof(MyType));
var propertyDocs = xDoc.Get(typeof(MyType).GetProperty(nameof(MyType.PropertyOne)));
```

# BitzArt.XDoc.PlainText

BitzArt.XDoc.PlainText is an extension package for BitzArt.XDoc that enables rendering XML documentation 
comments into plain text.

```csharp

using BitzArt.XDoc;
using Xdoc.Renderer.PlaintText;

...
    
var xdoc = new XDoc();

var typeDescrption = xDoc.Get(typeof(MyType)).ToPlainText();
var propertyDescrption = xDoc.Get(typeof(MyType).GetProperty(nameof(MyType.PropertyOne))).ToPlainText();

```

# BitzArt.XDoc.EntityFrameworkCore

BitzArt.XDoc.EntityFrameworkCore is an extension library that bridges XML documentation comments from C# 
code to Entity Framework Core database objects.


Example 1: Configure comments for all entities in your DbContext

```csharp
using BitzArt.XDoc;

public class MyDbContext : DbContext 
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure comments for all entities
        modelBuilder.ConfigureComments(new XDoc());
    }
}
```

Example 2: Configure comments for specific properties using Fluent API

```csharp
using BitzArt.XDoc;

...

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Customer>()
        .Property(c => c.Name)
        .HasComment<string, Customer>(c => c.Name);
}
```

## Recommendations
Each XDoc instance maintains its own cache of parsed documentation in a `ConcurrentDictionary<Assembly, AssemblyDocumentation>`. Using multiple instances unnecessarily duplicates this data in memory

If your application provides documentation-related services (like API documentation generation), maintaining a single pre-loaded XDoc instance can significantly improve response times.

### Possible approaches

#### Dependency Injection (Recommended)
Register XDoc as a singleton in your application's dependency injection container.

#### Factory
Create a factory that provides access to a shared XDoc instance:

By following these recommendations, you can ensure that XML documentation is parsed only once per assembly, maximizing performance and minimizing resource usage in your application.
