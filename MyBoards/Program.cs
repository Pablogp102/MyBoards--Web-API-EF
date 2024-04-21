using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;
using MyBoards.Entities;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using System.Xml;
using MyBoards.Dto;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<JsonOptions>(options => 
{ options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; options.SerializerOptions.IncludeFields = true; 
});

builder.Services.AddDbContext<MyBoardsContext>(
    option => option
    //.UseLazyLoadingProxies()
    .UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsConnectionString"))
    );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    { 
     c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<MyBoardsContext>();

var pendingMigations = dbContext.Database.GetPendingMigrations();
if (pendingMigations.Any())
{
    dbContext.Database.Migrate();
}

var users = dbContext.Users.ToList();
if (!users.Any())
{
    var user1 = new User()
    {
        Email = "User1@test.com",
        FullName = "User One",
        Address = new Address()
        {
            City = "Warszawa",
            Street = "Szeroka"
        }
    };

    var user2 = new User()
    {
        Email = "User2@test.com",
        FullName = "User Two",
        Address = new Address()
        {
            City = "Kraków",
            Street = "D³uga"
        }
    };

    dbContext.Users.AddRange(user1, user2);

    dbContext.SaveChanges();
}



app.MapGet("pagination", async (MyBoardsContext db) =>
{
// user imput 
var filter = "a";
string sortBY = "FullName";
bool sortByDescending = false;
int pageNumber = 1;
int pageSize = 10;
//

var query = db.Users
    .Where(u => filter == null ||
    (u.Email.ToLower().Contains(filter.ToLower()) || 
     u.FullName.ToLower().Contains(filter.ToLower()))
     );

var totalCount = query.Count();


if (sortBY != null)
{
    var columnsSelector = new Dictionary<string, Expression<Func<User, object>>>
    {
        { nameof(User.Email), user => user.Email },
        { nameof(User.FullName), user => user.FullName },

    };

    var sortByExpression = columnsSelector[sortBY];

    query = sortByDescending
    ? query.OrderByDescending(sortByExpression)
    : query.OrderBy(sortByExpression);
    query.OrderBy(sortByExpression);
}

    var result = query.Skip(pageSize * (pageNumber - 1))
                      .Take(pageSize)
                      .ToList();
    var pagedResult = new PagedResult<User>(result, totalCount, pageSize, pageNumber);

    return pagedResult;

});

app.MapGet("data1", async (MyBoardsContext db) =>
{
    var userComments = await db.Users
        .Include(u => u.Address)
        .Include(u => u.Comments)
        .Where(u => u.Address.Country == "Albania")
        .SelectMany(u => u.Comments.Select(c => new
        {
            FullName = u.FullName,
            Comment = c.Message
        }))
        .ToListAsync();

    return userComments;
});

app.MapGet("data2", async (MyBoardsContext db) =>
{
    var users = await db.Users
               .Include(u => u.Address)
               .Include(u => u.Comments)
               .Where(u => u.Address.Country == "Albania")
               .ToListAsync();

    foreach(var user in users)
    {
        
        foreach(var comment in user.Comments)
        {
            //Process
        }
    }


});

app.MapGet("data", async (MyBoardsContext db) =>
{
    var withAddress = true;
    var user = db.Users
    .First(u => u.Id == Guid.Parse("EBFBD70D-AC83-4D08-CBC6-08DA10AB0E61"));

    if (withAddress)
    {
        var result = new { FullName = user.FullName, Address = $"{user.Address.Street} {user.Address.City}" };
        return result;
    }
    return new { FullName = user.FullName, Address = "-" };
});

app.MapPost("update", async (MyBoardsContext db) =>
{
    var epic = await db.Epics.FirstAsync(epic => epic.Id == 1);

    var rejectedState = await db.WorkItemStates.FirstAsync(epic => epic.Value == "Rejected");

    epic.State = rejectedState;

    await db.SaveChangesAsync();

    return epic;
});

app.MapPost("create", async (MyBoardsContext db) =>
{
    var address = new Address()
    {
        Id = Guid.Parse("b323dd7c-776a-4cf6-a92a-12df154b4a2c"),
        City = "Kraków",
        Country = "Poland",
        Street = "D³uga"
    };

    var user = new User()
    {
        Email = "user@test.com",
        FullName = "Test User",
        Address = address
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();
    return user;
});

app.MapDelete("delete", async (MyBoardsContext db) => 
{
    var user = await db.Users
    .Include(u => u.Comments)
    .FirstAsync(u => u.Id == Guid.Parse("4EBB526D-2196-41E1-CBDA-08DA10AB0E61"));


    
    db.Remove(user);
    await db.SaveChangesAsync();
});

app.MapPost("delete/{className}/{id}", async (MyBoardsContext db, string className, string id) =>
{
   
    var classType = Type.GetType($"MyBoards.Entities.{className}");
    
    if (classType == null)
    {
        return $"Klasa {className} nie istnieje.";
    }

  
    var idProperty = classType.GetProperty("Id");
    if (idProperty == null)
    {
        return $"Klasa {className} nie posiada w³aœciwoœci Id";
    }
    var idType = idProperty.PropertyType;

   
    object entityId;
    try
    {
        if (idType == typeof(Guid))
        {
            entityId = Guid.Parse(id);
        }
        else if (idType == typeof(int))
        {
            entityId = int.Parse(id);
        }
        else if (idType == typeof(string))
        {
            entityId = id;
        }
        
        else
        {
            throw new NotSupportedException($"Typ identyfikatora {idType} nie jest obs³ugiwany.");
        }
    }
    catch (FormatException)
    {
        return $"Nieprawid³owy format identyfikatora dla klasy {className}.";
    }
    catch (InvalidCastException)
    {
        return $"Nie mo¿na przekonwertowaæ identyfikatora do w³aœciwego typu dla klasy {className}.";
    }
    catch (OverflowException)
    {
        return $"Przekroczono zakres dla identyfikatora klasy {className}.";
    }


    var findMethod = typeof(DbContext).GetMethod("Find", new Type[] { typeof(object[])}).MakeGenericMethod(classType);
    var entity = findMethod.Invoke(db, new object[] { new[] { entityId } });


  
    if (entity == null)
    {
        return $"Encja o wartoœci {id} nie istnieje w klasie {className}.";
       
    }
    else
    {
     
        db.Remove(entity);
        await db.SaveChangesAsync();
        return $"Encja o wartoœci {id} zosta³a usuniêta z klasy {className}.";
    }
});



app.Run();


