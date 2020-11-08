//--------------------------------------------------------------------------------------------
//File:   App.xaml.cs
//Desc:   This file defines a class App which contains all the setup logic for the ARM 
//            Simulator App.
//---------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace armsim
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Options opt = new Options();
            opt.VldOpt(e.Args);

            MainWindow wind = new MainWindow(opt);

            if (opt.Exec && opt.Filename != "")
            {
                wind.asim.Reset();
                wind.ProccessFile(opt.Filename, wind.asim.CompRAM.Cells);
                wind.asim.Run();
                Current.Shutdown();
            }
            else
            {
                wind.Show();
            }            
        }
    }
}
