using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MYSClient
{
    public partial class MainForm: MaterialForm
    {
        private readonly MaterialSkinManager materialSkinManager;

        public MainForm()
        {
            InitializeComponent();

            // Initialize MaterialSkinManager
            materialSkinManager = MaterialSkinManager.Instance;

            // Set this to false to disable backcolor enforcing on non-materialSkin components
            // This HAS to be set before the AddFormToManage()
            materialSkinManager.EnforceBackcolorOnAllComponents = true;

            // MaterialSkinManager properties
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo100, Accent.Pink200, TextShade.WHITE);

            materialSkinManager.EnforceBackcolorOnAllComponents = false;
            materialTextBox24.BackColor = Color.White; // ya da Color.Transparent




     

          
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void materialButton26_Click(object sender, EventArgs e)
        {
            MaterialDialog materialDialog = new MaterialDialog(this, "Dialog Title", "Dialogs inform users about a task and can contain critical information, require decisions, or involve multiple tasks.", "OK");
            DialogResult result = materialDialog.ShowDialog(this);

        }

        private void materialButton1_Click(object sender, EventArgs e)
        {

        }
    }
}
