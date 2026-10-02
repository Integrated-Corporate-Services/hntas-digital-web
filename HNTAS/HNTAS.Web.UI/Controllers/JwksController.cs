using Microsoft.AspNetCore.Http;
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
            var keyConfigs = _configuration.GetSection("Jwks:Keys").GetChildren();
            var jwkList = new List<object>();

            foreach (var keyConfig in keyConfigs)
            {
                var keyId = keyConfig["KeyId"];
                var envVarName = keyConfig["PublicKeyPemEnvVar"];

                if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(envVarName))
                {
                    continue;
                }

                var publicKeyPem = Environment.GetEnvironmentVariable(envVarName)?
                    .Replace("\\n", "\n");

                if (string.IsNullOrWhiteSpace(publicKeyPem))
                {
                    continue; // Skip keys whose environment variables are not set
                }

                using var rsa = RSA.Create();
                rsa.ImportFromPem(publicKeyPem);

                var parameters = rsa.ExportParameters(false);

                jwkList.Add(new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = keyId,
                    alg = "RS256",
                    n = Base64UrlEncoder.Encode(parameters.Modulus!),
                    e = Base64UrlEncoder.Encode(parameters.Exponent!)
                });
            }

            if (jwkList.Count == 0)
            {
                return StatusCode(500, new { error = "No valid public keys are configured." });
            }

            return Ok(new { keys = jwkList });
        }
    }
}
