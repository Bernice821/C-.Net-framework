using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardGame_Client
{
    public partial class FormStart : Form
    {
        public FormStart()
        {
            InitializeComponent();
        }

        private Form1 form1;

        //開始遊戲按鈕
        private void button1_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Form1 f = new Form1();//產生Form1的物件，才可以使用它所提供的Method
            f.ShowDialog(this);
            this.Close();
            //f.Visible = true;
        }
    }
}
