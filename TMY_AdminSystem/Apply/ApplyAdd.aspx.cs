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
                // 1. 初始化申請日期 (設為今天)
                txtApplyDate.Text = DateTime.Now.ToString("yyyy/MM/dd");

                // 2. 綁定大類別下拉選單
                BindCategories();

                // 3. 檢查是否為編輯/審核模式 (有 ID)
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
    }
}