using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TMY_AdminSystem.Announcements
{
    public partial class AnnouncementAdd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 檢查網址列是否有 ID
                if (!string.IsNullOrEmpty(Request.QueryString["ID"]))
                {
                    // 編輯模式
                    hdnAnnouncementID.Value = Request.QueryString["ID"];
                    litPageTitle.Text = "編輯公告";
                    btnDelete.Visible = true; // 顯示刪除按鈕

                    //LoadAnnouncementData(hdnAnnouncementID.Value);
                }
                else
                {
                    // 新增模式
                    litPageTitle.Text = "新增公告";
                    btnDelete.Visible = false; // 隱藏刪除按鈕

                    // 設定預設值
                    txtPublishDate.Text = DateTime.Now.ToString("yyyy-MM-ddTHH:mm");
                    rblStatus.SelectedValue = "0"; // 預設為草稿
                }
            }
        }
        //private void LoadAnnouncementData(string id)
        //{
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        string sql = "SELECT * FROM Announcements WHERE AnnouncementID = @ID";
        //        SqlCommand cmd = new SqlCommand(sql, conn);
        //        cmd.Parameters.AddWithValue("@ID", id);

        //        try
        //        {
        //            conn.Open();
        //            SqlDataReader reader = cmd.ExecuteReader();
        //            if (reader.Read())
        //            {
        //                txtTitle.Text = reader["Title"].ToString();
        //                txtContent.Text = reader["Content"].ToString();
        //                ddlCategory.SelectedValue = reader["CategoryID"].ToString();
        //                rblStatus.SelectedValue = reader["Status"].ToString();

        //                // 格式化日期以符合 <input type="datetime-local">
        //                DateTime publishDate = (DateTime)reader["PublishDate"];
        //                txtPublishDate.Text = publishDate.ToString("yyyy-MM-ddTHH:mm");

        //                // (假設 AuthorEmployeeID 是從登入 Session 來的，這裡不用設定)
        //            }
        //            reader.Close();

        //            // 載入附件
        //            LoadAttachments(id);
        //        }
        //        catch (Exception ex)
        //        {
        //            ValidationSummary1.HeaderText = "讀取資料時發生錯誤：" + ex.Message;
        //        }
        //    }
        //}

        //private void LoadAttachments(string announcementID)
        //{
        //    using (SqlConnection conn = new SqlConnection(connectionString))
        //    {
        //        string sql = "SELECT AttachmentID, FileName, FilePath FROM AnnouncementAttachments WHERE AnnouncementID = @ID";
        //        SqlDataAdapter da = new SqlDataAdapter(sql, conn);
        //        da.SelectCommand.Parameters.AddWithValue("@ID", announcementID);
        //        DataTable dt = new DataTable();
        //        da.Fill(dt);

        //        rptAttachments.DataSource = dt;
        //        rptAttachments.DataBind();
        //    }
        //}

        // 按下「儲存」按鈕
        protected void btnSave_Click(object sender, EventArgs e)
        {
            // 在這裡，您需要撰寫「儲存」的 SQL 邏輯 (INSERT 或 UPDATE)
            // 1. 取得所有欄位的值 (txtTitle.Text, ddlCategory.SelectedValue, txtPublishDate.Text, rblStatus.SelectedValue, txtContent.Text)
            // 2. 取得登入者的 EmployeeID (例如: Session["EmployeeID"])
            // 3. 判斷 hdnAnnouncementID.Value 是 "0" (INSERT) 還是 > "0" (UPDATE)
            // 4. 執行 SQL
            // 5. 成功後導回列表頁
            // Response.Redirect("AnnouncementList.aspx");
        }

        // 按下「刪除公告」按鈕
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // 在這裡執行軟刪除 (UPDATE Status = 99) 或硬刪除 (DELETE)
            // string id = hdnAnnouncementID.Value;
            // ... 執行 SQL ...
            // Response.Redirect("AnnouncementList.aspx");
        }

        // 按下「上傳檔案」按鈕
        protected void btnUpload_Click(object sender, EventArgs e)
        {
            // 檢查是否有選取檔案
            if (fileUploadAttachment.HasFiles)
            {
                // 取得目前公告 ID
                string id = hdnAnnouncementID.Value;
                if (id == "0")
                {
                    // 如果是「新增」模式，必須先儲存公告
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('請先儲存公告，才能上傳附件。');", true);
                    return;
                }

                // 處理多檔案上傳
                foreach (var file in fileUploadAttachment.PostedFiles)
                {
                    // 1. 產生安全的路徑 (e.g., /Uploads/Announcements/guid_filename.pdf)
                    // 2. file.SaveAs(serverPath);
                    // 3. 將檔案資訊 (FileName, FilePath, AnnouncementID) INSERT 到 AnnouncementAttachments 資料表
                }

                // 重新載入附件列表
                //LoadAttachments(id);
            }
        }

        // 按下「刪除附件」按鈕
        protected void rptAttachments_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "Delete")
            {
                string attachmentID = e.CommandArgument.ToString();

                // 1. (可選) 從伺服器硬碟刪除實體檔案
                // 2. 執行 SQL "DELETE FROM AnnouncementAttachments WHERE AttachmentID = @ID"

                // 重新載入附件列表
                //LoadAttachments(hdnAnnouncementID.Value);
            }
        }

        // 按下「返回列表」按鈕
        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("AnnList.aspx");
        }

        protected void btnSaveDraft_Click(object sender, EventArgs e)
        {
            Response.Redirect("AnnouncementList.aspx");
        }
    }
}