using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace trial 
{
    public partial class cour : Form
    {
        private string connectionString = @"Data Source=DESKTOP-HDIN6H2\SQLEXPRESS01;Initial Catalog=proj;Integrated Security=True";

        public cour()
        {
            InitializeComponent();
        }
        private Form previousForm;

public cour(Form callingForm)
{
    InitializeComponent();
    previousForm = callingForm;
}

        private void cour_Load(object sender, EventArgs e)
        {
            LoadCoursesData();
            ClearCourseFields(); // Clear fields on load for fresh entry
        }

        // Assume you have these methods, based on their calls in your provided code
        // You'll need to fill in the actual logic for Add/Update/Load Course Data
        private void LoadCoursesData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT CourseID, CourseName, CourseCode, Credits, Instructor FROM Courses ORDER BY CourseID"; // Adjust columns as per your DB schema

                    using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dataGridViewCourses.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading course data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearCourseFields()
        {
            // Assuming you have TextBoxes named txtCourseID, txtCourseName, etc.
            txtCourseID.Clear();
            txtCourseName.Clear();
            txtCourseCode.Clear();
            txtCredits.Clear();
            txtInstructor.Clear();
            txtCourseID.ReadOnly = false; // Allow editing CourseID for new entries
            txtCourseID.Focus(); // Set focus to the first input field
        }

        // Assuming these button click handlers exist from your original AdminForm setup
        private void btnAddCourse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourseID.Text) || string.IsNullOrWhiteSpace(txtCourseName.Text) ||
                string.IsNullOrWhiteSpace(txtCourseCode.Text) || string.IsNullOrWhiteSpace(txtCredits.Text) ||
                string.IsNullOrWhiteSpace(txtInstructor.Text))
            {
                MessageBox.Show("Please fill in all mandatory course fields.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseID;
            if (!int.TryParse(txtCourseID.Text.Trim(), out courseID))
            {
                MessageBox.Show("Invalid Course ID. Please enter a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCourseID.Focus();
                return;
            }

            decimal credits; // Use decimal for Credits if it can have fractional values (e.g., 3.5)
            if (!decimal.TryParse(txtCredits.Text.Trim(), out credits))
            {
                MessageBox.Show("Invalid Credits value. Please enter a valid number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCredits.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Assuming CourseID is NOT IDENTITY and you provide it manually
                    string query = "INSERT INTO Courses (CourseID, CourseName, CourseCode, Credits, Instructor) " +
                                   "VALUES (@CourseID, @CourseName, @CourseCode, @Credits, @Instructor)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseID", courseID);
                        cmd.Parameters.AddWithValue("@CourseName", txtCourseName.Text.Trim());
                        cmd.Parameters.AddWithValue("@CourseCode", txtCourseCode.Text.Trim());
                        cmd.Parameters.AddWithValue("@Credits", credits);
                        cmd.Parameters.AddWithValue("@Instructor", txtInstructor.Text.Trim());

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Course added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadCoursesData();
                        ClearCourseFields();
                    }
                }
                catch (SqlException sqlex)
                {
                    if (sqlex.Number == 2627 || sqlex.Number == 2601) // PK or Unique constraint violation
                    {
                        MessageBox.Show("Error: A course with this ID or code already exists. Please use unique values.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Database Error adding course: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (FormatException)
                {
                    MessageBox.Show("Please enter valid numeric values for Course ID and Credits.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdateCourse_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCourseID.Text))
            {
                MessageBox.Show("Please provide a Course ID to update or select a course from the list.", "Missing Course ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseID;
            if (!int.TryParse(txtCourseID.Text.Trim(), out courseID))
            {
                MessageBox.Show("Invalid Course ID. Please enter a valid whole number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCourseID.Focus();
                return;
            }

            decimal credits;
            if (!decimal.TryParse(txtCredits.Text.Trim(), out credits))
            {
                MessageBox.Show("Invalid Credits value. Please enter a valid number.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtCredits.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Courses SET CourseName=@CourseName, CourseCode=@CourseCode, Credits=@Credits, Instructor=@Instructor WHERE CourseID=@CourseID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CourseName", txtCourseName.Text.Trim());
                        cmd.Parameters.AddWithValue("@CourseCode", txtCourseCode.Text.Trim());
                        cmd.Parameters.AddWithValue("@Credits", credits);
                        cmd.Parameters.AddWithValue("@Instructor", txtInstructor.Text.Trim());
                        cmd.Parameters.AddWithValue("@CourseID", courseID);

                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("Course updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCoursesData();
                            ClearCourseFields();
                        }
                        else
                        {
                            MessageBox.Show("Update failed. Course with the provided ID not found.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                catch (SqlException sqlex)
                {
                    if (sqlex.Number == 2601) // Unique constraint violation
                    {
                        MessageBox.Show("A course with this code already exists. Please use a unique course code.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Database Error updating course: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnLoadCourses_Click(object sender, EventArgs e)
        {

        }

        // Your provided Delete Course code:
        private void btnDeleteCourse_Click(object sender, EventArgs e)
        {
            if (dataGridViewCourses.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a course from the table to delete.", "No Course Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int courseIdToDelete = Convert.ToInt32(dataGridViewCourses.SelectedRows[0].Cells["CourseID"].Value);

            if (MessageBox.Show(string.Format("Are you sure you want to delete Course ID: {0}?\nThis action cannot be undone.", courseIdToDelete),
                                    "Confirm Deletion",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        // IMPORTANT: If CourseID is a foreign key in other tables (e.g., Grades),
                        // you MUST delete related records first or configure ON DELETE CASCADE in your database.
                        // Example if there's a Grades table referencing CourseID:
                        // using (SqlCommand cmdDeleteGrades = new SqlCommand("DELETE FROM Grades WHERE CourseID = @CourseID", conn))
                        // {
                        //      cmdDeleteGrades.Parameters.AddWithValue("@CourseID", courseIdToDelete);
                        //      cmdDeleteGrades.ExecuteNonQuery();
                        // }

                        string query = "DELETE FROM Courses WHERE CourseID=@CourseID";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@CourseID", courseIdToDelete);

                            if (cmd.ExecuteNonQuery() > 0)
                            {
                                MessageBox.Show("Course deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadCoursesData();
                                ClearCourseFields();
                            }
                            else
                            {
                                MessageBox.Show("Delete failed. Course not found.", "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                    catch (SqlException sqlex)
                    {
                        if (sqlex.Number == 547) // Foreign key constraint violation error number
                        {
                            MessageBox.Show("Cannot delete course because there are associated records (e.g., grades or student enrollments) referencing this course. " +
                                            "Please delete related records first or configure 'ON DELETE CASCADE' for your foreign keys in the database.",
                                            "Foreign Key Violation", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Database Error deleting course: " + sqlex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Your provided Clear Course Fields code:
        private void btnClearCourseFields_Click(object sender, EventArgs e)
        {
            ClearCourseFields();
        }

        // Your provided DataGridView Cell Click code:
        private void dataGridViewCourses_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridViewCourses.Rows[e.RowIndex];

                txtCourseID.Text = row.Cells["CourseID"].Value.ToString();
                txtCourseName.Text = row.Cells["CourseName"].Value.ToString();
                txtCourseCode.Text = row.Cells["CourseCode"].Value.ToString();
                txtCredits.Text = row.Cells["Credits"].Value.ToString();
                txtInstructor.Text = row.Cells["Instructor"].Value.ToString();

                txtCourseID.ReadOnly = true; // Make ID read-only when editing
            }
        }

        private void dataGridViewCourses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtCourseID_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void butlogout_Click(object sender, EventArgs e)
        {
            previousForm.Show(); // Show the previous form
            this.Close();
        }

        private void cour_Load_1(object sender, EventArgs e)
        {

        }
         
    }
}
