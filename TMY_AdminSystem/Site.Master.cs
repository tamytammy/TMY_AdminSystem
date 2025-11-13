using System;
using System.Web.UI.WebControls;

namespace TMY_AdminSystem
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 如果有登入，顯示使用者名稱
            if (Session["Username"] != null)
                lblUser.Text = Session["Username"].ToString();
        }

        public void SetUserLabel(string username)
        {
            lblUser.Text = username;
        }


    }
}
