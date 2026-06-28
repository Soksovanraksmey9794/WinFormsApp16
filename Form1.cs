using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace WinFormsApp16
{



    public partial class Form1 : Form
    {



        private string connString = "Server=LAPTOP-4Q9KA1L3\\SQLEXPRESS;Database=FinanceTracker;Trusted_Connection=True;TrustServerCertificate=True;";

        // រក្សាទុក UserID របស់អ្នកដែលបាន Login ជោគជ័យ ដើម្បីយកទៅកត់ត្រា Income, Expense, Goals ឱ្យត្រូវតាមគណនីនីមួយៗ
        private int currentUserID = 0;
        private string currentFullName = "";
        public Form1()
        {
            InitializeComponent();
        }

        private void btnexit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Goals_Click(object sender, EventArgs e)
        {

        }

        private void Expense_Click(object sender, EventArgs e)
        {

        }

        private void DataGridView38_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void uiLabel4_Click(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {
            uiTabControlMenu1.SelectedIndex = 1;
        }

        private void uiLabel8_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

            uiTabControlMenu1.SelectedIndex = 0;
            uiTabControlMenu1.SelectedIndex = 0; // បើកផ្ទាំង Login មុនគេ

            // ---- ១. បន្ថែមជម្រើសទៅឱ្យ ComboBox របស់ Income ----
            uiComboBox1.Items.Clear();
            uiComboBox1.Items.Add("Salary");
            uiComboBox1.Items.Add("Business");
            uiComboBox1.Items.Add("Freelance");
            uiComboBox1.Items.Add("Investments");
            uiComboBox1.Items.Add("Other");
            uiComboBox1.SelectedIndex = 0; // ឱ្យវាជ្រើសរើសយកពាក្យទី១ជាលំនាំដើម

            // ---- ២. បន្ថែមជម្រើសទៅឱ្យ ComboBox របស់ Expense ----
            uiComboBox21.Items.Clear();
            uiComboBox21.Items.Add("Food");
            uiComboBox21.Items.Add("Rent");
            uiComboBox21.Items.Add("Utilities");
            uiComboBox21.Items.Add("Transportation");
            uiComboBox21.Items.Add("Entertainment");
            uiComboBox21.Items.Add("Other");
            uiComboBox21.SelectedIndex = 0;




        }

        private void uiButton9_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(uiTextBox4.Text) || string.IsNullOrEmpty(uiTextBox5.Text))
            {
                MessageBox.Show("សូមបំពេញ Username និង Password!", "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "SELECT UserID, FullName FROM Users WHERE Username = @User AND Password = @Pass";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@User", uiTextBox4.Text.Trim()); // uiTextBox4 គឺ Username
                cmd.Parameters.AddWithValue("@Pass", uiTextBox5.Text.Trim()); // uiTextBox5 គឺ Password

                try
                {
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            currentUserID = Convert.ToInt32(reader["UserID"]);
                            currentFullName = reader["FullName"].ToString();

                            MessageBox.Show("ស្វាគមន៍មកកាន់ប្រព័ន្ធ!", "ជោគជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // ប្តូរទៅកាន់ផ្ទាំង Dashboard (Index 2)
                            uiTabControlMenu1.SelectedIndex = 2;
                            name_user.Text = currentFullName; // បង្ហាញឈ្មោះនៅលើ Dashboard

                            // ទាញទិន្នន័យមកបង្ហាញតាមផ្ទាំងនីមួយៗរបស់ User នោះ
                            LoadDashboard();
                            LoadIncomeData();
                            LoadExpenseData();
                            LoadGoalsData();
                        }
                        else
                        {
                            MessageBox.Show("Username ឬ Password មិនត្រឹមត្រូវទេ!", "បរាជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("កំហុសភ្ជាប់ Database៖ " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRegiser31_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(TxtFullname31.Text) || string.IsNullOrEmpty(TxtUsername31.Text) || string.IsNullOrEmpty(TxtPW31.Text))
            {
                MessageBox.Show("សូមបំពេញព័ត៌មានដែលចាំបាច់ (FullName, Username, Password)!", "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (TxtPW31.Text != TxtConfirmPW31.Text)
            {
                MessageBox.Show("លេខសម្ងាត់ទាំងពីរមិនត្រូវគ្នានោះទេ!", "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO Users (FullName, Username, Email, Password, Role) VALUES (@Name, @User, @Email, @Pass, @Role)";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", TxtFullname31.Text.Trim());
                cmd.Parameters.AddWithValue("@User", TxtUsername31.Text.Trim());
                cmd.Parameters.AddWithValue("@Email", TxtEmail31.Text.Trim());
                cmd.Parameters.AddWithValue("@Pass", TxtPW31.Text.Trim());
                

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("បង្កើតគណនីជោគជ័យ!", "ព័ត៌មាន", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear ទិន្នន័យ និងត្រឡប់ទៅផ្ទាំង Login
                    BtnClear31_Click(null, null);
                    uiTabControlMenu1.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Username នេះមានគេប្រើរួចហើយ ឬមានកំហុស៖ " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
        private void LoadDashboard()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    // ១. គណនាសរុបចំណូល
                    SqlCommand cmdInc = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM Income WHERE UserID = @UserID", conn);
                    cmdInc.Parameters.AddWithValue("@UserID", currentUserID);
                    decimal totalIncome = Convert.ToDecimal(cmdInc.ExecuteScalar());

                    // ២. គណនាសរុបចំណាយ
                    SqlCommand cmdExp = new SqlCommand("SELECT ISNULL(SUM(Amount), 0) FROM Expense WHERE UserID = @UserID", conn);
                    cmdExp.Parameters.AddWithValue("@UserID", currentUserID);
                    decimal totalExpense = Convert.ToDecimal(cmdExp.ExecuteScalar());

                    decimal balance = totalIncome - totalExpense;

                    // បង្ហាញតម្លៃលើ Label នីមួយៗ (tic, tep, tbl គឺជាឈ្មោះ Label របស់អ្នក)
                    tic.Text = totalIncome.ToString("$,0.00");
                    tep.Text = totalExpense.ToString("$,0.00");
                    tbl.Text = balance.ToString("$,0.00");

                    // ៣. ទាញប្រវត្តិប្រតិបត្តិការចុងក្រោយ ៥ ជួរ មកបង្ហាញលើ Grid Dashboard
                    string gridQuery = "SELECT * FROM (" +
                                       "SELECT TOP 5 'Income' AS [Type], Source AS [Category], Amount, Date, Description FROM Income WHERE UserID = @UserID ORDER BY Date DESC" +
                                       ") AS Inc " +
                                       "UNION ALL " +
                                       "SELECT * FROM (" +
                                       "SELECT TOP 5 'Expense' AS [Type], Category AS [Category], Amount, Date, Description FROM Expense WHERE UserID = @UserID ORDER BY Date DESC" +
                                       ") AS Exp " +
                                       "ORDER BY Date DESC";

                    SqlCommand cmdGrid = new SqlCommand(gridQuery, conn);
                    cmdGrid.Parameters.AddWithValue("@UserID", currentUserID);

                    SqlDataAdapter da = new SqlDataAdapter(cmdGrid);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // បង្ហាញទៅកាន់ Grid
                    uiDataGridView2.DataSource = dt;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("កំហុសទាញទិន្នន័យ Dashboard៖ " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void uiButton5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(uiTextBox2.Text))
            {
                MessageBox.Show("សូមបញ្ចូលចំនួនទឹកប្រាក់ចំណូល!", "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO Income (UserID, Source, Amount, Date, Description) VALUES (@UserID, @Source, @Amount, @Date, @Desc)";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserID);
                cmd.Parameters.AddWithValue("@Source", uiComboBox1.SelectedItem?.ToString() ?? "Other");
                cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(uiTextBox2.Text)); // uiTextBox2 គឺចំនូនទឹកប្រាក់
                cmd.Parameters.AddWithValue("@Date", uiDatePicker2.Value);
                cmd.Parameters.AddWithValue("@Desc", uiTextBox3.Text); // uiTextBox3 គឺការពិពណ៌នា

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("រក្សាទុកទិន្នន័យចំណូលរួចរាល់!", "ជោគជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadIncomeData(); // ទាញទិន្នន័យអាប់ដេតចូល Grid វិញ
                    LoadDashboard();  // អាប់ដេតតម្លៃលេខលើ Dashboard
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }

        }

        private void LoadIncomeData()
        {
            string query = "SELECT IncomeID, Source, Amount, Date, Description FROM Income WHERE UserID = @UserID ORDER BY Date DESC";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                uiDataGridView3.DataSource = dt; // បង្ហាញលើ Grid ផ្ទាំង Income
            }
        }

        private void uiButton21_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(uiTextBox22.Text))
            {
                MessageBox.Show("សូមបញ្ចូលចំនួនទឹកប្រាក់ចំណាយ!", "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ក្នុង Table Expense របស់អ្នកមាន Column: ExpenseName, Category, Amount, Date, Description
            string query = "INSERT INTO Expense (UserID, ExpenseName, Category, Amount, Date, Description) " +
                           "VALUES (@UserID, @ExpenseName, @Cat, @Amount, @Date, @Desc)";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserID);
                cmd.Parameters.AddWithValue("@ExpenseName", uiTextBox21.Text); // uiTextBox21 សម្រាប់ឈ្មោះចំណាយ
                cmd.Parameters.AddWithValue("@Cat", uiComboBox21.SelectedItem?.ToString() ?? "Other");
                cmd.Parameters.AddWithValue("@Amount", Convert.ToDecimal(uiTextBox22.Text)); // uiTextBox22 សម្រាប់ទឹកប្រាក់
                cmd.Parameters.AddWithValue("@Date", uiDatePicker21.Value);
                cmd.Parameters.AddWithValue("@Desc", uiTextBox23.Text); // uiTextBox23 សម្រាប់ពិពណ៌នា

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("រក្សាទុកទិន្នន័យចំណាយរួចរាល់!", "ជោគជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadExpenseData(); // អាប់ដេត Grid ផ្ទាំង Expense
                    LoadDashboard();   // អាប់ដេត Dashboard
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
        private void LoadExpenseData()
        {
            string query = "SELECT ExpenseID, ExpenseName, Category, Amount, Date, Description FROM Expense WHERE UserID = @UserID ORDER BY Date DESC";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                uiDataGridView21.DataSource = dt; // បង្ហាញលើ Grid ផ្ទាំង Expense
            }
        }

        private void uiButton1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("សូមបំពេញឈ្មោះគោលដៅ និងទឹកប្រាក់កំណត់!", "ព្រមាន", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string query = "INSERT INTO Goals (UserID, GoalName, TargetAmount, SavedAmount) VALUES (@UserID, @Name, @Target, @Saved)";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserID);
                cmd.Parameters.AddWithValue("@Name", textBox1.Text);   // textBox1: ឈ្មោះគោលដៅសន្សំ
                cmd.Parameters.AddWithValue("@Target", Convert.ToDecimal(textBox2.Text)); // textBox2: ទឹកប្រាក់គោលដៅសរុប
                cmd.Parameters.AddWithValue("@Saved", string.IsNullOrEmpty(textBox3.Text) ? 0 : Convert.ToDecimal(textBox3.Text));  // textBox3: លុយសន្សំបានបច្ចុប្បន្ន

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("បង្កើតគោលដៅសន្សំជោគជ័យ!", "ជោគជ័យ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGoalsData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void LoadGoalsData()
        {
            // ទាញទិន្នន័យព្រមទាំងគណនាភាគរយ (%) សម្រេចបាន Progress រួចបង្ហាញលើ Grid
            string query = "SELECT GoalID, GoalName, TargetAmount, SavedAmount, " +
                           "CAST(CASE WHEN TargetAmount = 0 THEN 0 ELSE (SavedAmount / TargetAmount) * 100 END AS DECIMAL(5,2)) AS [Progress %] " +
                           "FROM Goals WHERE UserID = @UserID";
            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserID", currentUserID);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                uiDataGridView1.DataSource = dt; // បង្ហាញលើ Grid ផ្ទាំង Goals
            }
        }

    }
}

