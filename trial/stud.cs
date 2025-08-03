using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace trial
{
    public partial class stud : Form
    {

        private string connectionString = @"Data Source=DESKTOP-HDIN6H2\SQLEXPRESS01;Initial Catalog=proj;Integrated Security=True";

        public stud()
        {
            InitializeComponent();
        }
        private Form previousForm;

        public stud(Form callingForm)
        {
            InitializeComponent();
            previousForm = callingForm;
        }

        private void stud_Load(object sender, EventArgs e)
        {
            LoadStudentsData();
            ClearStudentFields();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT DepartmentID FROM Departments";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbStudentDepartment.DataSource = dt;
                    cmbStudentDepartment.ValueMember = "DepartmentID";     // underlying value

                    cmbStudentDepartment.SelectedIndex = -1; // Optional: no pre-selected item
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading departments: " + ex.Message);
                }
            }
        }

        private void LoadStudentsData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // SQL query using LEFT JOIN to get DepartmentName (even if student has no department)
                    string query = "SELECT S.StudentID, S.FirstName, S.LastName, S.DateOfBirth, S.ContactNo, S.Email, S.EnrollmentDate, D.DepartmentName, S.DepartmentID " +
                                   "FROM Students AS S " +
                                   "LEFT JOIN Departments AS D ON S.DepartmentID = D.DepartmentID " +
                                   "ORDER BY S.StudentID"; // Order for better viewing

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridViewStudents.DataSource = dt; // Set the DataGridView's data source
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading student data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void ClearStudentFields()
        {
            txtStudentID.Clear();
            txtFirstName.Clear();
            txtLastName.Clear();
            dtpDateOfBirth.Value = DateTime.Now; // Reset to current date
            txtContactNo.Clear();
            txtEmail.Clear();
            dtpEnrollmentDate.Value = DateTime.Now; // Reset to current date
            cmbStudentDepartment.SelectedIndex = 0; // Reset to default "Select Department"
            txtStudentID.ReadOnly = false; // Make StudentID editable again for new entries
            txtStudentID.Focus(); // Set focus to the first input field
        }

        private void btnAddStudent_Click(object sender, EventArgs e)
        {
            // Basic validation: Check if mandatory fields are not empty
            if (string.IsNullOrWhiteSpace(txtStudentID.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtContactNo.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("Please fill in all mandatory fields (Student ID, First Name, Last Name, Contact No, Email).", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentID;
            if (!int.TryParse(txtStudentID.Text.Trim(), out studentID))
            {
                MessageBox.Show("Invalid Student ID. Please enter a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtStudentID.Focus();
                return;
            }

            long contactNo;
            if (!long.TryParse(txtContactNo.Text.Trim(), out contactNo))
            {
                MessageBox.Show("Invalid Contact Number. Please enter a valid numeric contact number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtContactNo.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO Students (StudentID, FirstName, LastName, DateOfBirth, ContactNo, Email, EnrollmentDate, DepartmentID) " +
                                   "VALUES (@StudentID, @FirstName, @LastName, @DateOfBirth, @ContactNo, @Email, @EnrollmentDate, @DepartmentID)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@StudentID", studentID);
                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@DateOfBirth", dtpDateOfBirth.Value);
                        cmd.Parameters.AddWithValue("@ContactNo", contactNo);
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@EnrollmentDate", dtpEnrollmentDate.Value);
                        cmd.Parameters.AddWithValue("@DepartmentID", (cmbStudentDepartment.SelectedValue != null && (int)cmbStudentDepartment.SelectedValue != 0) ? (object)(int)cmbStudentDepartment.SelectedValue : DBNull.Value);

                        cmd.ExecuteNonQuery(); // Execute the INSERT statement
                        MessageBox.Show("Student added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadStudentsData();    // Refresh the DataGridView
                        ClearStudentFields();  // Clear input fields after adding
                    }
                }
                catch (SqlException sqlex)
                {
                    // Catch specific SQL errors for more informative messages
                    if (sqlex.Number == 2627 || sqlex.Number == 2601) // Primary Key violation (duplicate StudentID) or Unique constraint violation (e.g., Email)
                    {
                        MessageBox.Show("Error: A student with this ID or a unique field (like Email) already exists. Please use unique values.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (sqlex.Number == 547) // Foreign key constraint violation (e.g., invalid DepartmentID)
                    {
                        MessageBox.Show("Invalid Department ID selected. Please choose an existing department.", "Foreign Key Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Database Error adding student: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please enter valid numeric values for Student ID and Contact Number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void btnUpdateStudent_Click(object sender, EventArgs e)
        {
            // Basic validation: Check if StudentID is provided for update
            if (string.IsNullOrWhiteSpace(txtStudentID.Text))
            {
                MessageBox.Show("Please provide a Student ID to update or select a student from the list.", "Missing Student ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentID;
            if (!int.TryParse(txtStudentID.Text.Trim(), out studentID))
            {
                MessageBox.Show("Invalid Student ID. Please enter a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtStudentID.Focus();
                return;
            }

            long contactNo;
            if (!string.IsNullOrWhiteSpace(txtContactNo.Text) && !long.TryParse(txtContactNo.Text.Trim(), out contactNo))
            {
                MessageBox.Show("Invalid Contact Number. Please enter a valid numeric contact number or leave it empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtContactNo.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Students SET " +
                                   "FirstName = @FirstName, " +
                                   "LastName = @LastName, " +
                                   "DateOfBirth = @DateOfBirth, " +
                                   "ContactNo = @ContactNo, " +
                                   "Email = @Email, " +
                                   "EnrollmentDate = @EnrollmentDate, " +
                                   "DepartmentID = @DepartmentID " +
                                   "WHERE StudentID = @StudentID"; // Condition for update

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Add parameters
                        cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@DateOfBirth", dtpDateOfBirth.Value);
                        cmd.Parameters.AddWithValue("@ContactNo", txtContactNo.Text.Trim());
                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@EnrollmentDate", dtpEnrollmentDate.Value);
                        cmd.Parameters.AddWithValue("@StudentID", studentID); // Used in WHERE clause

                        // Handle DepartmentID from ComboBox
                        if (cmbStudentDepartment.SelectedValue != null && (int)cmbStudentDepartment.SelectedValue != 0)
                        {
                            cmd.Parameters.AddWithValue("@DepartmentID", (int)cmbStudentDepartment.SelectedValue);
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@DepartmentID", DBNull.Value);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery(); // Execute the UPDATE statement
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Student updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadStudentsData();    // Refresh grid
                            ClearStudentFields();  // Clear fields
                        }
                        else
                        {
                            MessageBox.Show("No student found with the given ID. Update failed.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (SqlException sqlex)
                {
                    // Specific SQL error handling
                    if (sqlex.Number == 2601) // Unique constraint violation (e.g., if Email must be unique)
                    {
                        MessageBox.Show("A record with this unique value (e.g., Email) already exists.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (sqlex.Number == 547) // Foreign key constraint violation
                    {
                        MessageBox.Show("Invalid Department ID selected. Please choose an existing department.", "Foreign Key Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Database Error updating student: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please enter valid numeric values for Student ID and Contact Number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDeleteStudent_Click(object sender, EventArgs e)
        {
            if (dataGridViewStudents.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a student from the table to delete.", "No Student Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int studentId = Convert.ToInt32(dataGridViewStudents.SelectedRows[0].Cells["StudentID"].Value);

            if (MessageBox.Show("Are you sure you want to delete the student with ID: " + studentId + "?\nThis action cannot be undone.",
                                     "Confirm Deletion",
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        // IMPORTANT: If StudentID is a foreign key in Grades with ON DELETE NO ACTION,
                        // you MUST delete related grades first, or configure your foreign key to CASCADE DELETE.
                        // Example of deleting grades first (uncomment if needed and FK is NO ACTION):
                        // using (SqlCommand cmdDeleteGrades = new SqlCommand("DELETE FROM Grades WHERE StudentID = @StudentID", conn))
                        // {
                        //     cmdDeleteGrades.Parameters.AddWithValue("@StudentID", studentId);
                        //     cmdDeleteGrades.ExecuteNonQuery();
                        // }

                        string query = "DELETE FROM Students WHERE StudentID=@StudentID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@StudentID", studentId);

                            if (cmd.ExecuteNonQuery() > 0)
                            {
                                MessageBox.Show("Student deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadStudentsData();
                                ClearStudentFields();
                            }
                            else
                            {
                                MessageBox.Show("Delete failed. Student not found.", "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (SqlException sqlex)
                    {
                        if (sqlex.Number == 547) // Foreign key constraint violation error number
                        {
                            MessageBox.Show("Cannot delete student because there are associated records (e.g., grades) referencing this student. " +
                                            "Please delete related records first or configure 'ON DELETE CASCADE' for your foreign keys in the database.",
                                            "Foreign Key Violation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Database Error deleting student: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void btnClearStudentFields_Click(object sender, EventArgs e)
        {
            ClearStudentFields();
        }

        private void btnLoadStudents_Click(object sender, EventArgs e)
        {
            LoadStudentsData();
            ClearStudentFields();
            MessageBox.Show("Student data refreshed.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void dataGridViewStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ensure a valid row is clicked (e.g., not the header row)
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridViewStudents.Rows[e.RowIndex];

                txtStudentID.Text = row.Cells["StudentID"].Value.ToString();
                txtFirstName.Text = row.Cells["FirstName"].Value.ToString();
                txtLastName.Text = row.Cells["LastName"].Value.ToString();

                // Handle nullable fields for ContactNo and Email
                txtContactNo.Text = row.Cells["ContactNo"].Value != DBNull.Value ? row.Cells["ContactNo"].Value.ToString() : string.Empty;
                txtEmail.Text = row.Cells["Email"].Value != DBNull.Value ? row.Cells["Email"].Value.ToString() : string.Empty;

                // Handle DateTimePicker for DateOfBirth and EnrollmentDate (check for DBNull.Value)
                dtpDateOfBirth.Value = row.Cells["DateOfBirth"].Value != DBNull.Value ? Convert.ToDateTime(row.Cells["DateOfBirth"].Value) : DateTime.Now;
                dtpEnrollmentDate.Value = row.Cells["EnrollmentDate"].Value != DBNull.Value ? Convert.ToDateTime(row.Cells["EnrollmentDate"].Value) : DateTime.Now;

                // Populate the Department ComboBox
                if (row.Cells["DepartmentID"].Value != DBNull.Value)
                {
                    int departmentId = Convert.ToInt32(row.Cells["DepartmentID"].Value);
                    cmbStudentDepartment.SelectedValue = departmentId;
                }
                else
                {
                    cmbStudentDepartment.SelectedIndex = 0; // Select the default "-- Select Department --"
                }

                // Make StudentID ReadOnly when a row is selected for Update/Delete
                txtStudentID.ReadOnly = true;
            }
        }

        private void dtpDateOfBirth_ValueChanged(object sender, EventArgs e)
        {
            // Your logic here if any specific action is needed on date change.
        }

        private void stud_Load_1(object sender, EventArgs e)
        {

        }

        private void cmbStudentDepartment_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void LoadDepartmentsComboBox()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT DepartmentID, DepartmentName FROM Departments ORDER BY DepartmentName", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<KeyValuePair<int, string>> departments = new List<KeyValuePair<int, string>>();
                            departments.Add(new KeyValuePair<int, string>(0, "-- Select Department --")); // Default selection

                            while (reader.Read())
                            {
                                departments.Add(new KeyValuePair<int, string>(reader.GetInt32(0), reader.GetString(1)));
                            }

                            // Bind to Student Department ComboBox
                            if (cmbStudentDepartment != null)
                            {
                                cmbStudentDepartment.DataSource = new BindingSource(departments, null);
                                cmbStudentDepartment.DisplayMember = "Value";
                                cmbStudentDepartment.ValueMember = "Key";
                                cmbStudentDepartment.SelectedIndex = 0;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading departments for ComboBox: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            previousForm.Show(); // Show the previous form
            this.Close();
        }
    }
}
