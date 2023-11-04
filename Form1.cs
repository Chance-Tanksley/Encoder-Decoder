using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Encoder
{
    public partial class Form1 : Form
    {
        //FIELDS
        public string widthnHeight;
        public int intWidth = 0;
        public int intHeight = 0;
        public Bitmap image;
        public string path;
        public string fileType;
        public string fileCmt;
        public string maxPixel;

        public Form1()
        {
            InitializeComponent();
        }

        //METHODS
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //open file dialog 
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                try
                {
                    path = openFileDialog1.FileName;
                    pictureBox1.Image = ConvertPPM(path);

                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        private Bitmap ConvertPPM(string path) 
        {
            //streamreader to read ppm file
            StreamReader streamReader = new StreamReader(path);
            
            string width = "";
            string height = "";
            string[] stringArry = new string[2];
            
            int r = 0;
            int g = 0;
            int b = 0;
           
            //for loop to collect lines from ppm file to then convert into a bitmap
            for(int i = 0; i <= 3; i++)
            {
                //if statements accessing the the first 4 lines
                if(i == 0)
                {
                    fileType = streamReader.ReadLine();
                }
                else if(i == 1)
                {
                    fileCmt = streamReader.ReadLine();
                }
                else if (i == 2)
                {
                    widthnHeight = streamReader.ReadLine();
                    stringArry = widthnHeight.Split(' ');
                    width = stringArry[0];
                    height = stringArry[1];
                    

                    intWidth = Convert.ToInt32(width);
                    intHeight = Convert.ToInt32(height);

                }
                else if (i == 3)
                {
                    maxPixel = streamReader.ReadLine();
                }
                
            }
            
            image = new Bitmap(intWidth, intHeight);
            
            //if statements to check if file is a P3 file or a P6 file
            if (fileType == "P3")
            {
                //for loop running through the image to collect rgb and set each pixel
                for (int y  = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        r = int.Parse(streamReader.ReadLine());
                        g = int.Parse(streamReader.ReadLine());
                        b = int.Parse(streamReader.ReadLine());
                        Color currentClr = new Color();
                        currentClr = Color.FromArgb(r, g, b);
                        image.SetPixel(x, y, currentClr);
                    }
                }
            }
            if(fileType == "P6")
            {
                streamReader.Close();
                char currentByte = ' ';
                FileStream fileStream = new FileStream(path,FileMode.Open);
                //for loop to skip the first four lines of the P6 file since data is already collected
                for(int i = 0; i < 4; i++) 
                {
                    currentByte = (char)fileStream.ReadByte();
                    while (currentByte != '\n')
                    {
                        currentByte = (char)fileStream.ReadByte();
                    }
                }
                //for loop running through the image to collect rgb and set each pixel
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        r = fileStream.ReadByte();
                        g = fileStream.ReadByte();
                        b = fileStream.ReadByte();
                        Color currentClr = new Color();
                        currentClr = Color.FromArgb(r, g, b);
                        image.SetPixel(x, y, currentClr);
                    }
                }
                fileStream.Close();
            }
            


            return image;
        }

        //method to prep the image for encoding 
        public void PrepImage()
        {
            
            for (int y = 0; y < intHeight; y++)
            {
                for (int x = 0; x < intWidth; x++)
                {
                    Color currentClr = new Color();
                    currentClr = image.GetPixel(x, y);
                    if (currentClr.B == 32)
                    {
                        currentClr = Color.FromArgb(currentClr.R, currentClr.G, 31);
                        image.SetPixel(x, y, currentClr);
                    }
                    else if(currentClr.B >= 48 && currentClr.B <= 57)
                    {
                        currentClr = Color.FromArgb(currentClr.R, currentClr.G, 47);
                        image.SetPixel(x, y, currentClr);
                    }
                    else if (currentClr.B >= 65 && currentClr.B <= 74)
                    {
                        currentClr = Color.FromArgb(currentClr.R, currentClr.G, 64);
                        image.SetPixel(x, y, currentClr);
                    }
                    else if (currentClr.B >= 75 && currentClr.B <= 90)
                    {
                        currentClr = Color.FromArgb(currentClr.R, currentClr.G, 91);
                        image.SetPixel(x, y, currentClr);
                    }

                }
            }
        }

        public Bitmap EncodeImage(string text)
        {
            int startY = 0;
            int startX = 0;
            Color currentClr = new Color();

            //for loop that runs through the message and inputs each char as a converted int into the blue in each color
            for (int index = 0; index < text.Length; index++)
            {
                if(startX == intWidth)
                {
                    startY++;
                    startX = 0;
                }
                currentClr = image.GetPixel(startX, startY);
                int currentBlue = text[index];
                currentClr = Color.FromArgb(currentClr.R, currentClr.G, currentBlue);
                image.SetPixel(startX, startY, currentClr);
                currentClr = Color.FromArgb(currentClr.R, currentClr.G, 31);
                startX ++;
            }
            return image;
        }

        public void ConvertBack(string path)
        {
            StreamWriter streamWrite = new StreamWriter(path);
            streamWrite.WriteLine(fileType);
            streamWrite.WriteLine(fileCmt);
            streamWrite.WriteLine(widthnHeight);
            streamWrite.WriteLine(maxPixel);

            //if and for loops that convert the bitmap back into a P3 ppm file
            if (fileType == "P3")
            {
                for (int y = 0; y < intHeight; y++)
                {
                    for(int x = 0; x < intWidth; x++) 
                    {
                        Color currentClr =  new Color();
                        currentClr = image.GetPixel(x, y);
                        streamWrite.WriteLine(currentClr.R.ToString());
                        streamWrite.WriteLine(currentClr.G.ToString());
                        streamWrite.WriteLine(currentClr.B.ToString());
                    }
                }
                streamWrite.Close();
            }  
        }

        //button click that runs the methods to encode the message into the image
        private void button1_Click(object sender, EventArgs e)
        {
            string text = textBox1.Text;
            text = text.ToUpper();
            PrepImage();
            pictureBox2.Image = EncodeImage(text);
        }

       

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                
                ConvertBack(saveFileDialog1.FileName);
                
            }
        }
    }
}
