using System.Data;
using bd2.Application.Abstractions;
using bd2.Application.Services;
using bd2.Infrastructure.Repositories;
using bd2.Web.Auth;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection("Host=localhost;Port=5432;Database=postgres;Username=myuser;Password=mypassword;Include Error Detail=True"));
builder.Services.AddScoped(typeof(GenericRepository<>));
builder.Services.AddScoped<IHallRepository, HallRepository>();
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<IOriginRepository, OriginRepository>();
builder.Services.AddScoped<IWorkerRepository, WorkerRepository>();
builder.Services.AddScoped<IStagingRepository, StagingRepository>();
builder.Services.AddScoped<IPerformanceRepository, PerformanceRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<OriginsManagementService>();
builder.Services.AddScoped<InventoryManagementService>();
builder.Services.AddScoped<WorkerManagementService>();
builder.Services.AddScoped<StagingManagementService>();
builder.Services.AddScoped<PerformanceManagementService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();

    try
    {
        dbConnection.Open();
        using var transaction = dbConnection.BeginTransaction();

        string basePath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "bd2.Infrastructure", "SQL");
        string[] sqlFiles =
        {
            "DropTables.sql", "CreateTheatreSchemeDone.sql", "AddTestData.sql", "SupportDbFunctionsDone.sql",
            "TriggersDone.sql", "TransactionsDone.sql", "ViewsAndFunctionsDone.sql", "IndexesDone.sql", "Roles.sql"
        };

        foreach (var file in sqlFiles)
        {
            string filePath = Path.Combine(basePath, file);
            string script = File.ReadAllText(filePath);
            using var command = dbConnection.CreateCommand();
            command.CommandText = script.Trim();
            command.Transaction = transaction;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка выполнения SQL: {ex.Message}");
    }
    finally
    {
        dbConnection.Close();
    }
}

app.UseHsts();

app.UseHttpsRedirection();

app.UseRouting();
app.UseMiddleware<AuthMiddleware>();
app.UseExceptionHandler("/Error");
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();