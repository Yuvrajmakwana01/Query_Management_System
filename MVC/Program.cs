using Npgsql;
using Repository.Implementations;
using Repository.Interfaces;
using Repository.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IAdminInterface, AdminRepository>();
builder.Services.AddScoped<IAccountInterface,AccountRepository>();
builder.Services.AddScoped<IEmployeeInterface,EmployeeRepository>();
builder.Services.AddScoped<IUserInterface, UserRepository>();
builder.Services.AddScoped<IQueryInterface, QueryRepository>();
// Custom Redis service
builder.Services.AddSingleton<RedisServices>();


builder.Services.AddScoped<NpgsqlConnection>(conn =>
{
    var connectionString = conn.GetRequiredService<IConfiguration>().GetConnectionString("pgconn");
    return new NpgsqlConnection(connectionString);
});


// Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var redisConnectionString = configuration.GetConnectionString("Redis");
    if (string.IsNullOrEmpty(redisConnectionString))
        throw new InvalidOperationException("Redis connection string is missing in appsettings.json");

    return ConnectionMultiplexer.Connect(redisConnectionString);
});


// Redis database
builder.Services.AddSingleton<IDatabase>(sp =>
{
    var multiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
    return multiplexer.GetDatabase();
});


// Redis cache for session
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis"); // reuse same connection string
    options.InstanceName = "Session_";
});


builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// async Task IndexDataOnStartup()
// {
//     using var scope = app.Services.CreateScope();
//     var contactRepo = scope.ServiceProvider.GetRequiredService<ContactBAL>();
//     var esService = scope.ServiceProvider.GetRequiredService<ElasticsearchService>();
//     try
//     {
//         await esService.CreateIndexAsync();
//         var contacts = await contactRepo.GetAll();
//         if (contacts.Count > 0)
//         {
//             foreach (var contact in contacts)
//             {
//                 await esService.IndexContactAsync(contact);
//             }

//             Console.WriteLine($"✅ {contacts.Count} contacts indexed successfully in ElasticSearch.");
//         }
//         else
//         {
//             Console.WriteLine("⚠️ No contacts found in PostgreSQL.");
//         }
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"❌ Error indexing contacts: {ex.Message}");
//     }
// }


// await IndexDataOnStartup();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
