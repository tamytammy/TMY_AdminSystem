using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

namespace TMY_AdminSystem.Apply
{
    public partial class ApplyAdd : System.Web.UI.Page
    {
        string connStr = ConfigurationManager.ConnectionStrings["TMY_DB"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtApplyDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
                BindCategories();

                //是否為審核模式
                if (!string.IsNullOrEmpty(Request.QueryString["ID"]))
                {
                    string formID = Request.QueryString["ID"];
                    hdnFormID.Value = formID;
                    LoadFormData(formID);
                }
            }
        }



        // 載入申請表單資料
        private void LoadFormData(string formID)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 1. 查詢公文主檔
                string sql = @"SELECT TOP(1) 
                        F.FormNumber, F.TypeID, F.Title, F.Content, 
                        F.ApplicantID, F.DepartmentID, F.CurrentHandlerID, F.Status, F.CreateDate,
                        T.CategoryID  -- 用 JOIN 查出大類別 ID
                       FROM Flow_Forms F
                       LEFT JOIN Flow_Types T ON F.TypeID = T.TypeID
                       WHERE F.FormID = @FormID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@FormID", formID);

                try
                {
                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        // --- A. 回填基本欄位 ---
                        lblFormNumber.Text = dr["FormNumber"].ToString();
                        txtTitle.Text = dr["Title"].ToString();
                        txtApplyDate.Text = Convert.ToDateTime(dr["CreateDate"]).ToString("yyyy/MM/dd");

                        // 處理下拉選單回填 (需先有大類別，才能觸發小類別)
                        string categoryID = dr["CategoryID"].ToString();
                        string typeID = dr["TypeID"].ToString();

                        if (ddlCategory.Items.FindByValue(categoryID) != null)
                        {
                            ddlCategory.SelectedValue = categoryID;
                            // 手動觸發小類別綁定 (因為這是在後端設值，不會觸發 SelectedIndexChanged)
                            BindTypes(categoryID);
                            if (ddlType.Items.FindByValue(typeID) != null)
                            {
                                ddlType.SelectedValue = typeID;
                            }
                        }

                        // --- B. 處理內容拆分 (原因 vs 成效) ---
                        // 因為存檔時我們是合併存入 Content，讀取時嘗試簡單拆分
                        string fullContent = dr["Content"].ToString();
                        string[] separator = new string[] { "【預期成效】" };
                        string[] parts = fullContent.Split(separator, StringSplitOptions.None);

                        if (parts.Length > 0)
                            txtReason.Text = parts[0].Replace("【申請原因】", "").Trim();

                        if (parts.Length > 1)
                            txtOutcome.Text = parts[1].Trim();

                        // --- C. 狀態顯示 (Badge) ---
                        int status = Convert.ToInt32(dr["Status"]);
                        UpdateStatusLabel(status);

                        // --- D. 判斷權限模式 (編輯 vs 審核 vs 唯讀) ---
                        int currentHandlerID = dr["CurrentHandlerID"] != DBNull.Value ? Convert.ToInt32(dr["CurrentHandlerID"]) : 0;
                        int applicantID = Convert.ToInt32(dr["ApplicantID"]);
                        int myID = Convert.ToInt32(Session["UserID"] ?? 0);

                        SetViewMode(status, applicantID, currentHandlerID, myID);
                    }
                    dr.Close();

                    // 2. 載入簽核歷程
                    BindLogs(formID);

                    // 3. 載入附件
                    BindAttachments(formID);

