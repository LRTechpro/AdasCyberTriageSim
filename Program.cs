using System;
using System;
using System.Windows.Forms;

namespace AdasCyberTriageSim
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormMain());
        }
    }
}

