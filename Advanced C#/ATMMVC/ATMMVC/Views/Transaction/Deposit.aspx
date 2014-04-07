<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ATMMVC.Models.TransactionModel>" %>

<asp:Content ID="depositTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Deposit
</asp:Content>
<asp:Content ID="depositContent" ContentPlaceHolderID="MainContent" runat="server">
    <% if (Request.IsAuthenticated)
       { %>
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            $('input.deposit').focus();
            $('input.deposit').val("");
        });
    </script>
    <h2>
        Make a deposit</h2>
    <% using (Html.BeginForm())
       { %>
    <%: Html.ValidationSummary(true, "Making a deposit was unsuccessful. Please correct the errors and try again.") %>
    <%
        if (TempData["successD"] != null)
        {
            TempData["successD"] = null;
    %>
    <h4>
        You have successfully made the deposit!</h4>
    <br />
    <% } %>
    <div>
        <label>
            Enter the amount:</label>
        <div class="editor-field">
            $
            <%: Html.TextBoxFor(m => m.credit, new { @class = "deposit" })%>
        </div>
        <input type="hidden" name="account" value="<%: Session["accountNumber"] %>" />
        <p>
            <input type="submit" value="Deposit" />
        </p>
    </div>
    <% } %>
    <% }
            else
            { %>
    <h4 style="color: red;">
        In order to see this page you have to be logged in!</h4>
    <h4>
        Please, try again.</h4>
    <% } %>
</asp:Content>
