using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using VMS.TPS.Common.Model.API;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using Image = VMS.TPS.Common.Model.API.Image;
using OxyPlot.Annotations;

namespace HalcyonGantryAngle
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        //private static readonly PlotView PlotView = new PlotView();
        public readonly ScriptContext scriptcontext;
        public double[,] image { get; set; }
        public PlotModel PlotModel { get; set; }

        public LinearColorAxis arrayLinearColorAxis = new LinearColorAxis{ Palette = OxyPalettes.Gray(255) };

        public EllipseAnnotation Circle1 = new EllipseAnnotation { Fill = OxyColors.Transparent, Stroke = OxyColors.Red, StrokeThickness = 2 };
        public EllipseAnnotation Circle2 = new EllipseAnnotation { Fill = OxyColors.Transparent, Stroke = OxyColors.Blue, StrokeThickness = 2 };

        public HeatMapSeries imageHeatMap = new HeatMapSeries { };

        public double dpmm; // dot per milimeter
        public readonly double radius1 = 13.3; // starting radius
        public readonly double radius2 = 7.5;

        public double mouseX = 0; // mouse cursor
        public double mouseY = 0;

        public int lowerPixelValue; // for line color series
        public int upperPixelValue;

        public int minPixelValue; // in loaded array
        public int maxPixelValue;

        public double a = 5.7; // cm
        public double b = 5.7; // cm
        public double SAD = 100.0; // cm
        public double pi = Math.PI;


        public MainWindow( ScriptContext _scriptcontext)
        {
            this.scriptcontext = _scriptcontext;

            InitializeComponent();

            this.Tab1TextBoxCircle1Radius.Text = this.radius1.ToString();
            this.Tab1TextBoxCircle2Radius.Text = this.radius2.ToString();

            this.dpmm = 1.0 / _scriptcontext.Image.XRes;

            image = GetImage(_scriptcontext);
            PlotModel = CreatePlotModel();

            myPlot.Model = PlotModel;

            CalculateDeviation();
            DefineSliderWindowStartingValue();
            DefineSliderLevelStartingValue();
        }


        public PlotModel CreatePlotModel()
        {
            var plotModel = new PlotModel { PlotType = PlotType.Cartesian };
            AddAxes(plotModel);
            CreateSeries(plotModel);
            AddCircles(plotModel);

            return plotModel;
        }

        public double[,] GetImage(ScriptContext scriptcontext)
        {
            Image portalDose = scriptcontext.Image;

            int sizeX = portalDose.XSize;
            int sizeY = portalDose.YSize;

            int level = portalDose.Level;
            int window = portalDose.Window;

            this.lowerPixelValue = level - window / 2;
            this.upperPixelValue = level + window / 2;

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

            this.minPixelValue = (int)minVal;
            this.maxPixelValue = (int)maxVal;

            return pixelsPort2;
        }

        public void AddAxes(PlotModel plotModel)
        {
            this.arrayLinearColorAxis.Maximum = this.upperPixelValue;
            this.arrayLinearColorAxis.Minimum = this.lowerPixelValue;
            plotModel.Axes.Add(this.arrayLinearColorAxis);
        }

        public void DefineSliderWindowStartingValue()
        {
            this.SliderWindow.Minimum = 0;
            this.SliderWindow.Maximum = this.maxPixelValue;
            this.SliderWindow.Value = this.scriptcontext.Image.Window;

            this.SliderWindowLabel.Content = this.SliderWindow.Value.ToString();
        }

        public void DefineSliderLevelStartingValue()
        {
            this.SliderLevel.Minimum = this.minPixelValue;
            this.SliderLevel.Maximum = this.maxPixelValue;
            this.SliderLevel.Value = this.scriptcontext.Image.Level;

            this.SliderLevelLabel.Content = this.SliderLevel.Value.ToString();
        }


        public void CreateSeries(PlotModel plotModel)
        {
            var series = new HeatMapSeries
            {
                X0 = 0,
                X1 = image.GetLength(0),
                Y0 = 0,
                Y1 = image.GetLength(1),
                Interpolate = false,
                RenderMethod = HeatMapRenderMethod.Bitmap,
                Data = image
            };

            series.MouseDown += (s, e) =>
            {
                this.mouseX = (s as HeatMapSeries).InverseTransform(e.Position).X;
                this.mouseY = (s as HeatMapSeries).InverseTransform(e.Position).Y;
            };

            this.imageHeatMap = series;
            plotModel.Series.Add(series);
        }

        public void AddCircles(PlotModel plotModel)
        {
            double centerX = this.scriptcontext.Image.XSize / 2;
            double centerY = this.scriptcontext.Image.YSize / 2;

            this.Circle1.X = centerX;
            this.Circle1.Y = centerY;
            this.Circle1.Width = 2 * this.radius1 * this.dpmm;
            this.Circle1.Height = 2 * this.radius1 * this.dpmm;

            this.Circle2.X = centerX;
            this.Circle2.Y = centerY;
            this.Circle2.Width = 2 * this.radius2 * this.dpmm;
            this.Circle2.Height = 2 * this.radius2 * this.dpmm;

            plotModel.Annotations.Add(this.Circle1);
            plotModel.Annotations.Add(this.Circle2);
        }

        public void CalculateDeviation()
        {
            double devX = Math.Abs(this.Circle1.X - this.Circle2.X) / this.dpmm;
            double devY = Math.Abs(this.Circle1.Y - this.Circle2.Y) / this.dpmm;

            //double temp = 10.0 * (this.a + this.SAD * this.b / (this.b + this.SAD)); // mm
            double temp = 10.0  * (this.a + this.b); // mm
            double angleX = Math.Asin(devX / temp) * 180.0 / this.pi;
            double angleY = Math.Asin(devY / temp) * 180.0 / this.pi;


            this.DeviationX.Content = devX.ToString("F2");
            this.DeviationY.Content = devY.ToString("F2");

            this.DeviationAngleX.Content = angleX.ToString("F2");
            this.DeviationAngleY.Content = angleY.ToString("F2");
        }

        private void UpdatePlotOnCircleChange()
        {
            this.PlotModel.InvalidatePlot(true);
            CalculateDeviation();
        }

        private void Button_Click_Circle1_Right(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle1Delta.Text) * this.dpmm;
            this.Circle1.X += delta;
            UpdatePlotOnCircleChange();
        }
        private void Button_Click_Circle1_Left(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle1Delta.Text) * this.dpmm;
            this.Circle1.X -= delta;
            UpdatePlotOnCircleChange();
        }
        private void Button_Click_Circle1_Up(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle1Delta.Text) * this.dpmm;
            this.Circle1.Y += delta;
            UpdatePlotOnCircleChange();
        }
        private void Button_Click_Circle1_Down(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle1Delta.Text) * this.dpmm;
            this.Circle1.Y -= delta;
            UpdatePlotOnCircleChange();
        }

        private double ConvertTextToDouble(string text)
        {
            if (Double.TryParse(text, out double result))
            {
                return result;
            }
            else
            {
                return Double.NaN;
            }
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            int ind = txt.CaretIndex;
            txt.Text = txt.Text.Replace(",", ".");
            txt.CaretIndex = ind;
        }

        private void LostFocusCircle1Radius(object sender, RoutedEventArgs e)
        {
            double radius = ConvertTextToDouble(this.Tab1TextBoxCircle1Radius.Text) * this.dpmm;
            this.Circle1.Width = 2 * radius;
            this.Circle1.Height = 2 * radius;
            UpdatePlotOnCircleChange();
        }

        private void LostFocusCircle1Thickness(object sender, RoutedEventArgs e)
        {
            double thickness = ConvertTextToDouble(this.Tab1TextBoxCircle1Thickness.Text);
            this.Circle1.StrokeThickness = thickness;
            UpdatePlotOnCircleChange();
        }

        private void Button_Click_Circle1_Cursor(object sender, RoutedEventArgs e)
        {
            this.Circle1.X = this.mouseX;
            this.Circle1.Y = this.mouseY;
            UpdatePlotOnCircleChange();
        }


        private void Button_Click_Circle2_Right(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle2Delta.Text) * this.dpmm;
            this.Circle2.X += delta;
            UpdatePlotOnCircleChange();
        }
        private void Button_Click_Circle2_Left(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle2Delta.Text) * this.dpmm;
            this.Circle2.X -= delta;
            UpdatePlotOnCircleChange();
        }
        private void Button_Click_Circle2_Up(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle2Delta.Text) * this.dpmm;
            this.Circle2.Y += delta;
            UpdatePlotOnCircleChange();
        }
        private void Button_Click_Circle2_Down(object sender, RoutedEventArgs e)
        {
            double delta = ConvertTextToDouble(this.Tab1TextBoxCircle2Delta.Text) * this.dpmm;
            this.Circle2.Y -= delta;
            UpdatePlotOnCircleChange();
        }

        private void LostFocusCircle2Radius(object sender, RoutedEventArgs e)
        {
            double radius = ConvertTextToDouble(this.Tab1TextBoxCircle2Radius.Text) * this.dpmm;
            this.Circle2.Width = 2 * radius;
            this.Circle2.Height = 2 * radius;
            UpdatePlotOnCircleChange();
        }

        private void LostFocusCircle2Thickness(object sender, RoutedEventArgs e)
        {
            double thickness = ConvertTextToDouble(this.Tab1TextBoxCircle2Thickness.Text);
            this.Circle2.StrokeThickness = thickness;
            UpdatePlotOnCircleChange();
        }

        private void Button_Click_Circle2_Cursor(object sender, RoutedEventArgs e)
        {
            this.Circle2.X = this.mouseX;
            this.Circle2.Y = this.mouseY;
            UpdatePlotOnCircleChange();
        }

        private void InterpolateChanged(object sender, RoutedEventArgs e)
        {
            if ((bool)this.Interpolation.IsChecked)
            {
                this.imageHeatMap.Interpolate = true;
            }
            else
            {
                this.imageHeatMap.Interpolate = false;
            }
            this.PlotModel.InvalidatePlot(true);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int level = (int)this.SliderLevel.Value;
            int window = (int)this.SliderWindow.Value;

            this.lowerPixelValue = level - window / 2;
            this.upperPixelValue = level + window / 2;

            this.SliderWindowLabel.Content = window.ToString();
            this.SliderLevelLabel.Content = level.ToString();

            this.arrayLinearColorAxis.Maximum = this.upperPixelValue;
            this.arrayLinearColorAxis.Minimum = this.lowerPixelValue;
            this.PlotModel.InvalidatePlot(true);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow helpwindow = new HelpWindow();
            helpwindow.Owner = this;
            helpwindow.Show();
        }
    }
}
