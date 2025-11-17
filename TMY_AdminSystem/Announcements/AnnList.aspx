<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<%@ Page Title="公告管理" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AnnList.aspx.cs" Inherits="TMY_AdminSystem.Announcements.AnnList" %>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

   <h3 class="fw-bold mb-3">公告管理</h3>
    <hr />

    <div class="card mb-4 shadow-sm">
        <div class="card-body">
            <asp:Panel runat="server" ID="pnlQuery" DefaultButton="btnSearch">
                <div class="row g-3 align-items-end">
                    
                    <div class="col-md-3">
                        <label for="<%= ddlCategoryFilter.ClientID %>" class="form-label">公告分類</label>
                        <asp:DropDownList ID="ddlCategoryFilter" runat="server" 
                            CssClass="form-select"
                            AppendDataBoundItems="true" 
                            DataTextField="CategoryName" 
                            DataValueField="CategoryID">
                            <asp:ListItem Text="--- 全部分類 ---" Value="0" />
                        </asp:DropDownList>

                    </div>

                    <div class="col-md-3">
                        <label for="<%= txtKeywordFilter.ClientID %>" class="form-label">關鍵字</label>
                        <asp:TextBox ID="txtKeywordFilter" runat="server" placeholder="搜尋標題或內容" CssClass="form-control" />
                    </div>

                    <div class="col-md-2">
                        <label for="<%= txtDateStart.ClientID %>" class="form-label">發布日期 (起)</label>
                        <asp:TextBox ID="txtDateStart" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>

                    <div class="col-md-2">
                         <label for="<%= txtDateEnd.ClientID %>" class="form-label">發布日期 (迄)</label>
                        <asp:TextBox ID="txtDateEnd" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>

                    <div class="col-md-2">
                        <div class="d-flex gap-2">
                            <asp:Button ID="btnSearch" runat="server" Text="查詢" OnClick="btnSearch_Click" CssClass="btn btn-primary w-100" />
                            <asp:Button ID="btnClear" runat="server" Text="清除" OnClick="btnClear_Click" CssClass="btn btn-outline-secondary w-100" CausesValidation="false" />
                        </div>
                    </div>

                </div>
            </asp:Panel>
        </div>
    </div>

    <div class="text-end mb-3">
        <asp:Button ID="btnAddNew" runat="server" Text="＋ 新增公告" OnClick="btnAddNew_Click" CssClass="btn btn-success" CausesValidation="false" />
    </div>

    <asp:UpdatePanel ID="upnlGridView" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            
            <div class="card shadow-sm">
                <div class="card-body p-0"> <%-- p-0 讓表格貼齊 card 邊緣 --%>
                    <asp:GridView ID="gvAnnouncements" runat="server"
                        AutoGenerateColumns="False" 
                        AllowPaging="True" 
                        AllowSorting="True"
                        PageSize="10"
                        CssClass="table table-hover table-bordered align-middle mb-0"
                        EmptyDataText="查無任何公告資料。"
                        Width="100%"
                        OnRowCommand="gvAnnouncements_RowCommand" >
                        
                        <Columns>
                            <asp:BoundField DataField="Title" HeaderText="標題" SortExpression="Title" />
                            
                            <asp:BoundField DataField="CategoryName" HeaderText="分類" SortExpression="CategoryName" ItemStyle-Width="120px" />
                            
                            <asp:BoundField DataField="PublishDate" HeaderText="發布日期" SortExpression="PublishDate" DataFormatString="{0:yyyy/MM/dd}" ItemStyle-Width="120px" />
                            
                            <asp:TemplateField HeaderText="狀態" SortExpression="Status" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%-- 後端 C# 函式會回傳 Bootstrap Badge 樣式 --%>
                                    <%# FormatStatus(Eval("Status")) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="AuthorName" HeaderText="發布者" SortExpression="AuthorName" ItemStyle-Width="120px" />
                            
                            <asp:BoundField DataField="UpdateDate" HeaderText="最後更新" SortExpression="UpdateDate" DataFormatString="{0:yyyy/MM/dd HH:mm}" ItemStyle-Width="160px" />

                            <asp:TemplateField HeaderText="操作" ItemStyle-Width="140px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <asp:LinkButton ID="lnkEdit" runat="server" 
                                        Text="編輯" 
                                        CommandName="EditRow" 
                                        CommandArgument='<%# Eval("AnnouncementID") %>'
                                        CssClass="btn btn-outline-primary btn-sm" />
                                    <asp:LinkButton ID="lnkDelete" runat="server" 
                                        Text="刪除" 
                                        CommandName="DeleteRow" 
                                        CommandArgument='<%# Eval("AnnouncementID") %>'
                                        CssClass="btn btn-outline-danger btn-sm"
                                        OnClientClick="return confirm('您確定要刪除這筆公告嗎？');" 
                                        Style="margin-left: 5px;" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <HeaderStyle CssClass="table-light" /> <%-- BS5 表格標頭樣式 --%>
                        <PagerStyle CssClass="p-3" HorizontalAlign="Center" /> <%-- BS5 Pager 樣式 --%>
                    </asp:GridView>

                </div>
            </div>



        </ContentTemplate>
        <Triggers>
            <%-- 告訴 UpdatePanel，這兩個按鈕會觸發它更新 --%>
            <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnClear" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>



</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
