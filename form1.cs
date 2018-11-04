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


namespace _171102
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private IplImage SRCIMG;                                           //  ���摜


        //=============================================================
        //
        //  �r�b�g�}�b�v�摜�G���A���b�N
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
        //  �r�b�g�}�b�v�쐬
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
        // �\������
        //
        //=============================================================
        private void ViewBitmap(PictureBox pbox, IplImage image)
        {
            int lpx;
            int lpy;
            int HT = image.Height;
            int WD = image.Width;
            Bitmap bmp = null;     //�@�r�b�g�}�b�v

            CreateBitmap(out bmp, image);
            //  ���ڃr�b�g�}�b�v�ɒ���t����
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
        //�@�Ǎ��݂���ѕ\������
        //
        //=============================================================
        private void LoadAndView(string Path)
        {
            //--------------------------------------------------------
            //  �摜�A�����[�h
            //--------------------------------------------------------
            if (SRCIMG != null)
            {
                Cv.ReleaseImage(SRCIMG);
                SRCIMG = null;
            }
            //--------------------------------------------------------
            SRCIMG = Cv.LoadImage(Path);                            //  
            //--------------------------------------------------------
            ViewBitmap(pictureBox1, SRCIMG);                        //  �\������
        }

        //=============================================================
        //
        // �摜�Ǎ��݃{�^��
        //
        //=============================================================
        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult mr;

            selectimage.Title = "�摜��I��";
            selectimage.FileName = "*.png";
            selectimage.Filter = "�摜|*.png|���ׂ�|*.*";
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
        // �O���[�X�P�[���ϊ�����
        //
        //=============================================================
        private void GrayScale(PictureBox pbox,IplImage image)
        {
            IplImage gray;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);
            ViewBitmap(pbox, gray);
            Cv.ReleaseImage(gray);
        }

        //=============================================================
        //
        // �O���[�X�P�[���{�^��
        //
        //=============================================================
        private void btnGrayscale_Click(object sender, EventArgs e)
        {
            GrayScale(pictureBox1, SRCIMG);
        }

        //=============================================================
        //
        // ��l���ϊ�����
        //
        //=============================================================
        private void Binarization(PictureBox pbox,IplImage image)
        {
            IplImage gray;
            IplImage Binarization;

            gray = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            Cv.CvtColor(image, gray, ColorConversion.RgbToGray);

            Binarization = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            Cv.Threshold(gray, Binarization, 200, 255, ThresholdType.Binary ); //臒l160��2�l�摜�ɕϊ�
           // Cv.Threshold(gray, Binarization, 0, 255, ThresholdType.Binary | ThresholdType.Otsu); //臒l�������Őݒ�
            Cv.AdaptiveThreshold(gray,Binarization, 255);

            ViewBitmap(pbox, Binarization);
            Cv.ReleaseImage(gray);
            Cv.ReleaseImage(Binarization);




        }

        //=============================================================
        //
        // ��l���{�^��
        //
        //=============================================================
        private void btnBinarization_Click(object sender, EventArgs e)
        {
            Binarization(pictureBox1, SRCIMG);
        }

        //=============================================================
        //
        // �Z�s�A�ϊ�����
        //
        //=============================================================
        private void Sepia(PictureBox pbox,IplImage image)
        {
            IplImage hsv;
            IplImage hue;
            IplImage satulation;
            IplImage value;
            IplImage merge;
            IplImage sepia;

            int hval = 105;
            int sval = 90;

            hsv = Cv.CreateImage(image.Size, BitDepth.U8, 3);
            hue = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            satulation = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            value = Cv.CreateImage(image.Size, BitDepth.U8, 1);
            merge = Cv.CreateImage(image.Size, BitDepth.U8, 3);
            sepia = Cv.CreateImage(image.Size, BitDepth.U8, 3);

            Cv.CvtColor(image, hsv, ColorConversion.RgbToHsv);
            Cv.Split(image,hue, satulation, value,null);
            Cv.Set(hue,hval,null);
            Cv.Set(satulation,sval,null);
            Cv.Merge(hue, satulation, value, null, merge);

            Cv.CvtColor(merge, sepia, ColorConversion.HsvToRgb);

            ViewBitmap(pbox, sepia);

            Cv.ReleaseImage(hsv);
            Cv.ReleaseImage(hue);
            Cv.ReleaseImage(satulation);
            Cv.ReleaseImage(value);
            Cv.ReleaseImage(merge);
            Cv.ReleaseImage(sepia);




        }

        //=============================================================
        //
        // �Z�s�A�{�^��
        //
        //=============================================================
        private void btnSepia_Click(object sender, EventArgs e)
        {
            Sepia(pictureBox1, SRCIMG);
        }

        //=============================================================
        //
        // ���摜�{�^��
        //
        //=============================================================
        private void btnOrigin_Click(object sender, EventArgs e)
        {
            ViewBitmap(pictureBox1, SRCIMG);
        }
    }
}
