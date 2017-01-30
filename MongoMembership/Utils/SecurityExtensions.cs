using System.Security.Cryptography;

namespace MongoMembership.Utils
{
    internal static class SecurityExtensions
    {
        public static byte[] ComputeHash(this byte[] allBytes)
        {
            return HashAlgorithm.Create("SHA1").ComputeHash(allBytes);
        }

        public static void RngGenerator(this byte[] allBytes)
        {
            new RNGCryptoServiceProvider().GetBytes(allBytes);
        }
    }
}
