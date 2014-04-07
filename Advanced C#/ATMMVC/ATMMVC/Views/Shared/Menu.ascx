<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
	if (Request.IsAuthenticated)
	{
%>
<li>
	<%: Html.ActionLink("Home", "Index", "Home")%></li>
<li>
	<%: Html.ActionLink("Deposit", "Deposit", "Transaction")%></li>
<li>
	<%: Html.ActionLink("Withdraw", "Withdraw", "Transaction")%></li>
<li>
	<%: Html.ActionLink("Balance", "Balance", "Transaction", new {userName = Page.User.Identity.Name}, null)%></li>
<%
	}
	else
	{
%>
<br />
<%
	}
%>