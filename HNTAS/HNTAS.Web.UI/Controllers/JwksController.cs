using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace HNTAS.Web.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JwksController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public JwksController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("/.well-known/jwks.json")]
        public IActionResult Get()
        {
            var keyId = _configuration["Jwks:KeyId"];

            if (string.IsNullOrEmpty(keyId))
            {
                return StatusCode(500, new { error = "JWK configuration is missing." });
            }

            var publicKeyPem = Environment.GetEnvironmentVariable("ONELOGIN_PUBLIC_KEY")?
                .Replace("\\n", "\n");

            if (string.IsNullOrWhiteSpace(publicKeyPem))
            {
                return StatusCode(500, new { error = "Public key is not configured." });
            }

            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyPem);

            var parameters = rsa.ExportParameters(false);

            return Ok(new
            {
                keys = new[]
                {
                    new
                    {
                        kty = "RSA",
                        use = "sig",
                        kid = keyId,
                        alg = "RS256",
                        n = Base64UrlEncoder.Encode(parameters.Modulus!),
                        e = Base64UrlEncoder.Encode(parameters.Exponent!)
                    }
                }
            });
        }
    }
}
