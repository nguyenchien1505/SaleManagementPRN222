using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebBanHang.BLL.Services.Interfaces;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private const string CartSessionKey = "ShoppingCart";
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IPromotionService _promotionService;
        private readonly ICartService _cartService;

        public CartController(IProductService productService, IOrderService orderService, IPromotionService promotionService, ICartService cartService)
        {
            _productService = productService;
            _orderService = orderService;
            _promotionService = promotionService;
            _cartService = cartService;
        }

        // ──────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────
        private async Task<List<CartItemVM>> GetCartItemsAsync()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                var items = await _cartService.GetCartByUserIdAsync(userId.Value);
                return items.Select(i => new CartItemVM
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    ImageUrl = i.ImageUrl,
                    Price = i.Price,
                    Quantity = i.Quantity
                }).ToList();
            }

            var json = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(json)
                ? new List<CartItemVM>()
                : JsonSerializer.Deserialize<List<CartItemVM>>(json)!;
        }

        private void SaveCart(List<CartItemVM> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
        }

        // ──────────────────────────────────────────────
        // GET: /Customer/Cart/Index
        // ──────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var cart = await GetCartItemsAsync();

            var activePromotionsDto = await _promotionService.GetAllPromotionsAsync("Active", null);

            var activePromos = activePromotionsDto.Select(p => new WebBanHang.DAL.Entities.Promotion
            {
                PromotionId = p.PromotionId,
                Code = p.Code,
                DiscountType = p.DiscountType,
                Value = p.Value,
                MinOrderValue = p.MinOrderValue,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status
            }).ToList();

            ViewBag.ActivePromotions = activePromos;

            return View(cart);
        }

        // ──────────────────────────────────────────────
        // POST: /Customer/Cart/AddToCart  (AJAX)
        // ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (HttpContext.Session.GetString("Role") == "Admin")
            {
                return Json(new { success = false, message = "Tài khoản Quản trị viên (Admin) chỉ dùng để quản lý hệ thống, không thể đặt hàng mua sắm." });
            }

            if (quantity < 1) quantity = 1;

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });

            if (product.Status != "Active")
                return Json(new { success = false, message = "Sản phẩm không còn bán." });

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                var existing = await _cartService.GetCartItemAsync(userId.Value, productId);
                if ((existing?.Quantity ?? 0) + quantity > product.StockQuantity)
                    return Json(new { success = false, message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho." });

                await _cartService.AddToCartAsync(userId.Value, productId, quantity);
                int count = await _cartService.GetCartCountAsync(userId.Value);
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng!", cartCount = count });
            }

            // GUEST FLOW
            var cart = await GetCartItemsAsync();
            var guestExisting = cart.FirstOrDefault(c => c.ProductId == productId);

            if (guestExisting != null)
            {
                var newQty = guestExisting.Quantity + quantity;
                if (newQty > product.StockQuantity)
                    return Json(new { success = false, message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho." });
                guestExisting.Quantity = newQty;
            }
            else
            {
                if (quantity > product.StockQuantity)
                    return Json(new { success = false, message = $"Chỉ còn {product.StockQuantity} sản phẩm trong kho." });

                var primaryImg = product.Images?.FirstOrDefault(i => i.IsPrimary == true)?.ImageUrl
                              ?? product.Images?.FirstOrDefault()?.ImageUrl
                              ?? "/images/no-image.png";

                cart.Add(new CartItemVM
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    ImageUrl = primaryImg,
                    Price = product.SellingPrice,
                    Quantity = quantity
                });
            }

            SaveCart(cart);
            int totalItems = cart.Sum(c => c.Quantity);
            return Json(new { success = true, message = "Đã thêm vào giỏ hàng!", cartCount = totalItems });
        }

        // ──────────────────────────────────────────────
        // POST: /Customer/Cart/Remove
        // ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                await _cartService.RemoveFromCartAsync(userId.Value, productId);
            }
            else
            {
                var json = HttpContext.Session.GetString(CartSessionKey);
                if (!string.IsNullOrEmpty(json))
                {
                    var cart = JsonSerializer.Deserialize<List<CartItemVM>>(json)!;
                    var item = cart.FirstOrDefault(x => x.ProductId == productId);
                    if (item != null)
                    {
                        cart.Remove(item);
                        HttpContext.Session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
                    }
                }
            }

            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng";
            return RedirectToAction(nameof(Index));
        }

        // ──────────────────────────────────────────────
        // POST: /Customer/Cart/UpdateQuantity  (AJAX)
        // ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity < 1)
                return Json(new { success = false, message = "Số lượng không hợp lệ." });

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                await _cartService.UpdateQuantityAsync(userId.Value, productId, quantity);

                var items = await _cartService.GetCartByUserIdAsync(userId.Value);
                var item = items.FirstOrDefault(i => i.ProductId == productId);
                decimal cartTotal = items.Sum(i => i.Total);

                return Json(new
                {
                    success = true,
                    newItemTotal = (item?.Total ?? 0).ToString("N0"),
                    cartTotal = cartTotal.ToString("N0")
                });
            }

            var cart = await GetCartItemsAsync();
            var guestItem = cart.FirstOrDefault(c => c.ProductId == productId);
            if (guestItem == null)
                return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ." });

            guestItem.Quantity = quantity;
            SaveCart(cart);

            decimal newTotal = guestItem.Price * guestItem.Quantity;
            decimal total = cart.Sum(c => c.Price * c.Quantity);
            return Json(new { success = true, newItemTotal = newTotal.ToString("N0"), cartTotal = total.ToString("N0") });
        }

        // ──────────────────────────────────────────────
        // POST: /Customer/Cart/CheckoutAll
        // ──────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckoutAll(List<int> selectedProductIds, string? promoCode = null)
        {
       
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account", new { area = "" });

            var fullCart = await GetCartItemsAsync();
            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất 1 sản phẩm để thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            var selectedItems = fullCart.Where(c => selectedProductIds.Contains(c.ProductId)).ToList();
            if (!selectedItems.Any())
            {
                TempData["Error"] = "Sản phẩm chọn mua không còn trong giỏ hàng.";
                return RedirectToAction(nameof(Index));
            }

            var itemsToCheckout = selectedItems.Select(c => (c.ProductId, c.Quantity)).ToList();

            var result = await _orderService.CheckoutCartAsync(userId.Value, itemsToCheckout, promoCode);

            if (result.Success)
            {
                foreach (var id in selectedProductIds)
                {
                    await _cartService.RemoveFromCartAsync(userId.Value, id);
                }

                TempData["Success"] = result.Message;

                return RedirectToAction("MyOrders", "Order", new { area = "Customer" });
            }
            else
            {
                // BẮT BỆNH: Nếu lỗi trả về có chứa Exception ngầm, lôi tin nhắn thật của SQL ra hiển thị
                TempData["Error"] = result.Message;

                // In thêm thông báo lỗi hệ thống nếu lỗi chung chung để bạn đọc được ngay trên giao diện giỏ hàng


                return RedirectToAction(nameof(Index));
            }
        }

        // ──────────────────────────────────────────────
        // GET: /Customer/Cart/Count  (AJAX)
        // ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Count()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                int count = await _cartService.GetCartCountAsync(userId.Value);
                return Json(new { count = count });
            }
            var cart = await GetCartItemsAsync();
            return Json(new { count = cart.Sum(c => c.Quantity) });
        }

        // ──────────────────────────────────────────────
        // POST: /Customer/Cart/ValidatePromo  (AJAX)
        // ──────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ValidatePromo(string code, List<int> selectedProductIds)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để áp dụng mã giảm giá." });
            }

            if (selectedProductIds == null || !selectedProductIds.Any())
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một sản phẩm để áp mã." });
            }

            var cart = await GetCartItemsAsync();
            var selectedItems = cart.Where(c => selectedProductIds.Contains(c.ProductId)).ToList();

            if (!selectedItems.Any())
            {
                return Json(new { success = false, message = "Sản phẩm đã chọn không tồn tại trong giỏ hàng." });
            }

            decimal totalOrderAmount = selectedItems.Sum(c => c.Price * c.Quantity);

            var result = await _promotionService.ValidatePromotionAsync(code, totalOrderAmount);

            return Json(new
            {
                success = result.Success,
                message = result.Message,
                discountAmount = result.DiscountAmount,
                discountAmountDisplay = result.DiscountAmount.ToString("N0", new System.Globalization.CultureInfo("vi-VN"))
            });
        }
    }
}
