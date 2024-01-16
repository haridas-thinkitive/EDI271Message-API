using EDIViewer_Core.Interfaces.MessageOperation;
using EDIViewer_Core.Services.MessageOperation;
using EDIViewer_DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

//Log Services

builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());

var config = new ConfigurationBuilder()

  .SetBasePath(System.IO.Directory.GetCurrentDirectory())
  .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();

NLog.Config.LoggingConfiguration nlogConfig = new NLogLoggingConfiguration(config.GetSection("NLog"));

builder.Logging.ClearProviders();


// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Services
builder.Services.AddScoped<IMessageOperation, MessageOperationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
