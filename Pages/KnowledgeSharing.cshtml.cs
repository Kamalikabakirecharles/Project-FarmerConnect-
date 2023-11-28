using FarmerConnect.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;

namespace FarmerConnect.Pages
{
    public class KnowledgeSharingModel : PageModel
    {
        String stgcon = "Data Source=DESKTOP-8UTAP68\\SQLEXPRESS;Initial Catalog=FarmerConnect;Integrated Security=True";
        KnowledgeSharing knowledgesharing = new KnowledgeSharing();
        public string message = "";
        public List<KnowledgeSharing> knowledgesharingList = new List<KnowledgeSharing>();

        public void OnGet()
        {
            if(TempData.Count > 0)
            message = TempData["Message"] as string;
            getknowledgesharing();
        }

        public void getknowledgesharing()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");
                knowledgesharing.user_id = userId.Value;

                using (SqlConnection con = new SqlConnection(stgcon))
                {
                    string query = "SELECT post_id, title, content, timestamp FROM KnowledgeSharing where user_id=@user_id";
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@user_id", knowledgesharing.user_id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                KnowledgeSharing knowledgesharing = new KnowledgeSharing();

                                knowledgesharing.Id = reader.GetInt32(0);
                                knowledgesharing.title = reader.GetString(1);
                                knowledgesharing.content = reader.GetString(2);
                                knowledgesharing.timestamp = reader.GetDateTime(3);

                                knowledgesharingList.Add(knowledgesharing);
                            }
                        }
                    }
                    con.Close();
                }

            }
            catch (Exception ex)
            {
                message = "Problem: " + ex.Message;
            }
        }

        public void OnPost()
        {
            try {
                int? userId = HttpContext.Session.GetInt32("UserId");
                knowledgesharing.user_id = userId.Value;
                knowledgesharing.title = Request.Form["title"];
                knowledgesharing.content = Request.Form["content"];


            }catch(Exception ex)
            {
                message = "Problem: " + ex.Message;
            }
            using(SqlConnection con = new SqlConnection(stgcon))
            {
                string query = "INSERT INTO KnowledgeSharing(user_id, title, [content]) VALUES(@user_id, @title, @content)";

                try {
                    
                    con.Open();
                    using(SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@user_id", knowledgesharing.user_id);
                        cmd.Parameters.AddWithValue("@title", knowledgesharing.title);
                        cmd.Parameters.AddWithValue("@content", knowledgesharing.content);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if(rowsAffected > 0)
                        {
                            message = "Knowledge Posted";
                            knowledgesharing = new KnowledgeSharing();
                        }
                        else
                        {
                            message = "Knowldge Failed";
                        }
                    }
                    con.Close();

                }catch(Exception ex)
                {
                    if(ex.Message.Contains("conflicted with the FOREIGN KEY"))
                    {
                        message = "Your User ID is Not in the Database";
                    }
                    else
                    {
                        message = "Problem: "+ex.Message;
                    }
                    
                }
            }
        }
    }
}
