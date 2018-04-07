using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CETT
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                using (var mutex = new Mutex(true, "4e40a3ee-1fa6-4ab3-9cd1-bf0733d920ae", out var createNew))
                {
                    if (createNew)
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.Run(new Form1());
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Verb = "runas"
                });
            }
        }
    }
}
