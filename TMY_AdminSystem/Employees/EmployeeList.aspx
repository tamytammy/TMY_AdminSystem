<%@ Page Title="員工管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="EmployeeList.aspx.cs" Inherits="TMY_AdminSystem.Employees.EmployeeList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h3 class="fw-bold mb-3">員工管理</h3>
    <!-- 📢 最新公告區 -->
    <div class="alert alert-info shadow-sm mb-4" role="alert">
        <h5 class="alert-heading">📌 最新公告</h5>
        <ul class="mb-0">
            <li>【系統維護】本系統將於 11/10 (五) 18:00 - 20:00 進行維護，期間暫停服務。</li>
            <li>【人事公告】2024 年度特休天數已重置，請至個人資料頁確認。</li>
            <li>【薪資提醒】本月薪資將於 11/5 入帳，請留意銀行通知。</li>
        </ul>
    </div>
    <!-- 🔍 查詢區塊 -->
    <div class="card mb-4 shadow-sm">
        <div class="card-body">
            <div class="row g-3">

                <div class="col-md-3">
                    <asp:TextBox ID="txtKeyword" runat="server" CssClass="form-control" placeholder="搜尋姓名 / 員工編號"></asp:TextBox>
                </div>

                <div class="col-md-3">
                    <asp:DropDownList ID="ddlDept2" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">全部部門</asp:ListItem>
                        <asp:ListItem Value="HR">人資部</asp:ListItem>
                        <asp:ListItem Value="IT">資訊部</asp:ListItem>
                        <asp:ListItem Value="RD">研發部</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="col-md-3">
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Value="">全部在職狀態</asp:ListItem>
                        <asp:ListItem Value="On">在職</asp:ListItem>
                        <asp:ListItem Value="Leave">離職</asp:ListItem>
                    </asp:DropDownList>
                </div>

                <div class="col-md-3 d-grid">
                    <asp:Button ID="btnSearch" runat="server" Text="查詢" CssClass="btn btn-primary" />
                </div>

            </div>
        </div>
    </div>
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
            <asp:DropDownList ID="DropDownList2" runat="server" CssClass="form-select">
                <asp:ListItem Value="On">在職</asp:ListItem>
                <asp:ListItem Value="Leave">離職</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="col-md-4">
            <label class="form-label">角色 / 權限</label>
            <asp:DropDownList ID="ddlRole" runat="server" CssClass="form-select">
                <asp:ListItem Value="User">一般員工</asp:ListItem>
                <asp:ListItem Value="Manager">部門主管</asp:ListItem>
                <asp:ListItem Value="Admin">系統管理者</asp:ListItem>
            </asp:DropDownList>
        </div>

        <div class="col-12 d-grid mt-3">
            <asp:Button ID="btnSaveProfile" runat="server" CssClass="btn btn-primary" Text="💾 儲存資料" OnClick="btnSaveProfile_Click" />
        </div>
    </div>
</div>


            <!-- ✅ 薪資 Tab -->
            <div class="tab-pane fade" id="content-salary">
                <h5 class="fw-bold mb-3">💰 薪資記錄</h5>
                <p class="text-muted">此區顯示最近數期薪資資料。</p>
                <!-- ⚠️ 待填充：薪資 GridView -->
            </div>

            <!-- ✅ 出勤 Tab -->
            <div class="tab-pane fade" id="content-attendance">
                <h5 class="fw-bold mb-3">🕒 出勤記錄</h5>
                <p class="text-muted">此區顯示本月與歷史出勤、加班與請假資訊。</p>
                <!-- ⚠️ 待填充：出勤記錄清單 -->
            </div>

        </div>
    </div
    <!-- 📋 員工清單 -->
    <div class="card shadow-sm">
        <div class="card-body p-0">

            <asp:GridView ID="gvEmployees" runat="server" CssClass="table table-bordered table-hover mb-0"
                AutoGenerateColumns="False" AllowPaging="True" PageSize="10" DataKeyNames="EmployeeID">

                <Columns>
                    <asp:BoundField DataField="EmployeeID" HeaderText="員工編號" />
                    <asp:BoundField DataField="Name" HeaderText="姓名" />
                    <asp:BoundField DataField="Department" HeaderText="部門" />
                    <asp:BoundField DataField="JobTitle" HeaderText="職稱" />
                    <asp:BoundField DataField="EntryDate" HeaderText="入職日期" DataFormatString="{0:yyyy/MM/dd}" />
                    <asp:BoundField DataField="Status" HeaderText="狀態" />

                    <%--<!-- 管理者動作按鈕 -->--%>
                    <asp:TemplateField HeaderText="操作">
                        <ItemTemplate>
                            <asp:HyperLink ID="lnkEdit" runat="server" CssClass="btn btn-sm btn-warning me-1"
                                NavigateUrl='<%# "~/Employees/EmployeeEdit.aspx?id=" + Eval("EmployeeID") %>'>編輯</asp:HyperLink>

                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-danger"
                                CommandName="Delete" CommandArgument='<%# Eval("EmployeeID") %>'
                                OnClientClick="return confirm('確定要刪除這筆資料嗎？');">刪除</asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>

            </asp:GridView>

        </div>
    </div>

    <!-- ➕ 新增員工按鈕（後面會加入權限判斷） -->
    <div class="mt-3">
        <asp:Button ID="btnAdd" runat="server" Text="新增員工" CssClass="btn btn-success" />
    </div>

</asp:Content>
