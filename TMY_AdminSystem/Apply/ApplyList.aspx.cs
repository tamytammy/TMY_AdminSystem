using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TMY_AdminSystem.Apply
{
    public partial class ApplyList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

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