using EeD_BE_EeD.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EeD_BE_EeD.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ApplicationUser> Users { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Exchange> Exchanges { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<HeroSection> HeroSections { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -------------------- USER RELATIONS -----------------------

            // ✅ User -> Services
            modelBuilder.Entity<Service>()
                .HasOne(s => s.Owner)
                .WithMany(u => u.OfferedServices)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);


            // ✅ Messages (Sender -> Many Sent)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.MessagesSent)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Messages (Receiver -> Many Received)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(u => u.MessagesReceived)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);


            // ✅ Reviews (Reviewer)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Reviewer)
                .WithMany(u => u.ReviewsWritten)
                .HasForeignKey(r => r.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Reviews (ReviewedUser)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.ReviewedUser)
                .WithMany(u => u.ReviewsReceived)
                .HasForeignKey(r => r.ReviewedUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ✅ Testimonials
            modelBuilder.Entity<Testimonial>()
                .HasOne(t => t.User)
                .WithMany(u => u.Testimonials)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ✅ Notifications (Receiver)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // ✅ Notifications (Sender - optional)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // -------------------- EXCHANGE RELATIONS -----------------------

            modelBuilder.Entity<Exchange>()
                .HasOne(e => e.Requester)
                .WithMany()
                .HasForeignKey(e => e.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Exchange>()
                .HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Exchange>()
                .HasOne(e => e.RequestedService)
                .WithMany()
                .HasForeignKey(e => e.RequestedServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Exchange>()
                .HasOne(e => e.OfferedService)
                .WithMany()
                .HasForeignKey(e => e.OfferedServiceId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ActivityLog>(e =>
            {
                e.ToTable("ActivityLogs");

                e.HasIndex(x => x.CreatedAt);

                e.Property(x => x.Action)
                 .IsRequired()
                 .HasMaxLength(100);

                e.Property(x => x.Description)
                 .HasMaxLength(2000);

                e.HasOne(x => x.User)
                 .WithMany()                 // لو لاحقًا ضفت ICollection<ActivityLog> بـ ApplicationUser غيّرها لـ WithMany(u => u.ActivityLogs)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.SetNull);   // حذف المستخدم لا يكسر اللوج

                e.Property(x => x.CreatedAt)
                 .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
