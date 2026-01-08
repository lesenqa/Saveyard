using System;
using System.IO;
using System.Windows.Forms;

namespace Saveyard
{
    public partial class FormSettings : Form
    {
        Form1 main;

        public FormSettings(Form1 f)
        {
            InitializeComponent();
            main = f;

            // Заполняем поля данными из главной формы
            txtGame.Text = main.currentConfig.GameName;
            txtCategory.Text = main.currentConfig.CategoryName;
            txtSavesDir.Text = main.currentConfig.SavesDirectory;
            txtPrefix.Text = main.currentConfig.Prefix;
            txtPostfix.Text = main.currentConfig.Postfix;

            // Показываем имя текущего файла
            if (!string.IsNullOrEmpty(main.currentConfigPath))
                // txtFileName.Text = Path.GetFileName(main.currentConfigPath);
                txtFileName.Text = Path.GetFileNameWithoutExtension(main.currentConfigPath);
            else
                txtFileName.Text = main.currentConfig.GameName + " - " + main.currentConfig.CategoryName /* + ".cfg" */;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            main.currentConfig.GameName = txtGame.Text;
            main.currentConfig.CategoryName = txtCategory.Text;
            main.currentConfig.SavesDirectory = txtSavesDir.Text;
            main.currentConfig.Prefix = txtPrefix.Text;
            main.currentConfig.Postfix = txtPostfix.Text;

            // Обновляем путь сохранения, если имя файла ввели вручную
            if (!string.IsNullOrEmpty(txtFileName.Text /* + ".cfg" */))
            {
                main.currentConfigPath = Path.Combine("Configs", txtFileName.Text + ".cfg");
            }

            main.UpdateUI();
            this.Close();
        }

        private void btnBrowseDir_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                    txtSavesDir.Text = fbd.SelectedPath;
            }
        }

        private void btnUpdateName_Click(object sender, EventArgs e)
        {
            txtFileName.Text = txtGame.Text + " - " + txtCategory.Text /* + ".cfg" */;
        }
    }
}