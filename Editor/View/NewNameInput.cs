using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ARdevKit.View
{
    public partial class NewNameInput : Form
    {
        public NewNameInput()
        {
            InitializeComponent();
        }
        
        private void OK_bttn_Click(object sender, System.EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            return;
        }

        private void Cancel_bttn_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            return;
        }

        private void TextBox1_Enter(object sender, EventArgs e)
        {
            ((TextBox)sender).Clear();
            ((TextBox)sender).Enter -= TextBox1_Enter;
        }
    }
}
