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

            // 如果有 ?logout=1 → 清空 Session 後返回登入頁
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
                string sql = @"SELECT UserID, Username, PasswordHash, Salt, Role, FailedCount, LockUntil 
                       FROM Users 
                       WHERE Username=@u";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@u", username);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    
                    int userId = Convert.ToInt32(dr["UserID"]);
                    string dbUsername = dr["Username"].ToString();
                    string dbHash = dr["PasswordHash"].ToString();
                    string salt = dr["Salt"].ToString();
                    string role = dr["Role"].ToString();

                    int failedCount = dr["FailedCount"] == DBNull.Value ? 0 : Convert.ToInt32(dr["FailedCount"]);
                    DateTime? lockUntil = dr["LockUntil"] as DateTime?;


                    dr.Close(); 

                    //檢查帳號鎖定機制
                    if (lockUntil.HasValue && lockUntil.Value > DateTime.Now)
                    {
                        TimeSpan remain = lockUntil.Value - DateTime.Now;
                        lblMessage.Text = $"⛔ 帳號鎖定中，請 {remain.Minutes} 分鐘後再試。";
                        return;
                    }

                    // 比對密碼
                    string inputHash = HashPassword(password, salt);

                    if (dbHash == inputHash)
                    {
                        // 登入成功 → 清空 FailedCount & LockUntil
                        string resetSql = @"UPDATE Users 
                                    SET FailedCount = 0, LockUntil = NULL 
                                    WHERE Username=@u";

                        SqlCommand resetCmd = new SqlCommand(resetSql, conn);
                        resetCmd.Parameters.AddWithValue("@u", username);
                        resetCmd.ExecuteNonQuery();

                        // 寫入 Session
                        Session["UserID"] = userId;
                        Session["Username"] = dbUsername;
                        Session["UserRole"] = role;

                        Response.Redirect("~/MainMenu.aspx");
                    }
                    else
                    {
                        //密碼錯誤 → 增加錯誤次數
                        failedCount++;

                        if (failedCount >= 3)
                        {
                            // 超過 3 次 → 鎖定帳號 15 分鐘
                            string lockSql = @"UPDATE Users 
                                       SET FailedCount = @fc,
                                           LockUntil = @lockUntil
                                       WHERE Username=@u";

                            SqlCommand lockCmd = new SqlCommand(lockSql, conn);
                            lockCmd.Parameters.AddWithValue("@fc", failedCount);
                            lockCmd.Parameters.AddWithValue("@lockUntil", DateTime.Now.AddMinutes(15));
                            lockCmd.Parameters.AddWithValue("@u", username);
                            lockCmd.ExecuteNonQuery();

                            lblMessage.Text = "⛔ 錯誤超過 3 次，帳號已鎖定 15 分鐘。";
                            return;
                        }
                        else
                        {
                            // 一般錯誤 → 寫入 FailedCount
                            string updateError = @"UPDATE Users 
                                           SET FailedCount=@fc 
                                           WHERE Username=@u";

                            SqlCommand errorCmd = new SqlCommand(updateError, conn);
                            errorCmd.Parameters.AddWithValue("@fc", failedCount);
                            errorCmd.Parameters.AddWithValue("@u", username);
                            errorCmd.ExecuteNonQuery();

                            lblMessage.Text = $"❗ 密碼錯誤！你已錯誤 {failedCount} 次。";
                            return;
                        }
                    }
                }
                else
                {
                    lblMessage.Text = "❗ 帳號不存在！";
                }

                conn.Close();
            }
        }


        // 雜湊密碼方法
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
