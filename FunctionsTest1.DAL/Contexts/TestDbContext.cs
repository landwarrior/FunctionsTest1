using System;
using System.Collections.Generic;
using FunctionsTest1.DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace FunctionsTest1.DAL.Contexts;

public partial class TestDbContext : DbContext
{
    public TestDbContext()
    {
    }

    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwUserOrder> VwUserOrders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=192.168.33.150,1433;Database=TestDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Orders__3214EC07B1B418D6");

            entity.HasIndex(e => e.OrderDate, "IX_Orders_OrderDate");

            entity.HasIndex(e => e.UserId, "IX_Orders_UserId");

            entity.Property(e => e.OrderDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Orders__UserId__4222D4EF");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC0763DCBD57");

            entity.Property(e => e.UnitPrice).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Order__44FF419A");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__OrderDeta__Produ__45F365D3");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3214EC071441056A");

            entity.HasIndex(e => e.Name, "IX_Products_Name");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StockQuantity).HasDefaultValue(0);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC070B576119");

            entity.HasIndex(e => e.Email, "IX_Users_Email");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534CA995AF4").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");
        });

        modelBuilder.Entity<VwUserOrder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UserOrders");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
