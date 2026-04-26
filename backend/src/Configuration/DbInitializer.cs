using GymManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Configuration;

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
            Console.WriteLine($"Migration Error: {ex.Message}");
        }

        if (!context.Facilityzones.Any())
        {
            var zones = new[]
            {
                new FacilityZone { Name = "gym_floor" },
                new FacilityZone { Name = "cardio_area" },
                new FacilityZone { Name = "pool" },
                new FacilityZone { Name = "yoga_studio" },
                new FacilityZone { Name = "boxing_ring" },
                new FacilityZone { Name = "sauna" }
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
                new MembershipPlan { Name = "premium", DurationMonths = 12, Price = 4500.00m },   
                new MembershipPlan { Name = "student", DurationMonths = 1, Price = 300.00m }    
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
                new PlanAccess { PlanId = dbPlans[2].PlanId, ZoneId = dbZones[4].ZoneId }, 
                new PlanAccess { PlanId = dbPlans[2].PlanId, ZoneId = dbZones[5].ZoneId }  
            };
            context.PlanAccesses.AddRange(planAccesses);
            await context.SaveChangesAsync();
        }

        if (!context.Clients.Any())
        {
            var clients = new[]
            {
                new Client { Name = "ivan petrenko", Email = "ivan@test.com", Password = "pass", Phone = "+380501112233", CreatedAt = DateTime.Now.AddMonths(-5) },
                new Client { Name = "maria sydorenko", Email = "maria@test.com", Password = "pass", Phone = "+380679998877", CreatedAt = DateTime.Now.AddMonths(-3) },
                new Client { Name = "oleg bondar", Email = "oleg@test.com", Password = "pass", Phone = "+380935554433", CreatedAt = DateTime.Now.AddMonths(-2) },
                new Client { Name = "anna koval", Email = "anna@test.com", Password = "pass", Phone = "+380991234567", CreatedAt = DateTime.Now.AddMonths(-1) },
                new Client { Name = "dmytro shevchenko", Email = "dima@test.com", Password = "pass", Phone = "+380631112222", CreatedAt = DateTime.Now.AddDays(-10) }
            };
            context.Clients.AddRange(clients);
            await context.SaveChangesAsync();
        }

        if (!context.Memberships.Any())
        {
            var dbClients = await context.Clients.ToListAsync();
            var dbPlans = await context.Membershipplans.ToListAsync();

            var memberships = new List<Membership>();
            var invoices = new List<Invoice>();

            var date1 = DateTime.Now.AddMonths(-2);
            memberships.Add(new Membership { 
                ClientId = dbClients[0].ClientId, PlanId = dbPlans[0].PlanId, 
                StartDate = DateOnly.FromDateTime(date1), EndDate = DateOnly.FromDateTime(date1.AddMonths(1)), IsActive = false 
            });
            invoices.Add(new Invoice {
                ClientId = dbClients[0].ClientId, Amount = dbPlans[0].Price, Date =
                    DateOnly.FromDateTime(DateTime.SpecifyKind(date1.AddDays(1), DateTimeKind.Utc)), 
                Status = "paid", PaymentMethod = "card", Notes = "Basic Plan - Month 1"
            });

            var date2 = DateTime.Now.AddMonths(-1);
            memberships.Add(new Membership { 
                ClientId = dbClients[0].ClientId, PlanId = dbPlans[0].PlanId, 
                StartDate = DateOnly.FromDateTime(date2), EndDate = DateOnly.FromDateTime(date2.AddMonths(1)), IsActive = false 
            });
            invoices.Add(new Invoice {
                ClientId = dbClients[0].ClientId, Amount = dbPlans[0].Price, Date = DateOnly.FromDateTime(DateTime.SpecifyKind(date2.AddDays(1), DateTimeKind.Utc)),
                Status = "paid", PaymentMethod = "card", Notes = "Basic Plan - Month 2"
            });

            var date3 = DateTime.Now;
            memberships.Add(new Membership { 
                ClientId = dbClients[0].ClientId, PlanId = dbPlans[0].PlanId, 
                StartDate = DateOnly.FromDateTime(date3), EndDate = DateOnly.FromDateTime(date3.AddMonths(1)), IsActive = true 
            });
            invoices.Add(new Invoice {
                ClientId = dbClients[0].ClientId, Amount = dbPlans[0].Price, Date =
                    DateOnly.FromDateTime(DateTime.SpecifyKind(date3, DateTimeKind.Utc)),
                Status = "paid", PaymentMethod = "card", Notes = "Basic Plan - Current"
            });


            var dateMaria = DateTime.Now.AddMonths(-3);
            memberships.Add(new Membership { 
                ClientId = dbClients[1].ClientId, PlanId = dbPlans[2].PlanId, 
                StartDate = DateOnly.FromDateTime(dateMaria), EndDate = DateOnly.FromDateTime(dateMaria.AddYears(1)), IsActive = true 
            });
            invoices.Add(new Invoice {
                ClientId = dbClients[1].ClientId, Amount = dbPlans[2].Price, Date =
                    DateOnly.FromDateTime(DateTime.SpecifyKind(dateMaria.AddDays(2), DateTimeKind.Utc)),
                Status = "paid", PaymentMethod = "bank_transfer", Notes = "Annual Premium Subscription"
            });


            var dateOleg = DateTime.Now.AddMonths(-1);
            memberships.Add(new Membership { 
                ClientId = dbClients[2].ClientId, PlanId = dbPlans[1].PlanId, 
                StartDate = DateOnly.FromDateTime(dateOleg), EndDate = DateOnly.FromDateTime(dateOleg.AddMonths(6)), IsActive = true 
            });
            invoices.Add(new Invoice {
                ClientId = dbClients[2].ClientId, Amount = dbPlans[1].Price, Date =
                    DateOnly.FromDateTime(DateTime.SpecifyKind(dateOleg.AddDays(1), DateTimeKind.Utc)),
                Status = "paid", PaymentMethod = "cash", Notes = "6-month Standard"
            });


            var dateAnna = DateTime.Now;
            memberships.Add(new Membership { 
                ClientId = dbClients[3].ClientId, PlanId = dbPlans[3].PlanId, 
                StartDate = DateOnly.FromDateTime(dateAnna), EndDate = DateOnly.FromDateTime(dateAnna.AddMonths(1)), IsActive = false 
            });
            invoices.Add(new Invoice {
                ClientId = dbClients[3].ClientId, Amount = dbPlans[3].Price, Date = DateOnly.FromDateTime(
                    DateTime.SpecifyKind(dateAnna, DateTimeKind.Utc)),
                Status = "pending", PaymentMethod = "app", Notes = "Student discount waiting for payment"
            });
            
            context.Memberships.AddRange(memberships);
            await context.SaveChangesAsync();
            
            context.Invoices.AddRange(invoices);
            await context.SaveChangesAsync();
        }

        if (!context.Coaches.Any())
        {
            var coaches = new[]
            {
                new Coach { Name = "arnold s.", Specialization = "bodybuilding", Email = "arnold@gym.com", Password = "pass", CreatedAt = DateTime.Now },
                new Coach { Name = "serena w.", Specialization = "tennis/cardio", Email = "serena@gym.com", Password = "pass", CreatedAt = DateTime.Now },
                new Coach { Name = "bruce lee", Specialization = "martial arts", Email = "bruce@gym.com", Password = "pass", CreatedAt = DateTime.Now }
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
                    ClassTypeId = dbTypes[0].ClassTypeId, CoachId = dbCoaches[1].CoachId,
                    StartTime = DateTime.UtcNow.AddHours(2), EndTime = DateTime.UtcNow.AddHours(3), Capacity = 15
                },
                new Class
                {
                    ClassTypeId = dbTypes[1].ClassTypeId, CoachId = dbCoaches[0].CoachId,
                    StartTime = DateTime.UtcNow.AddDays(1).AddHours(10), EndTime = DateTime.UtcNow.AddDays(1).AddHours(11), Capacity = 10
                },
                new Class
                {
                    ClassTypeId = dbTypes[2].ClassTypeId, CoachId = dbCoaches[2].CoachId,
                    StartTime = DateTime.UtcNow.AddDays(2).AddHours(19), EndTime = DateTime.UtcNow.AddDays(2).AddHours(20).AddMinutes(30), Capacity = 20
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
                new Enrollment { ClientId = dbClients[1].ClientId, ClassId = dbClasses[0].ClassId, RegistrationTime = DateTime.UtcNow },
                new Enrollment { ClientId = dbClients[2].ClientId, ClassId = dbClasses[1].ClassId, RegistrationTime = DateTime.UtcNow }
            };
            context.Enrollments.AddRange(enrollments);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }
}