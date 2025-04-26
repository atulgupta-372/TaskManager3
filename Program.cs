using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TaskManager3.Data;
using TaskManager3.Models;
using TaskManager3.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add SQLite DB context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure JWT authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "DefaultKey"))
        };
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Manager API",
        Version = "v1"
    });
});

// Register AuthService
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Manager API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// WeatherForecast endpoint
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// Authentication endpoint
app.MapPost("/auth/login", async (AuthService authService, LoginRequest loginRequest) =>
{
    var token = await authService.Authenticate(loginRequest.Email, loginRequest.Password);
    if (token == null)
        return Results.Unauthorized();

    return Results.Ok(new { token });
});

// Board and TaskItem Management endpoints
app.MapPost("/boards", async (AppDbContext dbContext, Board board) =>
{
    dbContext.Boards.Add(board);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/boards/{board.Id}", board);
});

app.MapGet("/boards", async (AppDbContext dbContext) =>
{
    var boards = await dbContext.Boards.Include(b => b.Tasks).ToListAsync();
    return Results.Ok(boards);
});

app.MapPost("/tasks", async (AppDbContext dbContext, TaskItem task) =>
{
    dbContext.Tasks.Add(task);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/tasks/{task.Id}", task);
});

app.MapPut("/tasks/{id}", async (AppDbContext dbContext, int id, TaskItem updatedTask) =>
{
    var task = await dbContext.Tasks.FindAsync(id);
    if (task == null) return Results.NotFound();

    task.Title = updatedTask.Title;
    task.Description = updatedTask.Description;
    task.DueDate = updatedTask.DueDate;
    task.Priority = updatedTask.Priority;
    task.Status = updatedTask.Status;

    await dbContext.SaveChangesAsync();
    return Results.NoContent();
});

app.MapPut("/tasks/{taskId}/move", async (AppDbContext dbContext, int taskId, string newStatus) =>
{
    var task = await dbContext.Tasks.FindAsync(taskId);
    if (task == null) return Results.NotFound();

    task.Status = newStatus;
    await dbContext.SaveChangesAsync();
    return Results.Ok(task);
});

app.MapControllers();
app.Run();

// WeatherForecast class
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
