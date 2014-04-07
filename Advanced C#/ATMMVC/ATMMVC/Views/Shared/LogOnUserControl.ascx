<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%
	if (Request.IsAuthenticated)
	{
%>
Welcome <b>
	<%: Page.User.Identity.Name %></b>! [
<%: Html.ActionLink("Change Password", "ChangePassword", "Customer")%>
]
[
<%: Html.ActionLink("Log Off", "LogOff", "Customer")%>
]
<%
	}
	else
	{
%>
[
<%: Html.ActionLink("Log On", "LogOn", "Customer") %>
]
<%
	}
%>