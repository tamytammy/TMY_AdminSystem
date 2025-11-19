using System;
using System.Collections.Generic;
using System.Configuration;
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
        string connStr = ConfigurationManager.ConnectionStrings["TMY_DB"].ConnectionString;
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
        protected void tempSave(bool isDraft)
        {
                //首先比對isDraft來決定status的值，接下來比對id看是否已經存在，是新增或update
                int status;
                DateTime publishDate;

                if (isDraft)
                {
                    //草稿
                    status = 0;
                    if(rblPublishMode.SelectedValue == "2" && DateTime.TryParse(txtPublishDate.Text, out publishDate))
                    {
                        // 使用者已填排程日期
                    }
                    else
                    {
                        // 否則，使用今天日期 (或如果欄位中有值但格式不對，也用今天日期)
                        if (!DateTime.TryParse(txtPublishDate.Text, out publishDate))
                        {
                            publishDate = DateTime.Now;
                        }
                    }
                }
                else
                {
                    status = Convert.ToInt32(rblPublishMode.SelectedValue);
                    if (status == 1) // 立即發布
                    {
                        publishDate = DateTime.Now;
                    }
                    else // 2 = 排程發布
                    {
                        // 後端再次 double check
                        if (!DateTime.TryParse(txtPublishDate.Text, out publishDate))
                        {
    
                            return;
                        }

                        // 如果選了排程但時間在過去，就當作立即發布
                        if (publishDate <= DateTime.Now)
                        {
                            status = 1;
                            publishDate = DateTime.Now;
                        }
                    }
                }

                string sql = "";
                string id = hdnAnnouncementID.Value;   
                bool isNew = (id == "0");

                if (isNew)
                {
                    // 新增模式: INSERT
                    sql = @"INSERT INTO Announcements 
                            (Title, Content, CategoryID, EmployeeID, Status, PublishDate, CreateDate, UpdateDate)
                        VALUES 
                            (@Title, @Content, @CategoryID, @EmployeeID, @Status, @PublishDate, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();"; // 返回新產生的 ID
                }
                else
                {
                    // 編輯模式: UPDATE
                    sql = @"UPDATE Announcements SET
                            Title = @Title,
                            Content = @Content,
                            CategoryID = @CategoryID,
                            Status = @Status,
                            PublishDate = @PublishDate,
                            UpdateDate = GETDATE()
                        WHERE 
                            AnnouncementID = @ID";
                }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                    if (Session["UserID"] != null)
                    {
                        int authorID = Convert.ToInt32(Session["UserID"]);
                    }
                    
                    else
                    {
                        ShowMessage("無法取得使用者資訊，請重新登入。", true);
                        return;
                    }
                    

                    // 抓取 CKEditor 的內容 (不能用 .Text 屬性)
                    string editorContent = Request.Form[txtContent.UniqueID];

                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sql, conn);

                    // 加入共用參數
                    cmd.Parameters.AddWithValue("@Title", txtTitle.Text);
                    cmd.Parameters.AddWithValue("@Content", editorContent);
                    cmd.Parameters.AddWithValue("@CategoryID", ddlCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@PublishDate", publishDate);

                    if (isNew)
                    {
                        // 只在新增時加入作者 ID
                        cmd.Parameters.AddWithValue("@EmployeeID", Session["UserID"]);

                        // 執行並取得新 ID
                        string newId = cmd.ExecuteScalar().ToString();
                        hdnAnnouncementID.Value = newId; // 儲存後，將頁面更新為編輯模式
                    }
                    else
                    {
                        // 更新時指定 ID
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.ExecuteNonQuery();
                    }

                    // 決定要顯示的訊息
                    string alertMessage = isNew ? "新增成功" : "更新成功";

                    // 顯示 alert，然後執行 client-side 轉跳
                    string script = $"alert('{alertMessage}'); window.location='AnnList.aspx';";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "alertRedirect", script, true);

                    // (已移除) Response.Redirect("AnnList.aspx");
                }
            } 

        
        private void ShowMessage(string message, bool isError)
        {
            // 尋找您在 .aspx 頁面上 ID="ValidationSummary1" 的那個控制項
            if (ValidationSummary1 != null)
            {
                ValidationSummary1.HeaderText = message;
                // 根據是否為錯誤，切換 Bootstrap 樣式
                ValidationSummary1.CssClass = isError ? "alert alert-danger" : "alert alert-info";
            }
        }
        // 按下「儲存」按鈕
        protected void btnSave_Click(object sender, EventArgs e)
        {
            tempSave(false);
            // 在這裡，您需要撰寫「儲存」的 SQL 邏輯 (INSERT 或 UPDATE)
            // 1. 取得所有欄位的值 (txtTitle.Text, ddlCategory.SelectedValue, txtPublishDate.Text, rblStatus.SelectedValue, txtContent.Text)
            // 2. 取得登入者的 EmployeeID (例如: Session["EmployeeID"])
            // 3. 判斷 hdnAnnouncementID.Value 是 "0" (INSERT) 還是 > "0" (UPDATE)
            // 4. 執行 SQL
            // 5. 成功後導回列表頁
            //Response.Redirect("AnnList.aspx");
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
            tempSave(true);
        }
    }
}