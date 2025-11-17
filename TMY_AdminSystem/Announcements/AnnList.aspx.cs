using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TMY_AdminSystem.Announcements
{
    public partial class AnnList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // GridView 狀態轉換函式 (aspx 頁面中會呼叫)
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
            // 這是動態產生 WHERE 條件的關鍵
            StringBuilder whereClause = new StringBuilder();

            // 1. 分類
            if (ddlCategoryFilter.SelectedValue != "0" && !string.IsNullOrEmpty(ddlCategoryFilter.SelectedValue))
            {
                whereClause.AppendFormat(" AND (A.CategoryID = {0})", ddlCategoryFilter.SelectedValue);
            }

            // 2. 日期 (起)
            if (!string.IsNullOrEmpty(txtDateStart.Text))
            {
                whereClause.AppendFormat(" AND (A.PublishDate >= '{0}')", txtDateStart.Text);
            }

            // 3. 日期 (迄)
            if (!string.IsNullOrEmpty(txtDateEnd.Text))
            {
                // 包含當天，所以要到 23:59:59
                whereClause.AppendFormat(" AND (A.PublishDate <= '{0} 23:59:59')", txtDateEnd.Text);
            }

            // 4. 關鍵字 (查詢標題或內容)
            if (!string.IsNullOrEmpty(txtKeywordFilter.Text))
            {
                string keyword = txtKeywordFilter.Text.Trim().Replace("'", "''"); // 防止 SQL Injection
                whereClause.AppendFormat(" AND (A.Title LIKE N'%{0}%' OR A.Content LIKE N'%{0}%')", keyword);
            }

            // 取得原始 SQL
            string baseSql = @"
                SELECT 
                    A.AnnouncementID, A.Title, A.PublishDate, A.Status, A.UpdateDate, 
                    C.CategoryName, E.EmployeeName AS AuthorName
                FROM 
                    Announcements AS A
                INNER JOIN 
                    AnnouncementCategories AS C ON A.CategoryID = C.CategoryID
                LEFT JOIN 
                    Employees AS E ON A.AuthorEmployeeID = E.EmployeeID
                WHERE 1=1 "; // WHERE 1=1 方便後面串接 AND



            // 重新綁定 GridView
            gvAnnouncements.DataBind();
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
                Response.Redirect($"AnnouncementEdit.aspx?ID={announcementID}");
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
    }
}