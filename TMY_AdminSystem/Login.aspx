<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="TMY_AdminSystem.Login" %>

<!DOCTYPE html>
<html lang="zh-Hant">
<head runat="server">
    <meta charset="utf-8" />
    <title>登入系統 - TMY Admin</title>

    <!-- Bootstrap -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet" />

    <style>
        body { background: #f1f2f6; font-family: 'Microsoft JhengHei'; }
        .login-card {
            max-width: 420px;
            background: white;
            padding: 35px;
            margin: 80px auto;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="login-card">
            <h4 class="text-center fw-bold mb-4">TMY Admin System</h4>

            <asp:Label ID="lblMessage" runat="server" CssClass="text-danger fw-bold" />

            <div class="mb-3">
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control"
                    Placeholder="帳號"></asp:TextBox>
            </div>

            <div class="mb-3">
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control"
                    Placeholder="密碼"></asp:TextBox>
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="登入"
                CssClass="btn btn-primary w-100" OnClick="btnLogin_Click" />

            <%--<% remove register link %>
            <div class="text-center mt-3">
                <a href="Register.aspx" class="small">還沒有帳號？前往註冊</a>
            </div>
            --%>
        </div>
    </form>
</body>
</html>
