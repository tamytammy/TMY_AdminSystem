using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
                    // LoadFormData(formID); // (之後再實作載入資料)
                }
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
    }
}