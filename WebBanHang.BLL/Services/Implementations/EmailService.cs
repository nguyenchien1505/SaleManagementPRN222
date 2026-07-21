using System;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WebBanHang.BLL.Services.Interfaces;

namespace WebBanHang.BLL.Services.Implementations
{
    // Gửi email qua SMTP (cấu hình trong appsettings.json, mục "Smtp", xem SmtpSettings).
    // Nếu chưa cấu hình SMTP, service chỉ ghi log cảnh báo (Debug/Console) thay vì làm crash ứng dụng,
    // để không ảnh hưởng tới luồng nghiệp vụ chính (đăng ký / đặt hàng / xác nhận đơn).
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _settings;

        public EmailService(IOptions<SmtpSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                return;

            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.Username))
            {
                Debug.WriteLine($"[EmailService] Chưa cấu hình SMTP (mục 'Smtp' trong appsettings.json) - bỏ qua gửi email tới {toEmail}: {subject}");
                return;
            }

            try
            {
                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = _settings.EnableSsl
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.Username : _settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };
                message.To.Add(toEmail);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Không ném lỗi ra ngoài để không làm gián đoạn nghiệp vụ chính
                Debug.WriteLine($"[EmailService] Gửi email thất bại tới {toEmail}: {subject} - {ex.Message}");
            }
        }

        public Task SendRegistrationSuccessAsync(string toEmail, string fullName)
        {
            var subject = "Đăng ký tài khoản thành công";
            var body = $@"<p>Xin chào <b>{fullName}</b>,</p>
                          <p>Bạn đã đăng ký tài khoản thành công tại WebBanHang. Chúc bạn mua sắm vui vẻ!</p>";
            return SendAsync(toEmail, subject, body);
        }

        public Task SendOrderPlacedAsync(string toEmail, string fullName, string orderCode, decimal totalAmount)
        {
            var subject = $"Đặt hàng thành công - Đơn {orderCode}";
            var body = $@"<p>Xin chào <b>{fullName}</b>,</p>
                          <p>Đơn hàng <b>{orderCode}</b> của bạn đã được tạo thành công với tổng giá trị <b>{totalAmount:N0} xu</b>.</p>
                          <p>Đơn hàng đang chờ xác nhận. Chúng tôi sẽ thông báo khi đơn được xác nhận.</p>";
            return SendAsync(toEmail, subject, body);
        }

        public Task SendOrderConfirmedAsync(string toEmail, string fullName, string orderCode)
        {
            var subject = $"Đơn hàng {orderCode} đã được xác nhận";
            var body = $@"<p>Xin chào <b>{fullName}</b>,</p>
                          <p>Đơn hàng <b>{orderCode}</b> của bạn đã được xác nhận và đang được xử lý để giao đến bạn.</p>";
            return SendAsync(toEmail, subject, body);
        }
    }
}
