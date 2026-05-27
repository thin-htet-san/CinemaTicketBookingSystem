using CinemaTicketBookingSystem.Database.AppDbContextModels;
using CinemaTicketBookingSystem.WebApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Server=.;Database=CinemaTicketBookingSystem;User ID=sa;Password=sasa@123;TrustServerCertificate=True;"));

builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IShowtimeService, ShowtimeService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<ICinemaBranchService, CinemaBranchService>();
builder.Services.AddScoped<ITheaterHallService, TheaterHallService>();
builder.Services.AddScoped<IReportService, ReportService>();

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
