using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.BLL.DTOs;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.DAL.Context;
using WebBanHang.DAL.Entities;

namespace WebBanHang.BLL.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly WebBanHangContext _context;

        public CartService(WebBanHangContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedDate = DateTime.Now, UpdatedDate = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<List<CartItemDTO>> GetCartByUserIdAsync(int userId)
        {
            // Truy vấn trực tiếp từ bảng CartItems để tránh lỗi tham chiếu vòng qua bảng Cart
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId) // Lọc theo UserId của Cart
                .ToListAsync();

            if (cartItems == null || !cartItems.Any())
                return new List<CartItemDTO>();

            return cartItems.Select(ci =>
            {
                // Lấy hình ảnh đại diện an toàn
                var imageUrl = "/images/no-image.png";
                if (ci.Product?.ProductImages != null && ci.Product.ProductImages.Any())
                {
                    var primaryImg = ci.Product.ProductImages.FirstOrDefault(img => img.IsPrimary == true)?.ImageUrl
                                     ?? ci.Product.ProductImages.FirstOrDefault()?.ImageUrl;

                    if (!string.IsNullOrEmpty(primaryImg))
                        imageUrl = primaryImg;
                }

                return new CartItemDTO
                {
                    ProductId = ci.ProductId,
                    Name = ci.Product?.Name ?? "Sản phẩm không rõ tên",
                    ImageUrl = imageUrl,
                    Price = ci.Product?.SellingPrice ?? 0,
                    Quantity = ci.Quantity
                };
            }).ToList();
        }

        public async Task AddToCartAsync(int userId, int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem { CartId = cart.CartId, ProductId = productId, Quantity = quantity });
            }

            cart.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int userId, int productId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    _context.CartItems.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null) return;

            var item = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MergeCartAsync(int userId, List<CartItemDTO> sessionCart)
        {
            if (sessionCart == null || !sessionCart.Any()) return;

            var cart = await GetOrCreateCartAsync(userId);

            foreach (var sessionItem in sessionCart)
            {
                var dbItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == sessionItem.ProductId);
                if (dbItem != null)
                {
                    dbItem.Quantity += sessionItem.Quantity;
                }
                else
                {
                    cart.CartItems.Add(new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = sessionItem.ProductId,
                        Quantity = sessionItem.Quantity
                    });
                }
            }

            cart.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCartCountAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
        }

        public async Task<CartItemDTO?> GetCartItemAsync(int userId, int productId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var item = cart?.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item == null) return null;

            return new CartItemDTO
            {
                ProductId = item.ProductId,
                Price = item.Product.SellingPrice,
                Quantity = item.Quantity
            };
        }
    }
}
