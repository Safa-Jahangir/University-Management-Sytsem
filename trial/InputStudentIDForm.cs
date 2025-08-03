using System;
using System.Windows.Forms;

namespace trial
{
    public partial class InputStudentIDForm : Form
    {
        public int StudentID { get; private set; } // Property to get the ID

        public InputStudentIDForm()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentIDInput.Text))
            {
                MessageBox.Show("Student ID cannot be empty.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None; // Prevent closing if invalid
                return;
            }

            int id;
            if (!int.TryParse(txtStudentIDInput.Text, out id))
            {
                MessageBox.Show("Invalid Student ID. Please enter a numeric value.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None; // Prevent closing if invalid
                return;
            }

            StudentID = id; // Set the public property
            this.DialogResult = DialogResult.OK; // Indicate success
            this.Close();
        }

        private void InputStudentIDForm_Load(object sender, EventArgs e)
        {
            txtStudentIDInput.Focus(); // Set focus to the textbox
        }
    }
}
