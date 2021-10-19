using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;
using HalcyonGantryAngle;

namespace VMS.TPS
{
    public class Script
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext scriptcontext)
        {
            //var mainViewModel = new MainViewModel(scriptcontext);
            if (scriptcontext.Image == null)
            {
                MessageBox.Show("No active image", "Error");
                return;
            }

            var mainView = new MainWindow(scriptcontext);
            mainView.ShowDialog();
        }

        
    }
}
