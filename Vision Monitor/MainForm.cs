using System;
using System.Windows.Forms;
using lucidcode.LucidScribe.Plugin.Halovision;

namespace TestProject
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            VisionForm visionForm = new VisionForm();
            visionForm.Show();
        }
    }
}
