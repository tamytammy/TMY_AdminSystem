<%@ Page Title="員工管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="EmployeeList.aspx.cs" Inherits="TMY_AdminSystem.Employees.EmployeeList" %>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="../css/EmployeeList.css" rel="stylesheet" />
</asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h3 class="fw-bold mb-3">員工管理</h3>
    <!-- 📢 最新公告區 -->
    <div class="alert alert-info shadow-sm mb-4" role="alert">
        <h5 class="alert-heading">📌 最新公告</h5>
        <asp:UpdatePanel ID="upNews" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="true">
            <ContentTemplate>

                <div class="" role="alert">
                   

                    <ul class="mb-0 ps-3">
                        <asp:Repeater ID="rptLatestNews" runat="server" OnItemCommand="rptLatestNews_ItemCommand">
                            <ItemTemplate>
                                <li class="mb-1">
                                    <%-- 點擊標題觸發 ItemCommand (View) --%>
                                    <asp:LinkButton ID="lnkNews" runat="server"
                                        CommandName="View"
                                        CommandArgument='<%# Eval("AnnouncementID") %>'
                                        CssClass="text-decoration-none text-dark link-primary">
                                
                                        <%-- 格式：【分類】2025-10-27 標題 --%>
                                        <span class="fw-bold text-primary">【<%# Eval("CategoryName") %>】</span>
                                        <small class="text-muted ms-1"><%# Eval("PublishDate", "{0:yyyy-MM-dd}") %></small>
                                        <span class="fw-bold"> <%# Eval("Title") %></span>
                               
                                
                            </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate>
                                <%-- 如果沒有資料顯示這行 --%>
                                <asp:Label ID="lblEmpty" runat="server" Visible='<%# rptLatestNews.Items.Count == 0 %>' Text="目前沒有最新公告。" CssClass="text-muted"></asp:Label>
                            </FooterTemplate>
                        </asp:Repeater>
                    </ul>
                </div>

                <div class="modal fade" id="newsModal" tabindex="-1" aria-hidden="true">
                    <div class="modal-dialog modal-lg modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header bg-primary text-white">
                                <h5 class="modal-title">
                                    <asp:Label ID="lblModalTitle" runat="server"></asp:Label>
                                </h5>
                                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                            </div>
                            <div class="modal-body">
                                <div class="d-flex justify-content-between text-muted small mb-3 border-bottom pb-2">
                                    <span><i class="fa-regular fa-calendar"></i>發布日期：<asp:Label ID="lblModalDate" runat="server"></asp:Label></span>
                                    <span><i class="fa-solid fa-tag"></i>分類：<asp:Label ID="lblModalCategory" runat="server"></asp:Label></span>
                                </div>

                                <div class="mb-4">
                                    <asp:Literal ID="litModalContent" runat="server"></asp:Literal>
                                </div>

                                <asp:Panel ID="pnlAttachments" runat="server" Visible="false">
                                    <h6 class="fw-bold border-bottom pb-2"><i class="fa-solid fa-paperclip"></i>附件下載</h6>
                                    <ul class="list-unstyled">
                                        <asp:Repeater ID="rptModalAttachments" runat="server">
                                            <ItemTemplate>
                                                <li class="mb-2">
                                                    <a href='<%# ResolveUrl(Eval("FilePath").ToString()) %>' target="_blank" class="btn btn-outline-secondary btn-sm">
                                                        <i class="fa-solid fa-download"></i><%# Eval("FileName") %>
                                            </a>
                                                </li>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </ul>
                                </asp:Panel>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
                            </div>
                        </div>
                    </div>
                </div>

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <!-- 🔍 查詢區塊 -->
    <asp:Panel runat="server" ID="member_pl" Visible="false">
        <div class="card mb-4 shadow-sm">
            <div class="card-body">
                <div class="row g-3">

                    <div class="col-md-3">
                        <asp:TextBox ID="txtKeyword" runat="server" CssClass="form-control" placeholder="搜尋姓名 / 員工編號"></asp:TextBox>
                    </div>

                    <div class="col-md-3">
                        <asp:DropDownList ID="ddlDept2" runat="server" CssClass="form-select">
                            <asp:ListItem Value="0">全部部門</asp:ListItem>
                            <asp:ListItem Value="1">人資部</asp:ListItem>
                            <asp:ListItem Value="2">資訊部</asp:ListItem>
                            <asp:ListItem Value="3">研發部</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Value="">全部在職狀態</asp:ListItem>
                            <asp:ListItem Value="1">在職</asp:ListItem>
                            <asp:ListItem Value="0">離職</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <asp:UpdatePanel ID="upSearch" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>

                            <div class="col mb-3">
                                <div class="col-md-3 d-grid">
                                    <asp:Button ID="btnSearch" runat="server" Text="查詢"
                                        CssClass="btn btn-primary"
                                        OnClick="btnSearch_Click" />
                                </div>
                            </div>

                            <asp:GridView ID="gvResult" runat="server"
                                CssClass="gov-grid"
                                HeaderStyle-CssClass="gov-grid-header"
                                RowStyle-CssClass="gov-grid-row"
                                AlternatingRowStyle-CssClass="gov-grid-alt"
                                AutoGenerateColumns="False"
                                OnRowCommand="gvResult_RowCommand">

                                <Columns>
                                    <asp:TemplateField HeaderText="員工編號">
                                        <ItemTemplate>
                                            <%# "TMY" + String.Format("{0:000}", Eval("EmployeeID")) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="FullName" HeaderText="姓名" />
                                    <asp:BoundField DataField="Email" HeaderText="電子郵件" />
                                    <asp:BoundField DataField="DeptName" HeaderText="部門" />
                                    <asp:BoundField DataField="JobTitle" HeaderText="職稱" />
                                    <asp:BoundField DataField="JobGrade" HeaderText="職等" />
                                    <asp:BoundField DataField="HireDate" HeaderText="錄用日期" DataFormatString="{0:yyyy-MM-dd}" />
                                    <asp:TemplateField HeaderText="操作">
                                        <ItemTemplate>
                                            <asp:Button
                                                ID="btnOpenResetModal"
                                                runat="server"
                                                Text="重設密碼"
                                                CssClass="btn btn-warning btn-sm"
                                                CommandName="OpenResetModal"
                                                CommandArgument='<%# Eval("EmployeeID") %>' />
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>

                            </asp:GridView>


                        </ContentTemplate>
                    </asp:UpdatePanel>


                </div>
            </div>
        </div>
    </asp:Panel>
   
        <!-- 📑 Tabs 區域 -->
    <div class="card shadow-sm mb-4">
        <div class="card-header">
            <ul class="nav nav-tabs card-header-tabs" id="profileTabs">
                <li class="nav-item">
                    <a class="nav-link active" id="tab-profile" data-bs-toggle="tab" href="#content-profile">個人資料</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" id="tab-salary" data-bs-toggle="tab" href="#content-salary">薪資記錄</a>
                </li>
                <li class="nav-item">
                    <a class="nav-link" id="tab-attendance" data-bs-toggle="tab" href="#content-attendance">出勤記錄</a>
                </li>
            </ul>
        </div>

        <div class="tab-content p-4">

            <!-- ✅ 個人資料 Tab -->
            <div class="tab-pane fade show active" id="content-profile">
    <h5 class="fw-bold mb-3">👤 個人資料</h5>

    <asp:Label ID="lblProfileMsg" runat="server" CssClass="text-success"></asp:Label>

    <div class="row g-3 mt-2">
        <div class="col-md-4">
            <label class="form-label">姓名</label>
            <asp:TextBox ID="txtFullName" runat="server" CssClass="form-control"></asp:TextBox>
        </div>

        <div class="col-md-4">
            <label class="form-label">E-mail</label>
            <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
        </div>

        <div class="col-md-4">
            <label class="form-label">部門</label>
            <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select"></asp:DropDownList>
        </div>

        <div class="col-md-4">
            <label class="form-label">職位</label>
            <asp:TextBox ID="txtJobTitle" runat="server" CssClass="form-control"></asp:TextBox>
        </div>

        <div class="col-md-4">
            <label class="form-label">職級</label>
            <asp:TextBox ID="txtJobGrade" runat="server" CssClass="form-control" Placeholder="例：L1 / M2"></asp:TextBox>
        </div>

        <div class="col-md-4">
            <label class="form-label">入職日期</label>
            <asp:TextBox ID="txtHireDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
        </div>

        <div class="col-md-4">
            <label class="form-label">在職狀態</label>
            <asp:DropDownList ID="ddlStatus2" runat="server" CssClass="form-select">
                <asp:ListItem Value="1">在職</asp:ListItem>
                <asp:ListItem Value="0">離職</asp:ListItem>
            </asp:DropDownList>
        </div>

        <asp:UpdatePanel ID="upRole" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="row g-2">

                    <div class="col-md-6">
                        <label class="form-label">帳號權限</label>
                        <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlRole_SelectedIndexChanged">
                            <asp:ListItem Value="User">一般員工</asp:ListItem>
                            <asp:ListItem Value="Manager">部門主管</asp:ListItem>
                            <asp:ListItem Value="Admin">系統管理者</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-6">
                        <label class="form-label">對應職級</label>
                        <asp:DropDownList ID="ddlRoleID" runat="server" CssClass="form-select">
                            <%-- 預設值，後端會動態改變這裡 --%>
                            <asp:ListItem Value="1">一般員工</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                </div>
            </ContentTemplate>
        </asp:UpdatePanel>

        <div class="col-12 d-grid mt-3">
            <asp:Button ID="btnSaveProfile" runat="server" CssClass="btn btn-primary" Text="💾 儲存資料" OnClick="btnSaveProfile_Click" />
        </div>
    </div>
