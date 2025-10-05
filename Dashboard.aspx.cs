using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;

namespace Expense_Tracker
{
    public partial class Dashboard : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection("Data Source=.\\sqlexpress;Initial Catalog=Expense_Tracker;Integrated Security=True;");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadTransactions();
                UpdateStats();
            }
        }

        private void LoadTransactions(DateTime? fromDate = null, DateTime? toDate = null)
        {
            string query = "SELECT Date, Type, Amount, Note FROM Transactions";

            if (fromDate.HasValue && toDate.HasValue)
            {
                query += " WHERE Date BETWEEN @FromDate AND @ToDate";
            }
            else
            {
                // Default: show only this month's transactions
                query += " WHERE MONTH(Date) = MONTH(GETDATE()) AND YEAR(Date) = YEAR(GETDATE())";
            }

            query += " ORDER BY TransactionID DESC";

            SqlDataAdapter da = new SqlDataAdapter(query, con);

            if (fromDate.HasValue && toDate.HasValue)
            {
                da.SelectCommand.Parameters.AddWithValue("@FromDate", fromDate.Value);
                da.SelectCommand.Parameters.AddWithValue("@ToDate", toDate.Value);
            }

            DataTable dt = new DataTable();
            da.Fill(dt);
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }


        private void UpdateStats()
        {
            // 1️⃣ Monthly stats for MoneyReceived and MoneySpent
            string monthlyQuery = @"
            SELECT 
                SUM(CASE WHEN Type='Add' THEN Amount ELSE 0 END) AS MoneyReceived,
                SUM(CASE WHEN Type='Spent' THEN Amount ELSE 0 END) AS MoneySpent
            FROM Transactions
                WHERE MONTH(Date) = MONTH(GETDATE()) 
                AND YEAR(Date) = YEAR(GETDATE())
            ";

            SqlCommand cmdMonthly = new SqlCommand(monthlyQuery, con);
            con.Open();
            SqlDataReader dr = cmdMonthly.ExecuteReader();
            if (dr.Read())
            {
                decimal received = dr["MoneyReceived"] != DBNull.Value ? Convert.ToDecimal(dr["MoneyReceived"]) : 0;
                decimal spent = dr["MoneySpent"] != DBNull.Value ? Convert.ToDecimal(dr["MoneySpent"]) : 0;

                lblMoneyReceived.Text = received.ToString("0.00");
                lblMoneySpent.Text = spent.ToString("0.00");
            }
            dr.Close();

            // 2️⃣ Cumulative balance (all-time)
            string balanceQuery = @"
            SELECT 
                SUM(CASE WHEN Type='Add' THEN Amount ELSE 0 END) -
                SUM(CASE WHEN Type='Spent' THEN Amount ELSE 0 END) AS Balance
            FROM Transactions
            ";

            SqlCommand cmdBalance = new SqlCommand(balanceQuery, con);
            object result = cmdBalance.ExecuteScalar();
            decimal balance = result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            lblBalance.Text = balance.ToString("0.00");

            con.Close();
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                decimal amount;
                if (!decimal.TryParse(TextBox1.Text, out amount) || amount <= 0)
                {
                    lblMessage.Text = "Please enter a valid positive amount.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    TextBox1.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(TextBox2.Text))
                {
                    lblMessage.Text = "Please enter a 'From' name.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    TextBox2.Focus();
                    return;
                }

                string fromName = TextBox2.Text.Trim();

                string query = "INSERT INTO Transactions (Date, Type, Amount, Note) VALUES (@Date, @Type, @Amount, @Note)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Type", "Add");
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@Note", fromName);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                LoadTransactions();
                UpdateStats();

                TextBox1.Text = "";
                TextBox2.Text = "";
                TextBox1.Focus();

                lblMessage.Text = "Money added successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                decimal amount;
                if (!decimal.TryParse(TextBox3.Text, out amount) || amount <= 0)
                {
                    lblMessage.Text = "Please enter a valid positive amount.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    TextBox3.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(TextBox4.Text))
                {
                    lblMessage.Text = "Please enter a purpose.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    TextBox4.Focus();
                    return;
                }

                decimal currentBalance = Convert.ToDecimal(lblBalance.Text);
                if (amount > currentBalance)
                {
                    lblMessage.Text = "Cannot spend more than current balance.";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    TextBox3.Focus();
                    return;
                }

                string note = TextBox4.Text.Trim();

                string query = "INSERT INTO Transactions (Date, Type, Amount, Note) VALUES (@Date, @Type, @Amount, @Note)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Type", "Spent");
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@Note", note);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                LoadTransactions();
                UpdateStats();

                TextBox3.Text = "";
                TextBox4.Text = "";
                TextBox3.Focus();

                lblMessage.Text = "Money spent successfully!";
                lblMessage.ForeColor = System.Drawing.Color.Green;

                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        // Filter GridView by date
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            DateTime fromDate, toDate;
            if (!DateTime.TryParse(txtFromDate.Text, out fromDate) ||
                !DateTime.TryParse(txtToDate.Text, out toDate))
            {
                lblMessage.Text = "Enter valid From and To dates.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            toDate = toDate.AddDays(1).AddSeconds(-1);
            LoadTransactions(fromDate, toDate);

            lblMessage.Text = $"Showing transactions from {fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy}.";
            lblMessage.ForeColor = System.Drawing.Color.Green;
        }

        // Download CSV by date range
        protected void btnDownload_Click(object sender, EventArgs e)
        {
            DateTime fromDate, toDate;
            if (!DateTime.TryParse(txtFromDate.Text, out fromDate) ||
                !DateTime.TryParse(txtToDate.Text, out toDate))
            {
                lblMessage.Text = "Enter valid From and To dates.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            toDate = toDate.AddDays(1).AddSeconds(-1);

            string query = "SELECT Date, Type, Amount, Note FROM Transactions WHERE Date BETWEEN @FromDate AND @ToDate ORDER BY Date DESC";
            SqlDataAdapter da = new SqlDataAdapter(query, con);
            da.SelectCommand.Parameters.AddWithValue("@FromDate", fromDate);
            da.SelectCommand.Parameters.AddWithValue("@ToDate", toDate);

            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                StringBuilder csv = new StringBuilder();
                // Header
                csv.AppendLine("Date,Type,Amount,Note");

                // Rows
                foreach (DataRow row in dt.Rows)
                {
                    string date = ((DateTime)row["Date"]).ToString("dd-MMM-yyyy HH:mm");
                    string type = row["Type"].ToString();
                    string amount = row["Amount"].ToString();
                    string note = row["Note"].ToString().Replace(",", " ");

                    csv.AppendLine($"{date},{type},{amount},{note}");
                }

                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment;filename=Transactions.csv");
                Response.Charset = "";
                Response.ContentType = "text/csv";
                Response.Output.Write(csv.ToString());
                Response.Flush();
                Response.End();

                // Clear date textboxes after download
                txtFromDate.Text = "";
                txtToDate.Text = "";
            }
            else
            {
                lblMessage.Text = "No transactions found for this date range.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }
    }
}
