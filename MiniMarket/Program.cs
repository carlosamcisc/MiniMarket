using Microsoft.EntityFrameworkCore;
using MiniMarket.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<MiniMarketContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("miniMarket"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
     //pattern: "{controller=Home}/{action=Index}/{id?}")
     pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<MiniMarketContext>();

//    //  crea la base de datos si no existe
//    db.Database.EnsureCreated();
//}
//crear/actualizar BD automáticamente
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<MiniMarketContext>();
//    db.Database.Migrate();
//}

app.Run();
