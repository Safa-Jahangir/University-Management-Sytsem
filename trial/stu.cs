using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace trial
{
    public partial class stu : Form
    {
        SqlConnection conn = new SqlConnection(@"Data Source=DESKTOP-HDIN6H2\SQLEXPRESS01;Initial Catalog=proj;Integrated Security=True");
        private int _loggedInStudentID = -1;

        public stu()
        {
            InitializeComponent();
        }

        private void stu_Load(object sender, EventArgs e)
        {
            // Prompt for Student ID
            using (InputStudentIDForm inputForm = new InputStudentIDForm())
            {
                if (inputForm.ShowDialog(this) == DialogResult.OK)
                {
                    _loggedInStudentID = inputForm.StudentID;
                }
                else
                {
                    MessageBox.Show("Student ID is required to view your grades. Returning to login.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    button1_Click(null, null);
                    return;
                }
            }

            LoadStudentName();
            LoadStudentGrades();
        }

        private void LoadStudentName()
        {
            try
            {
                conn.Open();
                string query = "SELECT FirstName + ' ' + LastName AS FullName FROM Students WHERE StudentID = @StudentID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@StudentID", _loggedInStudentID);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    lblWelcome.Text = "Welcome, " + result.ToString();
                }
                else
                {
                    lblWelcome.Text = "Student not found.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading student name: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private void LoadStudentGrades()
        {
            try
            {
                conn.Open();

                string query = @"
                    SELECT
                        S.StudentID,
                        S.FirstName + ' ' + S.LastName AS FullName,
                        C.CourseName,
                        C.CourseID,
                        G.Grade,
                        G.Status
                    FROM
                        Students AS S
                    INNER JOIN
                        Grades AS G ON S.StudentID = G.StudentID
                    INNER JOIN
                        Courses AS C ON G.CourseID = C.CourseID
                    WHERE
                        S.StudentID = @LoggedInStudentID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@LoggedInStudentID", _loggedInStudentID);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dataGridViewStudentGrades.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading grades: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 loginForm = new Form1();
            loginForm.Show();
        }
    }
}
