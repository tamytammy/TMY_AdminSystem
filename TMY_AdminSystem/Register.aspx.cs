using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace TMY_AdminSystem
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnRegister_Click(object sender, EventArgs e)
        {
            string account = txtAccount.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();
            string confirm = txtConfirm.Text.Trim();
            string passwordHint = txtPasswordHint.Text.Trim();
            string role = "User";  // 預設角色

            if (password != confirm)
            {
                lblMsg.Text = "❗ 兩次輸入的密碼不一致";
                return;
            }

            if (account == "" || password == "")
            {
                lblMsg.Text = "❗ 帳號與密碼不可為空";
                return;
            }

            // 建立 Salt
            string salt = Guid.NewGuid().ToString("N").Substring(0, 8);

            // 建立 Hash 密碼
            string hashPwd = HashPassword(password, salt);

            string connStr = ConfigurationManager.ConnectionStrings["TMY_DB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"INSERT INTO Users 
                               (Username, PasswordHash, Salt, Email, Role, CreateDate, PasswordHint)
                               VALUES 
                               (@Username, @PasswordHash, @Salt, @Email, @Role, GETDATE(), @PasswordHint)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Username", account);
                cmd.Parameters.AddWithValue("@PasswordHash", hashPwd);
                cmd.Parameters.AddWithValue("@Salt", salt);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Role", role);
                cmd.Parameters.AddWithValue("@PasswordHint", passwordHint);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }

            lblMsg.Text = "✅ 註冊成功！請前往登入頁面";
        }

        private string HashPassword(string password, string salt)
        {
            SHA256 sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
            byte[] hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}
