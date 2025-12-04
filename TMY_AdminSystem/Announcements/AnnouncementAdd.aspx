<%@ Page Title="公告管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AnnouncementAdd.aspx.cs" Inherits="TMY_AdminSystem.Announcements.AnnouncementAdd" 
    ValidateRequest="false" ValidateRequestMode="Disabled" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%-- 隱藏欄位，用來儲存目前這筆公告的 ID (0 = 新增模式) --%>
    <asp:HiddenField ID="hdnAnnouncementID" runat="server" Value="0" />

    <%-- 頁面標題 --%>
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h3 class="fw-bold mb-0">
            <asp:Literal ID="litPageTitle" runat="server" Text="新增公告"></asp:Literal>
        </h3>
        <div>
            <asp:Button ID="btnBack" runat="server" Text="返回列表" CssClass="btn btn-secondary" OnClick="btnBack_Click" CausesValidation="false" />
        </div>
    </div>
    <hr />

    <div class="card shadow-sm mb-4">
        <div class="card-body p-4">

            <%-- 使用 ValidationSummary 顯示所有錯誤訊息 --%>
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" HeaderText="請修正以下錯誤：" />

            <div class="row g-3">
                <div class="col-md-12">
                    <label class="form-label">公告標題</label>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server"
                        ControlToValidate="txtTitle" ErrorMessage="請輸入公告標題。"
                        CssClass="text-danger" Display="Dynamic" />
                </div>

                <div class="col-md-4">
                    <label class="form-label">公告分類</label>
                    <asp:DropDownList ID="ddlCategory" runat="server"
                        CssClass="form-select"
                        DataTextField="CategoryName" DataValueField="CategoryID" DataSourceID="SqlSource1" />
                    <asp:RequiredFieldValidator ID="rfvCategory" runat="server"
                        ControlToValidate="ddlCategory" ErrorMessage="請選擇公告分類。"
                        CssClass="text-danger" Display="Dynamic" InitialValue="" />
                     <asp:SqlDataSource ID="SqlSource1" runat="server"
                        ConnectionString="<%$ ConnectionStrings:TMY_DB %>"
                        SelectCommand="SELECT CategoryID, CategoryName FROM AnnouncementCategories ORDER BY CategoryName">
                      </asp:SqlDataSource>
                </div>


                <div class="col-md-4">
                    <label class="form-label">發布方式</label>
                    <br />
                    <%-- 我們給 rblPublishMode 增加一個 CssClass="rblPublishMode" 讓 JavaScript 能夠找到它 --%>
                    <asp:RadioButtonList ID="rblPublishMode" runat="server" RepeatDirection="Horizontal" CssClass="mt-2 rblPublishMode">
                        <asp:ListItem Value="1" Selected="True"> 立即發布</asp:ListItem>
                        <asp:ListItem Value="2" style="margin-left: 20px;"> 排程發布</asp:ListItem>
                    </asp:RadioButtonList>
                </div>

                <div id="pnlPublishDate" class="col-md-4">
                    <label class="form-label">發布日期 (排程)</label>
                    <asp:TextBox ID="txtPublishDate" runat="server" CssClass="form-control" TextMode="DateTimeLocal" />
                    
                    <%-- 驗證器預設 Enabled="false"，由 JavaScript 根據 rblPublishMode 決定是否啟用 --%>
                    <asp:RequiredFieldValidator ID="rfvPublishDate" runat="server"
                        ControlToValidate="txtPublishDate" ErrorMessage="請選擇排程的發布日期。"
                        CssClass="text-danger" Display="Dynamic" Enabled="false" ValidationGroup="PublishValidation" />
                </div>

                <div class="col-md-12">
                    <label class="form-label">公告內容</label>
                    <asp:TextBox ID="txtContent" runat="server" TextMode="MultiLine" Rows="15" CssClass="form-control" />
                    <%-- CKEditor 會自動取代這個 TextBox --%>
                </div>

                <div class="col-md-12">
                    <label class="form-label">上傳附件</label>
                    <asp:FileUpload ID="fileUploadAttachment" runat="server" CssClass="form-control" AllowMultiple="true" />
                    <asp:Button ID="btnUpload" runat="server" Text="上傳檔案" CssClass="btn btn-outline-secondary btn-sm mt-2" OnClick="btnUpload_Click" CausesValidation="false" />
                </div>
                
                <div class="col-md-12">
                     <label class="form-label">目前附件</label>
                    <asp:Repeater ID="rptAttachments" runat="server" OnItemCommand="rptAttachments_ItemCommand">
                        <HeaderTemplate>
                            <ul class="list-group">
                        </HeaderTemplate>
                        <ItemTemplate>
                            <li class="list-group-item d-flex justify-content-between align-items-center">
                                <asp:HyperLink ID="lnkFileName" runat="server" 
                                    NavigateUrl='<%# Eval("FilePath") %>' Target="_blank"
                                    Text='<%# Eval("FileName") %>' />
                                <asp:LinkButton ID="btnDeleteAttachment" runat="server"
                                    CommandName="Delete" CommandArgument='<%# Eval("AttachmentID") %>'
                                    CssClass="btn-close" OnClientClick="return confirm('您確定要刪除這個附件嗎？');" />
                            </li>
                        </ItemTemplate>
                        <FooterTemplate>
                            </ul>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>

            </div>

            <hr />
            <div class="d-flex justify-content-between">
                <div>
                    <%-- 刪除按鈕：預設隱藏，只有編輯模式才顯示 --%>
                    <asp:Button ID="btnDelete" runat="server" Text="刪除公告" 
                        CssClass="btn btn-danger" 
                        Visible="false"
                        OnClick="btnDelete_Click"
                        OnClientClick="return confirm('您確定要刪除整筆公告嗎？此操作無法復原。');" 
                        CausesValidation="false" />
                </div>
                <div>
                    <%-- 儲存草稿按鈕 (不觸發驗證) --%>
                    <asp:Button ID="btnSaveDraft" runat="server" Text="儲存草稿" 
                        CssClass="btn btn-outline-secondary" 
                        OnClick="btnSaveDraft_Click" 
                        CausesValidation="false" />
                    <%-- 儲存按鈕會根據 rblStatus 的選擇來決定是 "草稿" 還是 "發布" --%>
                    <asp:Button ID="btnSave" runat="server" Text="儲存" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>
            </div>

        </div>
    </div>
