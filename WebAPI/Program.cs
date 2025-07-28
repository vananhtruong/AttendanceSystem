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
using Microsoft.OpenApi.Models;
using BusinessObject.Models;
using Hangfire;
using BusinessObject.DTOs;

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

// Data Access Layer
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<AttendanceRecordDAO>();
builder.Services.AddScoped<WorkScheduleDAO>();
builder.Services.AddScoped<WorkShiftDAO>();

// Repository Layer
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAttendanceRecordRepository, AttendanceRecordRepository>();
builder.Services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IWorkScheduleRepository, WorkScheduleRepository>();
builder.Services.AddScoped<NotificationDAO>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<SalaryDAO>();
builder.Services.AddScoped<ISalaryRepository, SalaryRepository>();
builder.Services.AddScoped<CorrectionRequestDAO>();
builder.Services.AddScoped<ICorrectionRequestRepository, CorrectionRequestRepository>();

builder.Services.AddScoped<IFaceRecognitionService, FaceRecognitionService>();
builder.Services.AddScoped<IWorkScheduleEvaluatorService, WorkScheduleEvaluatorService>();
builder.Services.AddScoped<IWorkScheduleUpdateService, WorkScheduleUpdateService>();

builder.Services.AddScoped<ScheduleStatusService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();






builder.Services.AddAutoMapper(typeof(WebAPI.MappingProfiles.UserProfile), 
    typeof(WebAPI.MappingProfiles.AttendanceRecordProfile),
    typeof(WebAPI.MappingProfiles.CorrectionRequestProfile),
    typeof(WebAPI.MappingProfiles.NotificationProfile),
    typeof(WebAPI.MappingProfiles.SalaryRecordProfile),
    typeof(WebAPI.MappingProfiles.WorkScheduleProfile));

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

// Hangfire configuration
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();


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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowRazor",
        policy =>
        {
            policy.WithOrigins(
                "http://attendance-system-web.runasp.net",
                "https://attendance-system-web.runasp.net",
                "http://localhost:5135",
                "https://localhost:7192",
                "http://localhost:5000",
                "https://localhost:5001"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true); // Allow all origins for development
        });
});

builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo API", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
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
app.UseStaticFiles(); // Enable static files serving
app.UseCors("AllowRazor");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<ScheduleStatusService>(
    "update-schedule-status",
    service => service.UpdateNotYetToAbsent(),
    "0 * * * *" // mỗi giờ 0 phút
);
RecurringJob.AddOrUpdate<ScheduleStatusService>(
    "send-late-checkin-reminder",
    service => service.SendReminderForLateCheckIn(),
    "*/10 * * * *"  // mỗi 10 phút
);

app.Run();



