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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label3.BackColor = Color.FromArgb(42, 109, 175);
            label3.ForeColor = Color.White;
            label3.BorderStyle = BorderStyle.None;
            label1.BackColor = Color.FromArgb(42, 109, 175);
            label1.ForeColor = Color.White;
            label1.BorderStyle = BorderStyle.None; label2.BackColor = Color.FromArgb(42, 109, 175);
            label2.ForeColor = Color.White;
            label2.BorderStyle = BorderStyle.None;
        }
        SqlConnection conn = new SqlConnection(@"Data Source=DESKTOP-HDIN6H2\SQLEXPRESS01;Initial Catalog=proj;Integrated Security=True");
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            String userid, userpass;
            userid = txt_id.Text;
            userpass = txt_pass.Text;
            if (userid == "admin_001" && userpass == "12345")
            {
                MessageBox.Show("Admin Login Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                admin admin = new admin();
                admin.Show();
                this.Hide();
            }
            else if (userid == "stud_002" && userpass == "54321")
            {
                MessageBox.Show("Student Login Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                stu student = new stu();
                student.Show();
                this.Hide();
            }

            else if (userid == "tchr_003" && userpass == "11223")
            {
                MessageBox.Show("Teacher Login Successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                teach teacher = new teach();
                teacher.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void but_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void but_clear_Click(object sender, EventArgs e)
        {
            txt_id.Clear();
            txt_pass.Clear();
            txt_id.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
