using FarmerConnect.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace FarmerConnect.Pages
{
    public class LoginModel : PageModel
    {
        String stgcon = "Data Source=DESKTOP-8UTAP68\\SQLEXPRESS;Initial Catalog=FarmerConnect;Integrated Security=True";
        Users users = new Users();
        public string message = "";
        public void OnGet()
        {
        }
        private string encryptPasswd(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                string base64Hash = Convert.ToBase64String(hashBytes);
                return base64Hash;
            }
        }
        private int GetUserIdFromDatabase(string email, string password)
        {
            using (SqlConnection connection = new SqlConnection(stgcon))
            {
                connection.Open();

                string query = "SELECT user_id FROM users WHERE email = @email AND password = @password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", password);

                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        int userId = (int)result;
                        Console.WriteLine("User ID retrieved from database: " + userId);
                        return userId;
                    }
                }
            }

            return 0; // Return some default value or handle it as needed
        }
        private string GetUserRoleFromDatabase(string email, string password)
        {
            using (SqlConnection connection = new SqlConnection(stgcon))
            {
                connection.Open();

                string query = "SELECT user_type FROM users WHERE email = @email AND password = @password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", password);

                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string userRole = result.ToString();
                        Console.WriteLine("User Role retrieved from database: " + userRole);
                        return userRole;
                    }
                }
            }

            return null; // Return some default value or handle it as needed
        }
        private bool UserExistsInRoleTable(int userId, string userRole)
        {
            using (SqlConnection connection = new SqlConnection(stgcon))
            {
                connection.Open();

                string query;

                if (userRole.Equals("Farmer", StringComparison.OrdinalIgnoreCase))
                {
                    query = "SELECT COUNT(*) FROM Farmers WHERE [user_id] = @userId";
                }
                else if (userRole.Equals("Agribusiness", StringComparison.OrdinalIgnoreCase))
                {
                    query = "SELECT COUNT(*) FROM Agribusinesses WHERE [user_id] = @userId";
                }
                else
                {
                    return false;
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
        public IActionResult OnPost()
        {

            try
            {
                users.email = Request.Form["email"];
                String encripted = encryptPasswd(Request.Form["password"]);
                users.password = encripted;
            }
            catch (Exception ex)
            {
                message = "There's a problem: " + ex.Message;
            }
            using (SqlConnection con = new SqlConnection(stgcon))
            {
                string query = "SELECT COUNT(*) FROM users WHERE [email]=@email AND [password]=@password";

                try
                {

                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@email", users.email);
                        cmd.Parameters.AddWithValue("@password", users.password);
                        int count = (int)cmd.ExecuteScalar();
                        int userId = GetUserIdFromDatabase(users.email, users.password);

                        if (count > 0)
                        {
                            message = "Welcome";

                            if (userId > 0)
                            {
                                HttpContext.Session.SetInt32("UserId", userId);
                                Console.WriteLine("User ID stored in session: " + userId);
                                con.Close();
                                string userRole = GetUserRoleFromDatabase(users.email, users.password);

                                if (userRole != null)
                                {
                                    HttpContext.Session.SetString("UserRole", userRole);

                                    if (!UserExistsInRoleTable(userId, userRole))
                                    {
                                        if (userRole.Equals("Farmer", StringComparison.OrdinalIgnoreCase))
                                        {
                                            return RedirectToPage("/FarmersInfo");
                                        }
                                        else if (userRole.Equals("Agribusiness", StringComparison.OrdinalIgnoreCase))
                                        {
                                            return RedirectToPage("/AgribusinessesInfo");
                                        }
                                        else
                                        {
                                            message = "Invalid User Role";
                                            return Page();
                                        }

                                    }
                                    else
                                    {
                                        if (userRole.Equals("Farmer", StringComparison.OrdinalIgnoreCase))
                                        {
                                            return RedirectToPage("/FarmerHome");
                                        }
                                        else if (userRole.Equals("Agribusiness", StringComparison.OrdinalIgnoreCase))
                                        {
                                            return RedirectToPage("/Agribusiness");
                                        }
                                        else
                                        {
                                            message = "Invalid User Role";
                                            return Page();
                                        }
                                    }
                                }
                                else
                                {
                                    message = "User Role not found";
                                    return Page();
                                }

                            }
                            else
                            {
                                message = "nosession found";
                                con.Close();
                                return Page();
                            }
                        }
                        else
                        {
                            message = "Wrong Password or Email";
                            con.Close();
                            return Page();

                        }
                    }


                }
                catch (Exception ex)
                {
                    message = "There's a problem: " + ex.Message;
                    con.Close();
                    return Page();
                }
            }
        }
    }
}
