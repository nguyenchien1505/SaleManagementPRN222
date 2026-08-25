using Microsoft.AspNetCore.Mvc;
using WebBanHang.Utilities;

namespace WebBanHang.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("generate-hash")]
        public IActionResult GenerateHash(string password = "123456")
        {
            var hash = PasswordHasher.HashPassword(password);
            var verified = PasswordHasher.VerifyPassword(password, hash);

            return Ok(new
            {
                password,
                hash,
                verified,
                sqlScript = $"UPDATE [User] SET PasswordHash = '{hash}' WHERE UserId > 0;"
            });
        }
    }
}
