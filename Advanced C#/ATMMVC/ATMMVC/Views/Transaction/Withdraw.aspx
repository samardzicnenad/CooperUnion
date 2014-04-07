<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ATMMVC.Models.TransactionModel>" %>

<asp:Content ID="withdrawTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Withdraw
</asp:Content>
<asp:Content ID="withdrawContent" ContentPlaceHolderID="MainContent" runat="server">
<%
	if (Request.IsAuthenticated)
	{
%>
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.7.1/jquery.min.js" type="text/javascript"></script>
    <script type="text/javascript">
        $(function () {
            $('input.withdraw').focus();
            $('input.withdraw').val("");
        });
    </script>
    <h2>
        Make a withdrawal</h2>
    <% using (Html.BeginForm())
       { %>
    <%: Html.ValidationSummary(true, "Making a withdrawal was unsuccessful. Please correct the errors and try again.") %>
    <%
        if (TempData["successW"] != null)
        {
            if (TempData["successW"] == "Y")
            {
    %>
    <h4>
        You have successfully made the withdrawal!</h4>
    <br />
    <% } else { %>
    <h4 style=" color:red; ">
        Error! You tried to withdraw amount larger than your balance!</h4>
    <br />
    <% }
            TempData["successW"] = null;
        }%>
    <div>
        <label>
            Enter the amount:</label>
        <div class="editor-field">
            $ <%: Html.TextBoxFor(m => m.debit, new { @class = "withdraw" })%>
        </div>
        <input type="hidden" name="account" value="<%: Session["accountNumber"] %>" />
        <p>
            <input type="submit" value="Withdraw" />
        </p>
    </div>
    <% } %>
    <% } else { %>
<h4 style="color:red;">In order to see this page you have to be logged in!</h4>
<h4>Please, try again.</h4>
<% } %>
</asp:Content>
