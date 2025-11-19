<%@ Page Title="填寫公文" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyAdd.aspx.cs" Inherits="TMY_AdminSystem.Apply.ApplyAdd" MaintainScrollPositionOnPostback="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hdnFormID" runat="server" Value="0" />

    <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h3 class="fw-bold mb-0">
                <asp:Label ID="lblPageTitle" runat="server" Text="📝 填寫公文申請"></asp:Label>
            </h3>
            <asp:Label ID="lblFormNumber" runat="server" CssClass="text-muted small"></asp:Label>
        </div>
        
        <%-- 右上角狀態顯示 --%>
        <div class="h4 mb-0">
            <asp:Label ID="lblCurrentStatus" runat="server" CssClass="badge bg-secondary" Text="新申請"></asp:Label>
        </div>
    </div>
    <hr />

    <div class="row">
        
        <div class="col-lg-8">
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-primary text-white fw-bold">
                    <i class="fa-solid fa-circle-info"></i> 申請內容
                </div>
                <div class="card-body">
                    
                    <asp:UpdatePanel ID="upCategory" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <div class="row g-3 mb-3">
                                <div class="col-md-6">
                                    <label class="form-label fw-bold">申請大類別</label>
                                    <asp:DropDownList ID="ddlCategory" runat="server" 
                                        CssClass="form-select" 
                                        AutoPostBack="true" 
                                        OnSelectedIndexChanged="ddlCategory_SelectedIndexChanged">
                                        <asp:ListItem Text="--- 請選擇 ---" Value="" />
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-6">
                                    <label class="form-label fw-bold">申請項目 (小類別)</label>
                                    <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="--- 請先選擇大類別 ---" Value="" />
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="rfvType" runat="server" ControlToValidate="ddlType" 
                                        ErrorMessage="請選擇申請項目" CssClass="text-danger small" Display="Dynamic" ValidationGroup="SubmitGroup" />
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>

                    <div class="row g-3 mb-3">
                        <div class="col-md-8">
                            <label class="form-label fw-bold">公文標題</label>
                            <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" placeholder="請輸入主旨"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle" 
                                ErrorMessage="請輸入標題" CssClass="text-danger small" Display="Dynamic" ValidationGroup="SubmitGroup" />
                        </div>
                        <div class="col-md-4">
                            <label class="form-label fw-bold">申請日期</label>
                            <asp:TextBox ID="txtApplyDate" runat="server" CssClass="form-control bg-light" ReadOnly="true"></asp:TextBox>
                        </div>
                        
                        <div class="col-md-12">
                            <label class="form-label">副標題 (選填)</label>
                            <asp:TextBox ID="txtSubtitle" runat="server" CssClass="form-control" placeholder="補充說明"></asp:TextBox>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label fw-bold">申請原因及目的</label>
                        <asp:TextBox ID="txtReason" runat="server" TextMode="MultiLine" Rows="5" CssClass="form-control"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvReason" runat="server" ControlToValidate="txtReason" 
                            ErrorMessage="請輸入申請原因" CssClass="text-danger small" Display="Dynamic" ValidationGroup="SubmitGroup" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label fw-bold">預期成效</label>
                        <asp:TextBox ID="txtOutcome" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control"></asp:TextBox>
                    </div>

                    <div class="mb-3">
                        <label class="form-label fw-bold"><i class="fa-solid fa-paperclip"></i> 相關附件</label>
                        <asp:FileUpload ID="fuAttachment" runat="server" CssClass="form-control mb-2" AllowMultiple="true" />
                        
                        <asp:Repeater ID="rptAttachments" runat="server">
                            <HeaderTemplate><ul class="list-group list-group-flush"></HeaderTemplate>
                            <ItemTemplate>
                                <li class="list-group-item d-flex justify-content-between align-items-center">
                                    <asp:HyperLink ID="lnkFile" runat="server" NavigateUrl='<%# Eval("FilePath") %>' Text='<%# Eval("FileName") %>' Target="_blank"></asp:HyperLink>
                                    <%-- 只有申請人且草稿狀態才顯示刪除鈕 --%>
                                    <asp:LinkButton ID="btnDelFile" runat="server" CssClass="text-danger" CommandName="DelFile" CommandArgument='<%# Eval("AttachmentID") %>'><i class="fa-solid fa-trash-can"></i></asp:LinkButton>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate></ul></FooterTemplate>
                        </asp:Repeater>
                    </div>

                </div>
            </div>
        </div>

        <div class="col-lg-4">
            
            <asp:Panel ID="pnlHistory" runat="server" Visible="false">
                <div class="card shadow-sm mb-4">
                    <div class="card-header bg-secondary text-white fw-bold">
                        <i class="fa-solid fa-clock-rotate-left"></i> 簽核歷程
                    </div>
                    <div class="card-body p-0">
                        <asp:GridView ID="gvLogs" runat="server" AutoGenerateColumns="false" CssClass="table table-sm table-striped mb-0" GridLines="None">
                            <Columns>
                                <asp:BoundField DataField="ApproverName" HeaderText="人員" />
                                <asp:BoundField DataField="ActionType" HeaderText="動作" />
                                <asp:BoundField DataField="ActionDate" HeaderText="時間" DataFormatString="{0:MM/dd HH:mm}" />
                                <asp:TemplateField HeaderText="意見">
                                    <ItemTemplate>
                                        <span title='<%# Eval("Comment") %>'><%# Eval("Comment").ToString().Length > 5 ? Eval("Comment").ToString().Substring(0,5)+"..." : Eval("Comment") %></span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlAuditInput" runat="server" Visible="false">
                <div class="card shadow-sm mb-4 border-warning">
                    <div class="card-header bg-warning text-dark fw-bold">
                        <i class="fa-solid fa-gavel"></i> 審核意見
                    </div>
                    <div class="card-body">
                        <label class="form-label">簽核意見 / 退件理由</label>
                        <asp:TextBox ID="txtAuditComment" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control mb-2"></asp:TextBox>
                    </div>
                </div>
            </asp:Panel>

            <div class="card shadow-sm">
                <div class="card-body">
                    
                    <div class="mb-3">
                        <label class="form-label fw-bold">下一關呈核對象</label>
                        <asp:DropDownList ID="ddlNextHandler" runat="server" CssClass="form-select">
                            <asp:ListItem Text="-- 自動判定 / 選擇主管 --" Value="" />
                            <%-- 這裡後端要綁定主管列表 --%>
                        </asp:DropDownList>
                    </div>

                    <div class="d-grid gap-2">
                        
                        <asp:Panel ID="pnlApplicantActions" runat="server">
                            <asp:Button ID="btnSubmit" runat="server" Text="🚀 送出呈核" CssClass="btn btn-primary"  ValidationGroup="SubmitGroup" />
                            <asp:Button ID="btnDraft" runat="server" Text="💾 儲存草稿" CssClass="btn btn-outline-secondary" CausesValidation="false" />
                        </asp:Panel>

                        <asp:Panel ID="pnlReviewerActions" runat="server" Visible="false">
                            <div class="row g-2">
                                <div class="col-6">
                                    <asp:Button ID="btnApprove" runat="server" Text="✅ 核准/呈轉" CssClass="btn btn-success w-100"  />
                                </div>
                                <div class="col-6">
                                    <asp:Button ID="btnClose" runat="server" Text="🏁 結案" CssClass="btn btn-dark w-100"  OnClientClick="return confirm('確定要直接結案嗎？');" />
                                </div>
                                <div class="col-12">
                                    <asp:Button ID="btnReject" runat="server" Text="❌ 退件" CssClass="btn btn-danger w-100"  />
                                </div>
                            </div>
                        </asp:Panel>

                        <hr />
                        <asp:Button ID="btnBack" runat="server" Text="返回列表" CssClass="btn btn-light border" PostBackUrl="~/Apply/ApplyList.aspx" CausesValidation="false" />
                    </div>

                </div>
            </div>

        </div>
    </div>

    <asp:ValidationSummary ID="ValidationSummary1" runat="server" ShowMessageBox="true" ShowSummary="false" ValidationGroup="SubmitGroup" />

</asp:Content>