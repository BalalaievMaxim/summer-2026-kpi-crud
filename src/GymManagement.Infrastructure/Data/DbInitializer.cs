using GymManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(GymManagementContext context)
    {
        try
        {
            await context.Database.MigrateAsync();
            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


        if (!context.Facilityzones.Any())
        {
            var zones = new[]
            {
                new FacilityZone { Name = "gym_floor" },
                new FacilityZone { Name = "cardio_area" },
                new FacilityZone { Name = "pool" },
                new FacilityZone { Name = "yoga_studio" },
                new FacilityZone { Name = "boxing_ring" }
            };
            context.Facilityzones.AddRange(zones);
            await context.SaveChangesAsync();
        }

        if (!context.Membershipplans.Any())
        {
            var plans = new[]
            {
                new MembershipPlan { Name = "basic", DurationMonths = 1, Price = 500.00m },
                new MembershipPlan { Name = "standard", DurationMonths = 6, Price = 2500.00m },
                new MembershipPlan { Name = "premium", DurationMonths = 12, Price = 4500.00m }
            };
            context.Membershipplans.AddRange(plans);
            await context.SaveChangesAsync();

            var dbPlans = await context.Membershipplans.ToListAsync();
            var dbZones = await context.Facilityzones.ToListAsync();

            var planAccesses = new List<PlanAccess>
            {
                new PlanAccess { PlanId = dbPlans[0].PlanId, ZoneId = dbZones[0].ZoneId },
                new PlanAccess { PlanId = dbPlans[0].PlanId, ZoneId = dbZones[1].ZoneId },

                new PlanAccess { PlanId = dbPlans[1].PlanId, ZoneId = dbZones[0].ZoneId },
                new PlanAccess { PlanId = dbPlans[1].PlanId, ZoneId = dbZones[1].ZoneId },
                new PlanAccess { PlanId = dbPlans[1].PlanId, ZoneId = dbZones[2].ZoneId },

                new PlanAccess { PlanId = dbPlans[2].PlanId, ZoneId = dbZones[0].ZoneId },
                new PlanAccess { PlanId = dbPlans[2].PlanId, ZoneId = dbZones[1].ZoneId },
                new PlanAccess { PlanId = dbPlans[2].PlanId, ZoneId = dbZones[2].ZoneId },
                new PlanAccess { PlanId = dbPlans[2].PlanId, ZoneId = dbZones[3].ZoneId },
                new PlanAccess { PlanId = dbPlans[2].PlanId, ZoneId = dbZones[4].ZoneId }
            };
            context.PlanAccesses.AddRange(planAccesses);
        }

        if (!context.Clients.Any())
        {
            var clients = new[]
            {
                new Client
                {
                    Name = "ivan petrenko", Email = "ivan@test.com", Password = "pass123", Phone = "+380501112233",
                    CreatedAt = DateTime.Now
                },
                new Client
                {
                    Name = "maria sydorenko", Email = "maria@test.com", Password = "pass123", Phone = "+380679998877",
                    CreatedAt = DateTime.Now
                },
                new Client
                {
                    Name = "oleg bondar", Email = "oleg@test.com", Password = "pass123", Phone = "+380935554433",
                    CreatedAt = DateTime.Now
                }
            };
            context.Clients.AddRange(clients);
            await context.SaveChangesAsync();

            var dbClients = await context.Clients.ToListAsync();
            var invoices = new[]
            {
                new Invoice
                {
                    ClientId = dbClients[0].ClientId, Amount = 500.00m,
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-10)), Status = "paid", PaymentMethod = "card",
                    Notes = "Monthly subscription"
                },
                new Invoice
                {
                    ClientId = dbClients[1].ClientId, Amount = 4500.00m,
                    Date = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2)), Status = "paid",
                    PaymentMethod = "bank_transfer", Notes = "Yearly subscription"
                },
                new Invoice
                {
                    ClientId = dbClients[2].ClientId, Amount = 100.00m, Date = DateOnly.FromDateTime(DateTime.Now),
                    Status = "pending", PaymentMethod = null, Notes = "One-time visit"
                }
            };
            context.Invoices.AddRange(invoices);
            await context.SaveChangesAsync();
            
        }

        if (!context.Coaches.Any())
        {
            var coaches = new[]
            {
                new Coach
                {
                    Name = "arnold s.", Specialization = "bodybuilding", Email = "arnold@gym.com",
                    Password = "terminator", CreatedAt = DateTime.Now
                },
                new Coach
                {
                    Name = "serena w.", Specialization = "tennis/cardio", Email = "serena@gym.com", Password = "tennis",
                    CreatedAt = DateTime.Now
                },
                new Coach
                {
                    Name = "bruce lee", Specialization = "martial arts", Email = "bruce@gym.com", Password = "dragon",
                    CreatedAt = DateTime.Now
                }
            };
            context.Coaches.AddRange(coaches);
            await context.SaveChangesAsync();
        }

        if (!context.Classtypes.Any())
        {
            var types = new[]
            {
                new ClassType { Name = "yoga_morning", Description = "Relaxing start of the day" },
                new ClassType { Name = "hiit", Description = "High Intensity Interval Training" },
                new ClassType { Name = "boxing_basics", Description = "Learn how to punch" }
            };
            context.Classtypes.AddRange(types);
            await context.SaveChangesAsync();
        }

        if (!context.Classes.Any())
        {
            var dbCoaches = await context.Coaches.ToListAsync();
            var dbTypes = await context.Classtypes.ToListAsync();

            var classes = new[]
            {
                new Class
                {
                    ClassTypeId = dbTypes[0].ClassTypeId,
                    CoachId = dbCoaches[1].CoachId,
                    StartTime = DateTime.Now.AddHours(2),
                    EndTime = DateTime.Now.AddHours(3),
                    Capacity = 15
                },
                new Class
                {
                    ClassTypeId = dbTypes[1].ClassTypeId,
                    CoachId = dbCoaches[0].CoachId,
                    StartTime = DateTime.Now.AddDays(1).AddHours(10),
                    EndTime = DateTime.Now.AddDays(1).AddHours(11),
                    Capacity = 10
                },
                new Class
                {
                    ClassTypeId = dbTypes[2].ClassTypeId,
                    CoachId = dbCoaches[2].CoachId,
                    StartTime = DateTime.Now.AddDays(2).AddHours(19),
                    EndTime = DateTime.Now.AddDays(2).AddHours(20).AddMinutes(30),
                    Capacity = 20
                }
            };
            context.Classes.AddRange(classes);
            await context.SaveChangesAsync();
        }
        
        if (!context.Enrollments.Any())
        {
            var dbClients = await context.Clients.ToListAsync();
            var dbClasses = await context.Classes.ToListAsync();

            var enrollments = new[]
            {
                new Enrollment
                {
                    ClientId = dbClients[1].ClientId, ClassId = dbClasses[0].ClassId, RegistrationTime = DateTime.Now
                }
            };
            context.Enrollments.AddRange(enrollments);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }
}