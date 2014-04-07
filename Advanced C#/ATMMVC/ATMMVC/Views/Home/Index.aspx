<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    C# MVC ATM simulation
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%: ViewData["Message"] %></h2>
    <%
        if (TempData["justRegistered"] != null)
        {
            TempData["justRegistered"] = null;
    %>
    Thank you for registering your account! <br />
    <h4>
        Your acccount number is:
        <%: Session["accountNumber"] %>!</h4>
    Please, choose your action.
    <%
        }
        else
        {
    %>
    This is a C# MVC simulation of a simple ATM.
    <br />
    <br />
    It offers you possibility to create an ATM user and to perform the following activities:
    <ul>
        <li>Deposit</li>
        <li>Withdraw</li>
        <li>Balance</li>
    </ul>
    <%
        }
    %>
</asp:Content>