<script src="https://cdn.ckeditor.com/4.22.1/standard/ckeditor.js"></script>
    <script type="text/javascript">

        // 建立一個函式來啟動編輯器
        function initializeCKEditor() {
            var contentTextBox = document.querySelector('[id$="txtContent"]');

            // 檢查 1: 元素是否存在
            // 檢查 2: 實例是否「尚未」建立
            if (contentTextBox && !CKEDITOR.instances[contentTextBox.id]) {
                CKEDITOR.replace(contentTextBox.id);
            }
        }

        // --- 事件綁定 ---

        // 1. 頁面【完全】載入後執行
        //    (使用 "load" 事件，確保 ckeditor.js 已經下載完畢)
        window.addEventListener("load", function () {
            initializeCKEditor();
        });

        // 2. 處理 ASP.NET AJAX (UpdatePanel) PostBack
        if (typeof (Sys) !== 'undefined') {
            var prm = Sys.WebForms.PageRequestManager.getInstance();

            prm.add_endRequest(function (sender, args) {

                // (檢查並銷毀舊實例)
                var existingInstance = CKEDITOR.instances[document.querySelector('[id$="txtContent"].id)]')];
                if (existingInstance) {
                    existingInstance.destroy();
                }

                // 重新建立
                initializeCKEditor();
            });
        }
    </script>
</asp:Content>

