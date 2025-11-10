using System;

namespace TMY_AdminSystem
{
    public partial class MainMenu : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 未登入 → 導回 Login 頁
            if (Session["Username"] == null)
            {
                Response.Redirect("~/Login.aspx");
            }

            // 顯示登入者名稱到主版頁
            if (!IsPostBack && Session["Username"] != null)
            {
                var master = (Site)Master;
                master.SetUserLabel(Session["Username"].ToString());
            }
        }
    }
}
