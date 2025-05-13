using System;

namespace ChatApp.Common.DAO
{
    public class GenerateRandomCode
    {
        private static Random random = new Random();

        public static string CreateCode()
        {
            int code = random.Next(100000, 1000000);
            return code.ToString();
        }
    }
}
