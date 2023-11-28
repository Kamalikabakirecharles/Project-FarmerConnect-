using FarmerConnect.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace FarmerConnect.Pages
{
    public class PurchasesModel : PageModel
    {
        String stgcon = "Data Source=DESKTOP-8UTAP68\\SQLEXPRESS;Initial Catalog=FarmerConnect;Integrated Security=True";
        public string message = "";
        public List<Transaction> transactionlist = new List<Transaction>();
        Transaction transaction = new Transaction();

        public void OnGet()
        {
            if (TempData.Count > 0)
                message = TempData["Message"] as string;
            getPurchases();
            Console.WriteLine("OnGet method called.");
        }
        public void getPurchases()
        {
            try {

                int? userId = HttpContext.Session.GetInt32("UserId");
                transaction.buyer_id = userId.Value;

                using (SqlConnection con = new SqlConnection(stgcon))
                {
                    string query = "SELECT transaction_id ,buyer_id ,seller_id ,resource_id ,service_id ,quantity, total_amount, transaction_date FROM Transactions WHERE buyer_id=@buyer_id";
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@buyer_id", transaction.buyer_id);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Transaction transaction = new Transaction();

                                transaction.transaction_id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                                transaction.buyer_id = reader.IsDBNull(1) ? 0 : reader.GetInt32(1);
                                transaction.seller_id = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                                transaction.resource_id = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                                transaction.service_id = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                transaction.quantity = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                                transaction.total_amount = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                                transaction.transaction_date = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7);

                                transactionlist.Add(transaction);
                            }
                        }
                    }

                    con.Close();
                }

            }
            catch(Exception e) {
                message = "Problem: " + e.Message;
            }
        }
    }
}
