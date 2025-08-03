using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace trial
{
    public partial class admin : Form
    {
        public admin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            stud studentsForm = new stud(this);
            this.Hide();
            studentsForm.ShowDialog();
            this.Show();
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cour coursesForm = new cour(this); 
            this.Hide(); 
            coursesForm.ShowDialog();
            this.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            dep departmentsForm = new dep(this);
            this.Hide();
            departmentsForm.ShowDialog();
            this.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 loginForm = new Form1();
            loginForm.Show();
            this.Close();
        }

        private void admin_Load(object sender, EventArgs e)
        {

        }

    }
}
