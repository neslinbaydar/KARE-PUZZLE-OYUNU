using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
namespace YazLabYabBoz
{
    public partial class Form1 : Form

    {
        bool debug = false;
        double EnYuksekSkor = -100.0;
        double Score = 0.0;
        public static Image[] imgarray = new Image[16]; //tanım:image dizisi

        public static Image orjinal_img = Image.FromFile("C:\\Users\\user\\Desktop\\YazLabYabBoz\\YazLabYabBoz\\YazLabYabBoz\\img\\dog.jpg");//resim okuma,resmn yolu
        public static int orjinal_size_x = orjinal_img.Width;
        public static int orjinal_size_y = orjinal_img.Height;

        public static Image resized_img;
        public static double resize_ratio = 500.0 / orjinal_size_x;
        public static int new_size_x = (int)(orjinal_size_x * resize_ratio);
        public static int new_size_y = (int)(orjinal_size_y * resize_ratio);

        // Kutu boyutu şu an resized resme göre belirleniyor.
        public static int kutu_x = new_size_x / 4;
        public static int kutu_y = new_size_y / 4;
        public static List<Control> button_list = new List<Control>();
        public static List<Control> kontrol_butonlari = new List<Control>();
        public static int button_list_begins_x = 178;
        public static int button_list_begins_y = 21;
        public static string skor_dosyasi_adresi = "enyuksekskor.txt";
        public static FileStream skor_dosyasi;

        public Form1()
        {
            InitializeComponent();
            Console.WriteLine("Start");

            define_resized_image();
            //* EventHelper eventHelper = new EventHelper();
            //* eventHelper.SetUpDragDropForEachButton(this);

            Console.WriteLine("Orjinal Boyut X: " + orjinal_size_x);
            Console.WriteLine("Orjinal Boyut Y: " + orjinal_size_y);
            Console.WriteLine("Resize Ratio: " + resize_ratio);
            Console.WriteLine("Resized Boyut X: " + new_size_x);
            Console.WriteLine("Resized Boyut Y: " + new_size_y);
            Console.WriteLine("Kutu Boyut X: " + kutu_x);
            Console.WriteLine("Kutu Boyut Y: " + kutu_y);

            // Buton boyutlarını ve koordinatlarını ayarla.
            set_button_list();
            kontrol_butonlari_ata();
            kontrol_butonlari_gizle();
            foreach (var button in button_list)
            {
                button.AllowDrop = true;
                button.DragEnter += buttons_DragEnter;
                button.DragDrop += buttons_DragDrop;
                button.MouseDown += buttons_MouseDown;
            }
            set_button_locations_and_sizes();
            set_highest_score();
            label2.Text = "";
            label4.Text = "";

        }

        public void kontrol_butonlari_ata()
        {
            for (int x = 17; x <= 32; x++)
            {
                var buttonName = string.Format("button{0}", x);
                var button = Controls.Find(buttonName, true).First();

                if (button != null)
                {
                    kontrol_butonlari.Add(button);
                }
            }
        }

        public void kontrol_butonlari_gizle()
        {
            if (!debug)
            {
                foreach (var button in kontrol_butonlari)
                {
                    button.Visible = false;
                }
                pictureBox1.Visible = false;
                this.Size = new Size(700, 480);
            }
        }

