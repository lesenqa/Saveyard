using System;
using System.Windows.Forms;

namespace Saveyard
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                // Попытка запуска главной формы
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                // Если программа падает при старте, ты увидишь это окно
                MessageBox.Show("error launching:\n\n" + ex.Message + "\n\n" + ex.StackTrace, "bro thats bad and critical");
            }
        }
    }
}