using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Bitmap ImagenAPeocesar; //La imagen original se almacena en una variable separada
        private int TamañoLinea = 3;//Tamaño de línea para resaltar el contorno encontrado
        byte gray(Color col)//fórmula para convertir un píxel de color a blanco y negro
        {
            return (byte)(0.3*col.R + 0.59*col.G + 0.11*col.B);
        }
        public Form1()
        {
            InitializeComponent();
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "JPEG files (*.jpg *.jpeg *.jfif)|*.jpg;*.jpeg;*.jfif"+
                "|PNG files (*.png)|*.png"+
                "|BMP files (*.bmp)|*.bmp"+
                "|TIF files (*.tif *.tiff)|*.tif;*.tiff"+
                "|GIF files (*.gif)|*.gif"+
                "|All files (*.jpg *.jpeg *.jfif *.png *.bmp *.tif *.tiff *.gif)|*.jpg;*.jpeg;*.jfif;*.png;*.bmp;*.tif;*.tiff;*.gif";
        }
        
        
        private void btnSubirImagen(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            Bitmap ImagenCargar = new Bitmap(openFileDialog1.FileName);
            ImagenAPeocesar = new Bitmap(ImagenCargar);
            for (int VueltaUno = 0; VueltaUno < ImagenCargar.Width; VueltaUno++)
            {
                for(int Vuelta2 = 0; Vuelta2 < ImagenCargar.Height; Vuelta2++)
                {
                    Color pix = ImagenCargar.GetPixel(VueltaUno, Vuelta2);
                    Color newpix = Color.FromArgb(pix.A, gray(pix), gray(pix), gray(pix));
                    ImagenCargar.SetPixel(VueltaUno, Vuelta2, newpix);
                }
            }
            CajaImagen.Image = ImagenCargar;
            CajaImagen.SizeMode = PictureBoxSizeMode.Zoom;
            BtnEjecutar.Enabled = true;
        }
        public Bitmap EscalaDeGris(Bitmap ImgCargada, int val)//obtenemos una imagen Blanco y Negro
        {
            Bitmap ImgBN = new Bitmap(ImgCargada);
            for (int i = 0; i < ImgBN.Width; i++)
            {
                for (int j = 0; j < ImgBN.Height; j++)
                {
                    var Pixel = ImgBN.GetPixel(i, j);
                    int r = Pixel.R;
                    int g = Pixel.G;
                    int b = Pixel.B;
                    if (r < val)
                        r = 0;
                    else
                        r = 255;
                    if (g < val)
                        g = 0;
                    else
                        g = 255;
                    if (b < val)
                        b = 0;
                    else
                        b = 255;
                    Color NuevoPixel = Color.FromArgb(ImgBN.GetPixel(i, j).A, r, g, b);
                    ImgBN.SetPixel(i, j, NuevoPixel);
                }
            }
            return ImgBN;
        }


        struct fiRo//Estructura de una línea única
        {
            public int GradosFi;//Fi
            public int ro;//Radio de la perpendicular a la recta
            public fiRo(int d, int r)
            {
                GradosFi = d;//Constructor
                ro = r;
            }
        };

        private void TransfoHough(object sender, EventArgs e)
        {
            try
            {
                Bitmap binarize = new Bitmap(ImagenAPeocesar);
                binarize = EscalaDeGris(ImagenAPeocesar, Int32.Parse(textBox1.Text));
                Bitmap bmp = new Bitmap(binarize);//Copiar la imagen original
                Dictionary<fiRo, int> map = new Dictionary<fiRo, int>();
                for (int i = 0; i < bmp.Width; i++)
                {
                    for (int j = 0; j < bmp.Height; j++)//Recorremos todos los pixeles
                    {
                        if (bmp.GetPixel(i, j).R == 0)//Si es un píxel negro, es un punto a considerar
                        {
                            for (int fiDegree = 0; fiDegree < 180; fiDegree++)
                            {
                                double fi = GradosARadianes(fiDegree);//Conversión de grados a radianes para mayor comodidad
                                double ro = BuscaRo(fi, i, j);//Obtener la longitud de la perpendicular
                                fiRo temp = new fiRo(fiDegree, (int)ro);
                                
                                if (!map.ContainsKey(temp))
                                {
                                    map.Add(temp, 0);
                                }
                                map[temp]++;
                            }
                        }
                    }
                }
                int maxval = map.Values.Max();//Número máximo de líneas idénticas
                var keyOfMax = map.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
                MessageBox.Show("MaxVal: " + maxval.ToString() + "; fi: " + keyOfMax.GradosFi + "; ro: " + keyOfMax.ro);//Datos de la línea detectada
                //Dibujamos la línea detectada
                draw(keyOfMax.GradosFi, keyOfMax.ro);
            }
            
            catch (Exception ex)
            {
                MessageBox.Show("Error " + ex) ;
            }
        }
        public void draw(int degree, int ro)
        {
            try
            {
                
                Graphics gr = Graphics.FromImage(CajaImagen.Image);
                
                int y0 = (int)((ro) / Math.Sin(GradosARadianes(degree)));
                int y1 = (int)((ro - CajaImagen.Image.Width * Math.Cos(GradosARadianes(degree)) / Math.Sin(GradosARadianes(degree))));
                gr.DrawLine(new Pen(Color.Red, TamañoLinea), new Point(0, y0), new Point(CajaImagen.Image.Width, y1));
                
                CajaImagen.Refresh();
                CajaImagen.Update();
                this.Invalidate();
            }
            catch(Exception err)
            {
                MessageBox.Show("Error " + err);
            }
          
        }
        double BuscaRo(double fi, int x, int y)//Obtener el radio de la perpendicular
        {
            return x * Math.Cos(fi) + y * Math.Sin(fi);//x e y: la coordenada del punto en la imagen desde la cual dibujamos
        }
        double GradosARadianes(int degree)//Convertimos a Radianes para funciones trigonométricas
        {
            return degree * (Math.PI / 180.0);
        }

    }
}
