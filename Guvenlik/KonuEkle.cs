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
    public partial class KonuEkle : Form
    {
        public KonuEkle()
        {
            InitializeComponent();
        }

        SQLiteConnection bag;

        private void KonuEkle_Load(object sender, EventArgs e)
        {
            bag = fnk.bag();
            dataGridView1.AllowUserToAddRows = false;
        }

        SQLiteCommand cmd;
        SQLiteCommand cekme;
        fonk fnk = new fonk();

        ArrayList cmbsoruid = new ArrayList();
        void cagir(string ders)
        {
            cmbDers.Items.Clear();
            cmbsoruid.Clear();
            bag.Open();
            cekme = new SQLiteCommand("SELECT id,SoruTipi FROM GvnDersler WHERE Seviye='"+ders+"'", bag);
            SQLiteDataReader drcek = cekme.ExecuteReader();
            while (drcek.Read())
            {
                cmbDers.Items.Add(drcek["SoruTipi"].ToString());
                cmbsoruid.Add(drcek["id"]);
            }
            cekme.Dispose();
            drcek.Close();
            bag.Close();
        }
        private void cmbSeviye_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDers.Items.Clear();
            dataGridView1.DataSource = null;
            dataGridView1.Enabled = false;
            txtKonu.Enabled = false;
            SoruID = -1;
            button1.Enabled = false; // ekle
            button2.Enabled = false; // seçileni sil
            button3.Enabled = false; // seçileni düzenle

            switch(cmbSeviye.SelectedIndex)
            {
                case 0: // İlk Okul
                    cagir("İlk Okul");
                    break;
                case 1: // Orta Okul
                    
                    cagir("Orta Okul");
                    break;
                case 2: // Lise
                    cagir("Lise");
                    break;
            }
        }

        SQLiteDataAdapter adp;
        int SoruID = -1;

        private void cmbDers_SelectedIndexChanged(object sender, EventArgs e)
        {
            int cmbdersindex = cmbDers.SelectedIndex;
            if (cmbdersindex == -1)
            {

            }
            else
            {
                int cmbdersSoruid = -1;
                cmbdersSoruid = Convert.ToInt32(cmbsoruid[cmbdersindex]);

                SoruID = -1;
                bag.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id='" + cmbdersSoruid + "'", bag);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    SoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                bag.Close();

                if (SoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Konu Ekle", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    adp = new SQLiteDataAdapter("SELECT Konu_id,Konu_Adi FROM Konular WHERE Konu_SoruID='" + SoruID + "'", bag);
                    dataGridViewx();
                }
            }

        }

        DataTable dt = new DataTable();

        void dataGridViewx()
        {
            dt = new DataTable();
            adp.Fill(dt);
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[0].DisplayIndex = 2;
            dataGridView1.Columns[1].Visible = false; // id
            dataGridView1.Columns[2].HeaderText = "Konu Adı";
            dataGridView1.Columns[2].ReadOnly = true;

            dataGridView1.Enabled = true;
            txtKonu.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(cmbSeviye.Text == "" || cmbDers.Text ==  "" || txtKonu.Text == "")
            {
                MessageBox.Show("Gerekli Alanları Doldurunuz.", "Konu Ekle", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                bag.Open();
                cmd = new SQLiteCommand("INSERT INTO Konular (Konu_SoruID, Konu_Adi) VALUES ("+SoruID+", '"+fnk.strtext(txtKonu.Text)+"')", bag);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                bag.Close();

                dataGridViewx(); // güncelle data

                //form1 çıkış sağla
                bag.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET dersGuncelleme=1", bag);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                bag.Close();

                MessageBox.Show("Başarıyla Eklendi.", "Konu Ekle", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            DialogResult buton = MessageBox.Show("Silmek istediğinize emin misiniz ?", "Konu Ekle", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (buton == DialogResult.Yes)
            {

                ArrayList konuSilinenid = new ArrayList();
                ArrayList KonuAdlari = new ArrayList();

                foreach (DataGridViewRow r in dataGridView1.Rows)
                {
                    if (r.Cells[0].Value != null && (bool)r.Cells[0].Value)
                    {
                        konuSilinenid.Add(r.Cells[1].Value);
                        KonuAdlari.Add(r.Cells[2].Value.ToString());
                    }
                }

                //Konuları sil.
                bag.Open();
                for (int i = 0; i < konuSilinenid.Count; i++)
                {
                    cmd = new SQLiteCommand("DELETE FROM Konular WHERE Konu_id="+ konuSilinenid[i], bag);
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
                bag.Close();

                bag.Open();
                for (int c = 0; c < konuSilinenid.Count; c++)
                {
                    cmd = new SQLiteCommand("DELETE FROM GvnSorular WHERE Konu_id=" + konuSilinenid[c] + "", bag);
                    cmd.ExecuteNonQuery();
                }
                cmd.Dispose();
                bag.Close();

                dataGridViewx(); // güncelle

                //form1 çıkış sağla
                bag.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=1, dersGuncelleme=1", bag);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                bag.Close();


                //Haftaya programla devre dışı bırakma !
                bag.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET HaftayaProgramlamaPA='Pasif', HPOrtaOkul='Pasif', HPLise='Pasif' WHERE id=1", bag);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                bag.Close();

                MessageBox.Show("Güncellendi. Otomatik programla pasif durumuna getirildi. Lütfen aktifleştiriniz.", "Ders Ekle", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

            }
            else
            {
                // emin değilmiş :D
            }
        }

        int duzenlenecekiD = -1;
        private void button3_Click(object sender, EventArgs e)
        {
            duzenlenecekiD = Convert.ToInt32(dataGridView1.CurrentRow.Cells[1].Value);
            if(duzenlenecekiD != -1)
            {
                txtKonu.Text = dataGridView1.CurrentRow.Cells[2].Value.ToString();
                cmbSeviye.Enabled = false;
                cmbDers.Enabled = false;
                button1.Visible = false; // ekle kapat
                button2.Visible = false; // seçileni sil.
                button5.Visible = true;
                button4.Visible = true; // güncellemeyi aç.

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (txtKonu.Text == "")
            {
                MessageBox.Show("Gerekli Alanları Doldurunuz.", "Konu Ekle", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {

                bag.Open();
                cmd = new SQLiteCommand("UPDATE Konular SET Konu_Adi='"+fnk.strtext(txtKonu.Text)+"' WHERE Konu_id=" + duzenlenecekiD, bag);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                bag.Close();

                bag.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET dersGuncelleme=1", bag);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                bag.Close();

                cmbSeviye.Enabled = true;
                cmbDers.Enabled = true;
                button1.Visible = true; // ekle kapat
                button2.Visible = true; // seçileni sil.
                button5.Visible = false;
                button4.Visible = false; // güncellemeyi aç.
                txtKonu.Text = "";
                duzenlenecekiD = -1;
                int hfz1 = cmbSeviye.SelectedIndex;
                int hfz2 = cmbDers.SelectedIndex;
                if (cmbSeviye.SelectedIndex == 0) cmbSeviye.SelectedIndex = 1;
                else cmbSeviye.SelectedIndex = 0;
                cmbSeviye.SelectedIndex = hfz1;
                cmbDers.SelectedIndex = hfz2;

                MessageBox.Show("Başarıyla Güncellendi.", "Konu Ekle", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            cmbSeviye.Enabled = true;
            cmbDers.Enabled = true;
            button1.Visible = true; // ekle kapat
            button2.Visible = true; // seçileni sil.
            button5.Visible = false;
            button4.Visible = false; // güncellemeyi aç.
            txtKonu.Text = "";
        }

    }
}
