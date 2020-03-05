using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace WindowsFormsApp4
{
    public partial class select_code_form : Form
    {
        TextBox sender_obj;
        List<string> codes;

        public select_code_form(TextBox sender, List<string> code_list_argv)
        {
            InitializeComponent();
            this.sender_obj = sender;
            this.codes = code_list_argv;
            //Array.Copy(this.codes, 0, code_list_argv, 0, code_list_argv.Length);
        }

        private void select_code_form_Load(object sender, EventArgs e)
        {
            for(int i=0;i<this.codes.Count;i++)
            {
                code_list.Items.Add(this.codes[i]);
            }
        }

        private void select_code_btn_Click(object sender, EventArgs e)
        {
            this.sender_obj.Text = ((string)code_list.Items[code_list.SelectedIndex]).Split('(')[0];
            this.Hide();
        }
    }
}
