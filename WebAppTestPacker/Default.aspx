<%@ Page Title="Home Page" Async="true" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebAppTestPacker._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <asp:TextBox ID="TextCommand" runat="server" Width="2183px">/subscriptions/{0}/resourceGroups/citrixwademorg/providers/Microsoft.Web/sites/citrixwebappdemo/TriggeredWebJobs/WebJobTestPacker/run?api-version=2015-08-01</asp:TextBox>
        <asp:DropDownList ID="DropDownListVerb" runat="server">
            <asp:ListItem Selected="True">Get</asp:ListItem>
            <asp:ListItem>Post</asp:ListItem>
        </asp:DropDownList>
    </div>

    <div class="row">
        <div class="col-md-4">
            <p>
                &nbsp;<asp:Button ID="ButtonExecute" runat="server" Text="Execute SP" OnClick="ButtonExecute_Click" />
                <asp:Button ID="ButtonExecuteUser" runat="server" OnClick="ButtonExecuteUser_Click" Text="Execute User" />
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
