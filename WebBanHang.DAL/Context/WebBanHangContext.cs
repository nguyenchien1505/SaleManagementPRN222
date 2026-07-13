using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Context;

public partial class WebBanHangContext : DbContext
{
    public WebBanHangContext()
    {
    }

    public WebBanHangContext(DbContextOptions<WebBanHangContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF475772B3");

            entity.HasIndex(e => e.OrderCode, "UQ__Orders__999B522956BCB11E").IsUnique();

            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderCode).HasMaxLength(30);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingPhone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.SubTotal)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
