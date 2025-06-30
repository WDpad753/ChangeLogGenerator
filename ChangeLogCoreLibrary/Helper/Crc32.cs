using ChangeLogCoreLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.Helper
{
    public static class Crc32
    {
        public static string CalculateHash(MapAzureJson input)
        {
            StringBuilder sb = new StringBuilder();
            int cnt = 0;
            foreach (var item in input.Value)
            {
                if (cnt == 0)
                {
                    sb.Append($"{item}");
                }
                else
                {
                    sb.Append($"|{item}");
                }
                cnt++;
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
                StringBuilder hashBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashBuilder.Append(b.ToString("x2"));
                }
                return hashBuilder.ToString();
            }
        }
    }
}
