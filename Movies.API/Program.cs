using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Movies.API.Contexts;
using Movies.API.Services;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    // Return a 406 when an unsupported media type was requested
    options.ReturnHttpNotAcceptable = true;

    // Add XML formatters
    //options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
    //options.InputFormatters.Add(new XmlSerializerInputFormatter(options));

    // Set XML as default format instead of JSON - the first formatter in the 
    // list is the default, so we insert the input/output formatters at 
    // position 0
    options.OutputFormatters.Insert(0,new XmlSerializerOutputFormatter());
    options.InputFormatters.Insert(0, new XmlSerializerInputFormatter(options));
}).AddNewtonsoftJson(setupAction =>
{
    setupAction.SerializerSettings.ContractResolver =
       new CamelCasePropertyNamesContractResolver();
});

// add support for compressing responses (eg gzip)
builder.Services.AddResponseCompression();


// suppress automatic model state validation when using the 
// ApiController attribute (as it will return a 400 Bad Request
// instead of the more correct 422 Unprocessable Entity when
// validation errors are encountered)
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// register the DbContext on the container, getting the connection string from
// appSettings (note: use this during development; in a production environment,
// it's better to store the connection string in an environment variable)
var connectionString = builder.Configuration["ConnectionStrings:MoviesDBConnectionString"];
builder.Services.AddDbContext<MoviesContext>(o => o.UseSqlite(connectionString));

builder.Services.AddScoped<IMoviesRepository, MoviesRepository>();
builder.Services.AddScoped<IPostersRepository, PostersRepository>();
builder.Services.AddScoped<ITrailersRepository, TrailersRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Movies API", Version = "v1" });
});

var app = builder.Build();


// use response compression (client should pass through 
// Accept-Encoding)
app.UseResponseCompression();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("swagger/v1/swagger.json", "Movies API (v1)");
    // serve UI at root
    c.RoutePrefix = string.Empty;
});

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// app.MapControllers();

// For demo purposes: clear the database 
// and refill it with dummy data  
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetService<MoviesContext>();
        // delete the DB if it exists
        context.Database.EnsureDeleted();
        // migrate the DB - this will also seed the DB with dummy data
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.Run();
