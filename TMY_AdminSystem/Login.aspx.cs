using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace TMY_AdminSystem
{
    public partial class Login : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["TMY_DB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

            // ✅ 如果有 ?logout=1 → 清空 Session 後返回登入頁
            if (!IsPostBack && Request.QueryString["logout"] == "1")
            {
                Session.Clear();
                Response.Redirect("~/Login.aspx");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT UserID, Username, PasswordHash, Salt, Role FROM Users WHERE Username=@u";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    string salt = dr["Salt"].ToString();
                    string dbHash = dr["PasswordHash"].ToString();   // 資料庫裡的雜湊密碼
                    string inputHash = HashPassword(password, salt); // 使用者輸入的密碼 + salt 去做雜湊比對

                    if (dbHash == inputHash)
                    {
                        // ✅ 登入成功 → 寫入 Session / 導頁
                        Session["UserID"] = dr["UserID"].ToString();
                        Session["Username"] = dr["Username"].ToString();
                        Session["UserRole"] = dr["Role"].ToString();

                        Response.Redirect("~/MainMenu.aspx");
                    }
                    else
                    {
                        lblMessage.Text = "❗ 帳號或密碼錯誤！";
                    }
                }
                else
                {
                    lblMessage.Text = "❗ 帳號不存在！";
                }
                conn.Close();
            }
        }

        // ✅ 雜湊密碼方法（跟 Register 同步）
        private string HashPassword(string password, string salt)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

    }
}
