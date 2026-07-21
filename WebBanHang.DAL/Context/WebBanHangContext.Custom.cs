using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.DAL.Entities;

namespace WebBanHang.DAL.Context
{
    public partial class WebBanHangContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            ConfigureProductHardDelete(modelBuilder);
            ConfigureCategoryHardDelete(modelBuilder);
        }
        private static void ConfigureProductHardDelete(ModelBuilder modelBuilder)
        {
            // 1. Xóa Product thì xóa luôn hình ảnh trong database
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Xóa Product thì xóa sản phẩm khỏi giỏ hàng
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            

            // 4. Không cho hard delete nếu Product đã có trong đơn hàng
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Không cho hard delete nếu Product đã có lịch sử kho
            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(it => it.Product)
                .WithMany(p => p.InventoryTransactions)
                .HasForeignKey(it => it.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            // Xóa User thì xóa luôn giỏ hàng của User
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Xóa Cart thì xóa luôn các CartItem
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            

            // Xóa User thì xóa hồ sơ Customer
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithOne(u => u.Customer)
                .HasForeignKey<Customer>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Customer đã có đơn hàng thì không cho xóa Customer/User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // User đã tạo đơn hàng thì không được hard delete
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CreatedByNavigation)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // User đã tạo sản phẩm thì không được hard delete
            modelBuilder.Entity<Product>()
                .HasOne(p => p.CreatedByNavigation)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            // User đã tạo giao dịch kho thì không được hard delete
            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(i => i.CreatedByNavigation)
                .WithMany(u => u.InventoryTransactions)
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureCategoryHardDelete(
            ModelBuilder modelBuilder)
        {
            // 1. Không cho xóa Category nếu vẫn còn Category con
            modelBuilder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.InverseParent)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Không cho xóa Category nếu vẫn còn Product
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
