using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace KonwerterTekstNaGrafika
{
    public partial class Form1 : Form
    {

        private Font czcionka = new Font(FontFamily.Families[1], 12);
        private int licznik;
        bool są_niezapisane = false;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                czcionka = fontDialog1.Font;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //sprawdzanie poprawności danych
            int szerokość, wysokość;
            bool szer = int.TryParse(textBox1.Text, out szerokość);
            bool wys = int.TryParse(textBox2.Text, out wysokość);
            if (wys == false || szer == false)
            {
                MessageBox.Show("Nieprawidłowy wymiar");
                return;
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                licznik = 0;
                Image img = new Bitmap(szerokość, wysokość);
                Graphics g = Graphics.FromImage(img);
                g.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);
                StreamReader sr = new StreamReader(openFileDialog1.FileName);
                string linia;
                Point p = new Point();
                p.X = 0;
                p.Y = 0;
                //p.Y = (int)-czcionka.Size;
                while (!sr.EndOfStream)
                {
                    linia = sr.ReadLine();
                    int długość_linii = (int)ZmierzStringa(linia, g);
                    if (długość_linii <= img.Width)
                    {
                        g.DrawString(linia, czcionka, Brushes.Black, p);
                        są_niezapisane = true;
                        //p.Y = (int)(p.Y + czcionka.Height - czcionka.Size);
                        if (p.Y + czcionka.Height + czcionka.Size > img.Height) //następna strona
                        {
                            NowaStrona(img, g, p);
                            p.Y = 0;
                        }
                        else
                        {
                            p.Y += czcionka.Height;
                        }
                    }
                    else //z podziałami wierszy
                    {
                        string słowo = null;
                        int długość_robocza = 0;
                        bool pierwszy = true;
                        //sprawdzanie czy cała linia zmieści sięna bieżącej stronie
                        linia = linia + ' '; //aby nie ignorował ostatniego znaku
                        if (p.Y != 0) //jak 0 oznacza nową stronęwięc nie ma sensu
                        {
                            float temp = 0;
                            string string_temp = null;
                            int ilość_lnii = 0;
                            for (int i = 0; i < linia.Length; i++)
                            {
                                if (linia[i] == ' ')
                                {
                                    //sprawdź czy mieści się w jednej linii
                                    temp = temp + ZmierzStringa(string_temp, g);
                                    if (temp > img.Width) //koniec linii
                                    {
                                        ilość_lnii++;
                                        temp = 0;
                                        string_temp = null;
                                    }
                                    else
                                    {
                                        string_temp = " ";
                                    }
                                }
                                else
                                {
                                    string_temp = string_temp + linia[i];
                                }
                            }
                            if (p.Y + czcionka.Height * ilość_lnii + czcionka.Size > img.Height)
                            {
                                NowaStrona(img, g, p);
                                p.Y = 0;
                            }
                        }
                        for (int i = 0; i < linia.Length; i++)
                        {
                            if (linia[i] == ' ' || i == linia.Length - 1)
                            {
                                float długość = ZmierzStringa(słowo, g);
                                if (długość > img.Width) //słowo nie zmieści się w żadnej linii
                                {
                                    string ciach = null;
                                    if (!pierwszy) //jeśli pierwsze słowo, linia gotowa, jeśli nie, trzeba przygotować
                                    {
                                        //zrób nową linię jeśli wolno
                                        if (p.Y + czcionka.Height + czcionka.Size > img.Height)
                                        {
                                            NowaStrona(img, g, p);
                                            p.Y = 0;
                                        }
                                        else
                                        {
                                            p.Y += czcionka.Height;
                                        }
                                    }
                                    p.X = 0; //na wszelki wypadek jeśli po innych mianach nie byłoby 0
                                    for (int j = 0; j < słowo.Length; j++) //początek przeglądu słowa z podziałami na linie
                                    {
                                        ciach = ciach + słowo[j];
                                        if (ZmierzStringa(ciach, g) > img.Width) //kiedy szerokość słowa przekroczy szerokość obrazu, trzeba umieścić je
                                        {
                                            //obetnij ostatni znak
                                            ciach = ciach.Substring(0, ciach.Length - 1);
                                            j--; //ponownie analizujemy obcięty znak
                                            g.DrawString(ciach, czcionka, Brushes.Black, p);
                                            pierwszy = false;
                                            if (p.Y + czcionka.Height + czcionka.Size > img.Height)
                                            {
                                                NowaStrona(img, g, p);
                                                p.Y = 0;
                                            }
                                            else
                                            {
                                                p.Y += czcionka.Height;
                                            }
                                            ciach = null;
                                        }
                                    }
                                    //reszta
                                    ciach = ciach + ' ';
                                    g.DrawString(ciach, czcionka, Brushes.Black, p);
                                    ciach = null;
                                }
                                else if(długość <= img.Width - długość_robocza) //słowo się zmieści w linii
                                {
                                    if (i != linia.Length) //jeśli nie ostatni znak, przygotuj miejsce na ewentualne kolejne słowo
                                        słowo = słowo + ' ';
                                    g.DrawString(słowo, czcionka, Brushes.Black, p);
                                    pierwszy = false;
                                    długość_robocza += (int)ZmierzStringa(słowo, g) + 1;
                                    p.X = długość_robocza;
                                }
                                else //słowo zmieści się w nowej pustej linii
                                {
                                    //czy można wejść do następnej linii? jeśli nie, następna strona
                                    if (p.Y + czcionka.Height + czcionka.Size > img.Height)
                                    {
                                        NowaStrona(img, g, p);
                                        p.Y = 0;
                                    }
                                    else
                                        p.Y += czcionka.Height;
                                    długość_robocza = 0;
                                    p.X = 0;
                                    if(i != linia.Length) //jeśli nie koniec linii, przygotuj miejsce
                                        słowo = słowo + ' ';
                                    g.DrawString(słowo, czcionka, Brushes.Black, p);
                                    pierwszy = false;
                                    długość_robocza += (int)ZmierzStringa(słowo, g) + 1;
                                    p.X = długość_robocza;
                                }
                                są_niezapisane = true;
                                słowo = null;
                            }
                            else
                            {
                                słowo = słowo + linia[i];
                            }
                        }
                        if (p.Y + czcionka.Height + czcionka.Size > img.Height)
                        {
                            NowaStrona(img, g, p);
                            p.Y = 0;
                        }
                        else
                        {
                            p.Y += czcionka.Height;
                        }
                    }
                    p.X = 0; //ustaw na nowej linii
                }
                if (są_niezapisane) ZapiszRysunek(img);
                sr.Close();
            }
            else
            {
                MessageBox.Show("Anulowano operację");
                return;
            }
        }

        public float ZmierzStringa(string s, Graphics g)
        {
            System.Drawing.Printing.PrintPageEventArgs do_mierzenia;
            Rectangle mb = new Rectangle(0, 0, 0, 0);
            System.Drawing.Printing.PageSettings ps = new System.Drawing.Printing.PageSettings();
            do_mierzenia = new System.Drawing.Printing.PrintPageEventArgs(g, mb, mb, ps);
            return do_mierzenia.Graphics.MeasureString(s, czcionka).Width;
        }

        public void ZapiszRysunek(Image img)
        {
            int indeks = openFileDialog1.FileName.LastIndexOf('\\');
            string temp = openFileDialog1.FileName.Substring(indeks+1);
            indeks = temp.LastIndexOf('.');
            temp = temp.Substring(0, indeks);
            
            if (!Directory.Exists("c:\\wyniki"))
            {
                Directory.CreateDirectory("c:\\wyniki");
            }
            Directory.SetCurrentDirectory("C:\\wyniki");
            string nazwa_pliku;
            do
            {
                nazwa_pliku = "c:\\wyniki\\" + temp;
                if (licznik < 10)
                    nazwa_pliku = nazwa_pliku + "00" + licznik.ToString();
                else if (licznik < 100)
                    nazwa_pliku = nazwa_pliku + "0" + licznik.ToString();
                else
                    nazwa_pliku = nazwa_pliku + licznik.ToString();
                nazwa_pliku = nazwa_pliku + ".jpeg";
                licznik++;
            } while (File.Exists(nazwa_pliku));
            img.Save(nazwa_pliku, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        public void NowaStrona(Image img, Graphics g, Point p)
        {
            ZapiszRysunek(img);
            są_niezapisane = false;
            //przygotowanie nowego rysunku
            g.FillRectangle(Brushes.White, 0, 0, img.Width, img.Height);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0: textBox1.Text = "480"; textBox2.Text = "234"; break;
            }
        }
    }

    
}
/*
            System.Drawing.Printing.PrintPageEventArgs do_mierzenia;
            Image szablon = new Bitmap("szablon.jpg");
            Image img = new Bitmap(szablon);
            Graphics g = Graphics.FromImage(img);
            Bitmap b = new Bitmap(100, 100);
            Point p = new Point(0,0);
            Font f = new Font(FontFamily.Families[3], 200);
            System.Drawing.Printing.PageSettings ps = new System.Drawing.Printing.PageSettings();
            Rectangle mb = new Rectangle(0, 0, 0, 0);
            do_mierzenia = new System.Drawing.Printing.PrintPageEventArgs(g, mb, mb, ps);
            int wysokość = (int)f.GetHeight(do_mierzenia.Graphics);
            p.Y = -100;
            g.DrawString("dwqwqwr", f, Brushes.Green, p);
            int line = f.Height;
            p.Y += line - 200;
            g.DrawString("ewwwrrwrwr", f, Brushes.Green, p);
            img.Save("plik.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            //b.Save("plik");
            string s;
            s = "Kotek";
            float szerokość = do_mierzenia.Graphics.MeasureString(s, f).Width;
*/