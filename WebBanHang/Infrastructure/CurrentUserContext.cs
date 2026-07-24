using WebBanHang.DAL.Abstractions;

namespace WebBanHang.Infrastructure
{
    public sealed class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        public int? UserId => HttpContext?.Session.GetInt32("UserId");

        public string UserName => HttpContext?.Session.GetString("Username")
            ?? HttpContext?.User?.Identity?.Name
            ?? "System";

        public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();

        public string? RequestPath => HttpContext?.Request.Path.Value;

        public string? Role => HttpContext?.Session.GetString("Role");
    }
}