                    // 4. 載入下一關主管 (若為審核模式)
                    BindNextHandlers();
                }
                catch (Exception ex)
                {
                     ShowMessage("資料載入失敗：" + ex.Message, true);
                }
            }
        }

        // 更新狀態標籤顯示
        private void UpdateStatusLabel(int status)
        {
            switch (status)
            {
                case 0: // 草稿
                    lblCurrentStatus.Text = "草稿";
                    lblCurrentStatus.CssClass = "badge bg-secondary";
                    break;
                case 1: // 審核中
                    lblCurrentStatus.Text = "審核中";
                    lblCurrentStatus.CssClass = "badge bg-primary";
                    break;
                case 2: // 已結案
                    lblCurrentStatus.Text = "已結案";
                    lblCurrentStatus.CssClass = "badge bg-success";
                    break;
                case 3: // 已退件
                    lblCurrentStatus.Text = "已退件";
                    lblCurrentStatus.CssClass = "badge bg-warning text-dark";
                    break;
                case 4: // 已撤銷
                    lblCurrentStatus.Text = "已撤銷";
                    lblCurrentStatus.CssClass = "badge bg-dark";
                    break;
            }
        }

        private void BindLogs(string formID)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 關聯 Employees 表取得姓名
                string sql = @"SELECT L.ActionType, L.ActionDate, L.Comment, 
                              E.EmployeeName AS ApproverName
                       FROM Flow_Logs L
                       LEFT JOIN Employees E ON L.ApproverID = E.EmployeeID
                       WHERE L.FormID = @FormID
                       ORDER BY L.ActionDate DESC"; // 最新的在上面

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@FormID", formID);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvLogs.DataSource = dt;
                gvLogs.DataBind();

                // 如果有歷程，顯示 Panel
                pnlHistory.Visible = (dt.Rows.Count > 0);
            }
        }

        private void BindAttachments(string formID)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT AttachmentID, FileName, FilePath FROM Flow_Attachments WHERE FormID = @FormID";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@FormID", formID);

                DataTable dt = new DataTable();
                da.Fill(dt);

                rptAttachments.DataSource = dt;
                rptAttachments.DataBind();
            }
        }
        private void BindNextHandlers()
        {
            // 這裡示範：抓取所有 "Manager" 職級以上的人員供選擇
            // 實務上可能會根據目前登入者的 ManagerID 自動預選
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 假設 RoleID >= 3 是主管
                string sql = "SELECT EmployeeID, EmployeeName FROM Employees WHERE RoleID >= 3 ORDER BY RoleID DESC";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlNextHandler.DataSource = dt;
                ddlNextHandler.DataTextField = "EmployeeName";
                ddlNextHandler.DataValueField = "EmployeeID";
                ddlNextHandler.DataBind();

                ddlNextHandler.Items.Insert(0, new ListItem("-- 請選擇下一關主管 --", ""));

                // 進階：如果 Session 中有 ManagerID，可以嘗試自動選取
                // string myManagerID = ... Get from DB ...
                // if (ddlNextHandler.Items.FindByValue(myManagerID) != null) ...
            }
        }
        private void BindCategories()
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string sql = "SELECT CategoryID, CategoryName FROM Flow_Categories ORDER BY CategoryID";
                SqlCommand cmd = new SqlCommand(sql, conn);

                try
                {
                    conn.Open();
                    ddlCategory.DataSource = cmd.ExecuteReader();
                    ddlCategory.DataTextField = "CategoryName";
                    ddlCategory.DataValueField = "CategoryID";
                    ddlCategory.DataBind();

                    // 加入預設選項
                    ddlCategory.Items.Insert(0, new ListItem("--- 請選擇大類別 ---", ""));
                }
                catch (Exception ex)
                {
                    // 建議要有 ShowMessage 函式來顯示錯誤
                    // ShowMessage("類別載入失敗：" + ex.Message, true);
                }
            }
        }

        protected void ddlCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 1. 先清空小類別選單
            ddlType.Items.Clear();
            ddlType.Items.Add(new ListItem("--- 請選擇項目 ---", ""));

            // 2. 取得使用者選擇的大類別 ID
            string categoryID = ddlCategory.SelectedValue;

            // 3. 如果有選值，才去資料庫抓小類別
            if (!string.IsNullOrEmpty(categoryID))
            {
                BindTypes(categoryID);
            }
        }

        private void BindTypes(string categoryID)
        {
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 這裡會去 Flow_Types 抓取對應 CategoryID 的項目
                string sql = "SELECT TypeID, TypeName FROM Flow_Types WHERE CategoryID = @CategoryID ORDER BY TypeID";
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CategoryID", categoryID);

                try
                {
                    conn.Open();
                    ddlType.DataSource = cmd.ExecuteReader();
                    ddlType.DataTextField = "TypeName";
                    ddlType.DataValueField = "TypeID";
                    ddlType.DataBind();

                    // 因為上面已經 Clear 過並加了預設項，這裡 DataBind 會自動Append在後面
                    // 如果 DataBind 把預設項蓋掉了，記得重新 Insert
                    if (ddlType.Items.Count > 0 && ddlType.Items[0].Value != "")
                    {
                        ddlType.Items.Insert(0, new ListItem("--- 請選擇項目 ---", ""));
                    }
                }
                catch (Exception ex)
                {
                    // ShowMessage("項目載入失敗：" + ex.Message, true);
                }
            }
        }

        protected void btnDraft_Click(object sender, EventArgs e)
        {
            // 草稿不需要必填驗證，且處理人仍是自己
            // 參數: Status=0, NextHandler=自己(或NULL)
            SaveApplication(0);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // 送出呈核，狀態變為 1 (簽核中)
            // 這裡我們會自動去抓取「直屬主管」作為下一關處理人
            SaveApplication(1);
        }

        private void SaveApplication(int targetStatus)
        {
            // 1. 取得目前登入者資訊 (申請人)
            // ❗ 請確保 Session["UserID"] 有值，或做好例外處理
            int applicantID = Convert.ToInt32(Session["UserID"]); 

            string applicantDeptID = "";
            int? managerID = null;

            // 2. 從資料庫取得申請人的「部門」與「直屬主管ID」
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                // 假設 Employees 表有 DepartmentID 和 ManagerID 欄位
                string sqlUser = "SELECT DeptID, ManagerID FROM Employees WHERE EmployeeID = @EmpID";
                SqlCommand cmdUser = new SqlCommand(sqlUser, conn);
                cmdUser.Parameters.AddWithValue("@EmpID", applicantID);
                conn.Open();
                SqlDataReader dr = cmdUser.ExecuteReader();
                if (dr.Read())
                {
                    applicantDeptID = dr["DeptID"].ToString();
                    if (dr["ManagerID"] != DBNull.Value)
                    {
                        managerID = Convert.ToInt32(dr["ManagerID"]);
                    }
                }
                conn.Close();
            }

            // 3. 決定「下一關處理人 (CurrentHandlerID)」
            object nextHandlerValue = DBNull.Value;

            if (targetStatus == 0)
            {
                // A. 如果是草稿，處理人還是自己 (方便之後編輯)
                nextHandlerValue = applicantID;
            }
            else
            {
                // B. 如果是送出，處理人變成「直屬主管」
                if (managerID.HasValue)
                {
                    nextHandlerValue = managerID.Value;
                }
                else
                {
                    // 例外：如果沒有主管 (可能是總經理申請)，則視為自動核准或特例
                    // 這裡先示範：如果沒有主管，就停留在自己身上，或顯示錯誤
                    // ShowMessage("系統錯誤：找不到您的直屬主管，無法呈核。", true);
                    // return;
                    nextHandlerValue = applicantID; // 暫時處置
                }
            }

            // 4. 準備資料
            string formID = hdnFormID.Value;
            bool isNew = (formID == "0" || string.IsNullOrEmpty(formID));

            // 產生單號 (僅新增時) - 範例格式: YYYYMMDD-001 (需實作流水號邏輯，這裡簡化用亂數或時間)
            string formNumber = isNew ? DateTime.Now.ToString("yyyyMMddHHmm") : "";

            // 組合 Content (將原因和預期成效合併存入)
            string combinedContent = $"【申請原因】\r\n{txtReason.Text.Trim()}\r\n\r\n【預期成效】\r\n{txtOutcome.Text.Trim()}";

            string sql = "";

            if (isNew)
            {
                // INSERT
                sql = @"INSERT INTO Flow_Forms 
                        (FormNumber, TypeID, Title, Content, ApplicantID, DepartmentID, CurrentHandlerID, Status, CreateDate, UpdateDate)
                        VALUES 
                        (@FormNumber, @TypeID, @Title, @Content, @ApplicantID, @DepartmentID, @CurrentHandlerID, @Status, GETDATE(), GETDATE());
                        SELECT SCOPE_IDENTITY();";
            }
            else
            {
                // UPDATE (針對草稿重新編輯後送出)
                sql = @"UPDATE Flow_Forms SET 
                        TypeID = @TypeID, 
                        Title = @Title, 
                        Content = @Content, 
                        CurrentHandlerID = @CurrentHandlerID, 
                        Status = @Status, 
                        UpdateDate = GETDATE() 
                        WHERE FormID = @FormID";
            }

            // 5. 執行資料庫存檔
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@TypeID", ddlType.SelectedValue);
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim());
                cmd.Parameters.AddWithValue("@Content", combinedContent);
                cmd.Parameters.AddWithValue("@CurrentHandlerID", nextHandlerValue);
                cmd.Parameters.AddWithValue("@Status", targetStatus);

                if (isNew)
                {
                    cmd.Parameters.AddWithValue("@FormNumber", formNumber);
                    cmd.Parameters.AddWithValue("@ApplicantID", applicantID);
                    cmd.Parameters.AddWithValue("@DepartmentID", applicantDeptID);

                    // 取得新 ID
                    object newIdObj = cmd.ExecuteScalar();
                    if (newIdObj != null) formID = newIdObj.ToString();
                }
                else
                {
                    cmd.Parameters.AddWithValue("@FormID", formID);
                    cmd.ExecuteNonQuery();
                }

                // 6. 如果是「送出呈核」，寫入 Log
                if (targetStatus == 1)
                {
                    string logSql = @"INSERT INTO Flow_Logs (FormID, ApproverID, ActionType, Comment, ActionDate) 
                                      VALUES (@FormID, @ApproverID, '送出申請', @Comment, GETDATE())";
                    SqlCommand cmdLog = new SqlCommand(logSql, conn);
                    cmdLog.Parameters.AddWithValue("@FormID", formID);
                    cmdLog.Parameters.AddWithValue("@ApproverID", applicantID);
                    cmdLog.Parameters.AddWithValue("@Comment", "新申請送出"); // 申請人通常不用寫意見，或可填寫備註
                    cmdLog.ExecuteNonQuery();
                }
            }

            // 7. 完成後的轉跳
            string msg = (targetStatus == 0) ? "草稿已儲存" : "申請已送出，等待主管簽核";
            string script = $"alert('{msg}'); window.location='ApplyList.aspx';";
            Page.ClientScript.RegisterStartupScript(this.GetType(), "Success", script, true);
        }

        /// <summary>
        /// 設定介面模式 (編輯 / 審核 / 唯讀)
        /// </summary>
        private void SetViewMode(int status, int applicantID, int currentHandlerID, int myID)
        {
            // 預設全部關閉，依情況開啟
            pnlApplicantActions.Visible = false; // 申請人按鈕 (送出/草稿)
            pnlReviewerActions.Visible = false;  // 審核人按鈕 (核准/退件)
            pnlAuditInput.Visible = false;       // 審核意見框

            // 預設欄位唯讀 (保護資料)
            DisableFormFields(true);

            // 情境 A: 我是申請人，且狀態是 草稿(0) 或 退件(3) -> 【編輯模式】
            if (myID == applicantID && (status == 0 || status == 3))
            {
                pnlApplicantActions.Visible = true;
                DisableFormFields(false); // 開放編輯
            }
            // 情境 B: 我是目前處理人，且狀態是 審核中(1) -> 【審核模式】
            else if (myID == currentHandlerID && status == 1)
            {
                pnlReviewerActions.Visible = true;
                pnlAuditInput.Visible = true;
                // 欄位保持唯讀，審核人只能看不能改內容
            }
            // 情境 C: 其他狀況 (已結案、看別人的單) -> 【純瀏覽模式】
            else
            {
                // 維持全部隱藏與唯讀
            }
        }

        // 輔助：鎖定或開放表單欄位
        private void DisableFormFields(bool isDisabled)
        {
            bool isEnabled = !isDisabled;
            ddlCategory.Enabled = isEnabled;
            ddlType.Enabled = isEnabled;
            txtTitle.Enabled = isEnabled;
            txtSubtitle.Enabled = isEnabled;
            txtReason.Enabled = isEnabled;
            txtOutcome.Enabled = isEnabled;
            fuAttachment.Enabled = isEnabled; // 附件上傳

            // 附件刪除按鈕需要在 Repeater ItemDataBound 處理，這裡先省略
        }
        /// <summary>
        /// (輔助函式) 在 ValidationSummary 顯示訊息
        /// </summary>
        /// <param name="message">訊息內容</param>
        /// <param name="isError">是否為錯誤訊息 (True=紅色, False=藍色)</param>
        private void ShowMessage(string message, bool isError)
        {
            // 尋找您在 .aspx 頁面上 ID="ValidationSummary1" 的那個控制項
            // 確保您的前端有 <asp:ValidationSummary ID="ValidationSummary1" ... />
            if (ValidationSummary1 != null)
            {
                ValidationSummary1.HeaderText = message;
                // 根據是否為錯誤，切換 Bootstrap 樣式
                ValidationSummary1.CssClass = isError ? "alert alert-danger" : "alert alert-info";

                // 如果原本是隱藏的，這裡可能需要視情況讓它顯示 (ValidationSummary 通常有錯誤就會自動顯示)
            }
        }
    }
}