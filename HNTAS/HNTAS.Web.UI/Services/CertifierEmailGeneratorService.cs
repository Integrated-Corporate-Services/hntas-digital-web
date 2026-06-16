using System.Text;

namespace HNTAS.Web.UI.Services
{
    public class CertifierEmailGeneratorService
    {
        private const string EmailPrefix = "Certifier";
        private const string EmailDomain = "@mailinator.com";
        private const int DigitCount = 5;
        private static readonly Random _random = new();

        public string GenerateCertifierEmail()
        {
            var digits = new StringBuilder(DigitCount);
            for (int i = 0; i < DigitCount; i++)
            {
                digits.Append(_random.Next(0, 10)); // generates a digit between 0 and 9
            }

            return $"{EmailPrefix}{digits}{EmailDomain}";
        }
    }
}
