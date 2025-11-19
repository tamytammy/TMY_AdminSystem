using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TMY_AdminSystem.Apply
{
    public partial class ApplyList : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["TMY_DB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 1. 初始化日期 (預設查近 3 個月，避免資料太多)
                txtDateStart.Text = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd");
                txtDateEnd.Text = DateTime.Now.ToString("yyyy-MM-dd");

                // 2. 【關鍵需求】預設載入「本部門案件」
                // 雖然前端預設可能是 MyApply，但這裡我們依需求強制改為 MyDept
                if (ddlViewMode.Items.FindByValue("MyDept") != null)
                {
                    ddlViewMode.SelectedValue = "MyDept";
                }

                // 3. 執行查詢
                BindGrid();
            }
        }
        private void BindGrid()
        {
            // 1. 取得目前登入者資訊
            // ❗ 務必確保 Session 有值，否則導回登入頁
            if (Session["UserID"] == null)
            {
                // Response.Redirect("Login.aspx"); 
                return;
            }

            int myID = Convert.ToInt32(Session["UserID"]);
            string myDeptID = "";

            // 先取得使用者的部門 ID (為了 "MyDept" 查詢用)
            // 實務上建議登入時就存入 Session["DeptID"]，這裡示範從 DB 抓
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sqlUser = "SELECT DeptID FROM Employees WHERE EmployeeID = @ID";
                SqlCommand cmdUser = new SqlCommand(sqlUser, conn);
                cmdUser.Parameters.AddWithValue("@ID", myID);
                conn.Open();
                object result = cmdUser.ExecuteScalar();
                if (result != null) myDeptID = result.ToString();
            }

            // 2. 準備基礎 SQL (關聯 Flow_Types, Employees 來顯示名稱)
            StringBuilder sql = new StringBuilder();
            sql.Append(@"
                SELECT 
                    F.FormID, 
                    F.FormNumber, 
                    F.Title, 
                    F.Status, 
                    F.CreateDate,
                    F.CurrentHandlerID,
                    F.ApplicantID,
                    C.CategoryName, 
                    T.TypeName,
                    E1.FullName AS ApplicantName,     -- 申請人姓名
                    D.DeptName,    -- 申請人部門 (這裡簡化直接顯示 ID，若有部門表可再 JOIN)
                    E2.FullName AS CurrentHandlerName -- 目前處理人姓名
                FROM Flow_Forms F
                LEFT JOIN Flow_Types T ON F.TypeID = T.TypeID
                LEFT JOIN Flow_Categories C ON T.CategoryID = C.CategoryID
                LEFT JOIN Employees E1 ON F.ApplicantID = E1.EmployeeID
                LEFT JOIN Employees E2 ON F.CurrentHandlerID = E2.EmployeeID
                LEFT JOIN Departments D ON E1.DeptID = D.DeptID
                WHERE 1=1 
            ");

            // 3. 處理「檢視模式」邏輯 (MyApply / MyAudit / MyDept)
            string viewMode = ddlViewMode.SelectedValue;

            switch (viewMode)
            {
                case "MyApply": // 我申請的
                    sql.Append(" AND F.ApplicantID = @MyID ");
                    break;

                case "MyAudit": // 待我簽核的
                    // 條件：目前處理人是我 AND 狀態是簽核中(1)
                    sql.Append(" AND F.CurrentHandlerID = @MyID AND F.Status = 1 ");
                    break;

                case "MyDept":  // 本部門所有的 (包含別人申請的)
                    // 條件：申請單的部門 = 我的部門 (且通常不顯示草稿 status=0，除非是自己的)
                    sql.Append(" AND F.DepartmentID = @MyDeptID ");
                    // 這裡有個細節：通常部門查詢不該看到同事的「草稿」，所以過濾掉 Status=0
                    // 除非是自己申請的草稿
                    sql.Append(" AND (F.Status <> 0 OR F.ApplicantID = @MyID) ");
                    break;
            }

            // 4. 處理其他篩選條件

            // (A) 公文類別
            if (!string.IsNullOrEmpty(ddlType.SelectedValue))
            {
                sql.Append(" AND F.TypeID = @TypeID ");
            }

            // (B) 關鍵字 (查單號 或 主旨)
            if (!string.IsNullOrEmpty(txtKeyword.Text.Trim()))
            {
                sql.Append(" AND (F.FormNumber LIKE @Keyword OR F.Title LIKE @Keyword) ");
            }

            // (C) 狀態
            if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                sql.Append(" AND F.Status = @Status ");
            }

            // (D) 日期區間 (依 CreateDate)
            if (!string.IsNullOrEmpty(txtDateStart.Text))
            {
                sql.Append(" AND F.CreateDate >= @DateStart ");
            }
            if (!string.IsNullOrEmpty(txtDateEnd.Text))
            {
                // 結束日期要加一天 (變成該日的 23:59:59 概念) 或用 < Date+1
                sql.Append(" AND F.CreateDate < DATEADD(day, 1, @DateEnd) ");
            }

            // 5. 排序 (最新的在上面)
            sql.Append(" ORDER BY F.CreateDate DESC ");

            // 6. 執行查詢與綁定
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(sql.ToString(), conn);

                // 加入參數
                cmd.Parameters.AddWithValue("@MyID", myID);
                cmd.Parameters.AddWithValue("@MyDeptID", myDeptID);

                if (!string.IsNullOrEmpty(ddlType.SelectedValue))
                    cmd.Parameters.AddWithValue("@TypeID", ddlType.SelectedValue);

                if (!string.IsNullOrEmpty(txtKeyword.Text.Trim()))
                    cmd.Parameters.AddWithValue("@Keyword", "%" + txtKeyword.Text.Trim() + "%");

                if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);

                if (!string.IsNullOrEmpty(txtDateStart.Text))
                    cmd.Parameters.AddWithValue("@DateStart", txtDateStart.Text);

                if (!string.IsNullOrEmpty(txtDateEnd.Text))
                    cmd.Parameters.AddWithValue("@DateEnd", txtDateEnd.Text);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvApplyList.DataSource = dt;
                gvApplyList.DataBind();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvApplyList.PageIndex = 0; // 查詢後回到第一頁
            BindGrid();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            // 恢復預設值
            ddlViewMode.SelectedValue = "MyDept"; // 依您的需求，預設回部門
            ddlType.SelectedIndex = 0;
            txtKeyword.Text = "";
            ddlStatus.SelectedIndex = 0;
            txtDateStart.Text = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd");
            txtDateEnd.Text = DateTime.Now.ToString("yyyy-MM-dd");

            BindGrid();
        }

        protected void gvApplyList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "View" || e.CommandName == "Audit")
            {
                string formID = e.CommandArgument.ToString();
                // 導向詳情頁 (不管是查看還是審核，都去同一個頁面，由該頁面判斷權限)
                Response.Redirect($"ApplyDetail.aspx?ID={formID}");
            }
        }

        // GridView 資料列綁定 (控制按鈕顯示 / 狀態顏色)
        protected void gvApplyList_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // 1. 取得資料
                int currentHandlerID = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "CurrentHandlerID") == DBNull.Value ? 0 : DataBinder.Eval(e.Row.DataItem, "CurrentHandlerID"));
                int status = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Status"));

                // 取得目前登入者
                int myID = Convert.ToInt32(Session["UserID"] ?? 0);

                // 2. 找控制項
                LinkButton btnAudit = (LinkButton)e.Row.FindControl("btnAudit");
                LinkButton btnView = (LinkButton)e.Row.FindControl("btnView");

                // 3. 邏輯判斷
                // 審核按鈕：只有當「目前處理人 = 我」且「狀態 = 簽核中(1)」才顯示
                if (currentHandlerID == myID && status == 1)
                {
                    btnAudit.Visible = true;
                    btnView.Visible = false; // 要審核就不需要顯示單純查看
                }
                else
                {
                    btnAudit.Visible = false;
                    btnView.Visible = true;  // 其他狀況都顯示查看
                }
            }
        }
        // 格式化狀態 (回傳 Bootstrap Badge HTML)
        public string FormatStatus(object statusObj)
        {
            if (statusObj == null) return "";
            int status = Convert.ToInt32(statusObj);

            switch (status)
            {
                case 0: return "<span class='badge bg-secondary'>草稿</span>";
                case 1: return "<span class='badge bg-primary'>簽核中</span>";
                case 2: return "<span class='badge bg-success'>已結案</span>";
                case 3: return "<span class='badge bg-warning text-dark'>已退件</span>";
                case 4: return "<span class='badge bg-dark'>已撤銷</span>";
                default: return status.ToString();
            }
        }

        // 格式化目前處理人 (如果是結案，就不顯示人名)
        public string FormatHandler(object handlerName, object statusObj)
        {
            int status = Convert.ToInt32(statusObj);
            if (status == 2 || status == 4) return "-"; // 結案或撤銷
            if (status == 0) return "申請人"; // 草稿
            return handlerName?.ToString();
        }
    }
}