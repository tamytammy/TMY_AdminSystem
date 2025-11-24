using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                LoadProfileData(); 
                if (Session["UserID"] != null)
                {
                    int userId = Convert.ToInt32(Session["UserID"]);
                    int salaryId = 0;

                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        string sql = @"
            SELECT TOP 1 SalaryID
            FROM SalaryRecords
            WHERE EmployeeID = @emp
            ORDER BY SalaryYear DESC, SalaryMonth DESC";

                        SqlCommand cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@emp", userId);

                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            salaryId = Convert.ToInt32(result);
                        }
                    }

                    // 如果找到資料 → 載入薪資細項
                    if (salaryId > 0)
                    {
                        LoadSalaryDetail(salaryId);
                    }
                    else
                    {
                        lblProfileMsg.Text = "⚠ 尚無薪資資料";
                    }
                }


                //預設載入員工資料，當此使用者擁有權限
                BindEmployees();
                LoadAttendance();

                if(Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin")
                {
                    member_pl.Visible = true;
                    lnkAddEmployee.Visible = true;
                }
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

        // ✅ 載入登入者的薪資資料
        private void LoadSalaryDetail(int salaryId)
        {

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // 1. 抓 SalaryItems（所有薪資細項）
                string sql1 = @"SELECT ItemName, ItemAmount 
                        FROM SalaryItems 
                        WHERE SalaryID = @sid
                        ORDER BY ItemType DESC"; 

                SqlCommand cmd1 = new SqlCommand(sql1, conn);
                cmd1.Parameters.AddWithValue("@sid", salaryId);

                SqlDataAdapter da = new SqlDataAdapter(cmd1);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvSalaryDetail.DataSource = dt;
                gvSalaryDetail.DataBind();

                // 2. 抓主表 SalaryRecords（實領金額、日期）
                string sql2 = @"SELECT NetPay, PayDate 
                        FROM SalaryRecords 
                        WHERE SalaryID = @sid";

                SqlCommand cmd2 = new SqlCommand(sql2, conn);
                cmd2.Parameters.AddWithValue("@sid", salaryId);

                SqlDataReader dr = cmd2.ExecuteReader();
                if (dr.Read())
                {
                    lblNetPay.Text = Convert.ToInt32(dr["NetPay"]).ToString("C0");
                    lblPayDate.Text = Convert.ToDateTime(dr["PayDate"]).ToString("yyyy/MM/dd");
                }
                dr.Close();
            }
        }

        // ✅ 載入登入者的出勤資料
        private void LoadAttendance()
        {
            string empId = Session["UserID"].ToString();
            //int year = Convert.ToInt32(ddlAttYear.SelectedValue);
            //int month = Convert.ToInt32(ddlAttMonth.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
            SELECT WorkDate, 
                   CONVERT(VARCHAR(5), CheckInTime, 108) AS CheckInTime,
                   CONVERT(VARCHAR(5), CheckOutTime, 108) AS CheckOutTime,
                   WorkHours, 
                   OvertimeHours, 
                   LeaveType, 
                   Remark
            FROM AttendanceRecords
            WHERE EmployeeID = @emp
            ORDER BY WorkDate";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@emp", empId);
                //cmd.Parameters.AddWithValue("@y", year);
                //cmd.Parameters.AddWithValue("@m", month);

                conn.Open();

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvAttendance.DataSource = dt;
                gvAttendance.DataBind();
            }
        }





        // ✅ 儲存更新資料
        protected void btnSaveProfile_Click(object sender, EventArgs e)
        {
            int userId = Convert.ToInt32(Session["UserID"]);
            // 先清除紅框
            txtFullName.CssClass = txtFullName.CssClass.Replace(" input-error", "");
            txtHireDate.CssClass = txtHireDate.CssClass.Replace(" input-error", "");
            ddlDept.CssClass = ddlDept.CssClass.Replace(" input-error", "");

            // =============================
            // 🔍 必填檢查（全部寫在這裡）
            // =============================

            // 1. 姓名
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                txtFullName.CssClass += " input-error";
                lblProfileMsg.Text = "❌ 姓名為必填欄位！";
                return;
            }

            // 2. 入職日期
            if (string.IsNullOrWhiteSpace(txtHireDate.Text))
            {
                txtHireDate.CssClass += " input-error";
                lblProfileMsg.Text = "❌ 入職日期為必填欄位！";
                return;
            }

            // 3. 部門
            if (ddlDept.SelectedValue == "0" || string.IsNullOrEmpty(ddlDept.SelectedValue))
            {
                ddlDept.CssClass += " input-error";
                lblProfileMsg.Text = "❌ 請選擇部門！";
                return;
            }

            // -----------------------------
            // ❇ 必填檢查通過 → 開始資料寫入
            // -----------------------------
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    // 1. 檢查 Employees 表是否已有資料
                    string checkSql = @"SELECT COUNT(*) FROM Employees WHERE EmployeeID = @id";
                    SqlCommand checkCmd = new SqlCommand(checkSql, conn, transaction);
                    checkCmd.Parameters.AddWithValue("@id", userId);
                    int count = (int)checkCmd.ExecuteScalar();

                    // 2. 準備共用參數
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.Transaction = transaction;

                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.Parameters.AddWithValue("@FullName", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@DeptID", ddlDept.SelectedValue);
                    cmd.Parameters.AddWithValue("@JobTitle", txtJobTitle.Text);
                    cmd.Parameters.AddWithValue("@JobGrade", txtJobGrade.Text);

                    if (string.IsNullOrEmpty(txtHireDate.Text))
                        cmd.Parameters.AddWithValue("@HireDate", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@HireDate", txtHireDate.Text);

                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                    cmd.Parameters.AddWithValue("@Role", ddlRole.SelectedValue); // Users 用 (字串)

                    // ✅ 新增：加入 RoleID 參數 (從新的 ddlRoleID 取值)
                    cmd.Parameters.AddWithValue("@RoleID", ddlRoleID.SelectedValue);

                    // 3. 處理 Employees 資料表
                    if (count > 0)
                    {
                        // ✅ UPDATE Employees (補上 RoleID=@RoleID)
                        cmd.CommandText = @"UPDATE Employees SET 
                                    FullName=@FullName, Email=@Email, DeptID=@DeptID, 
                                    JobTitle=@JobTitle, JobGrade=@JobGrade, HireDate=@HireDate, 
                                    Status=@Status, Role=@Role, RoleID=@RoleID
                                WHERE EmployeeID=@id";
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        // ✅ INSERT Employees (補上 RoleID)
                        cmd.CommandText = @"INSERT INTO Employees 
                                    (EmployeeID, FullName, Email, DeptID, JobTitle, JobGrade, HireDate, Status, Role, RoleID, CreateTime)
                                VALUES 
                                    (@id, @FullName, @Email, @DeptID, @JobTitle, @JobGrade, @HireDate, @Status, @Role, @RoleID, GETDATE())";
                        cmd.ExecuteNonQuery();
                    }

                    // 4. 同步更新 Users 資料表
                    cmd.CommandText = @"UPDATE Users SET 
                                Role=@Role, 
                                Email=@Email 
                            WHERE UserID=@id";
                    cmd.ExecuteNonQuery();

                    // 5. 提交
                    transaction.Commit();
                    lblProfileMsg.Text = "✅ 資料儲存成功！";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    lblProfileMsg.Text = "❌ 儲存失敗：" + ex.Message;
                }
                finally
                {
                    conn.Close();
                }
            }
            lblProfileMsg.Text = "✅ 個人資料更新成功！";
        }

        //員工管理 查詢
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindEmployees();
            upSearch.Update(); 
        }

        protected void gvResult_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "OpenResetModal")
            {
                string empId = e.CommandArgument.ToString();

                // 暫存 employeeID 在 hidden label
                lblResetUserId.Text = empId;

                // 清空欄位
                txtNewPwd.Text = "";
                txtConfirmPwd.Text = "";
                lblResetMsg.Text = "";

                // 開啟 Modal
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "ShowResetPwdModal", "$('#resetPwdModal').modal('show');", true);
            }
        }

        //重設密碼確認按鈕
        protected void btnConfirmReset_Click(object sender, EventArgs e)
        {
            string newPwd = txtNewPwd.Text.Trim();
            string confirm = txtConfirmPwd.Text.Trim();
            int empId = Convert.ToInt32(lblResetUserId.Text);

            if (newPwd == "" || confirm == "")
            {
                lblResetMsg.Text = "❗ 密碼欄位不可為空！";
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowAgain", "$('#resetPwdModal').modal('show');", true);
                return;
            }

            if (newPwd != confirm)
            {
                lblResetMsg.Text = "❗ 兩次輸入的密碼不一致！";
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowAgain", "$('#resetPwdModal').modal('show');", true);
                return;
            }

            // 產生新 Salt
            string salt = Guid.NewGuid().ToString("N").Substring(0, 8);

            // Hash 新密碼
            string hashPwd = HashPassword(newPwd, salt);

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                string sql = @"UPDATE Users 
                       SET PasswordHash=@hash,
                           Salt=@salt,
                           FailedCount=0,
                           LockUntil=NULL
                       WHERE UserID=@id";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@hash", hashPwd);
                cmd.Parameters.AddWithValue("@salt", salt);
                cmd.Parameters.AddWithValue("@id", empId);

                cmd.ExecuteNonQuery();
                conn.Close();
            }

            // 顯示成功訊息
            //lblMessage.Text = "✅ 密碼已成功重設！";

            // 關閉 Modal
            ScriptManager.RegisterStartupScript(this, GetType(),
                "HideReset", "$('#resetPwdModal').modal('hide');", true);
        }

        private void BindEmployees()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = @"
            SELECT E.EmployeeID, E.FullName, E.Email, D.DeptName, 
                   E.JobTitle, E.JobGrade, E.HireDate
            FROM Employees E
            LEFT JOIN Departments D ON E.DeptID = D.DeptID
        ";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvResult.DataSource = dt;
                gvResult.DataBind();
            }
        }

        private string HashPassword(string password, string salt)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        //薪資紀錄查詢
        protected void btnSearchSalary_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlYear.SelectedValue) ||
                string.IsNullOrEmpty(ddlMonth.SelectedValue))
            {
                lblProfileMsg.Text = "❌ 請選擇完整的年份與月份！";
                upSalary.Update();
                return;
            }

            int year = Convert.ToInt32(ddlYear.SelectedValue);
            int month = Convert.ToInt32(ddlMonth.SelectedValue);
            int userId = Convert.ToInt32(Session["UserID"]);

            string sql = @"SELECT SalaryID 
                   FROM SalaryRecords
                   WHERE EmployeeID=@emp AND SalaryYear=@y AND SalaryMonth=@m";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@emp", userId);
                cmd.Parameters.AddWithValue("@y", year);
                cmd.Parameters.AddWithValue("@m", month);

                object result = cmd.ExecuteScalar();


                if (result == null)
                {
                    lblProfileMsg2.Text = "⚠ 該月份沒有薪資資料。";
                    gvSalaryDetail.DataSource = null;
                    gvSalaryDetail.DataBind();
                    lblNetPay.Text = "-";
                    lblPayDate.Text = "-";

                    upSalary.Update();
                    return;
                }

                int salaryId = Convert.ToInt32(result);
                LoadSalaryDetail(salaryId);
            }

            upSalary.Update();
        }

        //出勤資料查詢
        protected void btnSearchAttendance_Click(object sender, EventArgs e)
        {
            LoadAttendance();
            upAttendance.Update();
        }

        protected void gvAttendance_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string leave = DataBinder.Eval(e.Row.DataItem, "LeaveType")?.ToString();
                string overtime = DataBinder.Eval(e.Row.DataItem, "OvertimeHours")?.ToString();
                string checkin = DataBinder.Eval(e.Row.DataItem, "CheckInTime")?.ToString();

                // 請假（黃色）
                if (!string.IsNullOrEmpty(leave))
                {
                    e.Row.BackColor = System.Drawing.ColorTranslator.FromHtml("#FFF3CD"); // 淡黃
                    return;
                }

                // 無打卡（淡紅）
                if (string.IsNullOrEmpty(checkin))
                {
                    e.Row.BackColor = System.Drawing.ColorTranslator.FromHtml("#F8D7DA");
                    return;
                }

                // 加班（淡綠）
                if (decimal.TryParse(overtime, out decimal ot) && ot > 0)
                {
                    e.Row.BackColor = System.Drawing.ColorTranslator.FromHtml("#D4EDDA"); // 綠色
                    return;
                }

                // 正常出勤（白色） → 不需特別設定
            }
        }


        //新增帳號權限下拉選單
        protected void ddlRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 1. 先清空職級選單
            ddlRoleID.Items.Clear();

            // 2. 根據選擇的角色群組，填入對應的職級 ID
            string selectedRole = ddlRole.SelectedValue;

            switch (selectedRole)
            {
                case "User":
                    // User 對應 RoleID 1
                    ddlRoleID.Items.Add(new ListItem("一般員工", "1"));
                    break;

                case "Manager":
                    // Manager 對應 RoleID 2, 3
                    ddlRoleID.Items.Add(new ListItem("部門主任", "2"));
                    ddlRoleID.Items.Add(new ListItem("部門經理", "3"));
                    break;

                case "Admin":
                    // Admin 對應 RoleID 4, 5 (雖然您說通常不設置，但預留著比較保險)
                    ddlRoleID.Items.Add(new ListItem("總經理 (Rank 4)", "4"));
                    ddlRoleID.Items.Add(new ListItem("董事長 (Rank 5)", "5"));
                    break;

                default:
                    ddlRoleID.Items.Add(new ListItem("一般員工 (Rank 1)", "1"));
                    break;
            }
        }




    }

}