        private void btnKaristir_Click(object sender, EventArgs e)
        {
            // kontrol butonları 17 - 32 numaralılar
            for (int i = 0; i < kontrol_butonlari.Count(); i++)
            {
                kontrol_butonlari[i].Width = kutu_x;
                kontrol_butonlari[i].Height = kutu_y;

            }
            for (int i = 0; i < 4; i++) // sutun
            {
                for (int j = 0; j < 4; j++) // satir
                {
                    kontrol_butonlari[i * 4 + j].Location = new Point(625 + j * kutu_x, 21 + i * kutu_y);
                }
            }
            // kontrol butonları bitiş

            //resim parçalama ve image dizisine atama
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    var index = i * 4 + j;
                    imgarray[index] = new Bitmap(kutu_x, kutu_y);
                    var graphics = Graphics.FromImage(imgarray[index]);
                    graphics.DrawImage(resized_img, new Rectangle(0, 0, kutu_x, kutu_y), new Rectangle(j * kutu_x, i * kutu_y, kutu_x, kutu_y), GraphicsUnit.Pixel);
                    graphics.Dispose();
                    // kontrol butonları arkaplanını ayarla
                    kontrol_butonlari[index].BackgroundImage = imgarray[index]; //uretilenSayi a idi
                    // kontrol_butonlari[index].Text = "";
                    // kontrol butonları arkaplanını ayarla bitiş
                }
            }
            Random random_nesnesi = new Random();
            var karisik_sirali_sayilar = Enumerable.Range(0, 16).OrderBy(g => Guid.NewGuid()).Take(16).ToArray();
            int karisik_sirali_dizi_indexi = 0;
            foreach (var button in button_list)
            {
                var sira = karisik_sirali_sayilar[karisik_sirali_dizi_indexi];
                button.BackgroundImage = imgarray[sira];
                if (!debug) {
                    button.Text = "";
                }
                karisik_sirali_dizi_indexi += 1;
            }
            for (int i = 0; i < 16; i++)
            {
                var sonuc = dogru_yerde_mi_fonksiyonunu_hazirla_ve_cagir(button_list[i]);
                if (sonuc)
                {
                    butonu_sabitle(button_list[i]);
                    puan_ekle();
                    btnKaristir.Enabled = false;
                }
                else
                {
                    // puan_kir();
                }
            }
        }

        private void buttons_MouseDown(object sender, EventArgs e)
        {
            var evsahibi_buton = ((Button)sender);
            Console.WriteLine("Called: {0}_MouseDown", evsahibi_buton.Name);
            evsahibi_buton.DoDragDrop(evsahibi_buton, DragDropEffects.Move);
        }

        void buttons_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("Called: buttons_DragEnter");
            e.Effect = DragDropEffects.Move;
        }

        void buttons_DragDrop(object sender, DragEventArgs e)
        {
            var gelen_buton = ((Button)e.Data.GetData(typeof(Button)));
            var evsahibi_buton = ((Button)sender);
            Console.WriteLine("Called: {0}_DragDrop", evsahibi_buton.Name);
            Console.WriteLine(gelen_buton.Name + " buttonu {0} içine sürüklendi", evsahibi_buton.Name);
            if (gelen_buton.Name == evsahibi_buton.Name) { return; }
            Point temp = gelen_buton.Location;
            gelen_buton.Location = evsahibi_buton.Location;
            evsahibi_buton.Location = temp;

            // doğru yerde mi kontrol et, sürüklenen kontrol
            var sonuc = dogru_yerde_mi_fonksiyonunu_hazirla_ve_cagir(gelen_buton);
            var dogru_parca_no = satir_bul(gelen_buton) * 4 + sutun_bul(gelen_buton) + 1;
            Console.WriteLine("Sürüklenen Buton {0}, Doğru Parça Buton {1}", gelen_buton.Name, dogru_parca_no);
            if (sonuc)
            {
                butonu_sabitle(gelen_buton);
                puan_ekle();
            }
            else
            {
                puan_kir();
            }


            // doğru yerde mi kontrol et, kayan kontrol
            var sonuc_2 = dogru_yerde_mi_fonksiyonunu_hazirla_ve_cagir(evsahibi_buton);
            var dogru_parca_no_2 = satir_bul(evsahibi_buton) * 4 + sutun_bul(evsahibi_buton) + 1;
            Console.WriteLine("Kayan Buton {0}, Doğru Parça Buton {1}", evsahibi_buton.Name, dogru_parca_no_2);
            if (sonuc_2)
            {
                butonu_sabitle(evsahibi_buton);
                puan_ekle();
            }
            else
            {
                // puan_kir();
            }
        }

        void define_resized_image()
        {
            Console.WriteLine("Called: define_resized_image()");
            resized_img = (Image)(new Bitmap(new_size_x, new_size_y));
            var graphics = Graphics.FromImage(resized_img);
            graphics.DrawImage(orjinal_img, new Rectangle(0, 0, new_size_x, new_size_y), new Rectangle(0, 0, orjinal_size_x, orjinal_size_y), GraphicsUnit.Pixel);
            graphics.Dispose();
            Console.WriteLine("debug");
            pictureBox1.Image = orjinal_img;
        }

        void set_button_locations_and_sizes()
        {
            Console.WriteLine("Called: set_button_locations_and_sizes()");
            for (int i = 0; i < button_list.Count(); i++)
            {
                button_list[i].Width = kutu_x;
                button_list[i].Height = kutu_y;

            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    button_list[i * 4 + j].Location = new Point(button_list_begins_x + j * kutu_x, button_list_begins_y + i * kutu_y);

                }
            }
        }

        void set_highest_score()
        {
            var list = File.ReadLines(skor_dosyasi_adresi).Select(line => double.Parse(line.Trim())).ToList();
            foreach (var item in list)
            {
                if (item > EnYuksekSkor)
                {
                    EnYuksekSkor = item;
                }
            }
            skor_label.Text = EnYuksekSkor.ToString();
        }
        void set_button_list()
        {

            for (int x = 1; x <= 16; x++)
            {
                var buttonName = string.Format("button{0}", x);
                var button = Controls.Find(buttonName, true).First();

                if (button != null)
                {
                    button_list.Add(button);
                }
            }
        }

        bool dogru_yerde_mi_fonksiyonunu_hazirla_ve_cagir(Control button)
        {
            var parca_resim_indexi = satir_bul(button) * 4 + sutun_bul(button);
            Image parca_resim = imgarray[parca_resim_indexi];
            var sonuc = dogru_yerde_mi(button, parca_resim);
            return sonuc;
        }

        bool dogru_yerde_mi(Control buton, Image image)
        {

            //button19.BackgroundImage = buton1.BackgroundImage;
            //button20.BackgroundImage = image;
            List<Boolean> sonuclar = new List<Boolean>();
            Bitmap bmp = new Bitmap(image);
            Bitmap bmp2 = new Bitmap(buton.BackgroundImage);
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    Color pixel1 = bmp.GetPixel(x, y);
                    Color pixel2 = bmp2.GetPixel(x, y);
                    if (pixel1.R == pixel2.R && pixel1.G == pixel2.G && pixel1.B == pixel2.B)
                    {
                        sonuclar.Add(true);
                    }
                    else
                    {
                        sonuclar.Add(false);
                    }
                }
            }
            var uniq_sonuclar = sonuclar.Distinct();
            if (uniq_sonuclar.First().Equals(true) && uniq_sonuclar.Count() == 1)
            {
                Console.WriteLine("Doğru yerde!");
                return true;
            }
            else
            {
                Console.WriteLine("Yanlış yerde!");
                return false;
            }

            // buton1 'in arkaplan resmi ile buton2'nin arkaplan resmi aynı mı?
            // buton1 'in arkaplan resmi ile bellekteki sıralı 16'lık resim dizisindeki buton2 arkaplan resmi ile aynı mı?

        }

        int satir_bul(Control button)
        {
            var satir_indisi = (button.Location.Y - button_list_begins_y) / kutu_y;
            return satir_indisi;
        }

        int sutun_bul(Control button)
        {
            var sutun_indisi = (button.Location.X - button_list_begins_x) / kutu_x;
            return sutun_indisi;
        }

        void puan_ekle()
        {
            // puan ekle 
            Score = Score + 6.25;
            skor_label_guncelle_ve_oyun_bitti_ise_dosyaya_yaz();
        }

        void puan_kir()
        {
            // puan kır
            Score = Score - 6.25;
            skor_label_guncelle_ve_oyun_bitti_ise_dosyaya_yaz();
        }

        void skor_label_guncelle_ve_oyun_bitti_ise_dosyaya_yaz()
        {
            label4.Text = Score.ToString();
            if (Score > EnYuksekSkor)
            {
                skor_label.Text = Score.ToString();
            }
            if (puzzle_bitti_mi())
            {
                // Puzzle bitti skoru dosyanın sonuna ekle
                label2.Text = "Oyun Bitti!";
                skor_dosyasi = new FileStream(skor_dosyasi_adresi, FileMode.Append, FileAccess.Write);
                StreamWriter dosya = new StreamWriter(skor_dosyasi);
                dosya.WriteLine(Score.ToString());
                dosya.Close();
            }
        }
        bool puzzle_bitti_mi()
        {
            bool sonuc = true;
            foreach (var button in button_list)
            {
                if (button.AllowDrop == true)
                {
                    sonuc = false;
                }
            }
            return sonuc;

        }
        void butonu_sabitle(Control gelen_buton)
        {
            // butonu sabitle ve sürüklenemez yap
            gelen_buton.AllowDrop = false;
            gelen_buton.DragEnter -= buttons_DragEnter;
            gelen_buton.DragDrop -= buttons_DragDrop;
            gelen_buton.MouseDown -= buttons_MouseDown;
            Console.WriteLine("{0} Sabitlendi.", gelen_buton.Name);
        }


    }
}

