using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace trial // IMPORTANT: Replace with your actual project namespace
{
    public partial class dep : Form
    {
        // IMPORTANT: Define your connection string here or get it from a config file.
        // Replace "YourDatabaseName" with your actual database name.
        private string connectionString = @"Data Source=DESKTOP-HDIN6H2\SQLEXPRESS01;Initial Catalog=proj;Integrated Security=True";

        public dep()
        {
            InitializeComponent();
        }
                 private Form previousForm;

public dep(Form callingForm)
{
    InitializeComponent();
    previousForm = callingForm;
}
        private void dep_Load(object sender, EventArgs e)
        {
            LoadDepartmentsData();
            ClearDepartmentFields(); // Clear fields on load for fresh entry
            LoadDepartmentsComboBox(); // Load any combo boxes that depend on departments
        }

        // Your provided Department Management Methods:
        private void LoadDepartmentsData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT DepartmentID, DepartmentName, DepartmentCode FROM Departments ORDER BY DepartmentID";

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridViewDepartments.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading department data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearDepartmentFields()
        {
            // Assuming you have TextBoxes named txtDepartmentID, txtDepartmentName, txtDepartmentCode
            txtDepartmentID.Clear();
            txtDepartmentName.Clear();
            txtDepartmentCode.Clear();
            txtDepartmentID.ReadOnly = false; // Allow editing DepartmentID for new entries
            txtDepartmentID.Focus(); // Set focus to the first input field
        }

        // ASSUMPTION: You have a method to load department data into a ComboBox,
        // which was called from AdminForm, so it needs to be defined here.
       

        // Your provided Department Event Handlers:
        private void btnLoadDepartments_Click(object sender, EventArgs e)
        {
            LoadDepartmentsData();
            ClearDepartmentFields();
            LoadDepartmentsComboBox(); // Refresh comboboxes too
            MessageBox.Show("Department data refreshed.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadDepartmentsComboBox()
        { }
        private void btnAddDepartment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDepartmentID.Text) || string.IsNullOrWhiteSpace(txtDepartmentName.Text))
            {
                MessageBox.Show("Please fill in both Department ID and Department Name.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int departmentID;
            if (!int.TryParse(txtDepartmentID.Text.Trim(), out departmentID))
            {
                MessageBox.Show("Invalid Department ID. Please enter a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDepartmentID.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Departments (DepartmentID, DepartmentName,DepartmentCode) VALUES (@DepartmentID, @DepartmentName,@DepartmentCode)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                        cmd.Parameters.AddWithValue("@DepartmentName", txtDepartmentName.Text.Trim());
                        cmd.Parameters.AddWithValue("@DepartmentCode", txtDepartmentCode.Text.Trim());
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Department added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDepartmentsData();
                        ClearDepartmentFields();
                        LoadDepartmentsComboBox(); // Refresh comboboxes
                    }
                }
                catch (SqlException sqlex)
                {
                    if (sqlex.Number == 2627 || sqlex.Number == 2601) // PK or Unique constraint violation
                    {
                        MessageBox.Show("Error: A department with this ID or name already exists. Please use unique values.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Database Error adding department: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please enter a valid numeric value for Department ID.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdateDepartment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDepartmentID.Text))
            {
                MessageBox.Show("Please provide a Department ID to update or select a department from the list.", "Missing Department ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int departmentID;
            if (!int.TryParse(txtDepartmentID.Text.Trim(), out departmentID))
            {
                MessageBox.Show("Invalid Department ID. Please enter a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtDepartmentID.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Departments SET DepartmentName = @DepartmentName, DepartmentCode = @DepartmentCode WHERE DepartmentID = @DepartmentID;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentName", txtDepartmentName.Text.Trim());
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentID);
                        cmd.Parameters.AddWithValue("@DepartmentCode", txtDepartmentCode.Text.Trim());

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("Department updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDepartmentsData();
                            ClearDepartmentFields();
                            LoadDepartmentsComboBox(); // Refresh comboboxes
                        }
                        else
                        {
                            MessageBox.Show("Update failed. Department with the provided ID not found.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (SqlException sqlex)
                {
                    if (sqlex.Number == 2601) // Unique constraint violation (e.g., if DepartmentName must be unique)
                    {
                        MessageBox.Show("A department with this name already exists. Please use a unique department name.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Database Error updating department: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDeleteDepartment_Click(object sender, EventArgs e)
        {
            if (dataGridViewDepartments.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a department from the table to delete.", "No Department Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int departmentIdToDelete = Convert.ToInt32(dataGridViewDepartments.SelectedRows[0].Cells["DepartmentID"].Value);

            DialogResult confirm = MessageBox.Show(string.Format("Are you sure you want to delete Department ID: {0}?\nThis action cannot be undone.", departmentIdToDelete),
                                        "Confirm Delete",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Warning);

            if (confirm == DialogResult.No)
            {
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // IMPORTANT: If DepartmentID is a foreign key in Students or Courses (if applicable)
                    // with ON DELETE NO ACTION, you MUST set related DepartmentID to NULL or delete related records first.
                    // Example (setting students' DepartmentID to NULL):
                    using (SqlCommand cmdUpdateStudents = new SqlCommand("UPDATE Students SET DepartmentID = NULL WHERE DepartmentID = @DepartmentID", conn))
                    {
                        cmdUpdateStudents.Parameters.AddWithValue("@DepartmentID", departmentIdToDelete);
                        cmdUpdateStudents.ExecuteNonQuery();
                    }

                    string query = "DELETE FROM Departments WHERE DepartmentID=@DepartmentID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DepartmentID", departmentIdToDelete);

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("Department deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadDepartmentsData();
                            ClearDepartmentFields();
                            LoadDepartmentsComboBox(); // Refresh comboboxes
                        }
                        else
                        {
                            MessageBox.Show("Delete failed. Department not found.", "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (SqlException sqlex)
                {
                    if (sqlex.Number == 547) // Foreign key constraint violation error number
                    {
                        MessageBox.Show("Cannot delete department because there are still records (e.g., students or courses) referencing this department. " +
                                        "Please update or delete related records first, or configure 'ON DELETE CASCADE' for your foreign keys in the database.",
                                        "Foreign Key Violation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Database Error deleting department: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClearDepartmentFields_Click(object sender, EventArgs e)
        {
            ClearDepartmentFields();
        }

        private void dataGridViewDepartments_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridViewDepartments.Rows[e.RowIndex];

                txtDepartmentID.Text = row.Cells["DepartmentID"].Value.ToString();
                txtDepartmentName.Text = row.Cells["DepartmentName"].Value.ToString();
                txtDepartmentCode.Text = row.Cells["DepartmentCode"].Value.ToString();
                txtDepartmentID.ReadOnly = true; // Make ID read-only when editing
            }
        }

        private void txtDepartmentID_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            previousForm.Show(); // Show the previous form
            this.Close();
        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void dep_Load_1(object sender, EventArgs e)
        {

        }
    }
}
