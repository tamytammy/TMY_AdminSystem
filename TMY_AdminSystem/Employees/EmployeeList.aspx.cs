using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
//using System.Web.UI.Page;
using System.Web.UI.WebControls;

namespace TMY_AdminSystem.Employees
{
    public partial class EmployeeList : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["TMY_DB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDepartments();
                LoadProfileData(); // 載入個人資料
            }
        }
        // ✅ 載入部門 DropDownList
        private void LoadDepartments(int? currentDeptId = null)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT DeptID, DeptName FROM Departments";

                SqlCommand cmd = new SqlCommand(sql, conn);

                conn.Open();
                ddlDept.DataSource = cmd.ExecuteReader();
                ddlDept.DataTextField = "DeptName";
                ddlDept.DataValueField = "DeptID";
                ddlDept.DataBind();
                conn.Close();
            }

            // ✅ 如果該使用者已有部門 → 選取該部門
            if (currentDeptId.HasValue)
            {
                ddlDept.SelectedValue = currentDeptId.Value.ToString();
            }
            else
            {
                // ✅ 如果沒有資料，預設新增一個提示項目「請選擇部門」
                ddlDept.Items.Insert(0, new ListItem("請選擇部門", ""));
            }
        }


        // ✅ 載入登入者的 Employee 資料
        private void LoadProfileData()
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"SELECT FullName, Email, DeptID, JobTitle, JobGrade, HireDate, Status, Role 
                       FROM Employees WHERE EmployeeID=@id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    txtFullName.Text = dr["FullName"].ToString();
                    txtEmail.Text = dr["Email"].ToString();
                    txtJobTitle.Text = dr["JobTitle"].ToString();
                    txtJobGrade.Text = dr["JobGrade"].ToString();
                    ddlStatus.SelectedValue = dr["Status"].ToString();
                    ddlRole.SelectedValue = dr["Role"].ToString();

                    if (dr["HireDate"] != DBNull.Value)
                        txtHireDate.Text = Convert.ToDateTime(dr["HireDate"]).ToString("yyyy-MM-dd");

                    // ✅ 動態載入部門選項 & 指定預設部門
                    if (dr["DeptID"] != DBNull.Value)
                    {
                        LoadDepartments(Convert.ToInt32(dr["DeptID"]));
                    }
                    else
                    {
                        LoadDepartments(null); // 載入所有並顯示「請選擇部門」
                    }
                }
                conn.Close();
            }
        }


        // ✅ 儲存更新資料
        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // ✅ 先檢查是否已有資料
                string checkSql = @"SELECT COUNT(*) FROM Employees WHERE EmployeeID = @id";
                SqlCommand checkCmd = new SqlCommand(checkSql, conn);
                checkCmd.Parameters.AddWithValue("@id", userId);

                int count = (int)checkCmd.ExecuteScalar();

                string sql;

                if (count > 0)
                {
                    // ✅ 已有資料 → 執行 UPDATE
                    sql = @"UPDATE Employees SET 
                        FullName=@FullName,
                        Email=@Email,
                        DeptID=@DeptID,
                        JobTitle=@JobTitle,
                        JobGrade=@JobGrade,
                        HireDate=@HireDate,
                        Status=@Status,
                        Role=@Role
                    WHERE EmployeeID=@id";
                }
                else
                {
                    // ✅ 尚未有資料 → 執行 INSERT
                    sql = @"INSERT INTO Employees 
                        (EmployeeID, FullName, Email, DeptID, JobTitle, JobGrade, HireDate, Status, Role, CreateTime)
                    VALUES 
                        (@id, @FullName, @Email, @DeptID, @JobTitle, @JobGrade, @HireDate, @Status, @Role, GETDATE())";
                }

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@FullName", txtFullName.Text);
                cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                cmd.Parameters.AddWithValue("@DeptID", ddlDept.SelectedValue);
                cmd.Parameters.AddWithValue("@JobTitle", txtJobTitle.Text);
                cmd.Parameters.AddWithValue("@JobGrade", txtJobGrade.Text);
                cmd.Parameters.AddWithValue("@HireDate", txtHireDate.Text == "" ? (object)DBNull.Value : txtHireDate.Text);
                cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@Role", ddlRole.SelectedValue);

                cmd.ExecuteNonQuery();
                conn.Close();
            }

            lblProfileMsg.Text = "✅ 個人資料更新成功！";
        }

    }

}
