using System;

namespace MongoAccounting.Utils
{
    internal static class EnsureExtensions
    {
        public static void EnsureNotNull(this object o, string name = null)
        {
            if (o == null)
            {
                if (name.IsNullOrEmpty())
                {
                    throw new ArgumentException();
                }
                throw new ArgumentException(name);
            }
        }
    }
}