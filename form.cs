using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OpenCvSharp;

namespace _171207_edge
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private IplImage SRCIMG;                                           //  元画像


        //=============================================================
        //
        //  ビットマップ画像エリアロック
        //
        //=============================================================
        private BitmapData LockBitmap(Bitmap lBitmap, IplImage image, int n)
        {
            if (n == 1)
            {
                return lBitmap.LockBits(
                                new Rectangle(0, 0, image.Width, image.Height),
                                ImageLockMode.ReadWrite,
                                PixelFormat.Format8bppIndexed
                        );
            }
            else if (n == 3)
            {
                return lBitmap.LockBits(
                                new Rectangle(0, 0, image.Width, image.Height),
                                ImageLockMode.ReadWrite,
                                PixelFormat.Format24bppRgb
                        );
            }

            return null;
        }


        //=============================================================
        //
        //  ビットマップ作成
        //
        //=============================================================
        private void CreateBitmap(out Bitmap bmp, IplImage image)
        {
            if (image.NChannels == 1)
            {
                bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format8bppIndexed);

                ColorPalette pal = bmp.Palette;

                for (int i = 0; i < 256; ++i)
                {
                    pal.Entries[i] = Color.FromArgb(i, i, i);
                }
                //pal.Entries[128] = Color.FromArgb(255, 0, 0); 

                bmp.Palette = pal;
            }
            else
            {
                bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format24bppRgb);
            }
        }


        //=============================================================
        //
        // 表示処理
        //
        //=============================================================
        private void ViewBitmap(PictureBox pbox, IplImage image)
        {
            int lpx;
            int lpy;
            int HT = image.Height;
            int WD = image.Width;
            Bitmap bmp = null;     //　ビットマップ

            CreateBitmap(out bmp, image);
            //  直接ビットマップに張り付ける
            BitmapData pBitmapData1 = LockBitmap(bmp, image, image.NChannels);
            unsafe
            {
                byte* pdest1 = (byte*)pBitmapData1.Scan0.ToPointer();
                byte data;
                int pos = 0;

                if (image.NChannels == 3)
                {
                    for (lpy = 0; lpy < HT; lpy++)
                    {
                        pos = lpy * pBitmapData1.Stride;
                        for (lpx = 0; lpx < WD; lpx++)
                        {
                            int offset = lpy * image.WidthStep + lpx * 3;
                            data = Marshal.ReadByte(image.ImageData, offset + 0);
                            pdest1[pos + 0] = data;
                            data = Marshal.ReadByte(image.ImageData, offset + 1);
                            pdest1[pos + 1] = data;
                            data = Marshal.ReadByte(image.ImageData, offset + 2);
                            pdest1[pos + 2] = data;
                            pos += 3;
                        }

                    }
                }
                else if (image.NChannels == 1)
                {
                    for (lpy = 0; lpy < HT; lpy++)
                    {
                        pos = lpy * pBitmapData1.Stride;

                        for (lpx = 0; lpx < WD; lpx++)
                        {
                            int offset = lpy * image.WidthStep + lpx;
                            data = Marshal.ReadByte(image.ImageData, offset);
                            pdest1[pos + 0] = data;
                            pos++;
                        }
                    }
                    ColorPalette pal = bmp.Palette;

                    for (int i = 0; i < 256; ++i)
                    {
                        pal.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    bmp.Palette = pal;
                }
            }



            bmp.UnlockBits(pBitmapData1);

            if (pbox.Image != null)
            {
                pbox.Image.Dispose();
            }
            pbox.Image = bmp;
        }


        //=============================================================
        //
        //　読込みおよび表示処理
        //
        //=============================================================
        private void LoadAndView(string Path)
        {
            //--------------------------------------------------------
            //  画像アンロード
            //--------------------------------------------------------
            if (SRCIMG != null)
            {
                Cv.ReleaseImage(SRCIMG);
                SRCIMG = null;
            }
            //--------------------------------------------------------
            SRCIMG = Cv.LoadImage(Path);                            //  
            //--------------------------------------------------------
            ViewBitmap(pictureBox2, SRCIMG);                        //  表示処理
        }

        //=============================================================
        //
        // 画像読込みボタン
        //
        //=============================================================
        private void btnLodeImage_Click(object sender, EventArgs e)
        {
            DialogResult mr;

            selectimage.Title = "画像を選ぶ";
            selectimage.FileName = "*.png";
            selectimage.Filter = "画像|*.png|すべて|*.*";
            mr = selectimage.ShowDialog();

            if (mr == DialogResult.OK)
            {
                LoadAndView(selectimage.FileName);
            }

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        //=============================================================
        //
        // Sobel変換処理
        //
        //=============================================================
        private void Sobel(PictureBox pbox, IplImage image)
        {
            IplImage gray;
            IplImage sobel;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            sobel = Cv.CreateImage(image.Size, BitDepth.U8, 1);

            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);

            Cv.Sobel(gray, sobel, 1, 1);

            ViewBitmap(pbox, sobel);
            Cv.ReleaseImage(gray);
            Cv.ReleaseImage(sobel);
        }

        private void btn_sobel_Click(object sender, EventArgs e)
        {
            Sobel(pictureBox2, SRCIMG);
        }

        //=============================================================
        //
        // Laplace変換処理
        //
        //=============================================================
        private void Laplace(PictureBox pbox, IplImage image)
        {
            IplImage gray;
            IplImage laplace;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            laplace = Cv.CreateImage(image.Size, BitDepth.U8, 1);

            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);

            Cv.Laplace(gray, laplace);

            ViewBitmap(pbox, laplace);
            Cv.ReleaseImage(gray);
            Cv.ReleaseImage(laplace);
        }

        //=============================================================
        //
        // Canny変換処理
        //
        //=============================================================
        private void Canny(PictureBox pbox, IplImage image)
        {
            IplImage gray;
            IplImage canny;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            canny = Cv.CreateImage(image.Size, BitDepth.U8, 1);

            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);

            Cv.Canny(gray, canny, 50, 200);

            ViewBitmap(pbox, canny);
            Cv.ReleaseImage(gray);
            Cv.ReleaseImage(canny);
        }

        //=============================================================
        //
        // 古典的Hough変換処理
        //
        //=============================================================
        private void HoughStd(PictureBox pbox, IplImage image)
        {
            IplImage gray;
            IplImage canny;
            IplImage hstd;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            canny = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            hstd = Cv.CreateImage(image.Size, BitDepth.U8, 3);

            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);
            Cv.Canny(gray, canny, 50, 200);
            Cv.CvtColor(canny, hstd, ColorConversion.GrayToRgb);

            CvMemStorage storage = new CvMemStorage();
            CvSeq lines = Cv.HoughLines2(canny, storage, HoughLinesMethod.Standard, 1, Math.PI / 180, 120);

            for(int i=0; i<lines.Total; i++)
            {
                CvLineSegmentPolar elem = lines.GetSeqElem<CvLineSegmentPolar>(i).Value;

                float rho = elem.Rho;
                float theta = elem.Theta;
                double a = Math.Cos(theta);
                double b = Math.Sin(theta);
                double x0 = a * rho;
                double y0 = b * rho;

                CvPoint pt1 = new CvPoint(Cv.Round(x0 + 10000 * (-b)), Cv.Round(y0 + 10000 * (a)));
                CvPoint pt2 = new CvPoint(Cv.Round(x0 - 10000 * (-b)), Cv.Round(y0 - 10000 * (a)));
                Cv.Line(hstd, pt1, pt2, CvColor.Red, 1, LineType.AntiAlias, 0);

            }
            lines.Dispose();
            storage.Dispose();




            ViewBitmap(pbox, hstd);
            Cv.ReleaseImage(gray);
            Cv.ReleaseImage(canny);
            Cv.ReleaseImage(hstd);
            pictureBox2.Invalidate();

        }

        //=============================================================
        //
        // 確率的Hough変換処理
        //
        //=============================================================
        private void HoughPbl(PictureBox pbox, IplImage image)
        {
            IplImage gray;
            IplImage canny;
            IplImage hPbl;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            canny = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            hPbl = Cv.CreateImage(image.Size, BitDepth.U8, 3);

            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);
            Cv.Canny(gray, canny, 50, 200);
            Cv.CvtColor(canny, hPbl, ColorConversion.GrayToRgb);

            CvMemStorage storage = new CvMemStorage();
            CvSeq lines = Cv.HoughLines2(canny, storage, HoughLinesMethod.Probabilistic, 1, Math.PI / 180, 50, 10, 10);

            for (int i = 0; i < lines.Total; i++)
            {
                CvLineSegmentPoint elem = lines.GetSeqElem<CvLineSegmentPoint>(i).Value;
                Cv.Line(hPbl,elem.P1, elem.P2, CvColor.Red, 1, LineType.AntiAlias, 0);
            }
            lines.Dispose();
            storage.Dispose();




            ViewBitmap(pbox, hPbl);
            Cv.ReleaseImage(gray);
            Cv.ReleaseImage(canny);
            Cv.ReleaseImage(hPbl);
            pictureBox2.Invalidate();

        }

        /*//=============================================================
        //
        // 円検出　Hough変換処理
        //
        //=============================================================
        private void Houghcircle(PictureBox pbox, IplImage image)
        {
            IplImage gray;
            IplImage canny;
            IplImage hcir;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            canny = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            hcir = Cv.CreateImage(image.Size, BitDepth.U8, 3);

            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);
            Cv.Canny(gray, canny, 50, 200);
            Cv.CvtColor(canny, hcir, ColorConversion.GrayToRgb);

            CvMemStorage storage = new CvMemStorage();
            CvSeq circl = Cv.HoughCircles(gray, storage, HoughCirclesMethod.Gradient, 2, 10, 160, 50, 10, 20);

            foreach (CvCircleSegment crcl in circl)
            {
                Cv.Circle(crcl.Center, int Radius, CvColor.Red, 2);
            }
            lines.Dispose();
            storage.Dispose();




            ViewBitmap(pbox, hcir);
            Cv.ReleaseImage(gray);
            Cv.ReleaseImage(canny);
            Cv.ReleaseImage(hcir);
            pictureBox2.Invalidate();

        }*/

        private void btn_Sobel_Click(object sender, EventArgs e)
        {
            Sobel(pictureBox2, SRCIMG);
        }

        private void btn_Laplace_Click(object sender, EventArgs e)
        {
            Laplace(pictureBox2, SRCIMG);
        }

        private void btn_Canny_Click(object sender, EventArgs e)
        {
            Canny(pictureBox2, SRCIMG);
        }

        private void btnHoughStandard_Click(object sender, EventArgs e)
        {
            HoughStd(pictureBox2, SRCIMG);
        }

        private void btnHoughPbl_Click(object sender, EventArgs e)
        {
            HoughPbl(pictureBox2, SRCIMG);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
    }
}
