<%@Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="changePasswordTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Change Password
</asp:Content>

<asp:Content ID="changePasswordSuccessContent" ContentPlaceHolderID="MainContent" runat="server">
    <% if (Request.IsAuthenticated)
       { %>
    <h2>Change Password</h2>
    <p>
        Your password has been changed successfully.
    </p>
        <% }
            else
            { %>
    <h4 style="color: red;">
        In order to see this page you have to be logged in!</h4>
    <h4>
        Please, try again.</h4>
    <% } %>
</asp:Content>
