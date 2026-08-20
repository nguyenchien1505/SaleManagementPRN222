using Microsoft.AspNetCore.Mvc;
using WebBanHang.BLL.Services.Implementations;
using WebBanHang.BLL.Services.Interfaces;

namespace WebBanHang.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PaymentController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IConfiguration _configuration;
        private readonly IOrderService _orderService;

        public PaymentController(IConfiguration configuration, IOrderService orderService, ICartService cartService)
        {
            _configuration = configuration;
            _orderService = orderService;
            _cartService = cartService;
        }

        [HttpGet]
        [Route("Payment/CreatePayment")]
        public async Task<IActionResult> CreatePayment(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Home");
            }

            await _orderService.UpdatePaymentInfoAsync(orderId, "VNPay", "Pending");

            string url = _configuration["VnPay:BaseUrl"];
            string returnUrl = _configuration["VnPay:ReturnUrl"];
            string tmnCode = _configuration["VnPay:TmnCode"];
            string hashSecret = _configuration["VnPay:HashSecret"];

            PayLib pay = new PayLib();

            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", ((long)(order.TotalAmount * 100)).ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");

            string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }
            pay.AddRequestData("vnp_IpAddr", ipAddress);
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang {order.OrderCode}");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", order.OrderId.ToString());

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);
            return Redirect(paymentUrl);
        }

        [HttpGet]
        [Route("Payment/PaymentCallBack")]
        public async Task<IActionResult> PaymentCallBack()
        {
            var vnpayData = Request.Query;
            PayLib pay = new PayLib();

            foreach (var (key, value) in vnpayData)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    pay.AddResponseData(key, value);
                }
            }

            string orderIdStr = pay.GetResponseData("vnp_TxnRef");
            string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode");
            string vnp_TransactionNo = pay.GetResponseData("vnp_TransactionNo");
            string vnp_SecureHash = Request.Query["vnp_SecureHash"];
            string hashSecret = _configuration["VnPay:HashSecret"];

            bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret);

            if (!int.TryParse(orderIdStr, out int orderId))
            {
                TempData["Error"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("Index", "Home");
            }

            if (!checkSignature)
            {
                TempData["Error"] = "Lỗi xác thực chữ ký bảo mật (Signature failed).";
                return RedirectToAction("Index", "Home");
            }

            var order = await _orderService.GetOrderDetailsAsync(orderId);
            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Index", "Cart", new { area = "Customer" });
            }

            if (vnp_ResponseCode == "00")
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, "Paid");

                await _orderService.ConfirmOrderAndDeductStockAsync(orderId);

                if (order.OrderDetails != null)
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        await _cartService.RemoveFromCartAsync(order.CustomerId, detail.ProductId);
                    }
                }

                TempData["Success"] = $"Thanh toán thành công đơn hàng #{orderId}. Mã GD VNPay: {vnp_TransactionNo}";
                return RedirectToAction("MyOrders", "Order", new { area = "Customer" });
            }
            else
            {
                await _orderService.UpdatePaymentStatusAsync(orderId, "Failed");

                await _orderService.DeleteOrderIfFailedAsync(orderId);

                TempData["Error"] = $"Thanh toán thất bại hoặc bị hủy. Mã lỗi: {vnp_ResponseCode}. Đơn hàng đã được hủy.";

                return RedirectToAction("Index", "Cart", new { area = "Customer" });
            }
        }
    }
}