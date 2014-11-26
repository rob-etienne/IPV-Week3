using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            init();

            // TO DO: Read the color image from file
            Image<Bgr, Byte> image = ;
            CvInvoke.cvShowImage("Original image", image);

            // TO DO: Convert to gray level image, Gaussian blur the gray level image
                        Image<Gray, Byte> grayImage = ;
            
            // Make a copy
            Image<Gray, Byte> grayImageClone = grayImage.Clone();

            // TO DO: Apply adaptive threshold to the gray level image; try for blockSize: 5, and for param1: new Gray(2)
            
            CvInvoke.cvShowImage("After Adaptive Threshold", grayImage);

            // Find the lrgest object in the thresholded image
            findLargestObject(grayImage, 100, grayImage.Width, grayImage.Height);
            CvInvoke.cvShowImage("Largest Object", grayImage);

            // Find the corner points of the largest object
            Point LU, RU, LB, RB;
            findCornerPoints(grayImage, out LU, out RU, out LB, out RB);
            PointF[] src = { LU, RU, LB, RB };
            PointF[] dst = { new Point(0, 0), new Point(image.Width - 1, 0), new Point(0, image.Height - 1), new Point(image.Width - 1, image.Height - 1) };
            image.Draw(new LineSegment2D(LU, new Point(0, 0)), new Bgr(0, 0, 255), 1);
            image.Draw(new LineSegment2D(RU, new Point(image.Width - 1, 0)), new Bgr(0, 255, 0), 1);
            image.Draw(new LineSegment2D(LB, new Point(0, image.Height - 1)), new Bgr(255, 0, 0), 1);
            image.Draw(new LineSegment2D(RB, new Point(image.Width - 1, image.Height - 1)), new Bgr(255, 255, 0), 1);
            CvInvoke.cvShowImage("Corners", image);

            // Stretch the copy such that the corner points of the largest object are in the corners of the image
            Image<Gray, byte> imgpersp = grayImageClone.WarpPerspective<double>(CameraCalibration.GetPerspectiveTransform(src, dst), INTER.CV_INTER_LINEAR, WARP.CV_WARP_DEFAULT, new Gray(255));
            imgpersp = imgpersp.ThresholdAdaptive(new Gray(255), ADAPTIVE_THRESHOLD_TYPE.CV_ADAPTIVE_THRESH_MEAN_C, THRESH.CV_THRESH_BINARY_INV, 5, new Gray(2));
            CvInvoke.cvShowImage("Warp+threshold", imgpersp);

            // Extract all the cells one by one, and check if there is a digit in it
            Image<Gray, byte> cell;
            DigitRecognizer dr = new DigitRecognizer();
            dr.train(@"train-images.idx3-ubyte", @"train-labels.idx1-ubyte");
            for (int j = 0; j < 9; j++)
            {
                for (int i = 0; i < 9; i++)
                {
                    // TO DO: Extract cell [i,j]
                    cell = ;

                    // TO DO: Find the largest object in that cell, that has an area of at least 150 pixels,  a width of at most 70% of the width of the cell and 
                    // a height of at most 85% of the height of the cell 
                    Rectangle obj = ;
                    if (obj.Width * obj.Height > 0)
                    {
                        // Put the digit in the center of the cell
                        cell = center(cell, new Point(obj.X + obj.Width / 2, obj.Y + obj.Height / 2));
                        imageBoxMatrix[i,j].Image = cell;
                        // Classify the digit
                        int number = (int)dr.classify(cell);
                        Console.Write(number);
                    }
                    else
                    {
                        imageBoxMatrix[i, j].Image = cell;
                        Console.Write("_");
                    }
                }
                Console.WriteLine();
            }
        }

        private Emgu.CV.UI.ImageBox[,] imageBoxMatrix = new Emgu.CV.UI.ImageBox[9, 9];

        private void init()
        {
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                {
                    imageBoxMatrix[i, j] = new Emgu.CV.UI.ImageBox();
                    ((System.ComponentModel.ISupportInitialize)(imageBoxMatrix[i, j])).BeginInit();
                    SuspendLayout();
                    imageBoxMatrix[i, j].Location = new System.Drawing.Point(12 + 55 * i, 12 + 55 * j);
                    imageBoxMatrix[i, j].Name = "imageBox" + i + j;
                    imageBoxMatrix[i, j].Size = new System.Drawing.Size(50, 50);
                    Controls.Add(imageBoxMatrix[i, j]);
                    ((System.ComponentModel.ISupportInitialize)(imageBoxMatrix[i, j])).EndInit();
                    ResumeLayout(false);
                }
        }

        /// <summary>
        /// The outline of the sudoku must be in the image.
        /// The method will find the corner points of the sudoku.
        /// </summary>
        /// <param name="grayImage">A black and white image that contains the outline of the sudoku in wht; black is considered to be background.</param>
        /// <param name="LUm">Will contain the left upper point of the sudoku.</param>
        /// <param name="RUm">Will contain the right upper point of the sudoku.</param>
        /// <param name="LBm">Will contain the left bottom point of the sudoku.</param>
        /// <param name="RBm">Will contain the right bottom point of the sudoku.</param>
        private void findCornerPoints(Image<Gray, byte> grayImage, out Point LUm, out Point RUm, out Point LBm, out Point RBm)
        {
            var lines2 = grayImage.HoughLinesBinary(5, Math.PI / 60.0, 40, 100, 24);
            var lines = lines2[0];
            double minDistLU = 1000, distLU;
            Point LU = new Point(0, 0);
            LUm = new Point(0, 0);
            double minDistRU = 1000, distRU;
            Point RU = new Point(grayImage.Width - 1, 0);
            RUm = new Point(grayImage.Width - 1, 0);
            double minDistLB = 1000, distLB;
            Point LB = new Point(0, grayImage.Height - 1);
            LBm = new Point(0, grayImage.Height - 1);
            double minDistRB = 1000, distRB;
            Point RB = new Point(grayImage.Width - 1, grayImage.Height - 1);
            RBm = new Point(grayImage.Width - 1, grayImage.Height - 1);
            foreach (var line in lines)
            {
                distLU = Math.Sqrt(Math.Pow(line.P1.X - LU.X, 2) + Math.Pow(line.P1.Y - LU.Y, 2));
                if (distLU < minDistLU)
                {
                    minDistLU = distLU;
                    LUm = line.P1;
                }
                distLU = Math.Sqrt(Math.Pow(line.P2.X - LU.X, 2) + Math.Pow(line.P2.Y - LU.Y, 2));
                if (distLU < minDistLU)
                {
                    minDistLU = distLU;
                    LUm = line.P2;
                }

                distRU = Math.Sqrt(Math.Pow(line.P1.X - RU.X, 2) + Math.Pow(line.P1.Y - RU.Y, 2));
                if (distRU < minDistRU)
                {
                    minDistRU = distRU;
                    RUm = line.P1;
                }
                distRU = Math.Sqrt(Math.Pow(line.P2.X - RU.X, 2) + Math.Pow(line.P2.Y - RU.Y, 2));
                if (distRU < minDistRU)
                {
                    minDistRU = distRU;
                    RUm = line.P2;
                }

                distLB = Math.Sqrt(Math.Pow(line.P1.X - LB.X, 2) + Math.Pow(line.P1.Y - LB.Y, 2));
                if (distLB < minDistLB)
                {
                    minDistLB = distLB;
                    LBm = line.P1;
                }
                distLB = Math.Sqrt(Math.Pow(line.P2.X - LB.X, 2) + Math.Pow(line.P2.Y - LB.Y, 2));
                if (distLB < minDistLB)
                {
                    minDistLB = distLB;
                    LBm = line.P2;
                }

                distRB = Math.Sqrt(Math.Pow(line.P1.X - RB.X, 2) + Math.Pow(line.P1.Y - RB.Y, 2));
                if (distRB < minDistRB)
                {
                    minDistRB = distRB;
                    RBm = line.P1;
                }
                distRB = Math.Sqrt(Math.Pow(line.P2.X - RB.X, 2) + Math.Pow(line.P2.Y - RB.Y, 2));
                if (distRB < minDistRB)
                {
                    minDistRB = distRB;
                    RBm = line.P2;
                }
            }
        }

        /// <summary>
        /// Moves the content of the image such that the point cog is in the center.
        /// </summary>
        /// <param name="image">The original image. Will be changed in place.</param>
        /// <param name="cog">The whole image will be moved such that this point is in the center of the image.</param>
        /// <returns>The image is returned after centering.</returns>
        private Image<Gray, byte> center(Image<Gray, byte> image, Point cog)
        {
            Point LUc = new Point(0, 0);
            Point RUc = new Point(image.Width - 1, 0);
            Point LBc = new Point(0, image.Height - 1);
            Point RBc = new Point(image.Width - 1, image.Height - 1);
            Point cc = new Point(-image.Width / 2, -image.Height / 2);
            PointF[] dst = { LUc, RUc, LBc, RBc };
            cog.Offset(cc);
            LUc.Offset(cog);
            RUc.Offset(cog);
            LBc.Offset(cog);
            RBc.Offset(cog);
            PointF[] src = { LUc, RUc, LBc, RBc };

            Image<Gray, byte> imgpersp = image.WarpPerspective<double>(CameraCalibration.GetPerspectiveTransform(src, dst), INTER.CV_INTER_LINEAR, WARP.CV_WARP_DEFAULT, new Gray(255));
            return imgpersp;
        }

        /// <summary>
        /// Find the largest white object in the image that is not too small, and not too big.
        /// </summary>
        /// <param name="grayImage">A black and white image; black is considered to be background.</param>
        /// <param name="minArea">The object must have at least this number of pixels.</param>
        /// <param name="maxBoundingBoxWidth">The object must have at most this width.</param>
        /// <param name="maxBoundingBoxHeight">The object must have at most this height.</param>
        /// <returns>If an object is found, the bounding box is returned; otherwise an empty bounding box is returned.</returns>
        public Rectangle findLargestObject(Image<Gray, byte> grayImage, int minArea, int maxBoundingBoxWidth, int maxBoundingBoxHeight)
        {
            double largestarea = 0;
            Point location = new Point(0, 0);
            MCvConnectedComp comp = new MCvConnectedComp();
            Rectangle boundingBox = new Rectangle();

            // Floodfill every white pixel with new Gray(64), and while doing that, keep track of the largest area that was filled (check that it has the right area, width and height).
            for (var y = 0; y < grayImage.Height; y++)
            {
                for (var x = 0; x < grayImage.Width; x++)
                {
                    if (grayImage.Data[y, x, 0] >= 128)
                    {
                        // TO DO: perform the floodfill on the pixel

                        
                        // Check whether the blob is larger then the ones found before, and also whether the area, width and height satisfy the requirements
                        if (comp.area > largestarea && comp.area > minArea && comp.rect.Width < maxBoundingBoxWidth && comp.rect.Height < maxBoundingBoxHeight)
                        {
                            largestarea = comp.area;
                            location = new Point(x, y);
                            boundingBox = comp.rect;

                        }
                    }
                }
            }

            // TO DO: If there is blob found that has the right area, width and height, then Floodfill it with white 
            

            // TO DO: Fill all the other blobs with black to remove them
            


            return boundingBox;
        }
    }
}