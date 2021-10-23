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
using System.Threading;
using System.Globalization;

namespace VMS.TPS
{
    public class Script
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext scriptcontext)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            //var mainViewModel = new MainViewModel(scriptcontext);
            if (scriptcontext.Image == null)
            {
                MessageBox.Show("No active image", "Error");
                return;
            }

            int level = scriptcontext.Image.Level;
            int window = scriptcontext.Image.Window;
            int Xsize = scriptcontext.Image.XSize;
            int Ysize = scriptcontext.Image.YSize;
            double Xres = scriptcontext.Image.XRes;

            var img = GetImage(scriptcontext);
            double[,] image = img.Item1;
            int minPixelValue = img.Item2;
            int maxPixelValue = img.Item3;

            var mainView = new MainWindow(image, level, window, minPixelValue, maxPixelValue, Xsize, Ysize, Xres);
            mainView.Title += " (" + scriptcontext.Image.Id + ")";
            mainView.ShowDialog();
        }

        public Tuple<double[,], int, int> GetImage(ScriptContext scriptcontext)
        {
            Image portalDose = scriptcontext.Image;

            int sizeX = portalDose.XSize;
            int sizeY = portalDose.YSize;

            int level = portalDose.Level;
            int window = portalDose.Window;

            int[,] pixelsPort = new int[sizeX, sizeY];
            double[,] pixelsPort2 = new double[sizeX, sizeY];

            portalDose.GetVoxels(0, pixelsPort);

            double minVal = 0;
            double maxVal = 0;

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    double tempVal = (double)pixelsPort[i, j];

                    if (tempVal > maxVal)
                    {
                        maxVal = tempVal;
                    }
                    if (tempVal < minVal)
                    {
                        minVal = tempVal;
                    }
                    pixelsPort2[i, sizeY - j - 1] = tempVal;
                }
            }

            return Tuple.Create(pixelsPort2, (int)minVal, (int)maxVal);
        }


    }
}
