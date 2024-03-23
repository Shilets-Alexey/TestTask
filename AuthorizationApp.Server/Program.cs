using AuthorizationApp.BusinessLogic.Mapping;
using AuthorizationApp.BusinessLogic.Services;
using AuthorizationApp.DataBase.Context;
using AuthorizationApp.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
string connection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connection));
builder.Services.AddAuthorization();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<AdminsService>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(AppMappingProfile));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddAntiforgery();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseExceptionHandler();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseDeveloperExceptionPage();
app.MapControllers();

app.MapFallbackToFile("/index.html");
app.Run();
