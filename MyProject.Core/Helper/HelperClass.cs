using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;

namespace MyProject.Core.Helper
{
    public class HelperClass
    {
        public static async Task<string> GenerateUniqueStudentEmail(NpgsqlConnection conn, string fullName)
        {
            string baseEmail = fullName.ToLower().Replace(" ", "") + "@school.com"; // johndoe@school.com
            string email = baseEmail;
            int count = 1;

            while (await EmailExists(conn, email))
            {
                email = $"{fullName.ToLower().Replace(" ", "")}{count}@school.com"; // johndoe1@school.com
                count++;
            }

            return email;
        }

        private static async Task<bool> EmailExists(NpgsqlConnection conn, string email)
        {
            if (conn.State == System.Data.ConnectionState.Closed)
                await conn.OpenAsync();

            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_users WHERE c_email = @c_email", conn))
            {
                cmd.Parameters.AddWithValue("@c_email", email);
                int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return count > 0; // Returns true if email exists
            }
        }

        public static string GenerateRandomPassword(int length = 10)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$!";
            Random random = new Random();
            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(chars);
        }
    }
}