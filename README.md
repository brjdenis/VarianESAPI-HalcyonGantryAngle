# Halcyon gantry angle
Varian ESAPI script for (manually) analyzing a 2D image of the Cirs IsoCube with two circular objects that are used to calculate the Halcyon gantry angle deviation.

![image](image.png)

## Setup

To use the script, you must compile it on your system. You should be able to open the project with Visual Studio 2019 Community Edition. Open the .sln file in Dosimetry folder. 
The script was developed for Eclipse version 15.6. It may not work with other versions of Eclipse or Varian ESAPI.

1. You will need to restore NuGet package for compilation: OxyPlot, Evil-Dicom. Right click the solution -> Restore NuGet packages.
2. Don't forget the references to Varian dlls.
3. Compile as Release for x64.

## Standalone version (updated)

If you don't have ESAPI, you can still use the program. Go to the folder [*HalcyonGantryAngle_Standalone/Compiled*](https://github.com/brjdenis/VarianESAPI-HalcyonGantryAngle/tree/master/HalcyonGantryAngle_Standalone/Compiled) and copy the contents to your computer. Run the .EXE file and select a dicom file that you would like to analyze. That is all. 

The program will call .esapi.dll library but without going through the same path as it would if you called it via Eclipse. It is really botched-up, to be honest ... not a very good programmer here.

The standalone version is using [Evil-dicom](https://github.com/rexcardan/Evil-DICOM) to read the image data. The dlls are included.

There is a very small difference in how the image is displayed via ESAPI or the standalone program. You will notice some difference in gray values and how Window/Level is applied at the beginning. If the radius of the circles is not close to the radius of the circular slice, then there may be a problem with the decimal point representation. Change the culture of your system to "en-US".

## Use in Eclipse

1. In External Beam Planning open a 2D portal image of the phantom. You can set Window/Level before running the script. The script will display internal pixel values, not taking into account *intercept* and *slope*.
2. Run the script.
3. Read the Help to learn how to move the circles.

Angle deviations are calculated with this formula:

![image](imageeq.png)

Here *a* is the distance of the upper circle from the center of the cube, and *b* is the distance of the lower circle from the center of the cube (a = b = 5.7 cm). *X* and *Y* are the measured distances between the centers of the circles. This is an approximation. You can change the formula in *CalculateDeviation()*.

## Important note

**Before using this program see the [licence](https://github.com/brjdenis/VarianESAPI-HalcyonGantryAngle/blob/master/LICENSE) and make sure you understand it. The program comes with absolutely no guarantees of any kind.**

```
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```


## LICENSE

Published under the MIT license. 