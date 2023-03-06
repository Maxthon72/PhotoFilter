using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string fileName { get; set; } = string.Empty;
        public string fileNameSave { get; set; } = string.Empty;

        public Bitmap image { get; set; }


        int opt;

        public MainWindow()
        {
            InitializeComponent();
        }
        [DllImport(@"C:\Users\Macie\JAproject\WpfApp1\x64\Debug\ASMdll.dll")]
        unsafe static extern void NegativeASM(byte* begining, byte* output, int hight, int Instride);
        [DllImport(@"C:\Users\Macie\JAproject\WpfApp1\x64\Debug\ASMdll.dll")]
        unsafe private static extern void GrayscaleASM(byte* begining, byte* output, int width);//, int width, int Outstride

        public void imgBit() {

            this.image = new(fileName);
        }

        public void imgBitSave()
        {

            this.image = new(fileNameSave);
        }

        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }


        private void addFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "All supported graphics(*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|" +
            "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
            "Portable Network Graphic (*.png)|*.png|" +
            "Bitmaps (*.bmp)|*.bmp";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName == string.Empty)
            {
                addFile.Content = "You haven't selected a file. Try again!";
            }
            else
            {
                this.fileName = openFileDialog.FileName;
                imgBefore.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
            imgBit();
            photoData.Content = "File name: "+ this.fileName+"\nWidth: " + image.Width +"\nHeight"+ image.Height;
        }

        private unsafe void runProgram_Click(object sender, RoutedEventArgs e)
        {
            string filePath = Directory.GetCurrentDirectory();
            if (opt == 0)
            {
                imgAfter.Source = imgBefore.Source;
            }

            else if (opt == 1)
            {
                imgBit();
                BitmapData bmpData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly,
        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                long minTime = 0;
                long maxTime = 0;
                int stride = bmpData.Stride;
                int height = bmpData.Height;
                int width = bmpData.Width;
                byte* beginning = (byte*)bmpData.Scan0.ToPointer();

                Bitmap output = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                ColorPalette palette = output.Palette;
                for (int i = 0; i < 256; i++)
                {
                    System.Drawing.Color tmp = System.Drawing.Color.FromArgb(255, i, i, i);
                    palette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                }
                output.Palette = palette;

                BitmapData outputData = output.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                int outStride = outputData.Stride;

                byte* outputPtr = (byte*)outputData.Scan0.ToPointer();

                Stopwatch stopwatch = new Stopwatch();
                for (int i = 0; i < height; i++)
                {
                    stopwatch.Start();
                    GrayscaleASM(beginning + (i * stride), outputPtr + (i * outStride), width);
                    stopwatch.Stop();
                    if (minTime == 0 || minTime > stopwatch.ElapsedTicks)
                        minTime = stopwatch.ElapsedTicks;
                    if (maxTime == 0 || maxTime < stopwatch.ElapsedTicks)
                        maxTime = stopwatch.ElapsedTicks;
                    stopwatch.Reset();
                }
                

                output.UnlockBits(outputData);
                image.UnlockBits(bmpData);
                this.fileNameSave = filePath + "\\ASMGray.jpeg";
                output.Save(filePath + "\\ASMGray.jpeg", ImageFormat.Jpeg);
                imgAfter.Source = ToBitmapImage(output);
                output.Dispose();
                image.Dispose();
                timeLabel.Content = "Min number of ticks per pixel line: " + minTime + "\nMax number of ticks per pixel line: " + maxTime;
            }

            else if(opt == 2)
            {
                imgBit();
                BitmapData bmpData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly,
        System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                
                int stride = bmpData.Stride;
                int height = bmpData.Height;
                int width = bmpData.Width;
                byte* beginning = (byte*)bmpData.Scan0.ToPointer();

                Bitmap output = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

                ColorPalette palette = output.Palette;
                for (int i = 0; i < 256; i++)
                {
                    System.Drawing.Color tmp = System.Drawing.Color.FromArgb(255, i, i, i);
                    palette.Entries[i] = System.Drawing.Color.FromArgb(255, i, i, i);
                }
                output.Palette = palette;

                BitmapData outputData = output.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, 
                    System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
                int outStride = outputData.Stride;

                byte* outputPtr = (byte*)outputData.Scan0.ToPointer();

                long[] time = CSdll.Class1.GrayscaleCSharp(beginning, outputPtr, width, height,stride,outStride);

                output.UnlockBits(outputData);
                image.UnlockBits(bmpData);
                this.fileNameSave = filePath+"\\CSGray.jpeg";
                output.Save(filePath + "\\CSGray.jpeg", ImageFormat.Jpeg);
                imgAfter.Source = ToBitmapImage(output);
                output.Dispose();
                image.Dispose();
                timeLabel.Content = "Min number of ticks per pixel line: " + time[0] + "\nMax number of ticks per pixel line: " + time[1];
            }

            else if (opt == 3)
            {
                imgBit();
                BitmapData bmpData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly,
        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;
                int height = bmpData.Height;
                int width = bmpData.Width;
                long time = 0;
                Stopwatch stopwatch = new Stopwatch();
                
                byte* beginning = (byte*)bmpData.Scan0.ToPointer();

                Bitmap output = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, bmpData.Scan0);

                BitmapData outputData = output.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                int outStride = outputData.Stride;

                byte* outputPtr = (byte*)outputData.Scan0.ToPointer();

                stopwatch.Start();

                NegativeASM(beginning, outputPtr,height,stride);

                stopwatch.Stop();
                time = stopwatch.ElapsedTicks;

                output.UnlockBits(outputData);
                image.UnlockBits(bmpData);
                this.fileNameSave = filePath + "\\ASMNeg.jpeg";
                output.Save(filePath + "\\ASMNeg.jpeg", ImageFormat.Jpeg);
                imgAfter.Source = ToBitmapImage(output);
                output.Dispose();
                image.Dispose();
                timeLabel.Content = "Number of ticks to transfere whole picture: " + time;
            }

            else if (opt == 4)
            {
                imgBit();
                BitmapData bmpData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.WriteOnly,
        System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                int stride = bmpData.Stride;
                int height = bmpData.Height;
                int width = bmpData.Width;
                byte* beginning = (byte*)bmpData.Scan0.ToPointer();

                Bitmap output = new Bitmap(width, height, stride, System.Drawing.Imaging.PixelFormat.Format24bppRgb, bmpData.Scan0);

                BitmapData outputData = output.LockBits(new System.Drawing.Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                int outStride = outputData.Stride;

                byte* outputPtr = (byte*)outputData.Scan0.ToPointer();

                long time = CSdll.Class1.NegativeCSharp(beginning, outputPtr, height, stride);

                output.UnlockBits(outputData);
                image.UnlockBits(bmpData);
                this.fileNameSave = filePath+"\\CSNeg.jpeg";
                output.Save(filePath + "\\CSNeg.jpeg", ImageFormat.Jpeg);
                imgAfter.Source = ToBitmapImage(output);
                output.Dispose();
                image.Dispose();
                timeLabel.Content = "Number of ticks to transfere whole picture: " + time;
                
            }

        }

        private void save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Images|*.jpg";
            ImageFormat format = ImageFormat.Png;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
              
                imgBitSave();
                image.Save(sfd.FileName, ImageFormat.Jpeg);
                image.Dispose();
                
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedLib = chosenLanguage.SelectedItem.ToString().Split(new string[] { ": " }, StringSplitOptions.None).Last();
            if (selectedLib == "None")
                opt = 0;
            else if (selectedLib == "ASM Grayscale")
                opt = 1;
            else if (selectedLib == "C# Grayscale")
                opt = 2;
            else if (selectedLib == "ASM Negative")
                opt = 3;
            else if (selectedLib == "C# Negative")
                opt = 4;
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
