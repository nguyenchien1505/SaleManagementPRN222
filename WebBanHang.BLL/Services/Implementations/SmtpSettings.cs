namespace WebBanHang.BLL.Services.Implementations
{
    // Cấu hình SMTP, được bind từ mục "Smtp" trong appsettings.json (xem Program.cs)
    public class SmtpSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = "WebBanHang";
        public bool EnableSsl { get; set; } = true;
    }
}
