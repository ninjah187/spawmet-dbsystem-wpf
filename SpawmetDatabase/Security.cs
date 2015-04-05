using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace SpawmetDatabase
{
    public static class Security
    {
        public static string GetHash(string input)
        {
            using (var sha = SHA512.Create())
            {
                byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));

                return Encoding.UTF8.GetString(hashBytes);
            }
        }

        public static bool ValidateHash(string input, string hash)
        {
            if (GetHash(input) == hash)
            {
                return true;
            }
            return false;
        }
    }
}
