using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace WinFormsApp16
{
    public partial class Form1 : Form
    {
        // ─── Connection String ────────────────────────────────────────────────
        private readonly string connString =
            "Server=LAPTOP-4Q9KA1L3\\SQLEXPRESS;" +
            "Database=FinanceTracker;" +
            "Trusted_Connection=True;" +
            "TrustServerCertificate=True;";

        // ─── Current logged-in user ───────────────────────────────────────────
        private int currentUserID = 0;
        private string currentFullName = "";

        // ─── Constructor ──────────────────────────────────────────────────────
        public Form1()
        {
            InitializeComponent();
        }

        // =====================================================================
        //  FORM LOAD
        // =====================================================================
        private void Form1_Load(object sender, EventArgs e)
        {
            
            this.WindowState = FormWindowState.Maximized;

            // Show Login tab first
            uiTabControlMenu1.SelectedIndex = 0;

            // Populate Income source combo
            uiComboBox1.Items.Clear();
            uiComboBox1.Items.AddRange(new object[] {
                "Salary", "Business", "Freelance", "Investments", "Other"
            });
            uiComboBox1.SelectedIndex = 0;

            // Populate Expense category combo
            uiComboBox21.Items.Clear();
            uiComboBox21.Items.AddRange(new object[] {
                "Food", "Shopping", "Transport", "Education", "Other"
            });
            uiComboBox21.SelectedIndex = 0;

            // Set date pickers to today
            uiDatePicker2.Value = DateTime.Today;
            uiDatePicker21.Value = DateTime.Today;

            // Wire CellClick events
            uiDataGridView3.CellClick += uiDataGridView3_CellClick;
            uiDataGridView21.CellClick += uiDataGridView21_CellClick;
            uiDataGridView1.CellClick += uiDataGridView1_CellClick;

            // Wire tab-change event so Reports loads automatically
            uiTabControlMenu1.SelectedIndexChanged += uiTabControlMenu1_SelectedIndexChanged;
        }

        // =====================================================================
        //  TAB CHANGED  →  load Reports when user opens that tab (index 6)
        // =====================================================================
        private void uiTabControlMenu1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (uiTabControlMenu1.SelectedIndex == 6 && currentUserID > 0)
                LoadReports();
        }

        // =====================================================================
        //  EXIT
        // =====================================================================
        private void btnexit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("តើអ្នកពិតជាចង់ចេញពីប្រព័ន្ធ?", "បញ្ជាក់",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Application.Exit();
        }

        // =====================================================================
        //  LOGIN
        // =====================================================================
        private void uiButton9_Click(object sender, EventArgs e)
        {
            string username = uiTextBox4.Text.Trim();
            string password = uiTextBox5.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("សូមបំពេញ Username និង Password!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "SELECT UserID, FullName FROM Users " +
                                 "WHERE Username = @User AND Password = @Pass";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@User", username);
                        cmd.Parameters.AddWithValue("@Pass", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentUserID = Convert.ToInt32(reader["UserID"]);
                                currentFullName = reader["FullName"].ToString();

                                uiTextBox5.Text = "";  // Clear password for security

                                name_user.Text = currentFullName;
                                uiTabControlMenu1.SelectedIndex = 2; // Go to Dashboard

                                LoadDashboard();
                                LoadIncomeData();
                                LoadExpenseData();
                                LoadGoalsData();

                                MessageBox.Show($"ស្វាគមន៍មក {currentFullName}!",
                                    "ជោគជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Username ឬ Password មិនត្រឹមត្រូវ!",
                                    "បរាជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                uiTextBox5.Text = "";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("មិនអាចភ្ជាប់ Database បាន:\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Navigate to Register page
        private void label25_Click(object sender, EventArgs e)
        {
            uiTabControlMenu1.SelectedIndex = 1;
        }

        // =====================================================================
        //  REGISTER
        // =====================================================================
        private void BtnRegiser31_Click(object sender, EventArgs e)
        {
            string fullname = TxtFullname31.Text.Trim();
            string username = TxtUsername31.Text.Trim();
            string email = TxtEmail31.Text.Trim();
            string password = TxtPW31.Text.Trim();
            string confirm = TxtConfirmPW31.Text.Trim();

            if (string.IsNullOrEmpty(fullname) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                MessageBox.Show("សូមបំពេញ FullName, Username, Password!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Password និង Confirm Password មិនដូចគ្នា!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    // Check duplicate username
                    using (SqlCommand checkCmd = new SqlCommand(
                        "SELECT COUNT(*) FROM Users WHERE Username = @User", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@User", username);
                        if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Username នេះមានគេប្រើរួចហើយ!",
                                "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string sql = "INSERT INTO Users (FullName, Username, Email, Password, Role) " +
                                 "VALUES (@Name, @User, @Email, @Pass, @Role)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", fullname);
                        cmd.Parameters.AddWithValue("@User", username);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@Pass", password);
                        cmd.Parameters.AddWithValue("@Role", "User");
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("បង្កើតគណនីជោគជ័យ! សូម Login ចូល។",
                    "ជោគជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                BtnClear31_Click(null, null);
                uiTabControlMenu1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("មានបញ្ហា: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear31_Click(object sender, EventArgs e)
        {
            TxtFullname31.Text = "";
            TxtUsername31.Text = "";
            TxtEmail31.Text = "";
            TxtPW31.Text = "";
            TxtConfirmPW31.Text = "";
        }

        private void TxtBacktoLogin31_Click(object sender, EventArgs e)
        {
            uiTabControlMenu1.SelectedIndex = 0;
        }

        // =====================================================================
        //  DASHBOARD
        // =====================================================================
        private void LoadDashboard()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    decimal totalIncome = 0;
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT ISNULL(SUM(Amount),0) FROM Income WHERE UserID=@UID", conn))
                    {
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        totalIncome = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    decimal totalExpense = 0;
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT ISNULL(SUM(Amount),0) FROM Expense WHERE UserID=@UID", conn))
                    {
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        totalExpense = Convert.ToDecimal(cmd.ExecuteScalar());
                    }

                    tic.Text = "$" + totalIncome.ToString("N2");
                    tep.Text = "$" + totalExpense.ToString("N2");
                    tbl.Text = "$" + (totalIncome - totalExpense).ToString("N2");

                    // Recent transactions (last 5 Income + last 5 Expense)
                    string gridSql =
                        "SELECT TOP 10 * FROM (" +
                        "  SELECT TOP 5 'Income' AS [Type], Source AS [Category]," +
                        "         Amount, CAST(Date AS NVARCHAR) AS [Date], Description" +
                        "  FROM Income WHERE UserID=@UID1 ORDER BY Date DESC" +
                        ") A " +
                        "UNION ALL " +
                        "SELECT TOP 10 * FROM (" +
                        "  SELECT TOP 5 'Expense' AS [Type], Category," +
                        "         Amount, CAST(Date AS NVARCHAR) AS [Date], Description" +
                        "  FROM Expense WHERE UserID=@UID2 ORDER BY Date DESC" +
                        ") B " +
                        "ORDER BY [Date] DESC";

                    using (SqlCommand cmd = new SqlCommand(gridSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UID1", currentUserID);
                        cmd.Parameters.AddWithValue("@UID2", currentUserID);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        uiDataGridView2.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dashboard Error: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // =====================================================================
        //  INCOME  —  INSERT / UPDATE / DELETE / LOAD
        // =====================================================================
        private void uiButton5_Click(object sender, EventArgs e)   // INSERT
        {
            if (!ValidateIncomeForm()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Income (UserID, Source, Amount, Date, Description) " +
                                 "VALUES (@UID, @Source, @Amount, @Date, @Desc)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.Parameters.AddWithValue("@Source", uiComboBox1.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Amount", decimal.Parse(uiTextBox2.Text));
                        cmd.Parameters.AddWithValue("@Date", uiDatePicker2.Value.Date);
                        cmd.Parameters.AddWithValue("@Desc", uiTextBox3.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("បន្ថែមចំណូលរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearIncomeForm();
                LoadIncomeData();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton6_Click(object sender, EventArgs e)   // UPDATE
        {
            if (uiDataGridView3.CurrentRow == null)
            {
                MessageBox.Show("សូមជ្រើសរើសជួរដែលចង់កែ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateIncomeForm()) return;

            int incomeID = Convert.ToInt32(uiDataGridView3.CurrentRow.Cells["IncomeID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "UPDATE Income SET Source=@Source, Amount=@Amount, " +
                                 "Date=@Date, Description=@Desc " +
                                 "WHERE IncomeID=@ID AND UserID=@UID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Source", uiComboBox1.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Amount", decimal.Parse(uiTextBox2.Text));
                        cmd.Parameters.AddWithValue("@Date", uiDatePicker2.Value.Date);
                        cmd.Parameters.AddWithValue("@Desc", uiTextBox3.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID", incomeID);
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("កែប្រែចំណូលរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearIncomeForm();
                LoadIncomeData();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton7_Click(object sender, EventArgs e)   // DELETE
        {
            if (uiDataGridView3.CurrentRow == null)
            {
                MessageBox.Show("សូមជ្រើសរើសជួរដែលចង់លុប!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("តើអ្នកពិតជាចង់លុបចំណូលនេះ?", "បញ្ជាក់",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int incomeID = Convert.ToInt32(uiDataGridView3.CurrentRow.Cells["IncomeID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Income WHERE IncomeID=@ID AND UserID=@UID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", incomeID);
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("លុបចំណូលរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearIncomeForm();
                LoadIncomeData();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadIncomeData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT IncomeID, Source, Amount, Date, Description " +
                                 "FROM Income WHERE UserID=@UID ORDER BY Date DESC";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UID", currentUserID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    uiDataGridView3.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Income Load Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiDataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = uiDataGridView3.Rows[e.RowIndex];

            uiTextBox2.Text = row.Cells["Amount"].Value?.ToString() ?? "";
            uiTextBox3.Text = row.Cells["Description"].Value?.ToString() ?? "";

            string source = row.Cells["Source"].Value?.ToString() ?? "";
            int idx = uiComboBox1.Items.IndexOf(source);
            uiComboBox1.SelectedIndex = idx >= 0 ? idx : 0;

            if (DateTime.TryParse(row.Cells["Date"].Value?.ToString(), out DateTime d))
                uiDatePicker2.Value = d;
        }

        private bool ValidateIncomeForm()
        {
            if (string.IsNullOrEmpty(uiTextBox2.Text))
            {
                MessageBox.Show("សូមបញ្ចូលចំនួនទឹកប្រាក់!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(uiTextBox2.Text, out decimal val) || val <= 0)
            {
                MessageBox.Show("ចំនួនទឹកប្រាក់មិនត្រឹមត្រូវ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void ClearIncomeForm()
        {
            uiTextBox1.Text = "";
            uiTextBox2.Text = "";
            uiTextBox3.Text = "";
            uiComboBox1.SelectedIndex = 0;
            uiDatePicker2.Value = DateTime.Today;
        }

        // =====================================================================
        //  EXPENSE  —  INSERT / UPDATE / DELETE / LOAD
        // =====================================================================
        private void uiButton21_Click(object sender, EventArgs e)  // INSERT
        {
            if (!ValidateExpenseForm()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Expense (UserID, ExpenseName, Category, Amount, Date, Description) " +
                                 "VALUES (@UID, @Name, @Cat, @Amount, @Date, @Desc)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.Parameters.AddWithValue("@Name", uiTextBox21.Text.Trim());
                        cmd.Parameters.AddWithValue("@Cat", uiComboBox21.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Amount", decimal.Parse(uiTextBox22.Text));
                        cmd.Parameters.AddWithValue("@Date", uiDatePicker21.Value.Date);
                        cmd.Parameters.AddWithValue("@Desc", uiTextBox23.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("បន្ថែមចំណាយរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearExpenseForm();
                LoadExpenseData();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton22_Click(object sender, EventArgs e)  // UPDATE
        {
            if (uiDataGridView21.CurrentRow == null)
            {
                MessageBox.Show("សូមជ្រើសរើសជួរដែលចង់កែ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateExpenseForm()) return;

            int expID = Convert.ToInt32(uiDataGridView21.CurrentRow.Cells["ExpenseID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "UPDATE Expense SET ExpenseName=@Name, Category=@Cat, " +
                                 "Amount=@Amount, Date=@Date, Description=@Desc " +
                                 "WHERE ExpenseID=@ID AND UserID=@UID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", uiTextBox21.Text.Trim());
                        cmd.Parameters.AddWithValue("@Cat", uiComboBox21.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@Amount", decimal.Parse(uiTextBox22.Text));
                        cmd.Parameters.AddWithValue("@Date", uiDatePicker21.Value.Date);
                        cmd.Parameters.AddWithValue("@Desc", uiTextBox23.Text.Trim());
                        cmd.Parameters.AddWithValue("@ID", expID);
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("កែប្រែចំណាយរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearExpenseForm();
                LoadExpenseData();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton23_Click(object sender, EventArgs e)  // DELETE
        {
            if (uiDataGridView21.CurrentRow == null)
            {
                MessageBox.Show("សូមជ្រើសរើសជួរដែលចង់លុប!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("តើអ្នកពិតជាចង់លុបចំណាយនេះ?", "បញ្ជាក់",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int expID = Convert.ToInt32(uiDataGridView21.CurrentRow.Cells["ExpenseID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Expense WHERE ExpenseID=@ID AND UserID=@UID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", expID);
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("លុបចំណាយរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearExpenseForm();
                LoadExpenseData();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadExpenseData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "SELECT ExpenseID, ExpenseName, Category, Amount, Date, Description " +
                                 "FROM Expense WHERE UserID=@UID ORDER BY Date DESC";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UID", currentUserID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    uiDataGridView21.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Expense Load Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiDataGridView21_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = uiDataGridView21.Rows[e.RowIndex];

            uiTextBox21.Text = row.Cells["ExpenseName"].Value?.ToString() ?? "";
            uiTextBox22.Text = row.Cells["Amount"].Value?.ToString() ?? "";
            uiTextBox23.Text = row.Cells["Description"].Value?.ToString() ?? "";

            string cat = row.Cells["Category"].Value?.ToString() ?? "";
            int idx = uiComboBox21.Items.IndexOf(cat);
            uiComboBox21.SelectedIndex = idx >= 0 ? idx : 0;

            if (DateTime.TryParse(row.Cells["Date"].Value?.ToString(), out DateTime d))
                uiDatePicker21.Value = d;
        }

        private bool ValidateExpenseForm()
        {
            if (string.IsNullOrEmpty(uiTextBox21.Text))
            {
                MessageBox.Show("សូមបញ្ចូលឈ្មោះចំណាយ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrEmpty(uiTextBox22.Text) ||
                !decimal.TryParse(uiTextBox22.Text, out decimal val) || val <= 0)
            {
                MessageBox.Show("ចំនួនទឹកប្រាក់មិនត្រឹមត្រូវ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void ClearExpenseForm()
        {
            uiTextBox21.Text = "";
            uiTextBox22.Text = "";
            uiTextBox23.Text = "";
            uiComboBox21.SelectedIndex = 0;
            uiDatePicker21.Value = DateTime.Today;
        }

        // =====================================================================
        //  GOALS  —  INSERT / UPDATE / DELETE / LOAD
        // =====================================================================
        private void uiButton1_Click(object sender, EventArgs e)   // INSERT
        {
            if (!ValidateGoalForm()) return;

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Goals (UserID, GoalName, TargetAmount, SavedAmount) " +
                                 "VALUES (@UID, @Name, @Target, @Saved)";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.Parameters.AddWithValue("@Name", textBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@Target", decimal.Parse(textBox2.Text));
                        cmd.Parameters.AddWithValue("@Saved",
                            string.IsNullOrEmpty(textBox3.Text) ? 0m : decimal.Parse(textBox3.Text));
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("បន្ថែមគោលដៅរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearGoalForm();
                LoadGoalsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton2_Click(object sender, EventArgs e)   // UPDATE
        {
            if (uiDataGridView1.CurrentRow == null)
            {
                MessageBox.Show("សូមជ្រើសរើសគោលដៅដែលចង់កែ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateGoalForm()) return;

            int goalID = Convert.ToInt32(uiDataGridView1.CurrentRow.Cells["GoalID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "UPDATE Goals SET GoalName=@Name, TargetAmount=@Target, " +
                                 "SavedAmount=@Saved WHERE GoalID=@ID AND UserID=@UID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", textBox1.Text.Trim());
                        cmd.Parameters.AddWithValue("@Target", decimal.Parse(textBox2.Text));
                        cmd.Parameters.AddWithValue("@Saved",
                            string.IsNullOrEmpty(textBox3.Text) ? 0m : decimal.Parse(textBox3.Text));
                        cmd.Parameters.AddWithValue("@ID", goalID);
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("កែប្រែគោលដៅរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearGoalForm();
                LoadGoalsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiButton3_Click(object sender, EventArgs e)   // DELETE
        {
            if (uiDataGridView1.CurrentRow == null)
            {
                MessageBox.Show("សូមជ្រើសរើសគោលដៅដែលចង់លុប!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("តើអ្នកពិតជាចង់លុបគោលដៅនេះ?", "បញ្ជាក់",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int goalID = Convert.ToInt32(uiDataGridView1.CurrentRow.Cells["GoalID"].Value);

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string sql = "DELETE FROM Goals WHERE GoalID=@ID AND UserID=@UID";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ID", goalID);
                        cmd.Parameters.AddWithValue("@UID", currentUserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("លុបគោលដៅរួចរាល់!", "ជោគជ័យ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearGoalForm();
                LoadGoalsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGoalsData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    // FIX: ប្រើ DECIMAL(10,2) + cap ៩៩ % ដើម្បីចៀសវាង overflow
                    string sql =
                        "SELECT GoalID, GoalName, TargetAmount, SavedAmount, " +
                        "CAST(CASE WHEN TargetAmount = 0 THEN 0 " +
                        "ELSE CASE WHEN (SavedAmount * 100.0 / TargetAmount) > 100 THEN 100.00 " +
                        "ELSE ROUND(SavedAmount * 100.0 / TargetAmount, 2) END END " +
                        "AS DECIMAL(10,2)) AS [Progress %] " +
                        "FROM Goals WHERE UserID=@UID ORDER BY GoalID DESC";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@UID", currentUserID);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    uiDataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Goals Load Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void uiDataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = uiDataGridView1.Rows[e.RowIndex];

            textBox1.Text = row.Cells["GoalName"].Value?.ToString() ?? "";
            textBox2.Text = row.Cells["TargetAmount"].Value?.ToString() ?? "";
            textBox3.Text = row.Cells["SavedAmount"].Value?.ToString() ?? "";
            textBox4.Text = row.Cells["Progress %"].Value?.ToString() ?? "";
        }

        private bool ValidateGoalForm()
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("សូមបញ្ចូលឈ្មោះគោលដៅ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrEmpty(textBox2.Text) ||
                !decimal.TryParse(textBox2.Text, out decimal t) || t <= 0)
            {
                MessageBox.Show("ទឹកប្រាក់គោលដៅមិនត្រឹមត្រូវ!",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!string.IsNullOrEmpty(textBox3.Text) &&
                (!decimal.TryParse(textBox3.Text, out decimal s) || s < 0))
            {
                MessageBox.Show("ទឹកប្រាក់សន្សំមិនត្រឹមត្រូវ! សូមបញ្ចូលតែលេខ។",
                    "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void ClearGoalForm()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
        }

        // =====================================================================
        //  REPORTS
        // =====================================================================
        private void LoadReports()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();

                    decimal inc = GetScalar(conn,
                        "SELECT ISNULL(SUM(Amount),0) FROM Income WHERE UserID=@UID");
                    totalincome.Text = "$" + inc.ToString("N2");

                    decimal exp = GetScalar(conn,
                        "SELECT ISNULL(SUM(Amount),0) FROM Expense WHERE UserID=@UID");
                    totalexpense.Text = "$" + exp.ToString("N2");

                    balance2.Text = "$" + (inc - exp).ToString("N2");

                    SetCategory(conn, exp, "Food", progressBarfood, moneyfood, percentfood);
                    SetCategory(conn, exp, "Shopping", progressBarshopping, moneyshopping, percentshopping);
                    SetCategory(conn, exp, "Transport", progressBartransport, moneytransport, percenttransport);
                    SetCategory(conn, exp, "Education", progressBareducation, moneyeducation, percenteducation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reports Error: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private decimal GetScalar(SqlConnection conn, string sql)
        {
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@UID", currentUserID);
                return Convert.ToDecimal(cmd.ExecuteScalar());
            }
        }

        private void SetCategory(SqlConnection conn, decimal totalExp,
            string category, ProgressBar bar,
            Sunny.UI.UILabel moneyLbl, Sunny.UI.UILabel pctLbl)
        {
            // Use parameterized query instead of string interpolation
            decimal amt;
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(SUM(Amount),0) FROM Expense WHERE UserID=@UID AND Category=@Cat", conn))
            {
                cmd.Parameters.AddWithValue("@UID", currentUserID);
                cmd.Parameters.AddWithValue("@Cat", category);
                amt = Convert.ToDecimal(cmd.ExecuteScalar());
            }

            moneyLbl.Text = "$" + amt.ToString("N2");

            if (totalExp > 0)
            {
                int maxVal = (int)totalExp;
                bar.Maximum = maxVal > 0 ? maxVal : 1;
                bar.Value = Math.Min((int)amt, bar.Maximum);
                pctLbl.Text = ((amt / totalExp) * 100).ToString("0.0") + "%";
            }
            else
            {
                bar.Value = 0;
                pctLbl.Text = "0.0%";
            }
        }

        // Stub — tab click wired in Designer
        private void Reports_Click(object sender, EventArgs e) { }

        // =====================================================================
        //  EMPTY STUBS
        // =====================================================================
        private void Goals_Click(object sender, EventArgs e) { }
        private void Expense_Click(object sender, EventArgs e) { }
        private void Dashboard_Click(object sender, EventArgs e) { }
        private void uiLabel4_Click(object sender, EventArgs e) { }
        private void uiLabel6_Click(object sender, EventArgs e) { }
        private void uiLabel8_Click(object sender, EventArgs e) { }
        private void totalincome_Click(object sender, EventArgs e) { }
        private void DataGridView38_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
    }
}