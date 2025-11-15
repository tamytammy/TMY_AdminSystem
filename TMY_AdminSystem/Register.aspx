<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="TMY_AdminSystem.Register" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <title>Register</title>
    <style>
        body { font-family: Arial; padding: 40px; }
        .form-box { width: 350px; margin: auto; border: 1px solid #ccc; padding: 20px; border-radius: 6px; }
        input[type=text], input[type=password], input[type=email] {
             width: 94%; padding: 10px; margin-top: 8px; margin-bottom: 15px; border: 1px solid #aaa;
        }
        .btn { width: 100%; background: #0078d7; color: #fff; padding: 10px; border: none; cursor: pointer; }
        .btn:hover { background: #005fa3; }
        .msg { color: red; }
    </style>
</head>
<body>

    <form id="form1" runat="server">
        <div class="form-box">
            <h2>註冊帳號</h2>

            <asp:Label ID="lblMsg" runat="server" CssClass="msg"></asp:Label>

            <asp:TextBox ID="txtAccount" runat="server" Placeholder="帳號"></asp:TextBox>
            <asp:TextBox ID="txtEmail" runat="server" Placeholder="Email" TextMode="Email"></asp:TextBox>
            <asp:TextBox ID="txtPassword" runat="server" Placeholder="密碼" TextMode="Password"></asp:TextBox>
            <asp:TextBox ID="txtConfirm" runat="server" Placeholder="確認密碼" TextMode="Password"></asp:TextBox>
            <asp:TextBox ID="txtPasswordHint" runat="server" Placeholder="密碼提示" ></asp:TextBox>
            <asp:Button ID="btnRegister" runat="server" Text="註冊" CssClass="btn" OnClick="btnRegister_Click" />
        </div>
    </form>

</body>
</html>
