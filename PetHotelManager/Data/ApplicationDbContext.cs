namespace PetHotelManager.Data;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetHotelManager.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {

    }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<PrescriptionDetail> PrescriptionDetails { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //1. Pet - MedicalRecord
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Pet)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        //2. Appointment - Pet
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Pet)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        //3. Invoice - InvoiceDetail
        modelBuilder.Entity<InvoiceDetail>()
            .HasOne(d => d.Invoice)
            .WithMany(i => i.InvoiceDetails)
            .HasForeignKey(d => d.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        //4. Appointment - User
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.User)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        //5. Pet - User
        modelBuilder.Entity<Pet>()
            .HasOne(p => p.User)
            .WithMany(u => u.Pets)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        //6. Invoice - User
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.User)
            .WithMany(u => u.Invoices)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        //7. MedicalRecord - Veterinarian
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(m => m.Veterinarian)
            .WithMany()
            .HasForeignKey(m => m.VeterinarianId)
            .OnDelete(DeleteBehavior.Restrict);
    }


}