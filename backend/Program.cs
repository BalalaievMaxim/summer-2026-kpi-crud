using System.Text.Json.Serialization;
using System.Text;
using GymManagement.Application.Abstractions.Messaging;
using GymManagement.Application.Services;
using GymManagement.Application.Services.Interfaces;
using GymManagement.Application.DTOs;
using GymManagement.Application.Features.Auth.Queries.LoginClient;
using GymManagement.Application.Features.Classes.Commands.CreateClass;
using GymManagement.Application.Features.Classes.Commands.DeleteClass;
using GymManagement.Application.Features.Classes.Commands.RescheduleClass;
using GymManagement.Application.Features.Classes.Queries.GetClassAttendanceAnalytics;
using GymManagement.Application.Features.Classes.Queries.GetClassById;
using GymManagement.Application.Features.Classes.Queries.GetCoachEfficiencyAnalytics;
using GymManagement.Application.Features.Classes.Queries.GetCoachWorkload;
using GymManagement.Application.Features.Classes.Queries.GetScheduleForDate;
using GymManagement.Application.Features.Classes.Queries.GetScheduleForWeek;
using GymManagement.Application.Features.Clients.Commands.DeleteClient;
using GymManagement.Application.Features.Clients.Commands.RegisterClient;
using GymManagement.Application.Features.Clients.Commands.UpdateClient;
using GymManagement.Application.Features.Clients.Queries.GetClientActivityAnalytics;
using GymManagement.Application.Features.Clients.Queries.GetClientById;
using GymManagement.Application.Features.Clients.Queries.SearchClients;
using GymManagement.Application.Features.Clients.ReadModels;
using GymManagement.Application.Features.Coaches.Commands.DeleteCoach;
using GymManagement.Application.Features.Coaches.Commands.RegisterCoach;
using GymManagement.Application.Features.Coaches.Commands.UpdateCoachSpecialization;
using GymManagement.Application.Features.Coaches.Queries.GetAllCoaches;
using GymManagement.Application.Features.Coaches.Queries.GetCoachById;
using GymManagement.Application.Features.Coaches.Queries.GetCoachesBySpecialization;
using GymManagement.Application.Features.Coaches.ReadModels;
using GymManagement.Application.Features.Enrollments.Commands.CreateEnrollment;
using GymManagement.Application.Features.Invoices.Commands.CreateInvoice;
using GymManagement.Application.Features.Invoices.Commands.MarkInvoicePaid;
using GymManagement.Application.Features.Invoices.Queries.GetMonthlyRevenueByPlan;
using GymManagement.Application.Features.Invoices.Queries.GetPendingInvoicesForClient;
using GymManagement.Application.Features.MembershipPlans.Commands.CreateMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Commands.DeleteMembershipPlan;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlanById;
using GymManagement.Application.Features.MembershipPlans.Queries.GetMembershipPlans;
using GymManagement.Application.Features.Memberships.Commands.PurchaseMembership;
using GymManagement.Application.Features.Memberships.Queries.GetActiveMembershipsByClient;
using GymManagement.Domain.Billing;
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
builder.Services.AddScoped<IInvoiceAnalyticsRepository, InvoiceAnalyticsRepository>();
builder.Services.AddScoped<IClassTypeRepositoryPort, ClassTypeRepository>();
builder.Services.AddScoped<IClassRepositoryPort, ClassRepository>();
builder.Services.AddScoped<IClassScheduleRepository, ClassReadRepository>();
builder.Services.AddScoped<IEnrollmentRepositoryPort, EnrollmentRepository>();

builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientAnalyticsRepository, ClientAnalyticsRepository>();

builder.Services.AddScoped<ICoachRepository, CoachRepository>();

builder.Services.AddScoped<ClassFactory>();
builder.Services.AddScoped<EnrollmentFactory>();
builder.Services.AddScoped<InvoiceFactory>();

builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddScoped<ICommandHandler<CreateClassCommand, int>, CreateClassCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RescheduleClassCommand>, RescheduleClassCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteClassCommand>, DeleteClassCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetClassByIdQuery, GymClassDetails?>, GetClassByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetScheduleForDateQuery, IReadOnlyList<GymClassDetails>>, GetScheduleForDateQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetScheduleForWeekQuery, IReadOnlyList<GymClassDetails>>, GetScheduleForWeekQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetClassAttendanceAnalyticsQuery, IReadOnlyList<ClassAttendanceRow>>, GetClassAttendanceAnalyticsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetCoachWorkloadQuery, CoachWorkloadRow>, GetCoachWorkloadQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetCoachEfficiencyAnalyticsQuery, List<CoachEfficiencyRow>>, GetCoachEfficiencyAnalyticsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<PurchaseMembershipCommand>, PurchaseMembershipCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetActiveMembershipsByClientQuery, IReadOnlyList<MembershipDto>>, GetActiveMembershipsByClientQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateMembershipPlanCommand>, CreateMembershipPlanCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteMembershipPlanCommand>, DeleteMembershipPlanCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetMembershipPlansQuery, List<MembershipPlanDto>>, GetMembershipPlansQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetMembershipPlanByIdQuery, MembershipPlanDto?>, GetMembershipPlanByIdQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateInvoiceCommand, int>, CreateInvoiceCommandHandler>();
builder.Services.AddScoped<ICommandHandler<MarkInvoicePaidCommand>, MarkInvoicePaidCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetPendingInvoicesForClientQuery, List<InvoiceResponseDto>>, GetPendingInvoicesForClientQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetMonthlyRevenueByPlanQuery, List<TotalMembershipRevenueRow>>, GetMonthlyRevenueByPlanQueryHandler>();
builder.Services.AddScoped<ICommandHandler<CreateEnrollmentCommand, EnrollmentResultDto>, CreateEnrollmentCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RegisterClientCommand, int>, RegisterClientCommandHandler>();
builder.Services.AddScoped<IQueryHandler<LoginClientQuery, AuthResultDto>, LoginClientQueryHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateClientCommand>, UpdateClientCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteClientCommand>, DeleteClientCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetClientByIdQuery, ClientDto?>, GetClientByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<SearchClientsQuery, IReadOnlyList<ClientSummaryDto>>, SearchClientsQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetClientActivityAnalyticsQuery, List<ClientActivityRow>>, GetClientActivityAnalyticsQueryHandler>();
builder.Services.AddScoped<ICommandHandler<RegisterCoachCommand, int>, RegisterCoachCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteCoachCommand>, DeleteCoachCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateCoachSpecializationCommand>, UpdateCoachSpecializationCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetCoachByIdQuery, CoachDto?>, GetCoachByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetAllCoachesQuery, IReadOnlyList<CoachSummaryDto>>, GetAllCoachesQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetCoachesBySpecializationQuery, IReadOnlyList<CoachSummaryDto>>, GetCoachesBySpecializationQueryHandler>();

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
    var context = services.GetRequiredService<GymManagementContext>();
    await DbInitializer.InitializeAsync(context);
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
