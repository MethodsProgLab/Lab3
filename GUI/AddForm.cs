using System;
using System.Windows.Forms;

namespace GUI
{
    public partial class AddForm : Form
    {
        public string NameElement;
        public byte Level;
        public string MethodName;

        public AddForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NameElement = textBox1.Text;
            Level = Convert.ToByte(textBox2.Text);
            MethodName = textBox3.Text;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
