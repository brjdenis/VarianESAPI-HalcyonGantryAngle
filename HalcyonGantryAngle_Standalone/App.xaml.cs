// This app just reads a standalone dicom file, gets the matrix of values and some other data
// and pipes the data to MainWindow from HalcyonGantryAngle that esapi script is already using.
using EvilDICOM.Core;
using EvilDICOM.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HalcyonGantryAngle;
using System.IO;
using EvilDICOM.Core.Element;
using System.Threading;
using System.Globalization;

namespace HalcyonGantryAngle_Standalone
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            try
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    GetDicomDataAndOpenWindow(dlg.FileName);
                }
                else
                {
                    MessageBox.Show("No image selected");
                }
                Application.Current.Shutdown();
            }
            catch (Exception f)
            {
                MessageBox.Show(f.Message + "\n" + f.StackTrace, "Error");
                Application.Current.Shutdown();
            }
        }

        private void GetDicomDataAndOpenWindow(string filename)
        {
            var dcm = DICOMObject.Read(filename);

            ushort rows = (ushort)dcm.FindFirst(TagHelper.Rows).DData;
            ushort columns = (ushort)dcm.FindFirst(TagHelper.Columns).DData;

            double intercept = 0.0;
            var _intercept = dcm.FindFirst(TagHelper.RescaleIntercept) as AbstractElement<double>;
            if (_intercept != null)
            {
                intercept = _intercept.Data;
            }

            double slope = 1.0;
            var _slope = dcm.FindFirst(TagHelper.RescaleSlope) as AbstractElement<double>;
            if (_slope != null)
            {
                slope = _slope.Data;
            }

            double window = 0;
            var _window = dcm.FindFirst(TagHelper.WindowWidth) as AbstractElement<double>;
            if (_window != null)
            {
                window = _window.Data;
            }
            window = window / slope; // window and level are not in raw pixel values, but take into accout slope/intercept

            double level = 0;
            var _level = dcm.FindFirst(TagHelper.WindowCenter) as AbstractElement<double>;
            if (_level != null)
            {
                level = _level.Data;
            }
            level = (level - intercept) / slope; // window and level are not in raw pixel values, but take into accout slope/intercept

            string name = "";
            var _name = dcm.FindFirst(TagHelper.RTImageLabel) as AbstractElement<string>;
            if (_name != null)
            {
                name = _name.Data;
            }

            double SID = 1000.0;
            var _SID = dcm.FindFirst(TagHelper.RTImageSID) as AbstractElement<double>;
            if (_SID != null)
            {
                SID = _SID.Data;
            }

            List<byte> pixelData = (List<byte>)dcm.FindFirst(TagHelper.PixelData).DData_;
            
            List<double> pixelSpacing = (List<double>)dcm.FindFirst(TagHelper.ImagePlanePixelSpacing).DData_;
            
            double Xres = pixelSpacing[0] * (1000.0 / SID);

            int Xsize = columns;
            int Ysize = rows;

            double[,] image = new double[Xsize, Ysize];

            int indexColumn = 0;
            int indexRow = 0;

            for (int i = 0; i < pixelData.Count(); i += 2)
            {
                // taken from https://stackoverflow.com/questions/55883798/wrong-output-pixel-colors-grayscale-using-evildicom
                //original data - 16 bits unsigned
                ushort pixel = (ushort)(pixelData[i + 1] * 256 + pixelData[i]);

                double valgray = pixel;

                image[indexColumn, Ysize - 1 - indexRow] = valgray;

                indexColumn += 1;

                if (indexColumn > Xsize - 1)
                {
                    indexColumn = 0;
                    indexRow += 1;
                }
            }

            var minMaxValues = GetMinMaxValues(image, Xsize, Ysize);

            HalcyonGantryAngle.MainWindow mwd = new HalcyonGantryAngle.MainWindow(image, (int)level, (int)window, minMaxValues.Item1, minMaxValues.Item2, Xsize, Ysize, Xres);
            mwd.Title += " (" + Path.GetFileName(filename)+")" + " (" + name + ")";
            mwd.ShowDialog();
        }

        public Tuple<int, int> GetMinMaxValues(double[,] pixelsPort, int sizeX, int sizeY)
        {
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
                }
            }
            return Tuple.Create((int)minVal, (int)maxVal);
        }
    }
}