</div>

            <!-- ✅ 薪資 Tab -->
            <div class="tab-pane fade" id="content-salary">
                <h5 class="fw-bold mb-3">💰 薪資記錄</h5>
                 
                <div class="tab-pane fade show active" id="salary" role="tabpanel">

                    <asp:UpdatePanel ID="upSalary" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:Label ID="lblProfileMsg2" runat="server" CssClass="text-success"></asp:Label>
                            <!-- 年份 / 月份搜尋 -->
                            <div class="row mb-3">
                                <div class="col-md-3">
                                    <label>年份</label>
                                    <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-control">
                                        <asp:ListItem Value="">請選擇</asp:ListItem>
                                        <asp:ListItem Value="2025">2025</asp:ListItem>
                                        <asp:ListItem Value="2024">2024</asp:ListItem>
                                        <asp:ListItem Value="2023">2023</asp:ListItem>
                                    </asp:DropDownList>
                                </div>

                                <div class="col-md-3">
                                    <label>月份</label>
                                    <asp:DropDownList ID="ddlMonth" runat="server" CssClass="form-control">
                                        <asp:ListItem Value="">請選擇</asp:ListItem>
                                        <asp:ListItem Value="1">1</asp:ListItem>
                                        <asp:ListItem Value="2">2</asp:ListItem>
                                        <asp:ListItem Value="3">3</asp:ListItem>
                                        <asp:ListItem Value="4">4</asp:ListItem>
                                        <asp:ListItem Value="5">5</asp:ListItem>
                                        <asp:ListItem Value="6">6</asp:ListItem>
                                        <asp:ListItem Value="7">7</asp:ListItem>
                                        <asp:ListItem Value="8">8</asp:ListItem>
                                        <asp:ListItem Value="9">9</asp:ListItem>
                                        <asp:ListItem Value="10">10</asp:ListItem>
                                        <asp:ListItem Value="11">11</asp:ListItem>
                                        <asp:ListItem Value="12">12</asp:ListItem>
                                    </asp:DropDownList>
                                </div>


                                <div class="col-md-3">
                                    <asp:Button ID="btnSearchSalary" runat="server" CssClass="btn btn-primary mt-4"
                                        Text="查詢" OnClick="btnSearchSalary_Click" />
                                </div>
                            </div>

                            <!-- 薪資細項 GridView -->
                            <asp:GridView ID="gvSalaryDetail" runat="server" CssClass="table table-bordered"
                                AutoGenerateColumns="False">
                                <Columns>
                                    <asp:BoundField DataField="ItemName" HeaderText="項目" />
                                    <asp:BoundField DataField="ItemAmount" HeaderText="金額" DataFormatString="{0:C0}" />
                                </Columns>
                            </asp:GridView>

                            <label>實領金額：</label>
                            <asp:Label ID="lblNetPay" runat="server" />

                            <br />

                            <label>發薪日：</label>
                            <asp:Label ID="lblPayDate" runat="server" />

                        </ContentTemplate>

                        <Triggers>
                            <asp:AsyncPostBackTrigger ControlID="btnSearchSalary" EventName="Click" />
                        </Triggers>

                    </asp:UpdatePanel>


                </div>

            </div>

            <!-- ✅ 出勤 Tab -->
            <div class="tab-pane fade" id="content-attendance">
                <h5 class="fw-bold mb-3">🕒 出勤記錄</h5>
                <p class="text-muted">此區顯示本月與歷史出勤、加班與請假資訊。</p>
                <asp:UpdatePanel ID="upAttendance" runat="server" UpdateMode="Conditional">
                    <ContentTemplate>

                        <!-- 年 / 月 選擇 -->
                        <div class="row mb-3">
                            <div class="col-md-3">
                                <label>年份</label>
                                <asp:DropDownList ID="dd1Year_1" runat="server" CssClass="form-control">
                                    <asp:ListItem Value="">請選擇</asp:ListItem>
                                    <asp:ListItem Value="2025">2025</asp:ListItem>
                                    <asp:ListItem Value="2024">2024</asp:ListItem>
                                    <asp:ListItem Value="2023">2023</asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <div class="col-md-3">
                                <label>月份</label>
                                <asp:DropDownList ID="dd1Month_1" runat="server" CssClass="form-control">
                                    <asp:ListItem Value="">請選擇</asp:ListItem>
                                    <asp:ListItem Value="1">1</asp:ListItem>
                                    <asp:ListItem Value="2">2</asp:ListItem>
                                    <asp:ListItem Value="3">3</asp:ListItem>
                                    <asp:ListItem Value="4">4</asp:ListItem>
                                    <asp:ListItem Value="5">6</asp:ListItem>
                                    <asp:ListItem Value="6">6</asp:ListItem>
                                    <asp:ListItem Value="7">7</asp:ListItem>
                                    <asp:ListItem Value="8">8</asp:ListItem>
                                    <asp:ListItem Value="9">9</asp:ListItem>
                                    <asp:ListItem Value="10">10</asp:ListItem>
                                    <asp:ListItem Value="11">11</asp:ListItem>
                                    <asp:ListItem Value="12">12</asp:ListItem>
                                </asp:DropDownList>
                            </div>

                            <div class="col-md-3 d-grid">
                                <asp:Button ID="btnSearchAttendance" runat="server"
                                    Text="查詢" CssClass="btn btn-primary mt-4"
                                    OnClick="btnSearchAttendance_Click" />
                            </div>
                        </div>

                        <!-- GridView：出勤紀錄 -->
                        <asp:GridView ID="gvAttendance" runat="server"
                            AutoGenerateColumns="False"
                            CssClass="table table-bordered gov-grid"
                            HeaderStyle-CssClass="gov-grid-header"
                            RowStyle-CssClass="gov-grid-row"
                            AlternatingRowStyle-CssClass="gov-grid-alt"
                            OnRowDataBound="gvAttendance_RowDataBound">

                            <Columns>
                                <asp:BoundField DataField="WorkDate" HeaderText="日期" DataFormatString="{0:yyyy/MM/dd}" />
                                <asp:BoundField DataField="CheckInTime" HeaderText="上班時間" />
                                <asp:BoundField DataField="CheckOutTime" HeaderText="下班時間" />
                                <asp:BoundField DataField="WorkHours" HeaderText="工作時數" />
                                <asp:BoundField DataField="OvertimeHours" HeaderText="加班時數" />
                                <asp:BoundField DataField="LeaveType" HeaderText="假別" />
                                <asp:BoundField DataField="Remark" HeaderText="備註" />
                            </Columns>

                        </asp:GridView>

                    </ContentTemplate>

                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="btnSearchAttendance" EventName="Click" />
                    </Triggers>

                </asp:UpdatePanel>
            </div>

        </div>
    </div>

   

    <!-- 新增員工按鈕（後面會加入權限判斷） -->
    <div class="mt-3">
        <asp:HyperLink ID="lnkAddEmployee" runat="server"
            NavigateUrl="~/Register.aspx"
            CssClass="btn btn-success" Visible="false">
        新增員工
        </asp:HyperLink>
    </div>


    <!-- 重設密碼 Modal -->
    <div class="modal fade" id="resetPwdModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog modal-dialog-centered">
            <div class="modal-content">

                <div class="modal-header bg-primary text-white">
                    <h5 class="modal-title">重設密碼</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                </div>

                <div class="modal-body">

                    <asp:Label ID="lblResetUserId" runat="server" Visible="false"></asp:Label>

                    <div class="mb-3">
                        <label class="form-label">新密碼</label>
                        <asp:TextBox ID="txtNewPwd" runat="server" CssClass="form-control" TextMode="Password" />
                    </div>

                    <div class="mb-3">
                        <label class="form-label">確認密碼</label>
                        <asp:TextBox ID="txtConfirmPwd" runat="server" CssClass="form-control" TextMode="Password" />
                    </div>

                    <asp:Label ID="lblResetMsg" runat="server" CssClass="text-danger"></asp:Label>

                </div>

                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                    <asp:Button ID="btnConfirmReset" runat="server" Text="確認重設" CssClass="btn btn-primary" OnClick="btnConfirmReset_Click" />
                </div>

            </div>
        </div>
    </div>

    <script type="text/javascript">
    function openNewsModal() {
        var myModal = new bootstrap.Modal(document.getElementById('newsModal'));
        myModal.show();
    }
</script>

</asp:Content>
