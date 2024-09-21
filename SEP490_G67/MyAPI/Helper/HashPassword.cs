using System.Security.Cryptography;
using System.Text;

namespace MyAPI.Helper
{
    public class HashPassword
    {
        public string HashMD5Password(string password)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(passwordBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }

            return sb.ToString();
        }
    }
}
