using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GIEXPERTCONTROLLib;


namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void loginbutton_Click(object sender, EventArgs e)
        {
            bool login;

            string idname = id_box.Text;
            //idname = "yooncs";
            string password = pw_box.Text;
            //password = "Thisis12#$";
            string AccountPassword = account_box.Text;
            //AccountPassword = "";
            string path = "C:\\SHINHAN-ii\\indi\\giexpertstarter.exe";

            login = axGiExpertControl1.StartIndi(idname, password, AccountPassword, path);


            if (login)
            {
                MessageBox.Show("연결 성공");
                Form2 fr = new Form2();
                fr.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("연결 실패");
            }

        }

    }
}
