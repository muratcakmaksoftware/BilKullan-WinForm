using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace Guvenlik
{
    public partial class GeriSayim : Form
    {
        public GeriSayim()
        {
            InitializeComponent();
        }

        SQLiteConnection baglanti;

        SQLiteCommand cekme;

        SQLiteCommand cmd;

        int dakika = 0;

        fonk fnk = new fonk();

        private void GeriSayim_Load(object sender, EventArgs e)
        {
            baglanti = fnk.bag();

            baglanti.Open();
            cekme = new SQLiteCommand("SELECT Dakika FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drdk = cekme.ExecuteReader();
            while (drdk.Read())
            {
                dakika = Convert.ToInt32(drdk["Dakika"]);
            }
            cekme.Dispose();
            drdk.Close();
            baglanti.Close();

            lblDakika.Text = Convert.ToString(dakika);
            lblSaniye.Text = "0";

            timer1.Enabled = true;

            timer1.Start();

        }
        int saniye;
        private void timer1_Tick(object sender, EventArgs e)
        {
            int dk = Convert.ToInt32(lblDakika.Text);
            saniye = Convert.ToInt32(lblSaniye.Text);
            if (dk == 0 && saniye == 0 || dk < 0) // zaman bitmis.
            {
                this.Close(); // kapat.
            }
            else
            {
                if (saniye == 0 || saniye < 0)
                {
                    dk = dk - 1;
                    lblDakika.Text = "" + dk + "";
                    lblSaniye.Text = "59";
                    saniye = 59;
                }
                else
                {
                    saniye--;
                    lblSaniye.Text = "" + saniye + "";
                }
            }

        }

        private void GeriSayim_FormClosing(object sender, FormClosingEventArgs e)
        {
            // sıfırlama

            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET Dakika=0", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();

        }
        

    }
}
