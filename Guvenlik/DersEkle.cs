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
using System.IO;

namespace Guvenlik
{
    public partial class DersEkle : Form
    {

        public DersEkle()
        {
            InitializeComponent();
        }

        SQLiteConnection baglanti;
        fonk fnk = new fonk();

        SQLiteCommand cmd;
        SQLiteCommand cekme;

        private void button1_Click(object sender, EventArgs e)
        {

            
            if (txtDersEkle.Text == "" || cmbSeviye.Text == "" || txtKonu.Text == "")
            {
                MessageBox.Show("Gerekli Yerleri Doldurunuz.", "Yönetici Panel,", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {

                int soniD = 0;

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruID FROM GvnDersler ORDER BY SoruID DESC", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    try
                    {
                        soniD = Convert.ToInt32(drcek["SoruID"]); // En sondaki sayıyı al.
                    }
                    catch
                    {

                    }
                    break;// döngüden çık.
                }
                drcek.Close();
                cekme.Dispose();
                baglanti.Close();

                if (soniD == 0)
                {
                    soniD += 1; // sonid boş geldiği için yani tamamen boş bu yüzden baştan başlangıç gibi 1 den başlıyor.

                    try
                    {
                        
                        // Derslere Ekleme
                        baglanti.Open();
                        cmd = new SQLiteCommand("INSERT INTO GvnDersler (SoruTipi, SoruID, Seviye) VALUES ('" + txtDersEkle.Text.Replace("'","''") + "', " + soniD + ", '"+cmbSeviye.Text+"')", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();

                        // Konuya ekleme
                        baglanti.Open();
                        cmd = new SQLiteCommand("INSERT INTO Konular (Konu_Adi, Konu_SoruID) VALUES ('" +txtKonu.Text.Replace("'","''") + "', " + soniD + ")", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();

                    }
                    catch ( Exception exx )
                    {
                        MessageBox.Show("Ders Eklerken Hata Oluştu.\n\n"+exx.ToString()+"", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }


                    baglanti.Open();
                    cmd = new SQLiteCommand("UPDATE GvnGenel SET dersGuncelleme=1", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();

                    datagridviewx(); // listeyi güncelle.


                    MessageBox.Show("Kaydedildi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    try
                    {
                        soniD += 1; // son sayıyı +1 arttır yeni id belirle.
                        // Derslere Ekleme
                        baglanti.Open();
                        cmd = new SQLiteCommand("INSERT INTO GvnDersler (SoruTipi, SoruID, Seviye) VALUES ('" + txtDersEkle.Text.Replace("'", "''") + "', " + soniD + ", '" + cmbSeviye.Text + "')", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();

                        // Konuya ekleme
                        baglanti.Open();
                        cmd = new SQLiteCommand("INSERT INTO Konular (Konu_Adi, Konu_SoruID) VALUES ('" + txtKonu.Text.Replace("'", "''") + "', " + soniD + ")", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                    catch ( Exception exx )
                    {
                        MessageBox.Show("Ders Eklerken Hata Oluştu.\n\n" + exx.ToString() + "\n\n" + exx.ToString() + "", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }

                    baglanti.Open();
                    cmd = new SQLiteCommand("UPDATE GvnGenel SET dersGuncelleme=1", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();

                    datagridviewx(); // listeyi güncelle.


                    MessageBox.Show("Kaydedildi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }

            }
        }

        SQLiteDataAdapter adp;

        DataTable dt = null;

        void datagridviewx()
        {
            dt = new DataTable();
            if (dt == null)
            {
            }
            else
            {
                adp.Fill(dt);
                dataGridView1.DataSource = dt;
                dataGridView1.Columns[0].DisplayIndex = 4; // seçim
                dataGridView1.Columns[0].ReadOnly = false;
                dataGridView1.Columns[1].Visible = false; // id
                dataGridView1.Columns[2].Visible = false; // soruid
                
                dataGridView1.Columns[3].HeaderText = "Dersler";
                dataGridView1.Columns[4].HeaderText = "Seviye";
                dataGridView1.Columns[4].ReadOnly = true; // SEVİYE
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // ekle liste güncelleme
            SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adp);

            builder.GetUpdateCommand();
            sorulariSil();
            adp.Update(dt);

            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET dersGuncelleme=1, eklemeVarmi=1", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();

            if (dersSilmeDegisiklik == true)
            {
                //Haftaya programla devre dışı bırakma !
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET HaftayaProgramlamaPA='Pasif', HPOrtaOkul='Pasif', HPLise='Pasif' WHERE id=1", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                dersSilmeDegisiklik = false;

                MessageBox.Show("Güncellendi. Otomatik programla pasif durumuna getirildi. Lütfen aktifleştiriniz.", "Ders Ekle", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                MessageBox.Show("Güncellendi.", "Ders Ekle", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        ArrayList silinecekDersler = new ArrayList();

        bool dersSilmeDegisiklik = false;

        private void button3_Click(object sender, EventArgs e)
        {
            // Seçilenleri kaldır.
            foreach (DataGridViewRow c in dataGridView1.Rows)
            {
                foreach (DataGridViewRow r in dataGridView1.Rows)
                {
                    if (r.Cells[0].Value != null && (bool)r.Cells[0].Value)
                    {
                        if (r.Cells[2].Value.ToString() != "")
                        {
                            silinecekDersler.Add(r.Cells[2].Value.ToString()); // silinen derslerın id lerini al.
                        }
                        dataGridView1.Rows.Remove(r); // datagridview den kaldır.
                        dersSilmeDegisiklik = true;


                    }
                    else if (r.Cells[0].Value == null)
                    {
                        // boş geç
                    }
                }
            }

        }

        private void DersEkle_Load(object sender, EventArgs e)
        {
            baglanti = fnk.bag();
            adp = new SQLiteDataAdapter("SELECT id,SoruID,SoruTipi,Seviye FROM GvnDersler", baglanti);
            datagridviewx();
            dataGridView1.AllowUserToAddRows = false;
        }

        ArrayList silinecekResimler = new ArrayList();

        void sorulariSil()
        {

            int listCount = silinecekDersler.Count;
            if (listCount == 0)
            {
            }
            else
            {
                try
                {
                    baglanti.Open();
                    for (int i = 0; i < listCount; i++)
                    {
                        // 1.Sorgu silinecek soruların image listesini al.
                        cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE SoruID=" + silinecekDersler[i] + "", baglanti);
                        SQLiteDataReader drsil = cekme.ExecuteReader();
                        while (drsil.Read())
                        {
                            if (drsil["ResimYol"].ToString() != "")
                            { silinecekResimler.Add(drsil["ResimYol"]); }
                        }
                        drsil.Close();
                        cekme.Dispose();
                        // 2.Sorgu soruları sil.
                        cmd = new SQLiteCommand("DELETE FROM GvnSorular WHERE SoruID=" + silinecekDersler[i] + "", baglanti);
                        cmd.ExecuteNonQuery();

                        cmd = new SQLiteCommand("DELETE FROM Konular WHERE Konu_SoruID=" + silinecekDersler[i] + "", baglanti);
                        cmd.ExecuteNonQuery();
                    }
                    cmd.Dispose();
                    baglanti.Close();
                }
                catch
                {
                    MessageBox.Show("Verileri silerken bir hata oluştu !", "Ders Ekle", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

                ResimSil(); // resimleri yollarını diziye aktardık ve silelim.
                silinecekDersler.Clear(); // yeni liste için temizle
            }
        }

        void ResimSil()
        {
            int uzunluk = silinecekResimler.Count;
            if (uzunluk == 0)
            {
            }
            else
            {
                try
                {
                    for (int i = 0; i < uzunluk; i++)
                    {
                        File.Delete(@"" + Application.StartupPath.ToString() + "\\" + silinecekResimler[i].ToString() + "");
                    }
                }
                catch
                {
                    MessageBox.Show("Resimleri silerken bir hata oluştu!", "Ders Ekle", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                silinecekResimler.Clear();
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            KonuEkle ke = new KonuEkle();
            ke.ShowDialog();
        }

    }
}
