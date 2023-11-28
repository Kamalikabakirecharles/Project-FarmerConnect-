using FarmerConnect.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace FarmerConnect.Pages
{
   public class ResourcesModel : PageModel
    {
        String stgcon = "Data Source=DESKTOP-8UTAP68\\SQLEXPRESS;Initial Catalog=FarmerConnect;Integrated Security=True";
        public string message = "";
        Resources resources = new Resources();

        public void OnGet()
        {
        }

        public void OnPost() {

            try
            {
                resources.seller_id = int.Parse(Request.Form["seller_id"]);
                resources.name = Request.Form["name"];
                resources.quantity = int.Parse(Request.Form["quantity"]);
                resources.price = int.Parse(Request.Form["price"]);
                resources.description = Request.Form["description"];

            }
            catch (Exception ex)
            {
                message = "Problem: " + ex.Message;
            }
            using (SqlConnection con = new SqlConnection(stgcon))
            {
                string query = "INSERT INTO Resources(seller_id, name, quantity, price, description) VALUES(@seller_id, @name, @quantity, @price, @description)";

                try
                {

                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@seller_id", resources.seller_id);
                        cmd.Parameters.AddWithValue("@name", resources.name);
                        cmd.Parameters.AddWithValue("@quantity", resources.quantity);
                        cmd.Parameters.AddWithValue("@price", resources.price);
                        cmd.Parameters.AddWithValue("@description", resources.description);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            message = "Resource Saved";
                            resources = new Resources();
                        }
                        else
                        {
                            message = "Failed to save";
                        }
                    }
                    con.Close();

                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("conflicted with the FOREIGN KEY"))
                    {
                        message = "Your User ID is Not in the Database";
                    }
                    else
                    {
                        message = "Problem: " + ex.Message;
                    }

                }
            }

        }
    }
}
