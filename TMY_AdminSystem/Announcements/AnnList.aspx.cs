using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;           // 解決 DataTable 錯誤
using System.Data.SqlClient; // 解決 SqlConnection, SqlCommand 錯誤
using System.Configuration;  // 解決 ConfigurationManager 錯誤

namespace TMY_AdminSystem.Announcements
{
    public partial class AnnList : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["TMY_DB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                // 1. 綁定下拉選單
                BindCategories();

                // 2. 【關鍵】第一次載入時，必須呼叫 BindGrid()
                BindGrid();
            }
        }

        /// <summary>
        /// 綁定「公告分類」下拉選單
        /// </summary>
        private void BindCategories()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 您的 SQL 語法 (我加入了排序，讓選單比較整齊)
                string sql = "SELECT CategoryID, CategoryName FROM AnnouncementCategories ORDER BY CategoryID";

                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // 設定資料來源
                    ddlCategoryFilter.DataSource = dt;

                    // 設定顯示文字 (CategoryName) 與 對應的值 (CategoryID)
                    ddlCategoryFilter.DataTextField = "CategoryName";
                    ddlCategoryFilter.DataValueField = "CategoryID";

                    // 執行綁定
                    // 因為前端設有 AppendDataBoundItems="true"，資料會接在 "--- 全部分類 ---" 後面
                    ddlCategoryFilter.DataBind();
            }
        }
        public string FormatStatus(object statusObj)
        {
            int status = Convert.ToInt32(statusObj);
            switch (status)
            {
                case 0:
                    return "<span style='color: #888;'>草稿</span>";
                case 1:
                    return "<span style='color: green; font-weight: bold;'>已發布</span>";
                case 2:
                    return "<span style='color: #f0ad4e;'>已排程</span>";
                default:
                    return "未知";
            }
        }

        // 1. 查詢按鈕
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            // 按下查詢時，回到第一頁
            if (gvAnnouncements.AllowPaging)
            {
                gvAnnouncements.PageIndex = 0;
            }

            // 呼叫共用的查詢函式
            BindGrid();
        }

        // 2. 清除條件按鈕
        protected void btnClear_Click(object sender, EventArgs e)
        {
            // 還原所有查詢控制項
            ddlCategoryFilter.SelectedIndex = 0;
            txtDateStart.Text = string.Empty;
            txtDateEnd.Text = string.Empty;
            txtKeywordFilter.Text = string.Empty;

            // 還原 SQL (移除 WHERE) 並重新綁定
            btnSearch_Click(sender, e); // 重新執行一次查詢 (此時所有條件都為空)
        }

        // 3. 新增公告按鈕
        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            // 導向到「新增/編輯」頁面 (我們假設叫做 AnnouncementEdit.aspx)
            Response.Redirect("AnnouncementAdd.aspx");
        }

        // 4. GridView 的操作按鈕 (編輯/刪除)
        protected void gvAnnouncements_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // 取得被點擊的 Row 的 AnnouncementID
            int announcementID = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "EditRow")
            {
                // 導向到編輯頁面，並帶上 ID
                Response.Redirect($"AnnouncementAdd.aspx?ID={announcementID}");
            }

            if (e.CommandName == "DeleteRow")
            {
                // 執行刪除 (建議使用軟刪除 Soft Delete，例如更新 Status = 99)
                // 這裡我們先用 SqlDataSource 來執行 Delete
                // (您需要先在 SqlDataSourceAnnouncements 中定義 DeleteCommand)

                /*
                // 範例：定義 DeleteCommand
                SqlDataSourceAnnouncements.DeleteCommand = "UPDATE Announcements SET Status = 99 WHERE AnnouncementID = @AnnouncementID";
                SqlDataSourceAnnouncements.DeleteParameters.Clear();
                SqlDataSourceAnnouncements.DeleteParameters.Add("AnnouncementID", announcementID.ToString());
                SqlDataSourceAnnouncements.Delete();
                */

                // 暫時先提示 (因為我們還沒實作刪除)
                Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('執行刪除 ID: {announcementID}。請實作刪除邏輯。');", true);

                // 重新綁定 GridView
                gvAnnouncements.DataBind();
            }
        
    }

        //5. 核心查詢
        private void BindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 1. 基礎 SQL (這跟您原本的一樣)
                StringBuilder sql = new StringBuilder();
                sql.Append(@"
            SELECT 
                A.AnnouncementID, A.Title, A.PublishDate, A.Status, A.UpdateDate, 
                C.CategoryName, E.FullName AS AuthorName
            FROM Announcements AS A
            INNER JOIN AnnouncementCategories AS C ON A.CategoryID = C.CategoryID
            LEFT JOIN Employees AS E ON A.EmployeeID = E.EmployeeID
            WHERE A.Status <> 99 "); // 預設過濾已刪除

                // 2. 動態加入條件 (改用參數化 @Param 寫法)

                // [分類]
                if (ddlCategoryFilter.SelectedValue != "0" && !string.IsNullOrEmpty(ddlCategoryFilter.SelectedValue))
                {
                    sql.Append(" AND A.CategoryID = @CategoryID ");
                }

                // [日期起]
                if (!string.IsNullOrEmpty(txtDateStart.Text))
                {
                    sql.Append(" AND A.PublishDate >= @DateStart ");
                }

                // [日期迄]
                if (!string.IsNullOrEmpty(txtDateEnd.Text))
                {
                    // 包含當天: < 隔天
                    sql.Append(" AND A.PublishDate < DATEADD(day, 1, @DateEnd) ");
                }

                // [關鍵字]
                if (!string.IsNullOrEmpty(txtKeywordFilter.Text.Trim()))
                {
                    sql.Append(" AND (A.Title LIKE @Keyword OR A.Content LIKE @Keyword) ");
                }

                // 3. 排序
                sql.Append(" ORDER BY A.PublishDate DESC ");

                // 4. 準備執行
                SqlCommand cmd = new SqlCommand(sql.ToString(), conn);

                // 5. 加入參數值 (與上面的 SQL 對應)
                if (ddlCategoryFilter.SelectedValue != "0" && !string.IsNullOrEmpty(ddlCategoryFilter.SelectedValue))
                    cmd.Parameters.AddWithValue("@CategoryID", ddlCategoryFilter.SelectedValue);

                if (!string.IsNullOrEmpty(txtDateStart.Text))
                    cmd.Parameters.AddWithValue("@DateStart", txtDateStart.Text);

                if (!string.IsNullOrEmpty(txtDateEnd.Text))
                    cmd.Parameters.AddWithValue("@DateEnd", txtDateEnd.Text);

                if (!string.IsNullOrEmpty(txtKeywordFilter.Text.Trim()))
                    cmd.Parameters.AddWithValue("@Keyword", "%" + txtKeywordFilter.Text.Trim() + "%");

                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // 6. 綁定資料
                    gvAnnouncements.DataSource = dt;
                    gvAnnouncements.DataBind();
                }
                catch (Exception ex)
                {
                    // 錯誤處理
                    // 先把錯誤訊息中的單引號 (') 和換行符號拿掉，避免 JavaScript 語法錯誤
                    string cleanMessage = ex.Message.Replace("'", "").Replace("\r", "").Replace("\n", "");

                    // 組合 JavaScript
                    string script = $"alert('查詢發生錯誤：{cleanMessage}');";

                    // 執行 Alert
                    ClientScript.RegisterStartupScript(this.GetType(), "ErrorAlert", script, true);
                }
            }
        }


    }
}