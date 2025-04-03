using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyProject.Core.Interface;
using MyProject.Core.Models;
using Npgsql;

namespace MyProject.Core.Implementation
{
    public class UserImplementation : IuserInterface
    {
        private readonly NpgsqlConnection _connection;

        public UserImplementation(NpgsqlConnection npgsqlConnection)
        {
            _connection = npgsqlConnection;
        }

        public async Task<(bool success, string message)> userSignup(UserModel newUser)
        {
            try
            {
                if (_connection.State != System.Data.ConnectionState.Closed)
                {
                    await _connection.CloseAsync();
                }

                // user exists
                using (NpgsqlCommand cmd = new NpgsqlCommand("select COUNT(*)  from t_users where c_email=@c_email", _connection))
                {
                    cmd.Parameters.AddWithValue("@c_email", newUser.Email);

                    await _connection.OpenAsync();
                    int existingUserCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                    if (existingUserCount > 0)
                    {
                        return (false, "Email already exists.");
                    }
                }

                await _connection.CloseAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(@"insert into t_users (c_username,c_password_hash,c_role,c_email) values  (@c_username,@c_password_hash,@c_role,@c_email) ", _connection))
                {
                    cmd.Parameters.AddWithValue("@c_username", newUser.Username);
                    cmd.Parameters.AddWithValue("@c_password_hash", newUser.PasswordHash);
                    cmd.Parameters.AddWithValue("@c_role", newUser.Role);
                    cmd.Parameters.AddWithValue("@c_email", newUser.Email);
                    await _connection.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
                return (true, "User registered successfully!");

            }
            catch (Exception ex)
            {
                Console.WriteLine("error in usersignup in userImplementation");
                Console.WriteLine(ex.Message);
                return (false, "error in user login");
            }
            finally
            {
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    await _connection.CloseAsync();
                }
            }
        }

        public async Task<(bool success, string message, UserModel)> userLogin(UserLoginModel userlogin)
        {
            try
            {
                if (_connection.State == System.Data.ConnectionState.Closed)
                {
                    await _connection.OpenAsync();
                }
                using (var cmd = new NpgsqlCommand("select * from t_users where c_email=@c_email", _connection))
                {
                    cmd.Parameters.AddWithValue("@c_email", userlogin.Email);


                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!reader.HasRows)
                        {
                            return (false, "Invalid login credentials!", null);
                        }

                        await reader.ReadAsync(); // Move cursor to first row
                        var passwordHash = (string)reader["c_password_hash"];

                        if (passwordHash != userlogin.Password)
                        {
                            return (false, "Password is wrong", null);
                        }

                        var sc = new UserModel
                        {
                            UserId = (int)reader["c_user_id"],
                            Username = (string)reader["c_username"],
                            Email = (string)reader["c_email"],
                            Role = (string)reader["c_role"],
                            CreatedAt = (DateTime)reader["c_created_at"],
                            PasswordHash = null
                        };
                        return (true, "Login successful", sc);
                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("error in login in userimplement");
                System.Console.WriteLine(ex.Message);
                return (false, "Error occurred during login.", null);
            }
            finally
            {
                await _connection.CloseAsync();
            }
        }



    }
}