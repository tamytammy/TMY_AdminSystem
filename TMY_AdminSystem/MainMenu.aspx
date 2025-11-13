<%@ Page Title="首頁" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="MainMenu.aspx.cs" Inherits="TMY_AdminSystem.MainMenu" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
     <h2 class="fw-bold mb-4">系統主選單</h2>

    <div class="row g-4">
        <div class="col-md-4">
            <div class="card shadow-sm">
                <div class="card-body text-center">
                    <i class="fa fa-users fa-3x text-primary mb-3"></i>
                    <h5 class="card-title">員工管理</h5>
                    <p class="card-text text-muted">新增、編輯、搜尋員工資料</p>
                    <a href="./Employees/EmployeeList.aspx" class="btn btn-primary btn-sm">進入管理</a>
                </div>
            </div>
        </div>

        <div class="col-md-4">
            <div class="card shadow-sm">
                <div class="card-body text-center">
                    <i class="fa fa-bullhorn fa-3x text-warning mb-3"></i>
                    <h5 class="card-title">公告管理</h5>
                    <p class="card-text text-muted">發布 / 檢視系統公告</p>
                    <a href="./Announcements/AnnList.aspx" class="btn btn-warning btn-sm">進入管理</a>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card shadow-sm">
                <div class="card-body text-center">
                    <i class="fas fa-file-signature fa-3x text-success mb-3"></i>
                    <h5 class="card-title">公文審核申請</h5>
                    <p class="card-text text-muted">申請、審核、退回、紀錄與統計管理</p>
                    <a href="./Application/ApplicationList.aspx" class="btn btn-success btn-sm">進入管理</a>
                </div>
            </div>
        </div>
<%--        <div class="col-md-4">
            <div class="card shadow-sm">
                <div class="card-body text-center">
                    <i class="fa fa-download fa-3x text-success mb-3"></i>
                    <h5 class="card-title">匯出報表</h5>
                    <p class="card-text text-muted">匯出 Excel / PDF / Word / 壓縮檔等</p>
                    <a href="./Reports/Export.aspx" class="btn btn-success btn-sm">進入管理</a>
                </div>
            </div>
        </div>--%>


    </div>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
