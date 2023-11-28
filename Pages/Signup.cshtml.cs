using FarmerConnect.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace FarmerConnect.Pages
{
    public class SignupModel : PageModel
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

        public void OnPost() {
        
            try {

                users.username = Request.Form["username"];
                users.email = Request.Form["email"];
                String encripted = encryptPasswd(Request.Form["password"]);
                users.password = encripted;
                users.user_type = Request.Form["role"];

            } catch (Exception ex) {
                message = "There's a problem: " + ex.Message;
            }
            using (SqlConnection con = new SqlConnection(stgcon))
            {
                string query = "INSERT INTO users(username, email, password, user_type) VALUES(@username, @email, @password, @user_type);";

                try {
                
                    con.Open();
                    
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@username", users.username);
                        cmd.Parameters.AddWithValue ("@email", users.email);
                        cmd.Parameters.AddWithValue("@password", users.password);
                        cmd.Parameters.AddWithValue("@user_type", users.user_type);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            message = "Account Created";
                            users = new Users();
                        }
                        else
                        {
                            message = "Account Not Created";
                        }
                    }
                    con.Close();

                } catch (Exception ex) {
                    message = "There's a problem: "+ex.Message;
                }
            }
        }
    }

}
