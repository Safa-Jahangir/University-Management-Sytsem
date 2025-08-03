using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace trial
{
    public partial class teach : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=DESKTOP-HDIN6H2\SQLEXPRESS01;Initial Catalog=proj;Integrated Security=True");

        public teach()
        {
            InitializeComponent();
        }


        // Event handler for when the Teacher form loads
        private void teach_Load(object sender, EventArgs e)
        {
            
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }

            LoadStudentsComboBox(); 
            LoadCoursesComboBox();  
            LoadGradesData();       
            ClearGradeFields();    
        }

        

        // Method to populate the Student ComboBox
        private void LoadStudentsComboBox()
        {
            List<KeyValuePair<int, string>> students = new List<KeyValuePair<int, string>>();
            students.Add(new KeyValuePair<int, string>(0, "-- Select Student --")); // Default item
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand("SELECT StudentID, FirstName, LastName FROM Students ORDER BY LastName, FirstName", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int studentId = reader.GetInt32(0);
                    string studentName = reader.GetString(1) + " " + reader.GetString(2); // FirstName LastName
                    students.Add(new KeyValuePair<int, string>(studentId, studentName));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading students for ComboBox: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Always ensure the connection is closed in the finally block
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            cmbStudents.DataSource = new BindingSource(students, null); 
            cmbStudents.DisplayMember = "Value"; 
            cmbStudents.ValueMember = "Key";     
            cmbStudents.SelectedIndex = 0; 
        }

        // Method to populate the Course ComboBox
        private void LoadCoursesComboBox()
        {
            List<KeyValuePair<int, string>> courses = new List<KeyValuePair<int, string>>();
            courses.Add(new KeyValuePair<int, string>(0, "-- Select Course --")); 

            try
            {
               
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                SqlCommand cmd = new SqlCommand("SELECT CourseID, CourseName FROM Courses ORDER BY CourseName", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                
                while (reader.Read())
                {
                    int courseId = reader.GetInt32(0);
                    string courseName = reader.GetString(1);
                    courses.Add(new KeyValuePair<int, string>(courseId, courseName));
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading courses for ComboBox: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Always ensure the connection is closed in the finally block
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            cmbCourses.DataSource = new BindingSource(courses, null); 
            cmbCourses.DisplayMember = "Value"; 
            cmbCourses.ValueMember = "Key";     
            cmbCourses.SelectedIndex = 0; 
        }

        private void LoadGradesData()
        {
            try
            {
                
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                // JOIN Grades with Students and Courses to get readable names
                string query = "SELECT G.GradeID, S.StudentID, S.FirstName, S.LastName, " +
                               "C.CourseID, C.CourseName, G.Grade, G.Status " +
                               "FROM Grades AS G " +
                               "INNER JOIN Students AS S ON G.StudentID = S.StudentID " +
                               "INNER JOIN Courses AS C ON G.CourseID = C.CourseID " +
                               "ORDER BY S.LastName, C.CourseName"; 

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                dataGridViewGrades.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading grades data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Method to clear all grade input fields
        private void ClearGradeFields()
        {
            txtGradeID.Clear();
            cmbStudents.SelectedIndex = 0; 
            cmbCourses.SelectedIndex = 0;  
            txtGrade.Clear();
            chkStatus.Checked = false;
            txtGrade.Focus();
        }

        // Button to load/refresh grade data
        private void btnLoadGrades_Click(object sender, EventArgs e)
        {
            LoadGradesData();
            ClearGradeFields(); // Clear fields after refreshing
        }

        // Button to add a new grade
        private void btnAddGrade_Click(object sender, EventArgs e)
        {
            if ((int)cmbStudents.SelectedValue == 0 || (int)cmbCourses.SelectedValue == 0 || string.IsNullOrWhiteSpace(txtGrade.Text))
            {
                MessageBox.Show("Please select a student, a course, and enter a grade.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
               if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string query = "INSERT INTO Grades (StudentID, CourseID, Grade, Status) " +
                               "VALUES (@StudentID, @CourseID, @Grade, @Status)";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@StudentID", (int)cmbStudents.SelectedValue);
                cmd.Parameters.AddWithValue("@CourseID", (int)cmbCourses.SelectedValue);
                cmd.Parameters.AddWithValue("@Grade", txtGrade.Text);
                cmd.Parameters.AddWithValue("@Status", chkStatus.Checked);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Grade added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGradesData();
                ClearGradeFields();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627) // Primary Key violation (or unique constraint)
                {
                    MessageBox.Show("Error: A grade already exists for this student in this course.", "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex.Number == 547) // Foreign key constraint violation
                {
                    MessageBox.Show("Invalid Student or Course selected. Please ensure they exist.", "Foreign Key Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }


        // Event handler for clicking a cell in the DataGridView
        private void dataGridViewGrades_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) 
            {
                DataGridViewRow row = dataGridViewGrades.Rows[e.RowIndex];

                txtGradeID.Text = row.Cells["GradeID"].Value.ToString();
                txtGrade.Text = row.Cells["Grade"].Value != DBNull.Value ? row.Cells["Grade"].Value.ToString() : string.Empty;
                chkStatus.Checked = row.Cells["Status"].Value != DBNull.Value && Convert.ToBoolean(row.Cells["Status"].Value);

                // Set Student ComboBox
                if (row.Cells["StudentID"].Value != DBNull.Value)
                {
                    cmbStudents.SelectedValue = Convert.ToInt32(row.Cells["StudentID"].Value);
                }
                else
                {
                    cmbStudents.SelectedIndex = 0;
                }

                // Set Course ComboBox
                if (row.Cells["CourseID"].Value != DBNull.Value)
                {
                    cmbCourses.SelectedValue = Convert.ToInt32(row.Cells["CourseID"].Value);
                }
                else
                {
                    cmbCourses.SelectedIndex = 0;
                }
            }
        }


        // Button to update an existing grade
        private void btnUpdateGrade_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGradeID.Text) || string.IsNullOrWhiteSpace(txtGrade.Text))
            {
                MessageBox.Show("Please select a grade to update and ensure the grade value is not empty.", "Missing Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
               if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                string query = "UPDATE Grades SET " +
                               "Grade = @Grade, " +
                               "Status = @Status " +
                               "WHERE GradeID = @GradeID";

                SqlCommand cmd = new SqlCommand(query, conn);

                cmd.Parameters.AddWithValue("@Grade", txtGrade.Text);
                cmd.Parameters.AddWithValue("@Status", chkStatus.Checked);
                cmd.Parameters.AddWithValue("@GradeID", int.Parse(txtGradeID.Text));

                int rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    MessageBox.Show("Grade updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGradesData();
                    ClearGradeFields();
                }
                else
                {
                    MessageBox.Show("No grade found with the given ID. Update failed.", "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating grade: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
               if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // Button to delete a grade
        private void btnDeleteGrade_Click(object sender, EventArgs e)
        {
            if (dataGridViewGrades.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a grade from the list to delete.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int gradeIdToDelete = Convert.ToInt32(dataGridViewGrades.SelectedRows[0].Cells["GradeID"].Value);

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete Grade ID: " + gradeIdToDelete.ToString() + "?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }

                    string query = "DELETE FROM Grades WHERE GradeID = @GradeID";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@GradeID", gradeIdToDelete);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Grade deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadGradesData();
                        ClearGradeFields();
                    }
                    else
                    {
                        MessageBox.Show("No grade found with the given ID. Deletion failed.", "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting grade: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                   if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }
        }

        // Button to clear the input fields
        private void btnClearFields_Click(object sender, EventArgs e)
        {
            ClearGradeFields();
        }

        // Button to go bacl to login form
        private void btnTeacherLogout_Click(object sender, EventArgs e)
        {
            Form1 loginForm = new Form1();
            loginForm.Show();
            this.Close();
        }
    }
}