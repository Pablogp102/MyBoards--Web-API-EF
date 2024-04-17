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


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyBoardsContext>(
    option => option.UseSqlServer(builder.Configuration.GetConnectionString("MyBoardsConnectionString"))
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
app.MapGet("data", async (MyBoardsContext db) =>
{
    var authorsCommentsCountsQuery = db.Comments
        .GroupBy(c => c.AuthorId)
        .Select(g => new { g.Key, Count = g.Count() });



    var authorsCommentsCounts = await authorsCommentsCountsQuery.ToListAsync();


    var topAuthor = authorsCommentsCounts
    .First(a => a.Count == authorsCommentsCounts.Max(acc => acc.Count));

    var userDetails = db.Users.First(u => u.Id == topAuthor.Key);
    
    return new { userDetails, commentCout = topAuthor.Count };
});

app.MapPost("update", async (MyBoardsContext db) =>
{
    var epic = await db.Epics.FirstAsync(epic => epic.Id == 1);

    var rejectedState = await db.WorkItemStates.FirstAsync(epic => epic.Value == "Rejected");

    epic.State = rejectedState;

    await db.SaveChangesAsync();

    return epic;
});

app.MapPost("create/{className}/{id}/{value}", async (MyBoardsContext db, string className, string id, string value) =>
{
    // SprawdŸ, czy istnieje klasa o podanej nazwie
    var classType = Type.GetType($"MyBoards.Entities.{className}");

    if (classType == null)
    {
        return $"Klasa {className} nie istnieje.";
    }

    // SprawdŸ, czy podane ID jest unikalne
    var existingEntity = await db.FindAsync(classType, int.Parse(id));
    if (existingEntity != null)
    {
        return $"Encja o identyfikatorze {id} ju¿ istnieje w klasie {className}.";
    }

    // Utwórz now¹ instancjê encji
    var entity = Activator.CreateInstance(classType);

    // Ustaw wartoœæ dla w³aœciwoœci "Value" encji
    var valueProperty = classType.GetProperty("Value");
    valueProperty.SetValue(entity, value);

    // Ustawiæ pobieranie w³asciwosci z poszczegolnych klas tak aby mozna bylo ustawiaæ ich wartoœci z poziomu swaggera :D





    // Dodaj encjê do kontekstu bazy danych
    db.Add(entity);
    await db.SaveChangesAsync();

    return $"Nowa encja zosta³a utworzona w klasie {className} z identyfikatorem {id} i wartoœci¹ {value}.";
});





app.MapPost("delete/{className}/{id}", async (MyBoardsContext db, string className, string id) =>
{
    // SprawdŸ, czy istnieje klasa o podanej nazwie
    var classType = Type.GetType($"MyBoards.Entities.{className}");
    
    if (classType == null)
    {
        return $"Klasa {className} nie istnieje.";
    }

    // Pobierz w³aœciwoœæ Id z klasy i jej typ
    var idProperty = classType.GetProperty("Id");
    if (idProperty == null)
    {
        return $"Klasa {className} nie posiada w³aœciwoœci Id";
    }
    var idType = idProperty.PropertyType;

    // Parsuj identyfikator do odpowiedniego typu
    object entityId;
    try
    {
        entityId = Convert.ChangeType(id, idType);
    }
    catch(FormatException)
    {
        return $"Nieprawid³owy format identyfikatora dla klasy {className}.";
    }
    catch(InvalidCastException)
    {
        return $"Nie mo¿na przekonwertowaæ identyfikatora do w³aœciwego typu dla klasy {className}.";
    }
    catch (OverflowException)
    {
        return $"Przekroczono zakres dla identyfikatora klasy {className}.";
    }

    // Spróbuj znaleŸæ encjê o podanym identyfikatorze w bazie danych
    var findMethod = typeof(DbContext).GetMethod("Find", new Type[] { typeof(object[])}).MakeGenericMethod(classType);
    var entity = findMethod.Invoke(db, new object[] { new[] { entityId } });


    // Spróbuj znaleŸæ encjê o podanej wartoœci w bazie danych
  
    if (entity == null)
    {
        return $"Encja o wartoœci {id} nie istnieje w klasie {className}.";
       
    }
    else
    {
        // Usuñ encjê
        db.Remove(entity);
        await db.SaveChangesAsync();
        return $"Encja o wartoœci {id} zosta³a usuniêta z klasy {className}.";
    }
});



app.Run();


