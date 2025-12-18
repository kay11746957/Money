using Money.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure logging for production
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<YahooFinanceService>();
builder.Services.AddSingleton<DcaCalculatorService>();
builder.Services.AddScoped<BacktestService>();
builder.Services.AddScoped<PortfolioBacktestService>();
builder.Services.AddSingleton<EtfSuggestionService>();

var app = builder.Build();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application starting in {Environment} environment", app.Environment.EnvironmentName);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Azure handles HTTPS, so we can skip redirection in production
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

logger.LogInformation("Application started successfully");

app.Run();
