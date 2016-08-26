<%@ Page Title="Home Page" Async="true" validateRequest="false" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebAppTestPacker3._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <asp:DropDownList ID="DropDownListVerb" runat="server">
            <asp:ListItem Selected="True">Get</asp:ListItem>
            <asp:ListItem>Post</asp:ListItem>
        </asp:DropDownList>
        <asp:TextBox ID="TextCommand" runat="server" Width="2183px" Font-Size="Smaller" Height="41px">/subscriptions?api-version=2015-08-01</asp:TextBox>
    </div>

    <div class="row">
        <div class="col-md-4">
            <p>
                &nbsp;<asp:Button ID="ButtonExecute" runat="server" Text="Execute SP" OnClick="ButtonExecute_Click" />
                <asp:Button ID="ButtonExecuteUser" runat="server" OnClick="ButtonExecuteUser_Click" Text="Execute User" />
                <asp:Button ID="ButtonExecuteAPI" runat="server" OnClick="ButtonExecuteAPI_Click" Text="Execute API" />
            </p>
            <p>
                <asp:TextBox ID="TextBoxResult" runat="server" Height="364px" Width="2220px" TextMode="MultiLine"></asp:TextBox>
            </p>
        </div>
        <div class="col-md-4">
            <h2>&nbsp;</h2>
        </div>
    </div>

</asp:Content>
