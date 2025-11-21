using Microsoft.EntityFrameworkCore;
using ReviewService;

var builder = WebApplication.CreateBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;

// Подключение к БД
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Автоматическая миграция
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("DB migrated successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error connecting to DB: {ex.Message}");
    }
}
 // Swagger
app.UseSwagger();
app.UseSwaggerUI();

// HTTP 8080
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
