using System;
using System.Collections.Generic;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagement.Configuration;

public partial class GymManagementContext : DbContext
{
    public GymManagementContext()
    {
    }

    public GymManagementContext(DbContextOptions<GymManagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }
    public virtual DbSet<ClassType> Classtypes { get; set; }
    public virtual DbSet<Client> Clients { get; set; }
    public virtual DbSet<Coach> Coaches { get; set; }
    public virtual DbSet<Enrollment> Enrollments { get; set; }
    public virtual DbSet<FacilityZone> Facilityzones { get; set; }
    public virtual DbSet<Invoice> Invoices { get; set; }
    public virtual DbSet<Membership> Memberships { get; set; }
    public virtual DbSet<MembershipPlan> Membershipplans { get; set; }
    
   
    public virtual DbSet<PlanAccess> PlanAccesses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("class_pkey");
            entity.ToTable("class");

            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.ClassTypeId).HasColumnName("class_type_id");
            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.EndTime).HasColumnType("timestamp without time zone").HasColumnName("end_time");
            entity.Property(e => e.StartTime).HasColumnType("timestamp without time zone").HasColumnName("start_time");

            entity.HasOne(d => d.ClassType).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ClassTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("class_class_type_id_fkey");

            entity.HasOne(d => d.Coach).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CoachId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("class_coach_id_fkey");
        });

        modelBuilder.Entity<ClassType>(entity =>
        {
            entity.HasKey(e => e.ClassTypeId).HasName("classtype_pkey");
            entity.ToTable("classtype");
            entity.HasIndex(e => e.Name, "classtype_name_key").IsUnique();

            entity.Property(e => e.ClassTypeId).HasColumnName("class_type_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("client_pkey");
            entity.ToTable("client");
            entity.HasIndex(e => e.Email, "client_email_key").IsUnique();
            entity.HasIndex(e => e.Phone, "client_phone_key").IsUnique();

            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Password).HasMaxLength(255).HasColumnName("password");
            entity.Property(e => e.Phone).HasMaxLength(20).HasColumnName("phone");
        });

        modelBuilder.Entity<Coach>(entity =>
        {
            entity.HasKey(e => e.CoachId).HasName("coach_pkey");
            entity.ToTable("coach");
            entity.HasIndex(e => e.Email, "coach_email_key").IsUnique();

            entity.Property(e => e.CoachId).HasColumnName("coach_id");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("created_at");
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Password).HasMaxLength(255).HasColumnName("password");
            entity.Property(e => e.Specialization).HasMaxLength(100).HasColumnName("specialization");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("enrollment_pkey");
            entity.ToTable("enrollment");
            entity.HasIndex(e => new { e.ClientId, e.ClassId }, "enrollment_client_id_class_id_key").IsUnique();

            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.RegistrationTime).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnType("timestamp without time zone").HasColumnName("registration_time");

            entity.HasOne(d => d.Class).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("enrollment_class_id_fkey");

            entity.HasOne(d => d.Client).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("enrollment_client_id_fkey");
        });

        modelBuilder.Entity<FacilityZone>(entity =>
        {
            entity.HasKey(e => e.ZoneId).HasName("facilityzone_pkey");
            entity.ToTable("facilityzone");
            entity.HasIndex(e => e.Name, "facilityzone_name_key").IsUnique();

            entity.Property(e => e.ZoneId).HasColumnName("zone_id");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("name");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("invoice_pkey");
            entity.ToTable("invoice");

            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.Amount).HasPrecision(10, 2).HasColumnName("amount");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Date).HasDefaultValueSql("CURRENT_DATE").HasColumnName("date");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50).HasColumnName("payment_method");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'pending'::character varying")
                .HasColumnName("status")
                .HasConversion<string>(); 

            entity.HasOne(d => d.Client).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("invoice_client_id_fkey");
        });

        modelBuilder.Entity<Membership>(entity =>
        {
            entity.HasKey(e => e.MembershipId).HasName("membership_pkey");
            entity.ToTable("membership");

            entity.Property(e => e.MembershipId).HasColumnName("membership_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");

            entity.HasOne(d => d.Client).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("membership_client_id_fkey");

            entity.HasOne(d => d.MembershipPlan).WithMany(p => p.Memberships)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("membership_plan_id_fkey");
        });

        modelBuilder.Entity<MembershipPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("membershipplan_pkey");
            entity.ToTable("membershipplan");
            entity.HasIndex(e => e.Name, "membershipplan_name_key").IsUnique();

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.DurationMonths).HasColumnName("duration_months");
            entity.Property(e => e.Name).HasMaxLength(100).HasColumnName("name");
            entity.Property(e => e.Price).HasPrecision(10, 2).HasColumnName("price");
        });
        
        modelBuilder.Entity<PlanAccess>(entity =>
        {
          
            entity.ToTable("planaccess"); 
            entity.HasKey(e => new { e.PlanId, e.ZoneId }).HasName("planaccess_pkey");
            
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.ZoneId).HasColumnName("zone_id");

            entity.HasOne(d => d.MembershipPlan)
                .WithMany(p => p.PlanAccesses)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("planaccess_plan_id_fkey")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.FacilityZone)
                .WithMany(p => p.PlanAccesses)
                .HasForeignKey(d => d.ZoneId)
                .HasConstraintName("planaccess_zone_id_fkey")
                .OnDelete(DeleteBehavior.Cascade);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}