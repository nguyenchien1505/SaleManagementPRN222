using System.Threading.Tasks;

namespace WebBanHang.BLL.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string toEmail, string subject, string htmlBody);

        // 1. Đăng ký thành công
        Task SendRegistrationSuccessAsync(string toEmail, string fullName);

        // 2. Đặt hàng thành công
        Task SendOrderPlacedAsync(string toEmail, string fullName, string orderCode, decimal totalAmount);

        // 3. Xác nhận đơn hàng
        Task SendOrderConfirmedAsync(string toEmail, string fullName, string orderCode);
    }
}
