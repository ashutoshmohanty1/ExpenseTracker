<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="Expense_Tracker.Dashboard" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Expense Tracker</title>
    <link rel="stylesheet" href="style.css" type="text/css" />

    <!-- jQuery and jQuery UI CSS/JS -->
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css" />
    <script src="https://code.jquery.com/jquery-3.6.4.min.js"></script>
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.min.js"></script>

    <script type="text/javascript">
        $(function () {
            // Initialize datepickers
            $("#<%= txtFromDate.ClientID %>").datepicker({ dateFormat: 'dd-mm-yy' });
            $("#<%= txtToDate.ClientID %>").datepicker({ dateFormat: 'dd-mm-yy' });
        });
    </script>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h1>Expense Tracker</h1>

            <!-- Stats -->
            <div class="stats">
                <div class="stat-box">
                    Money Received: 
                    <span class="stat-value">
                        <asp:Label ID="lblMoneyReceived" runat="server" Text="0"></asp:Label>
                    </span>
                </div>
                <div class="stat-box">
                    Money Spent: 
                    <span class="stat-value">
                        <asp:Label ID="lblMoneySpent" runat="server" Text="0"></asp:Label>
                    </span>
                </div>
                <div class="stat-box">
                    Balance: 
                    <span class="stat-value">
                        <asp:Label ID="lblBalance" runat="server" Text="0"></asp:Label>
                    </span>
                </div>
            </div>

            <!-- Add Money -->
            <div class="section">
                <h3>Add Money</h3>
                <asp:TextBox ID="TextBox1" runat="server" Placeholder="Amount" CssClass="txt"></asp:TextBox>
                <asp:TextBox ID="TextBox2" runat="server" Placeholder="From (Name)" CssClass="txt"></asp:TextBox>
                <br />
                <asp:Button ID="Button1" runat="server" Text="Add Money" CssClass="btn add" OnClick="Button1_Click" />
            </div>

            <!-- Spend Money -->
            <div class="section">
                <h3>Spend Money</h3>
                <asp:TextBox ID="TextBox3" runat="server" Placeholder="Amount" CssClass="txt"></asp:TextBox>
                <asp:TextBox ID="TextBox4" runat="server" Placeholder="Purpose" CssClass="txt"></asp:TextBox>
                <br />
                <asp:Button ID="Button2" runat="server" Text="Spend Money" CssClass="btn spend" OnClick="Button2_Click" />
            </div>

            <!-- Message Label -->
            <asp:Label ID="lblMessage" runat="server" Text="" CssClass="message"></asp:Label>

            <!-- Filter and Download Section -->
            <div class="section">
                <h3>Filter & Download Transactions</h3>
                <asp:TextBox ID="txtFromDate" runat="server" Placeholder="From Date" CssClass="txt"></asp:TextBox>
                <asp:TextBox ID="txtToDate" runat="server" Placeholder="To Date" CssClass="txt"></asp:TextBox>
                <br />
                <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn add" OnClick="btnFilter_Click" />
                <asp:Button ID="btnDownload" runat="server" Text="Download CSV" CssClass="btn spend" OnClick="btnDownload_Click" />
            </div>

            <!-- Transaction History -->
            <h2>Transaction History</h2>
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CssClass="history-table">
                <Columns>
                    <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy HH:mm}" />
                    <asp:BoundField DataField="Type" HeaderText="Type" />
                    <asp:BoundField DataField="Amount" HeaderText="Amount" />
                    <asp:BoundField DataField="Note" HeaderText="Note" />
                </Columns>
            </asp:GridView>

        </div>
    </form>
</body>
</html>
