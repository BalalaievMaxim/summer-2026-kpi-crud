using System.Text.Json.Serialization;
using System.Text;
using GymManagement.Application.Services;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Clients;
using GymManagement.Domain.Coaches;
using GymManagement.Domain.Enrollments;
using GymManagement.Domain.Ports;
using GymManagement.Infrastructure.Middleware;
using GymManagement.Infrastructure.Security;
using GymManagement.Infrastructure;
using GymManagement.Infrastructure.Persistence;
using GymManagement.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<GymManagementContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IMembershipRepositoryPort, MembershipRepository>();
builder.Services.AddScoped<IMembershipPlanRepositoryPort, MembershipPlanRepository>();
builder.Services.AddScoped<IInvoiceRepositoryPort, InvoiceRepository>();
builder.Services.AddScoped<IClassTypeRepositoryPort, ClassTypeRepository>();
builder.Services.AddScoped<IClassScheduleRepository, ClassRepository>();
builder.Services.AddScoped<IEnrollmentRepositoryPort, EnrollmentRepository>();

builder.Services.AddScoped<ClientRepository>();
builder.Services.AddScoped<IClientRepository>(sp => sp.GetRequiredService<ClientRepository>());
builder.Services.AddScoped<IClientAnalyticsRepository>(sp => sp.GetRequiredService<ClientRepository>());

builder.Services.AddScoped<ICoachRepository, CoachRepository>();

builder.Services.AddScoped<ClassFactory>();
builder.Services.AddScoped<EnrollmentFactory>();

builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<ICoachService, CoachService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IMembershipPlanService, MembershipPlanService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("JWT options are not configured.");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddCors(options =>
{
    var frontendUrl = builder.Configuration.GetValue<string>("Cors:FrontendUrl") ?? "http://localhost:5173";
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GymManagementContext>();
        await DbInitializer.InitializeAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
