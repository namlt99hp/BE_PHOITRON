using System.Text;
using System.Security.Cryptography;

namespace BE_PHOITRON.Infrastructure.Shared
{
    public class SecurityHelper
{
    public static string ToMD5(string input)
    {
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Chuyển sang chuỗi hex
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
}
