using System.Text;
using System.Security.Cryptography;
namespace BussinessLayer
{
    public class clsUtil
    {
        public static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

    }
}
