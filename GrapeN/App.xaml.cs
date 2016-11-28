using GrapeN.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;

namespace GrapeN
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //AppDomain.CurrentDomain.FirstChanceException += ExceptionThrowed;
        }

        void ExceptionThrowed(object sender, FirstChanceExceptionEventArgs e)
        {
            if (e.Exception.TargetSite.Name == "PushFrame") return;
            new ExceptionWindow(e).ShowDialog();
        }
    }
}
