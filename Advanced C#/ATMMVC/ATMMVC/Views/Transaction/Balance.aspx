<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<ATMMVC.Models.TransactionModel>>" %>

<asp:Content ID="balanceTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Balance
</asp:Content>
<asp:Content ID="balanceContent" ContentPlaceHolderID="MainContent" runat="server">
<%
    if (Request.IsAuthenticated && Model != null)
	{
%>
    <h3> For the account <%: Session["accountNumber"] %> balance is: $<%: String.Format("{0:F}",TempData["balance"]) %></h3>
    <h4> Bellow you can see the turnover for this account:</h4>
    <table style="width: 45%;">
        <tr>
            <th>
                Date/Time
            </th>
            <th>
                Credit
            </th>
            <th>
                Debit
            </th>
        </tr>
        <% foreach (var item in Model)
           { %>
        <tr>
            <td>
                <%: item.tStamp %>
            </td>
            <td>
                $ <%: String.Format("{0:F}", item.credit) %>
            </td>
            <td>
                $ <%: String.Format("{0:F}", item.debit) %>
            </td>
        </tr>
        <% } %>
    </table>
<% } else if (Request.IsAuthenticated) { %>
<h4 style="color:red;">It is not allowed to hit this page out of the regular execution mode!</h4>
<h4>Please, try again.</h4>
<% } else {%>
<h4 style="color:red;">In order to see this page you have to be logged in!</h4>
<h4>Please, try again.</h4>
<% } %>
</asp:Content>
