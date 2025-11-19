<%@ Page Title="公文簽核" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyList.aspx.cs" Inherits="TMY_AdminSystem.Apply.ApplyList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="d-flex justify-content-between align-items-center mb-3">
        <h3 class="fw-bold mb-3">公文簽核</h3>
        
        <%-- 新增申請按鈕 --%>
        <asp:Button ID="btnAdd" runat="server" Text="＋ 填寫公文" 
            PostBackUrl="~/Apply/ApplyAdd.aspx" 
            CssClass="btn btn-success shadow-sm" />
    </div>
    <hr />

    <div class="card mb-4 shadow-sm">
        <div class="card-header bg-light fw-bold">
            <i class="fa-solid fa-filter"></i> 查詢條件
        </div>
        <div class="card-body">
            <asp:Panel runat="server" ID="pnlQuery" DefaultButton="btnSearch">
                <div class="row g-3 align-items-end">
                    
                    <div class="col-md-3">
                        <label class="form-label fw-bold text-primary">檢視模式</label>
                        <asp:DropDownList ID="ddlViewMode" runat="server" CssClass="form-select border-primary">
                            <asp:ListItem Value="MyApply" Selected="True">我的申請案件</asp:ListItem>
                            <asp:ListItem Value="MyAudit">待我簽核案件</asp:ListItem>
                            <asp:ListItem Value="MyDept">本部門案件</asp:ListItem>
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <label class="form-label">公文類別</label>
                        <asp:DropDownList ID="ddlType" runat="server" 
                            CssClass="form-select"
                            AppendDataBoundItems="true"
                            DataSourceID="sdsFlowTypes"
                            DataTextField="TypeName" DataValueField="TypeID">
                            <asp:ListItem Text="--- 全部類別 ---" Value="" />
                        </asp:DropDownList>
                        
                        <%-- 連接 Flow_Types 資料表 --%>
                        <asp:SqlDataSource ID="sdsFlowTypes" runat="server" 
                            ConnectionString="<%$ ConnectionStrings:TMY_DB %>" 
                            SelectCommand="SELECT TypeID, TypeName FROM Flow_Types ORDER BY CategoryID, TypeName">
                        </asp:SqlDataSource>
                    </div>

                    <div class="col-md-3">
                        <label class="form-label">關鍵字</label>
                        <asp:TextBox ID="txtKeyword" runat="server" CssClass="form-control" placeholder="單號 / 主旨" />
                    </div>

                    <div class="col-md-3">
                        <label class="form-label">目前狀態</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="全部" Value="" />
                            <asp:ListItem Text="草稿" Value="0" />
                            <asp:ListItem Text="簽核中" Value="1" />
                            <asp:ListItem Text="已結案" Value="2" />
                            <asp:ListItem Text="已退件" Value="3" />
                            <asp:ListItem Text="已撤銷" Value="4" />
                        </asp:DropDownList>
                    </div>

                    <div class="col-md-3">
                        <label class="form-label">申請日期 (起)</label>
                        <asp:TextBox ID="txtDateStart" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>
                    <div class="col-md-3">
                        <label class="form-label">申請日期 (迄)</label>
                        <asp:TextBox ID="txtDateEnd" runat="server" TextMode="Date" CssClass="form-control" />
                    </div>

                    <div class="col-md-6 text-end">
                        <asp:Button ID="btnSearch" runat="server" Text="查詢"  CssClass="btn btn-primary px-4" OnClick="btnSearch_Click"/>
                        <asp:Button ID="btnClear" runat="server" Text="清除"  CssClass="btn btn-outline-secondary" OnClick="btnClear_Click"/>
                    </div>

                </div>
            </asp:Panel>
        </div>
    </div>

    <asp:UpdatePanel ID="upList" runat="server">
        <ContentTemplate>
            <div class="card shadow-sm">
                <div class="card-body p-0">
                    <asp:GridView ID="gvApplyList" runat="server"
                        AutoGenerateColumns="False"
                        DataKeyNames="FormID"
                        AllowPaging="True" PageSize="10"
                        AllowSorting="True"
                        CssClass="table table-hover table-striped align-middle mb-0"
                        EmptyDataText="沒有符合條件的公文資料。"
                        GridLines="None">
                        
                        <Columns>
                            <%-- 單號 --%>
                            <asp:BoundField DataField="FormNumber" HeaderText="單號" SortExpression="FormNumber" ItemStyle-Width="120px" />
                            
                            <%-- 類別 (顯示 大類 - 小類) --%>
                            <asp:TemplateField HeaderText="類別">
                                <ItemTemplate>
                                    <small class="text-muted"><%# Eval("CategoryName") %></small><br />
                                    <%# Eval("TypeName") %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- 主旨 (點擊可進入) --%>
                            <asp:TemplateField HeaderText="主旨" ItemStyle-CssClass="fw-bold">
                                <ItemTemplate>
                                    <asp:HyperLink ID="lnkTitle" runat="server" 
                                        NavigateUrl='<%# "ApplyAdd.aspx?ID=" + Eval("FormID") %>'
                                        Text='<%# Eval("Title") %>' CssClass="text-decoration-none text-dark"></asp:HyperLink>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- 申請資訊 --%>
                            <asp:TemplateField HeaderText="申請人">
                                <ItemTemplate>
                                    <div><%# Eval("ApplicantName") %></div>
                                    <small class="text-muted"><%# Eval("DeptName") %></small>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- 申請日期 --%>
                            <asp:BoundField DataField="CreateDate" HeaderText="申請日期" DataFormatString="{0:yyyy/MM/dd}" ItemStyle-Width="100px" />

                            <%-- 狀態 (Badge) --%>
                            <asp:TemplateField HeaderText="狀態" ItemStyle-Width="100px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%# FormatStatus(Eval("Status")) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- 目前處理人 --%>
                            <asp:TemplateField HeaderText="目前關卡">
                                <ItemTemplate>
                                    <%# FormatHandler(Eval("CurrentHandlerName"), Eval("Status")) %>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%-- 操作按鈕 --%>
                            <asp:TemplateField HeaderText="操作" ItemStyle-Width="120px" ItemStyle-HorizontalAlign="Center">
                                <ItemTemplate>
                                    <%-- 審核按鈕 (預設隱藏，RowDataBound 判斷權限後開啟) --%>
                                    <asp:LinkButton ID="btnAudit" runat="server" 
                                        CommandName="Audit" CommandArgument='<%# Eval("FormID") %>'
                                        CssClass="btn btn-danger btn-sm shadow-sm" Visible="false">
                                        <i class="fa-solid fa-pen-nib"></i> 審核
                                    </asp:LinkButton>

                                    <%-- 查看按鈕 --%>
                                    <asp:LinkButton ID="btnView" runat="server" 
                                        CommandName="View" CommandArgument='<%# Eval("FormID") %>'
                                        CssClass="btn btn-outline-primary btn-sm" Visible="false">
                                        查看
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>

                        <HeaderStyle CssClass="table-light fw-bold" />
                        <PagerStyle CssClass="p-3" HorizontalAlign="Center" />
                    </asp:GridView>
                </div>
            </div>
        </ContentTemplate>
        <Triggers>
            <asp:AsyncPostBackTrigger ControlID="btnSearch" EventName="Click" />
            <asp:AsyncPostBackTrigger ControlID="btnClear" EventName="Click" />
        </Triggers>
    </asp:UpdatePanel>

</asp:Content>