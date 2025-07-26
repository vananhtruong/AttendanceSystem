using DataAccessLayer;
using Repository;
using WebAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BusinessObject;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using BusinessObject.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers()
    .AddOData(opt =>
    {
        var odataBuilder = new ODataConventionModelBuilder();
        odataBuilder.EntitySet<User>("Users");
        opt.AddRouteComponents("odata", odataBuilder.GetEdmModel())
            .Select().Filter().OrderBy().Expand().SetMaxTop(100).Count().SkipToken();
    });

builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<WorkScheduleDAO>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IWorkScheduleRepository, WorkScheduleRepository>();
builder.Services.AddScoped<AttendanceRecordDAO>();
builder.Services.AddScoped<IAttendanceRecordRepository, AttendanceRecordRepository>();
builder.Services.AddScoped<NotificationDAO>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<SalaryDAO>();
builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();
//builder.Services.AddScoped<CorrectionRequestDAO>();
//builder.Services.AddScoped<ICorrectionRequestRepository>();



builder.Services.AddAutoMapper(typeof(WebAPI.MappingProfiles.UserProfile));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });




// OData EDM Model
var odataBuilder = new ODataConventionModelBuilder();
odataBuilder.EntitySet<User>("Users");
odataBuilder.EntitySet<AttendanceRecord>("AttendanceRecords");
odataBuilder.EntitySet<WorkSchedule>("WorkSchedules");
odataBuilder.EntitySet<SalaryRecord>("SalaryRecords");
odataBuilder.EntitySet<CorrectionRequest>("CorrectionRequests");
odataBuilder.EntitySet<Notification>("Notifications");

//builder.Services.AddControllers()
//    .AddOData(opt =>
//        opt.AddRouteComponents("odata", odataBuilder.GetEdmModel())
//            .Select().Filter().OrderBy().Expand().Count().SetMaxTop(100));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRazor",
        policy =>
        {
            policy.WithOrigins("https://localhost:7192")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowRazor");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
