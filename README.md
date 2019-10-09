# MongoDelta

![Azure DevOps builds (branch)](https://img.shields.io/azure-devops/build/adrianbradfield/df1cab1e-21bf-4a8e-a335-29d7a5b730ab/8/master?style=for-the-badge) ![Azure DevOps tests (compact)](https://img.shields.io/azure-devops/tests/adrianbradfield/df1cab1e-21bf-4a8e-a335-29d7a5b730ab/8/master?compact_message&style=for-the-badge) ![Azure DevOps coverage (branch)](https://img.shields.io/azure-devops/coverage/adrianbradfield/df1cab1e-21bf-4a8e-a335-29d7a5b730ab/8/master?style=for-the-badge) ![Nuget](https://img.shields.io/nuget/v/MongoDelta?style=for-the-badge) ![Nuget](https://img.shields.io/nuget/dt/MongoDelta?style=for-the-badge)

Implements a UnitOfWork pattern with change tracking for the C# MongoDb driver

To use this library, you can install it into your project from [Nuget](https://www.nuget.org/packages/MongoDelta/)
There are also extension methods available for integrating your UnitOfWork with ASP.NET Core 3's dependency injection from [Nuget](https://www.nuget.org/packages/MongoDelta.AspNetCore3/)

## The aims of this library are:
- Create a UnitOfWork base class with change tracking, for use with MongoDb
- Allow domain models to be incrementally updated (i.e. only properties that have changed should be updated)
- Integrate as closely as possible with the MongoDb C# driver for mapping
- Require no implementation leakage (i.e. data access logic) into the domain model

## Roadmap:
- [x] *V1.0* - UnitofWork pattern with change tracking
- [x] *V1.1* - Integrate with ASP.NET Core for proper dependency injection behavior
- [x] *V1.2* - Incremental updates for the root document
- [ ] *V1.3* - Incremental updates for sub-documents and numeric fields
- [ ] *V1.4* - Incremental updates for collection fields

## Getting Started
For this walkthrough, I will just be using a simple UserAggregate class for my domain model

```cs
class UserAggregate
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }

    private UserAggregate(){}

    public UserAggregate(string firstName, string surname)
    {
        FirstName = firstName;
        Surname = surname;
    }
}
```

Note that the parameterless private constructor is required for the MongoDb driver to create an instance of it

### Creating the UnitOfWork
To create a UnitOfWork that can persist this aggregate, you need to create a new class that extends from UnitOfWorkBase
```cs
class UserUnitOfWork : UnitOfWorkBase
{
    public UserUnitOfWork(IMongoDatabase database) : base(database, useTransactions: false)
    {
        RegisterRepository<UserAggregate>("users");
    }
    public MongoDeltaRepository<UserAggregate> Users => GetRepository<UserAggregate>();
}
```
The constructor for the base class requires an instance of IMongoDatabase. There is also an optional parameter that sets whether multi-document transactions should be used which defaults to true.

You can tell the unit of work to generate a repository for you by using the `RegisterRepository` method, passing in the model type, and the name of the collection to use for that type.

To access the repository outside the unit of work, you can create a get only property which calls the `GetRepository` method.

### Querying for documents
On the repository, there are 2 methods for querying:
- `QueryAsync` - Returns a readonly collection of models that match the filter
- `QuerySingleAsync` - Returns a single model that matches the filter or null. If there is more than one model that matches the filter, it will throw an exception

```cs
async Task QueryExample(Guid id){
  var unitOfWork = new UnitOfWork(_database);
  var allBobs = await unitOfWork.Users.QueryAsync(u => u.FirstName == "Bob");
  var singleUser = await unitOfWork.Users.QuerySingleAsync(u => u.Id == id);
}
```

### Adding a document
The repository also has an `Add` method that can be used to store a new model
```cs
async Task<Guid> AddExample()
{
  var unitOfWork = new UnitOfWork(_database);
  var user = new UserAggregate("John", "Smith");
  
  unitOfWork.Users.Add(user);
  await unitOfWork.CommitAsync();
  
  return user.Id;
}
```
Here we have added the user to the repository and committed the UnitOfWork. The users `Id` property has been automatically populated by the MongoDb driver

### Updating a document
```cs
async Task UpdateExample(Guid id)
{
  var unitOfWork = new UnitOfWork(_database);
  var user = await unitOfWork.Users.QuerySingleAsync(u => u.Id == id);
  
  user.FirstName = "Joan";
  await unitOfWork.CommitAsync();
}
```
Here MongoDelta has started tracking changes to the user model once it has been queried from the repository. When the `CommitAsync` method is called on the UnitOfWork, it will detect the user as being changed, and update it.

The change tracking works with the mapping set for the MongoDb driver. If you are mapping to backing fields, these changes will be picked up. Similarly if you aren't tracking a property at all, a change to that property won't cause the document to be updated.

### Removing a document
Removing a document works much the same way as adding one
```cs
async Task RemoveExample(Guid id)
{
  var unitOfWork = new UnitOfWork(_database);
  var user = await unitOfWork.Users.QuerySingleAsync(u => u.Id == id);
  
  unitOfWork.Users.Remove(user);
  await unitOfWork.CommitAsync();
}
```
The document you are removing must have been queried from a repository belonging to the same unit of work, otherwise it will throw an exception

### Updating only changed properties
You can set a class to update only changed properties in the same ways that you map the classes with the MongoDb Driver. To do it with code, you can use:
```cs
BsonClassMap.RegisterClassMap<AddressAggregate>(map =>
{
	map.UseDeltaUpdateStrategy();
});
```

Or alternatively using an attribute:
```cs
[UseDeltaUpdateStrategy]
public class AddressAggregate
{
	public Guid Id { get; set; }
	public string HouseNumber { get; set; }
	public string HouseName { get; set; }
	public string Street { get; set; }
	public string Town { get; set; }
	public string Postcode { get; set; }
}
```

When doing this, the model will be updated using an UpdateOne command rather than a ReplaceOne command, and only changed properties will be included in the update.

To update only changed properties on sub-objects, you need to make sure that the sub-objects type is also mapped to use the delta update strategy.

### Update numeric fields incrementally
It's possible to update numeric properties incrementally. To be able to do this, the property must be serializable to one of the BSON types: Int32, Int64, Double or Decimal128.
The class that contains the property must also use the delta update strategy. To map a property to be updated incrementally, you can do this in code by calling the `UpdateIncrementally` on the member map:
```cs
BsonClassMap.RegisterClassMap<IncrementNumeralsAggregate>(map =>
{
	map.UseDeltaUpdateStrategy();
	map.MapProperty(m => m.Integer).UpdateIncrementally();
});
```
Or alternatively you can use attributes:
```cs
[UseDeltaUpdateStrategy]
class IncrementalUpdateAggregate
{
	public Guid Id { get; set; }

	[UpdateIncrementally]
	public int Integer { get; set; }

	[UpdateIncrementally]
	public long Long { get; set; }

	[UpdateIncrementally]
	[BsonRepresentation(BsonType.Decimal128)]
	public decimal Decimal { get; set; }
}
```

As you can see from the above example, to be able to incrementally update a decimal, you need to explicitly tell MongoDB to serialize it as the Decimal128 type.
This is due to decimals being serialised as a string by default.

### Integration with ASP.NET Core 3
First you will need to intall this library from [Nuget](https://www.nuget.org/packages/MongoDelta.AspNetCore3/) into your application. This library provides a couple of extension methods to use when configuring your web application. `IApplicationBuilder.UseUnitOfWork` adds middleware to your application pipeline which creates a new instance of your UnitOfWork class for every request, whereas the `IServiceCollection.AddUnitOfWork` sets up the dependency injection to allow you to get the correct instance of your UnitOfWork when injecting it. These methods can be used like this:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();
    services.AddUnitOfWork<IUnitOfWork, UnitOfWork>();

    var client = new MongoClient("mongodb://localhost:27017/?retryWrites=false");
    var database = client.GetDatabase("AspNetCore3_Example");
    services.AddSingleton(database);
}

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseUnitOfWork<UnitOfWork>();
    app.UseRouting();

    ...
    
}
```

Notice that when you register your UnitOfWork with the service collection, you also have to provide an interface. It is important that you always use this interface when injecting your UnitOfWork, as if you use the concrete class, you will get a new instance every time.

You also need to register any dependencies that your UnitOfWork requires. In this example, the UnitOfWork required an instance of `IMongoDatabase`, so it has been registered as a Singleton.

The UnitOfWork is also registered transiently, this means that you should never inject it into another dependency that is registered as a Singleton.

## Performance
The performance of using this library has been benchmarked against doing similar operations directly with the Mongo DB driver. The benchmarking 
is performed against a local single node Mongo DB instance. The source code for the benchmarking can be found in the project `MongoDelta.Benchmarking`

### Configuration
``` ini

BenchmarkDotNet=v0.11.5, OS=Windows 10.0.18362
Intel Core i7-7500U CPU 2.70GHz (Kaby Lake), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.0.100-rc1-014190
  [Host]     : .NET Core 3.0.0-rc1-19456-20 (CoreCLR 4.700.19.45506, CoreFX 4.700.19.45604), 64bit RyuJIT
  DefaultJob : .NET Core 3.0.0-rc1-19456-20 (CoreCLR 4.700.19.45506, CoreFX 4.700.19.45604), 64bit RyuJIT

```

### Inserts

|                         Method | NumberOfRecords |     Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------------------- |---------------- |---------:|----------:|----------:|------:|--------:|
| **&#39;Mongo DB Driver - InsertMany&#39;** |               **1** | **21.99 ms** | **0.2746 ms** | **0.2293 ms** |  **1.00** |    **0.00** |
|                     MongoDelta |               1 | 21.90 ms | 0.4272 ms | 0.3996 ms |  0.99 |    0.01 |
|                                |                 |          |           |           |       |         |
| **&#39;Mongo DB Driver - InsertMany&#39;** |              **50** | **22.56 ms** | **0.4361 ms** | **0.4283 ms** |  **1.00** |    **0.00** |
|                     MongoDelta |              50 | 22.89 ms | 0.4522 ms | 0.4441 ms |  1.02 |    0.03 |
|                                |                 |          |           |           |       |         |
| **&#39;Mongo DB Driver - InsertMany&#39;** |             **500** | **26.14 ms** | **0.4514 ms** | **0.4223 ms** |  **1.00** |    **0.00** |
|                     MongoDelta |             500 | 28.74 ms | 0.4237 ms | 0.3756 ms |  1.10 |    0.02 |

### Replace

|                           Method | NumberOfRecords |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------------------------- |---------------- |----------:|----------:|----------:|------:|--------:|
| **&#39;Mongo DB Driver - Bulk Replace&#39;** |               **1** |  **1.420 ms** | **0.0328 ms** | **0.0306 ms** |  **1.00** |    **0.00** |
|  &#39;MongoDelta - Replace Strategy&#39; |               1 |  1.586 ms | 0.0129 ms | 0.0108 ms |  1.11 |    0.02 |
|                                  |                 |           |           |           |       |         |
| **&#39;Mongo DB Driver - Bulk Replace&#39;** |              **50** |  **4.389 ms** | **0.0442 ms** | **0.0414 ms** |  **1.00** |    **0.00** |
|  &#39;MongoDelta - Replace Strategy&#39; |              50 |  4.960 ms | 0.0990 ms | 0.0972 ms |  1.13 |    0.03 |
|                                  |                 |           |           |           |       |         |
| **&#39;Mongo DB Driver - Bulk Replace&#39;** |             **500** | **29.488 ms** | **0.5826 ms** | **0.4548 ms** |  **1.00** |    **0.00** |
|  &#39;MongoDelta - Replace Strategy&#39; |             500 | 33.359 ms | 0.4267 ms | 0.3992 ms |  1.13 |    0.02 |

### Update

|                               Method | NumberOfRecords |      Mean |     Error |    StdDev | Ratio | RatioSD |
|------------------------------------- |---------------- |----------:|----------:|----------:|------:|--------:|
|      **&#39;Mongo DB Driver - Bulk Update&#39;** |               **1** |  **1.446 ms** | **0.0165 ms** | **0.0129 ms** |  **1.00** |    **0.00** |
| &#39;MongoDelta - Delta Update Strategy&#39; |               1 |  1.750 ms | 0.0344 ms | 0.0526 ms |  1.23 |    0.04 |
|                                      |                 |           |           |           |       |         |
|      **&#39;Mongo DB Driver - Bulk Update&#39;** |              **50** |  **5.651 ms** | **0.1074 ms** | **0.1054 ms** |  **1.00** |    **0.00** |
| &#39;MongoDelta - Delta Update Strategy&#39; |              50 |  7.778 ms | 0.1115 ms | 0.0989 ms |  1.38 |    0.03 |
|                                      |                 |           |           |           |       |         |
|      **&#39;Mongo DB Driver - Bulk Update&#39;** |             **500** | **41.686 ms** | **1.2455 ms** | **1.8642 ms** |  **1.00** |    **0.00** |
| &#39;MongoDelta - Delta Update Strategy&#39; |             500 | 58.756 ms | 0.8969 ms | 0.7951 ms |  1.41 |    0.07 |
