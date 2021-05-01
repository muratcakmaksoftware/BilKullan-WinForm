using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Collections;

namespace Guvenlik
{
    public partial class KullaniciAyarlari : Form
    {
        public KullaniciAyarlari()
        {
            InitializeComponent();
        }

        SQLiteConnection baglanti;

        fonk fnk = new fonk();

        SQLiteCommand cmd;
        SQLiteCommand cekme;
        SQLiteDataAdapter adp;
        DataTable dt;

        private void KullaniciAyarlari_Load(object sender, EventArgs e)
        {
            baglanti = fnk.bag();
            adp = new SQLiteDataAdapter("SELECT * FROM GvnKullanici WHERE 1", baglanti); // Gvn Soruların içindeki seçilen sorunun id ile filtreleyerek sadece o soruları al.;
            dataGridView1.AllowUserToAddRows = false;

            dataGridViewVeri();
        }

        void dataGridViewVeri()
        {

            dt = new DataTable();
            if (dt == null)
            {

            }
            else
            {
                adp.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView1.Columns[0].DisplayIndex = 6;
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[4].ReadOnly = true;
                dataGridView1.Columns[5].ReadOnly = true;
                dataGridView1.Columns[6].ReadOnly = true;
                

                dataGridView1.Columns[1].Visible = false; // id
                dataGridView1.Columns[2].HeaderText = "Kullanıcı Adı";
                dataGridView1.Columns[3].HeaderText = "Şifre";
                dataGridView1.Columns[4].HeaderText = "Adı";
                dataGridView1.Columns[5].HeaderText = "Yaş";
                dataGridView1.Columns[6].HeaderText = "Seviye";
            }
        }

        private void button1_Click(object sender, EventArgs e) // Kayıt Button
        {
            if(txtAdi.Text == "" || txtKadi.Text == "" || txtSifre.Text == "" || txtSifreTekrar.Text == "" || cmbSeviye.Text == "")
            {
                MessageBox.Show("Lütfen gerekli alanları doldurunuz.", "Kullanıcı Ayarları", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else if (txtSifre.Text != txtSifreTekrar.Text)
            {
                MessageBox.Show("Şifreler Eşleşmiyor.", "Kullanıcı Ayarları", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                txtSifre.Text = "";
                txtSifreTekrar.Text = "";
            }
            else
            {
                // Kaydet
                baglanti.Open();
                cmd = new SQLiteCommand("INSERT INTO GvnKullanici (Kadi,Sifre,Adi,Yas,Seviye) VALUES ('"+txtKadi.Text.Replace("'","''")+"', '"+txtSifre.Text.Replace("'","''")+"', '"+txtAdi.Text.Replace("'","''")+"', "+Convert.ToInt32(numYas.Value)+", '"+cmbSeviye.Text.ToString()+"')", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                MessageBox.Show("Kullanıcı Kayıt Edildi.", "Kullanıcı Ayarları", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                dataGridViewVeri();
            }
        }

        int duzenlenecekiD = -1;

        private void button4_Click(object sender, EventArgs e) // Seçileni Düzenle Button
        {


            duzenlenecekiD = Convert.ToInt32(dataGridView1.CurrentRow.Cells[1].Value); // seçilen satırın id değerini al.

            if (duzenlenecekiD == -1)
            {
                // boş değer. yani seçilmemiş.
                MessageBox.Show("Lütfen kullanıcı seçiniz.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string seviye = "";
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnKullanici WHERE id=" + duzenlenecekiD + "", baglanti);
                SQLiteDataReader drduzenle = cekme.ExecuteReader();
                while (drduzenle.Read())
                {
                    txtKadi.Text = drduzenle["Kadi"].ToString();
                    txtSifre.Text = drduzenle["Sifre"].ToString();
                    txtSifreTekrar.Text = drduzenle["Sifre"].ToString();
                    txtAdi.Text = drduzenle["Adi"].ToString();
                    numYas.Value = Convert.ToInt32(drduzenle["Yas"]);
                    seviye = drduzenle["Seviye"].ToString();
                }
                cekme.Dispose();
                drduzenle.Close();
                baglanti.Close();

               switch(seviye)
               {
                   case "İlk Okul":
                       cmbSeviye.SelectedIndex = 0;
                       break;
                   case "Orta Okul":
                       cmbSeviye.SelectedIndex = 1;
                        break;
                   case "Lise":
                       cmbSeviye.SelectedIndex = 2;
                       break;
                   default:
                       cmbSeviye.Text = "";
                       break;

               }


                button1.Visible = false;
                button2.Visible = true;
                button5.Visible = true;

            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            txtAdi.Text = "";
            txtKadi.Text = "";
            txtSifre.Text = "";
            txtSifreTekrar.Text = "";
            numYas.Value = 0;
            cmbSeviye.Text = "";
            duzenlenecekiD = -1;

            button1.Visible = true;
            button2.Visible = false;
            button5.Visible = false;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(txtAdi.Text == "" || txtKadi.Text == "" || txtSifre.Text == "" || txtSifreTekrar.Text == "" || cmbSeviye.Text == "")
            {
                MessageBox.Show("Lütfen gerekli alanları doldurunuz.", "Kullanıcı Ayarları", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else if (txtSifre.Text != txtSifreTekrar.Text)
            {
                MessageBox.Show("Şifreler Eşleşmiyor.", "Kullanıcı Ayarları", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                txtSifre.Text = "";
                txtSifreTekrar.Text = "";
            }
            else
            {
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnKullanici SET Kadi='"+txtKadi.Text.Replace("'","''")+"', Sifre='"+txtSifre.Text.Replace("'","''")+"', Adi='"+txtAdi.Text.Replace("'","''")+"', Yas="+Convert.ToInt32(numYas.Value)+", Seviye='"+cmbSeviye.Text.ToString()+"' WHERE id="+ duzenlenecekiD, baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                MessageBox.Show("Kullanıcı bilgileri güncellendi.", "Kullanıcı Ayarları", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                button1.Visible = true;
                button2.Visible = false;
                button5.Visible = false;
                dataGridViewVeri(); 
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            ArrayList silincekler = new ArrayList();
            ArrayList analiz = new ArrayList();
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells[0].Value != null && (bool)r.Cells[0].Value)
                {
                    silincekler.Add(r.Cells[1].Value); // seçilen id leri alıyor
                    analiz.Add(r.Cells[7].Value); // seçilen k_id analiz sonuçlarını silmek için.

                }
            }


            baglanti.Open();
            for (int i = 0; i < silincekler.Count; i++)
            {
                cmd = new SQLiteCommand("DELETE FROM GvnKullanici WHERE id=" + silincekler[i], baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            baglanti.Close();

            //Analiz sonuçlarını sil
            baglanti.Open();
            for (int i = 0; i < analiz.Count; i++)
            {
                cmd = new SQLiteCommand("DELETE FROM GvnAnaliz WHERE K_id=" + analiz[i], baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            baglanti.Close();

            MessageBox.Show("Kullanıcılar Başarıyla Silindi.", "Kullanıcı Ayarları", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            dataGridViewVeri();
        }

    }
}
