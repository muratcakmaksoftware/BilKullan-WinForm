using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net;
using Excel = Microsoft.Office.Interop.Excel;
using Ionic.Zip;

namespace Guvenlik
{
    public partial class YoneticiPaneli : Form
    {

        public YoneticiPaneli() // 1454
        {
            InitializeComponent();
        }

        SQLiteConnection baglanti;

        SQLiteCommand cmd;
        SQLiteCommand cekme;

        SQLiteDataAdapter adp;

        SQLiteDataAdapter adp2;

        public DataTable dt = null;

        public DataTable dt2 = null;

        public string secilenResimAdi = "";

        fonk fnk = new fonk();
        string DosyaUzantisi = "";
        private void button4_Click(object sender, EventArgs e)
        {
           
            OpenFileDialog openFile1 = new OpenFileDialog();
            DialogResult dr = new DialogResult();
            openFile1.Filter = "Resimler (*.BMP;*.JPG;*.PNG;*.GIF)|*.BMP;*.JPG;*.PNG;*.GIF";
            openFile1.Multiselect = true;
            openFile1.Title = "Resmi Seçin";
            openFile1.CheckFileExists = true;
            openFile1.CheckPathExists = true;

            dr = openFile1.ShowDialog();

            if (dr == DialogResult.OK)
            {
                try
                {

                    // TEK TEK EKLEME
                    //ımageList1.Images.Add(Image.FromFile(openFile2.FileName));

                    //Image secilenResim = Image.FromFile(openFile1.FileName);

                    /////////////////////////////////////////////////////////////////
                    
                    txtResimYol.Text = openFile1.FileName.ToString();

                    FileInfo ff = new FileInfo(openFile1.FileName); // Dosya uzantısı alma
                    DosyaUzantisi = ff.Extension;

                    secilenResimAdi = openFile1.SafeFileName.ToString();
                    secilenResimAdi = secilenResimAdi.Substring(0, secilenResimAdi.Length - DosyaUzantisi.Length);// sadece dosya adına çevir.

                    pictureBox1.ImageLocation = openFile1.FileName; 
                    

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Resim Eklenemedi.\n\n" + ex.ToString());
                }
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (richA.Text == "" || richB.Text == "" || richC.Text == "" || richD.Text == "" || richSoru.Text == "" || cmbCevap.Text == "" || cmbSeviye.Text == "" || cmbDers.Text == "" || cmbKonu.Text == "")
            {
                // Eksik var.
                MessageBox.Show("Lütfen gerekli alanları doldurunuz.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string resYol = "";
                if (secilenResimAdi == "")
                {
                    // boş geç
                }
                else
                {
                    secilenResimAdi += "-" + fnk.dateformat(DateTime.Now.ToShortDateString().ToString()) + "-" + fnk.dateformat(DateTime.Now.ToLongTimeString().ToString());
                    pictureBox1.Image.Save(fnk.systemYol() + "\\image\\" + secilenResimAdi + "" + DosyaUzantisi);
                    resYol = @"image\" + secilenResimAdi + ""+ DosyaUzantisi;
                }
                try
                {
                    baglanti.Open();
                    cmd = new SQLiteCommand("INSERT INTO GvnSorular (Soru, Ask,Bsk,Csk,Dsk,ResimYol, Cevap, SoruID, SoruTipi,Konu_Adi,Konu_id,Seviye) VALUES ('" + fnk.strtext(richSoru.Rtf) + "', '" + fnk.strtext(richA.Rtf) + "', '" + fnk.strtext(richB.Rtf) + "', '" + fnk.strtext(richC.Rtf) + "', '" + fnk.strtext(richD.Rtf) + "','" + fnk.strtext(resYol) + "', '" + cmbCevap.Text.ToString() + "', " + SoruID + ", '" + cmbDers.Text.ToString() + "', '"+cmbKonu.Text+"', '"+Konu_id+"', '"+cmbSeviye.Text+"')", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();

                    datagridViewx(); // listeyi yenileme.
                    secilenResimAdi = "";

                    // Yenileme işlemleri.
                    pictureBox1.Image = null;
                    richSoru.Text = "";
                    richA.Text = "";
                    richB.Text = "";
                    richC.Text = "";
                    richD.Text = "";
                    txtResimYol.Text = "";
                    cmbCevap.Items.Clear();
                    cmbCevap.Items.Add("A");
                    cmbCevap.Items.Add("B");
                    cmbCevap.Items.Add("C");
                    cmbCevap.Items.Add("D");
                    

                    baglanti.Open();
                    cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=1", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();

                    MessageBox.Show("Kaydedildi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                catch ( Exception exx )
                {
                    MessageBox.Show("Kayıt Hatası Oluştu.\n\n" + exx.ToString() + "", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            baglanti.Open();
            if (silinenSoruid.Count == 0) // eğer hiç silinen id yoksa soru silinmemiş demektir.
            {

            }
            else
            { // Eğer soru silinmiş ise güncelle.                

                for (int i = 0; i < silinenSoruid.Count; i++)
                {
                    cmd = new SQLiteCommand("DELETE FROM GvnSorular WHERE id=" + silinenSoruid[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
                // resim sil metoduna gönderildi.
                resimSil();
                datagridViewx(); // güncel listi yazdır.

                // checkedli olan soru siliniyor. bu yüzden haftaya programlamayı baştan programlamasını istiyoruz.
                // silinen sorular var pasife cevirip bastan ayarlamasını sagla. otomatik programı.

                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=1, HaftayaProgramlamaPA='Pasif'", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
            }

            MessageBox.Show("Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);    
        }

        ArrayList silinenSoruid = new ArrayList();
        private void button2_Click(object sender, EventArgs e)
        {
            // düzenleme işlemi iptal
            duzenlenecekiD = -1;
            button1.Visible = true;
            button13.Visible = false;
            button14.Visible = false;
            cmbDers.Enabled = true;
            richSoru.Text = "";
            richA.Text = "";
            richB.Text = "";
            richC.Text = "";
            richD.Text = "";
            txtResimYol.Text = "";
            pictureBox1.Image = null;
            secilenResimAdi = "";
            cmbCevap.Items.Clear();
            cmbCevap.Items.Add("A");
            cmbCevap.Items.Add("B");
            cmbCevap.Items.Add("C");
            cmbCevap.Items.Add("D");
            
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                if (r.Cells[0].Value != null && (bool)r.Cells[0].Value)
                {
                    if (r.Cells[7].Value.ToString() != "" && r.Cells[7].Value != null)
                    { silinenResim.Add(r.Cells[7].Value.ToString()); }// silinen resimi diziye ekle ve silinme işlemine gönderilcek sonra silinecek.
                    silinenSoruid.Add(r.Cells[1].Value.ToString()); // silinen soru id leri
                        
                    dataGridView1.Rows.Remove(r); // datagridview den kaldır.
                }
            }
            
        }

        ArrayList konuidleri;
        ArrayList konu_adlari;
        int SoruID = -1;
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // cmbDers
        {
            // Sıfırlama işlemleri
            silinenResim.Clear();
            Konu_id = -1;
            kapat();
            cmbKonu.Items.Clear();

            int cmbdersindex = cmbDers.SelectedIndex;
            if (cmbdersindex == -1)
            {

            }
            else
            {
                int cmbdersSoruid = -1;
                cmbdersSoruid = Convert.ToInt32(cmbsoruid[cmbdersindex]);

                SoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id='" + cmbdersSoruid + "'", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    SoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (SoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    konuidleri = new ArrayList();
                    konu_adlari = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + SoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {
                        cmbKonu.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        konuidleri.Add(drveri["Konu_id"]);
                        konu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();
                }
            }

        }

        void datagridViewx()
        {
            dt = new DataTable();
            if (dt == null)
            {
            }
            else
            {
                adp.Fill(dt);
                dataGridView1.DataSource = dt;

                dt.Columns.Add("sira");
                int toplamveri = dt.Rows.Count;
                int artim = 1;
                foreach (DataRow r in dt.Rows)
                {
                    r["sira"] = artim;
                    artim++;
                }

                dataGridView1.Columns[0].DisplayIndex = 9; //checkbox
                dataGridView1.Columns["sira"].DisplayIndex = 0;
                dataGridView1.Columns[0].ReadOnly = false; // Checkbox
                dataGridView1.Columns[1].ReadOnly = true;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].ReadOnly = true;
                dataGridView1.Columns[4].ReadOnly = true;
                dataGridView1.Columns[5].ReadOnly = true;
                dataGridView1.Columns[6].ReadOnly = true;
                dataGridView1.Columns[7].ReadOnly = true;
                dataGridView1.Columns[8].ReadOnly = true;
                dataGridView1.Columns["sira"].ReadOnly = true;

                dataGridView1.Columns["sira"].Width = 86;
                dataGridView1.Columns[2].Width = 86;
                dataGridView1.Columns[3].Width = 86;
                dataGridView1.Columns[4].Width = 86;
                dataGridView1.Columns[5].Width = 86;
                dataGridView1.Columns[6].Width = 86;
                dataGridView1.Columns[7].Width = 86;
                dataGridView1.Columns[8].Width = 86;

                dataGridView1.Columns["sira"].HeaderText = "Sıra";
                dataGridView1.Columns[1].Visible = false; // id
                dataGridView1.Columns[2].HeaderText = "Soru";
                dataGridView1.Columns[3].HeaderText = "A Şıkkı";
                dataGridView1.Columns[4].HeaderText = "B Şıkkı";
                dataGridView1.Columns[5].HeaderText = "C Şıkkı";
                dataGridView1.Columns[6].HeaderText = "D Şıkkı";
                dataGridView1.Columns[7].HeaderText = "Resim";
                dataGridView1.Columns[8].HeaderText = "Cevap";
                // rich convert normal text
                
                foreach (DataGridViewRow c in dataGridView1.Rows)
                {
                    if (c.Cells[2].Value != null)
                    {
                        richSoru.Rtf = c.Cells[2].Value.ToString();
                        c.Cells[2].Value = richSoru.Text;

                        richA.Rtf = c.Cells[3].Value.ToString();
                        c.Cells[3].Value = richA.Text;

                        richB.Rtf = c.Cells[4].Value.ToString();
                        c.Cells[4].Value = richB.Text;

                        richC.Rtf = c.Cells[5].Value.ToString();
                        c.Cells[5].Value = richC.Text;

                        richD.Rtf = c.Cells[6].Value.ToString();
                        c.Cells[6].Value = richD.Text;
                    }
                }
                // temizle.
                richSoru.Text = "";
                richA.Text = "";
                richB.Text = "";
                richC.Text = "";
                richD.Text = "";
            }



        }


        public string bugunTarih = "";

        private void YoneticiPaneli_Load(object sender, EventArgs e)
        {
            baglanti = fnk.bag();
            // sifre girişi için başlangıçta formu küçültme.
            
            this.Size = new Size(331, 160);

            // Login Ekranı
            panel2.Visible = true;
            tabControl1.Visible = false;

        }


        string seviye = "";
        void Hatirlat () // LOAD
        {
            
            label35.Text = "*İnternet Üzerinden Güncelleme yapıldığında manuel olarak eklenen sorular\nsilinecektir.";
            lblDownloadSure.Text = "";
            // Yardım Mesajı
            toolTip1.ToolTipTitle = "Bilgi";
            toolTip1.ToolTipIcon = ToolTipIcon.Info;
            toolTip1.IsBalloon = true;
            toolTip1.SetToolTip(txtEmail, "Email ve şifrenizi girdiğinizde kendi email'inize Analiz sonuçlarını göndermiş olucaksınız.");
            toolTip1.SetToolTip(txtEmailSifre, "Email ve şifrenizi girdiğinizde kendi email'inize Analiz sonuçlarını göndermiş olucaksınız.");

            cmbAktarmaSec.SelectedIndex = 0;

            // gizli butonlar.
            button13.Visible = false;
            button14.Visible = false;

            dataGridView1.AllowUserToAddRows = false; // soru
            dataGridView3.AllowUserToAddRows = false; // otomatik
            dataGridView2.AllowUserToAddRows = false; // analiz
            dateTimePicker1.Checked = false;
            dateTimePicker2.Checked = false;
            bugunTarih = DateTime.Today.ToShortDateString();

            menuStrip1.Visible = false;

            richSoru.Enabled = false;
            txtResimYol.Enabled = false;
            richD.Enabled = false;
            richC.Enabled = false;
            richB.Enabled = false;
            richA.Enabled = false;
            cmbDers.Enabled = true;
            cmbCevap.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button10.Enabled = false;
            button12.Enabled = false;

            dataGridView1.Enabled = false;


            string cmbTestSoru = "";
            
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT TestSoruTuru, OneriYazisi,OneriYazisi2 FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drtestsoru = cekme.ExecuteReader();
            while (drtestsoru.Read())
            {
                cmbTestSoru = drtestsoru["TestSoruTuru"].ToString();
                txtOneri.Text = drtestsoru["OneriYazisi"].ToString();
                txtOneriYazisi2.Text = drtestsoru["OneriYazisi2"].ToString();
                break;
            }
            drtestsoru.Close();
            cekme.Dispose();
            baglanti.Close();

            int engel = -1;

            baglanti.Open();
            cekme = new SQLiteCommand("SELECT AnaGuvenlik FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drAnaGuvenlik = cekme.ExecuteReader();
            while (drAnaGuvenlik.Read())
            {
                engel = Convert.ToInt32(drAnaGuvenlik["AnaGuvenlik"]);
                break;
            }
            cekme.Dispose();
            drAnaGuvenlik.Close();
            baglanti.Close();

            if (engel == 1)
            {
                lblGuvenlikDurumu.Text = "Güvenlik Durumu : Aktif Engellenenler ctrl+alt+delete, alt+f4, alt+shift+esc, alt+tab vb.";
            }
            else
            {
                lblGuvenlikDurumu.Text = "Güvenlik Durumu : Pasif Engeller kaldırıldı.";
            }

            ///////////////////////////////////////
            // Hatırlatmalar

            int mailcmb = -1;
            seviye = "";
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT TestdgrCevapDk, TestSoruBit, Email, EmailSifre, EmailKodu, Seviye FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drx = cekme.ExecuteReader();
            while (drx.Read())
            {
                numTestDogruCvpDk.Value = Convert.ToInt32((drx["TestdgrCevapDk"]));
                numTestKacSoruBit.Value = Convert.ToInt32((drx["TestSoruBit"]));
                txtEmail.Text = Convert.ToString(drx["Email"].ToString());
                txtEmailSifre.Text = Convert.ToString(drx["EmailSifre"].ToString());
                mailcmb = Convert.ToInt32(drx["EmailKodu"]);
                seviye = drx["Seviye"].ToString();              

                break;
            }
            cekme.Dispose();
            drx.Close();
            baglanti.Close();

            if (mailcmb == -1) // boş
            {

            }
            else
            {
                cmbmail.SelectedIndex = mailcmb;
            }

            switch (seviye)
            {
                case "İlk Okul":
                    cmbSoruSeviye.SelectedIndex = 0;
                    break;
                case "Orta Okul":
                    cmbSoruSeviye.SelectedIndex = 1;
                    break;
                case "Lise":
                    cmbSoruSeviye.SelectedIndex = 2;
                    break;
            }
            kullaniciAnalizGetir();
            
        }

        void kullaniciAnalizGetir()
        {
            cmbKullanici.Items.Clear();
            kullaniciadi = new ArrayList();
            kullaniciId = new ArrayList();
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT * FROM GvnKullanici WHERE 1", baglanti);
            SQLiteDataReader drcek = cekme.ExecuteReader();
            while(drcek.Read())
            {
                cmbKullanici.Items.Add(drcek["Adi"].ToString());
                kullaniciadi.Add(drcek["Adi"].ToString());
                kullaniciId.Add(Convert.ToInt32(drcek["id"]));                
            }
            cekme.Dispose();
            drcek.Close();
            baglanti.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string sfx = "";
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT sifre FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drx = cekme.ExecuteReader();
            while (drx.Read())
            {
                sfx = (drx["sifre"].ToString());
            }

            cekme.Dispose();
            drx.Close();
            baglanti.Close();

            if (sfx == txtsifre.Text)
            {
                Hatirlat();
                panel2.Visible = false;
                tabControl1.Visible = true;
                menuStrip1.Visible = true;                
                timer2.Enabled = true;
                // formu asıl boyularına çevir.

                this.Size = new Size(873, 739);

                // Engelleri Aç
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET EngellensinMi=0", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                // Yönetici Giriş Yaptı Güvenlik kapatılıyor.
                try
                {
                    // Ekle. Bilgisayarı Kilitle

                    RegistryKey rkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies", true);
                    rkey.CreateSubKey("System", RegistryKeyPermissionCheck.Default);
                    rkey.Close();

                    RegistryKey rkey2 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey2.SetValue("DisableTaskMgr", 0);
                    rkey2.Close();

                    RegistryKey rkey3 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey3.SetValue("DisableLockWorkstation", 0);
                    rkey3.Close();

                    RegistryKey rkey4 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey4.SetValue("DisableChangePassword", 0);
                    rkey4.Close();

                    RegistryKey rkey5 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey5.SetValue("HideFastUserSwitching", 0);
                    rkey5.Close();

                    RegistryKey rkey6 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                    rkey6.SetValue("NoLogoff", 0);
                    rkey6.Close();
                }
                catch
                {
                }
                
            }
            else
            {
                MessageBox.Show("Şifre Yanlış!", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

        }

        ArrayList silinenResim = new ArrayList();

        void resimSil()
        {
            int uzunluk = silinenResim.Count;
            if (uzunluk == 0)
            {
            }
            else
            {
                try
                {
                    for (int i = 0; i < uzunluk; i++)
                    {
                        File.Delete(@""+fnk.systemYol().ToString()+"\\" + silinenResim[i] + "");
                    }
                    silinenResim.Clear();
                }
                catch (Exception exx)
                {
                    MessageBox.Show("Resmi Silerken Hata Oluştu :\n\n" + exx.ToString(),"Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            
        }


        

        private void timer1_Tick(object sender, EventArgs e)
        {
            doubleClick = 0;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            
        }

        private void YoneticiPaneli_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Engelleri Devam Ettir.
            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET EngellensinMi=1, ScreenEngel=1", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();
            //f1.engellensinMi = 1;
            // Ekle. Bilgisayarı Kilitle
            if (AnaGuvenlik == 1)
            {
                try
                {
                    RegistryKey rkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies", true);
                    rkey.CreateSubKey("System", RegistryKeyPermissionCheck.Default);
                    rkey.Close();

                    RegistryKey rkey2 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey2.SetValue("DisableTaskMgr", 1);
                    rkey2.Close();

                    RegistryKey rkey3 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey3.SetValue("DisableLockWorkstation", 1);
                    rkey3.Close();

                    RegistryKey rkey4 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey4.SetValue("DisableChangePassword", 1);
                    rkey4.Close();

                    RegistryKey rkey5 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey5.SetValue("HideFastUserSwitching", 1);
                    rkey5.Close();

                    RegistryKey rkey6 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                    rkey6.SetValue("NoLogoff", 1);
                    rkey6.Close();
                }
                catch
                {
                    }
            }
        }

        protected override System.Boolean ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                this.Close();
                return true;
            }
            else if (keyData == Keys.Enter)
            {
                button5.PerformClick();
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        DataGridView dw = new DataGridView();


        /////////////// ANALİZ //////////////////////


        public void datagridviewFiltre()
        {

            if (filtre == "Günlük")
            {

                DateTime dt = DateTime.Today;

                dw = new DataGridView(); // dw temizle.
                /*
                DateTime dt2 = dt.AddDays(-3); // salı
                sali = dt2.ToShortDateString() + " Salı";*/

                dt2 = new DataTable();
                adp2.Fill(dt2);
                dataGridView2.DataSource = dt2;
                dataGridView2.Columns[0].Visible = false; // id
                dataGridView2.Columns[1].HeaderText = "Tarih";
                dataGridView2.Columns[2].HeaderText = "Ders";
                dataGridView2.Columns[3].HeaderText = "Soru Sayısı";
                dataGridView2.Columns[4].HeaderText = "Doğru Cevap";
                dataGridView2.Columns[5].HeaderText = "Yanlış Cevap";
                dataGridView2.Columns[6].Visible = false; 


                dw = dataGridView2; // dw aktar bilgileri.

                DataView DV = new DataView(dt2); // dt2 içinde arama.

                bool durum = false; // kontrol var yok.
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT Tarih FROM GvnAnaliz WHERE K_id="+ Kid, baglanti);
                SQLiteDataReader dr = cekme.ExecuteReader();
                while (dr.Read())
                {
                    DV.RowFilter = string.Format("Tarih LIKE '%{0}%'", dt.ToShortDateString()); // text ile arama.

                    dw.DataSource = DV;

                    int satirsayisi = dw.RowCount;

                    if (satirsayisi >= 1)
                    {
                        durum = true;
                    }
                    else
                    {
                        durum = false;
                    }
                }
                cekme.Dispose();
                dr.Close();
                baglanti.Close();

                if (durum == false)
                {
                    MessageBox.Show("Kayıt Yok.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    // arama bulundu.
                }

                ArrayList dersSayisi = new ArrayList();
                int DersSayisi = 0;
                int ToplamSoru = 0;
                int ToplamDogruCevap = 0;
                int ToplamYanlisCevap = 0;
                // 2,3,4,5
                foreach (DataGridViewRow r in dataGridView2.Rows)
                {

                    if (r.Cells[2].Value == null)
                    {
                        // boş geç
                    }
                    else
                    {
                        dersSayisi.Add(Convert.ToString(r.Cells[2].Value));
                    }
                    ToplamSoru += Convert.ToInt32(r.Cells[3].Value);
                    ToplamDogruCevap += Convert.ToInt32(r.Cells[4].Value);
                    ToplamYanlisCevap += Convert.ToInt32(r.Cells[5].Value);
                }

                ArrayList ToplamDers = new ArrayList();

                if (dersSayisi.Count == 0)
                {
                    DersSayisi = 0;
                }
                else
                {
                    int dersSayiCount = dersSayisi.Count;
                    int ilkEkleme = 0;
                    for (int i = 0; i < dersSayiCount; i++)
                    {
                        if (ilkEkleme == 0)
                        {
                            ToplamDers.Add(dersSayisi[i]);
                            ilkEkleme++;
                        }
                        else
                        {
                            int dersCount2 = ToplamDers.Count;
                            bool aynikelimeVarmi = false;
                            for (int d = 0; d < dersCount2; d++)
                            {
                                if (ToplamDers[d].ToString() == dersSayisi[i].ToString()) // eğer listedeki ile eşleşirse türkçe ve türkçe ise
                                {
                                    aynikelimeVarmi = true;
                                    break; // aynısı var zaten ve çık.
                                }
                                else
                                {
                                    aynikelimeVarmi = false; //eğer eşleşmezse hiç ekle.
                                }
                            }

                            if (aynikelimeVarmi == true)
                            {
                                // eklenmedi.
                            }
                            else
                            {
                                // eklendi.
                                ToplamDers.Add(dersSayisi[i]);
                            }
                        }
                    }


                }

                DersSayisi = ToplamDers.Count;

                lblToplamDers.Text = "Toplam Ders : " + DersSayisi;
                lblToplamSoru.Text = "Toplam Soru : " + ToplamSoru.ToString();
                lblToplamDogruCevap.Text = "Toplam Doğru Cevap : " + ToplamDogruCevap.ToString();
                lblToplamYanlisCevap.Text = "Toplam Yanlış Cevap : " + ToplamYanlisCevap.ToString();

            }
            else if (filtre == "Haftalık")
            {

                ArrayList veritabaniDuzenle = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnAnaliz WHERE 1", baglanti);
                SQLiteDataReader drveri = cekme.ExecuteReader();
                while (drveri.Read())
                {
                    veritabaniDuzenle.Add(drveri["id"].ToString());
                }
                cekme.Dispose();
                drveri.Close();
                baglanti.Close();

                int xveriuzunluk = veritabaniDuzenle.Count;

                DateTime dt = DateTime.Today; // bugünün tarihi

                ArrayList Tarihler = new ArrayList(); // 7 gün eklenicek.

                for (int i = 0; i <= 6; i++) // bugun eklendıgı için 6 kere dönücek.
                {
                    DateTime dt2 = dt.AddDays(-i);
                    Tarihler.Add(dt2.ToShortDateString()); // çıkarılan günün tarihi ver.

                }

                DataTable tbl = new DataTable();

                // column isimleri
                tbl.Columns.Add("Tarih");
                tbl.Columns.Add("Ders");
                tbl.Columns.Add("Soru Sayısı");
                tbl.Columns.Add("Doğru Cevap");
                tbl.Columns.Add("Yanlış Cevap");
                bool eslesti = false;
                // arrayliste 
                if (xveriuzunluk == 0)
                {
                    MessageBox.Show("Veritabanında hiç kayıt yok!", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    DataRow[] rows = new DataRow[xveriuzunluk]; // veri tabanındakı kayıt kadar sayı.
                    int adet = 0;
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Tarih,Ders,SoruSayisi,dCevap,yCevap FROM GvnAnaliz WHERE K_id="+Kid, baglanti);
                    SQLiteDataReader dr = cekme.ExecuteReader();
                    while (dr.Read())
                    {
                        for (int i = 0; i <= 6; i++)
                        {
                            if (dr["Tarih"].ToString() == Tarihler[i].ToString())
                            {
                                eslesti = true;
                                DataRow row = tbl.NewRow();
                                row["Tarih"] = dr["Tarih"].ToString();
                                row["Ders"] = dr["Ders"].ToString();
                                row["Soru Sayısı"] = dr["SoruSayisi"].ToString();
                                row["Doğru Cevap"] = dr["dCevap"].ToString();
                                row["Yanlış Cevap"] = dr["yCevap"].ToString();

                                rows[adet] = row;
                                adet++;
                                break;

                            }
                        }

                    }
                    cekme.Dispose();
                    dr.Close();
                    baglanti.Close();
                    if (eslesti == true)
                    {
                        tbl = rows.CopyToDataTable();
                    }
                    else
                    {
                        MessageBox.Show("Kayıt Yok.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }

                    dataGridView2.DataSource = tbl;
                    
                }

                ArrayList dersSayisi = new ArrayList();
                int DersSayisi = 0;
                int ToplamSoru = 0;
                int ToplamDogruCevap = 0;
                int ToplamYanlisCevap = 0;
                // 1,2,3,4 sebebi ayri kendim manuel olarak listeledim bu yuzden id olmadigi icin 01234 seklinde oldu almamiz gereknelerde 1234 dur
                foreach (DataGridViewRow r in dataGridView2.Rows)
                {

                    if (r.Cells[1].Value == null)
                    {
                        // boş geç
                    }
                    else
                    {
                        dersSayisi.Add(Convert.ToString(r.Cells[1].Value));
                    }
                    ToplamSoru += Convert.ToInt32(r.Cells[2].Value);
                    ToplamDogruCevap += Convert.ToInt32(r.Cells[3].Value);
                    ToplamYanlisCevap += Convert.ToInt32(r.Cells[4].Value);
                }

                ArrayList ToplamDers = new ArrayList();

                if (dersSayisi.Count == 0)
                {
                    DersSayisi = 0;
                }
                else
                {
                    int dersSayiCount = dersSayisi.Count;
                    int ilkEkleme = 0;
                    for (int i = 0; i < dersSayiCount; i++)
                    {
                        if (ilkEkleme == 0)
                        {
                            ToplamDers.Add(dersSayisi[i]);
                            ilkEkleme++;
                        }
                        else
                        {
                            int dersCount2 = ToplamDers.Count;
                            bool aynikelimeVarmi = false;
                            for (int d = 0; d < dersCount2; d++)
                            {
                                if (ToplamDers[d].ToString() == dersSayisi[i].ToString()) // eğer listedeki ile eşleşirse türkçe ve türkçe ise
                                {
                                    aynikelimeVarmi = true;
                                    break; // aynısı var zaten ve çık.
                                }
                                else
                                {
                                    aynikelimeVarmi = false; //eğer eşleşmezse hiç ekle.
                                }
                            }

                            if (aynikelimeVarmi == true)
                            {
                                // eklenmedi.
                            }
                            else
                            {
                                // eklendi.
                                ToplamDers.Add(dersSayisi[i]);
                            }
                        }
                    }


                }

                DersSayisi = ToplamDers.Count;

                lblToplamDers.Text = "Toplam Ders : " + DersSayisi;
                lblToplamSoru.Text = "Toplam Soru : " + ToplamSoru.ToString();
                lblToplamDogruCevap.Text = "Toplam Doğru Cevap : " + ToplamDogruCevap.ToString();
                lblToplamYanlisCevap.Text = "Toplam Yanlış Cevap : " + ToplamYanlisCevap.ToString();

            }
            else if (filtre == "Aylık")
            {
                ArrayList veritabaniDuzenle = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnAnaliz WHERE 1", baglanti);
                SQLiteDataReader drveri = cekme.ExecuteReader();
                while (drveri.Read())
                {
                    veritabaniDuzenle.Add(drveri["id"].ToString());
                }
                cekme.Dispose();
                drveri.Close();
                baglanti.Close();

                int xveriuzunluk = veritabaniDuzenle.Count;

                DateTime dt = DateTime.Today; // bugünün tarihi

                ArrayList Tarihler = new ArrayList(); // 7 gün eklenicek.

                for (int i = 0; i <= 29; i++) // bugun eklendıgı için 30 kere dönücek.
                {
                    DateTime dt2 = dt.AddDays(-i);
                    Tarihler.Add(dt2.ToShortDateString());

                }

                DataTable tbl = new DataTable();

                // column isimleri
                tbl.Columns.Add("Tarih");
                tbl.Columns.Add("Ders");
                tbl.Columns.Add("Soru Sayısı");
                tbl.Columns.Add("Doğru Cevap");
                tbl.Columns.Add("Yanlış Cevap");

                bool eslesti = false;
                if (xveriuzunluk == 0)
                {
                    MessageBox.Show("Veritabanında hiç kayıt yok!", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    DataRow[] rows = new DataRow[xveriuzunluk]; // veri tabanındakı kayıt kadar sayı.
                    int adet = 0;
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Tarih,Ders,SoruSayisi,dCevap,yCevap FROM GvnAnaliz WHERE K_id="+Kid, baglanti);
                    SQLiteDataReader dr = cekme.ExecuteReader();
                    while (dr.Read())
                    {
                        for (int i = 0; i <= 29; i++)
                        {
                            if (dr["Tarih"].ToString() == Tarihler[i].ToString())
                            {
                                eslesti = true;
                                DataRow row = tbl.NewRow();
                                row["Tarih"] = dr["Tarih"].ToString();
                                row["Ders"] = dr["Ders"].ToString();
                                row["Soru Sayısı"] = dr["SoruSayisi"].ToString();
                                row["Doğru Cevap"] = dr["dCevap"].ToString();
                                row["Yanlış Cevap"] = dr["yCevap"].ToString();

                                rows[adet] = row;
                                adet++;
                                break;

                            }
                        }

                    }
                    cekme.Dispose();
                    dr.Close();
                    baglanti.Close();
                    if (eslesti == true)
                    {
                        tbl = rows.CopyToDataTable();
                    }
                    else
                    {
                        MessageBox.Show("Kayıt Yok.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }

                    dataGridView2.DataSource = tbl;
                    
                }

                ArrayList dersSayisi = new ArrayList();
                int DersSayisi = 0;
                int ToplamSoru = 0;
                int ToplamDogruCevap = 0;
                int ToplamYanlisCevap = 0;
                // 1,2,3,4 sebebi ayri kendim manuel olarak listeledim bu yuzden id olmadigi icin 01234 seklinde oldu almamiz gereknelerde 1234 dur
                foreach (DataGridViewRow r in dataGridView2.Rows)
                {

                    if (r.Cells[1].Value == null)
                    {
                        // boş geç
                    }
                    else
                    {
                        dersSayisi.Add(Convert.ToString(r.Cells[1].Value));
                    }
                    ToplamSoru += Convert.ToInt32(r.Cells[2].Value);
                    ToplamDogruCevap += Convert.ToInt32(r.Cells[3].Value);
                    ToplamYanlisCevap += Convert.ToInt32(r.Cells[4].Value);
                }

                ArrayList ToplamDers = new ArrayList();

                if (dersSayisi.Count == 0)
                {
                    DersSayisi = 0;
                }
                else
                {
                    int dersSayiCount = dersSayisi.Count;
                    int ilkEkleme = 0;
                    for (int i = 0; i < dersSayiCount; i++)
                    {
                        if (ilkEkleme == 0)
                        {
                            ToplamDers.Add(dersSayisi[i]);
                            ilkEkleme++;
                        }
                        else
                        {
                            int dersCount2 = ToplamDers.Count;
                            bool aynikelimeVarmi = false;
                            for (int d = 0; d < dersCount2; d++)
                            {
                                if (ToplamDers[d].ToString() == dersSayisi[i].ToString()) // eğer listedeki ile eşleşirse türkçe ve türkçe ise
                                {
                                    aynikelimeVarmi = true;
                                    break; // aynısı var zaten ve çık.
                                }
                                else
                                {
                                    aynikelimeVarmi = false; //eğer eşleşmezse hiç ekle.
                                }
                            }

                            if (aynikelimeVarmi == true)
                            {
                                // eklenmedi.
                            }
                            else
                            {
                                // eklendi.
                                ToplamDers.Add(dersSayisi[i]);
                            }
                        }
                    }


                }

                DersSayisi = ToplamDers.Count;

                lblToplamDers.Text = "Toplam Ders : " + DersSayisi;
                lblToplamSoru.Text = "Toplam Soru : " + ToplamSoru.ToString();
                lblToplamDogruCevap.Text = "Toplam Doğru Cevap : " + ToplamDogruCevap.ToString();
                lblToplamYanlisCevap.Text = "Toplam Yanlış Cevap : " + ToplamYanlisCevap.ToString();

            }
            else if (filtre == "Hepsi")
            {
                
                dt2 = new DataTable();
                adp2.Fill(dt2);
                dataGridView2.DataSource = dt2;
                dataGridView2.Columns[0].Visible = false; // id
                dataGridView2.Columns[1].HeaderText = "Tarih";
                dataGridView2.Columns[2].HeaderText = "Ders";
                dataGridView2.Columns[3].HeaderText = "Soru Sayısı";
                dataGridView2.Columns[4].HeaderText = "Doğru Cevap";
                dataGridView2.Columns[5].HeaderText = "Yanlış Cevap";
                dataGridView2.Columns[6].Visible = false;

                ArrayList dersSayisi = new ArrayList();
                int DersSayisi = 0;
                int ToplamSoru = 0;
                int ToplamDogruCevap = 0;
                int ToplamYanlisCevap = 0;
                // 2,3,4,5
                foreach (DataGridViewRow r in dataGridView2.Rows)
                {

                    if (r.Cells[2].Value == null)
                    {
                        // boş geç
                    }
                    else
                    {
                        dersSayisi.Add(Convert.ToString(r.Cells[2].Value));
                    }
                    ToplamSoru += Convert.ToInt32(r.Cells[3].Value);
                    ToplamDogruCevap += Convert.ToInt32(r.Cells[4].Value);
                    ToplamYanlisCevap += Convert.ToInt32(r.Cells[5].Value);
                }

                ArrayList ToplamDers = new ArrayList();

                if (dersSayisi.Count == 0)
                {
                    DersSayisi = 0;
                }
                else
                {
                    int dersSayiCount = dersSayisi.Count;
                    int ilkEkleme = 0;
                    for (int i = 0; i < dersSayiCount; i++)
                    {
                        if (ilkEkleme == 0)
                        {
                            ToplamDers.Add(dersSayisi[i]);
                            ilkEkleme++;
                        }
                        else
                        {
                            int dersCount2 = ToplamDers.Count;
                            bool aynikelimeVarmi = false;
                            for (int d = 0; d < dersCount2; d++)
                            {
                                if (ToplamDers[d].ToString() == dersSayisi[i].ToString()) // eğer listedeki ile eşleşirse türkçe ve türkçe ise
                                {
                                    aynikelimeVarmi = true;
                                    break; // aynısı var zaten ve çık.
                                }
                                else
                                {
                                    aynikelimeVarmi = false; //eğer eşleşmezse hiç ekle.
                                }
                            }

                            if (aynikelimeVarmi == true)
                            {
                                // eklenmedi.
                            }
                            else
                            {
                                // eklendi.
                                ToplamDers.Add(dersSayisi[i]);
                            }
                        }
                    }


                }

                DersSayisi = ToplamDers.Count;

                lblToplamDers.Text = "Toplam Ders : " + DersSayisi;
                lblToplamSoru.Text = "Toplam Soru : " + ToplamSoru.ToString();
                lblToplamDogruCevap.Text = "Toplam Doğru Cevap : " + ToplamDogruCevap.ToString();
                lblToplamYanlisCevap.Text = "Toplam Yanlış Cevap : " + ToplamYanlisCevap.ToString();
            }
            else
            {
                // hiçbiri değil filtreleme yok.
            }
         }

        
        public string filtre = "";

        private void cmbSurec_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbSurec.Text == "Günlük")
            {
                filtre = "Günlük";
                datagridviewFiltre();
            }
            else if (cmbSurec.Text == "Haftalık")
            {
                filtre = "Haftalık";
                datagridviewFiltre();
            }
            else if (cmbSurec.Text == "Aylık")
            {
                filtre = "Aylık";
                datagridviewFiltre();
            }
            else if (cmbSurec.Text == "Hepsi")
            {
                filtre = "Hepsi";
                datagridviewFiltre();
            }
            else
            {
                // seçim yok.
            }
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

            if (dateTimePicker1.Checked == true && dateTimePicker2.Checked == true)
            {
                TimeSpan strGun = (dateTimePicker1.Value - dateTimePicker2.Value);
                int fark = Convert.ToInt32(strGun.Days);
                string bugunCevir = bugunTarih.Substring(0, 10);
                string sonTarih = dateTimePicker1.Value.ToString();
                string sonTarihCevir = sonTarih.Substring(0, 10);
                if (Convert.ToDateTime(bugunCevir) < Convert.ToDateTime(sonTarihCevir))
                {
                    MessageBox.Show("Son Tarihi Bugünkü tarihten yüksek seçemezsiniz.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    if (fark < 0)
                    {
                        MessageBox.Show("Son Tarih İlk Tarihden Büyük olmamalı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                    else
                    {
                        if (fark == 0)
                        {
                            lblGun.Text = "Gün Sayısı : " + fark.ToString();
                            ArrayList veritabaniDuzenle = new ArrayList();

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnAnaliz WHERE 1", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                veritabaniDuzenle.Add(drveri["id"].ToString());
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            int xveriuzunluk = veritabaniDuzenle.Count;

                            DateTime dt = DateTime.Today; // bugünün tarihi

                            ArrayList Tarihler = new ArrayList(); // 7 gün eklenicek.

                            for (int i = 0; i <= fark; i++) // bugun eklendıgı için 30 kere dönücek.
                            {
                                DateTime dt2 = dt.AddDays(-i);
                                Tarihler.Add(dt2.ToShortDateString());

                            }

                            DataTable tbl = new DataTable();

                            // column isimleri
                            tbl.Columns.Add("Tarih");
                            tbl.Columns.Add("Ders");
                            tbl.Columns.Add("Soru Sayısı");
                            tbl.Columns.Add("Doğru Cevap");
                            tbl.Columns.Add("Yanlış Cevap");
                            bool eslestimi = false;

                            if (xveriuzunluk == 0)
                            {
                                MessageBox.Show("Veritabanında hiç kayıt yok!", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            else
                            {
                                DataRow[] rows = new DataRow[xveriuzunluk]; // veri tabanındakı kayıt kadar sayı.
                                int adet = 0;
                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT Tarih,Ders,SoruSayisi,dCevap,yCevap FROM GvnAnaliz WHERE K_id="+ Kid, baglanti);
                                SQLiteDataReader dr = cekme.ExecuteReader();
                                while (dr.Read())
                                {
                                    for (int i = 0; i <= fark; i++) // fark 0 oldugundan çıkarma yapmıyoruz.
                                    {
                                        if (dr["Tarih"].ToString() == Tarihler[i].ToString())
                                        {
                                            eslestimi = true;
                                            DataRow row = tbl.NewRow();
                                            row["Tarih"] = dr["Tarih"].ToString();
                                            row["Ders"] = dr["Ders"].ToString();
                                            row["Soru Sayısı"] = dr["SoruSayisi"].ToString();
                                            row["Doğru Cevap"] = dr["dCevap"].ToString();
                                            row["Yanlış Cevap"] = dr["yCevap"].ToString();

                                            rows[adet] = row;
                                            adet++;
                                            break;

                                        }
                                    }

                                }
                                cekme.Dispose();
                                dr.Close();
                                baglanti.Close();

                                if (eslestimi == true)
                                {
                                    tbl = rows.CopyToDataTable();
                                    
                                }
                                else
                                {
                                    MessageBox.Show("Kayıt Yok.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                                }
                                
                                dataGridView2.DataSource = tbl;

                                

                                ArrayList dersSayisi = new ArrayList();
                                int DersSayisi = 0;
                                int ToplamSoru = 0;
                                int ToplamDogruCevap = 0;
                                int ToplamYanlisCevap = 0;
                                // 1,2,3,4 sebebi ayri kendim manuel olarak listeledim bu yuzden id olmadigi icin 01234 seklinde oldu almamiz gereknelerde 1234 dur
                                foreach (DataGridViewRow r in dataGridView2.Rows)
                                {

                                    if (r.Cells[1].Value == null)
                                    {
                                        // boş geç
                                    }
                                    else
                                    {
                                        dersSayisi.Add(Convert.ToString(r.Cells[1].Value));
                                    }
                                    ToplamSoru += Convert.ToInt32(r.Cells[2].Value);
                                    ToplamDogruCevap += Convert.ToInt32(r.Cells[3].Value);
                                    ToplamYanlisCevap += Convert.ToInt32(r.Cells[4].Value);
                                }

                                ArrayList ToplamDers = new ArrayList();

                                if (dersSayisi.Count == 0)
                                {
                                    DersSayisi = 0;
                                }
                                else
                                {
                                    int dersSayiCount = dersSayisi.Count;
                                    int ilkEkleme = 0;
                                    for (int i = 0; i < dersSayiCount; i++)
                                    {
                                        if (ilkEkleme == 0)
                                        {
                                            ToplamDers.Add(dersSayisi[i]);
                                            ilkEkleme++;
                                        }
                                        else
                                        {
                                            int dersCount2 = ToplamDers.Count;
                                            bool aynikelimeVarmi = false;
                                            for (int d = 0; d < dersCount2; d++)
                                            {
                                                if (ToplamDers[d].ToString() == dersSayisi[i].ToString()) // eğer listedeki ile eşleşirse türkçe ve türkçe ise
                                                {
                                                    aynikelimeVarmi = true;
                                                    break; // aynısı var zaten ve çık.
                                                }
                                                else
                                                {
                                                    aynikelimeVarmi = false; //eğer eşleşmezse hiç ekle.
                                                }
                                            }

                                            if (aynikelimeVarmi == true)
                                            {
                                                // eklenmedi.
                                            }
                                            else
                                            {
                                                // eklendi.
                                                ToplamDers.Add(dersSayisi[i]);
                                            }
                                        }
                                    }


                                }

                                DersSayisi = ToplamDers.Count;

                                lblToplamDers.Text = "Toplam Ders : " + DersSayisi;
                                lblToplamSoru.Text = "Toplam Soru : " + ToplamSoru.ToString();
                                lblToplamDogruCevap.Text = "Toplam Doğru Cevap : " + ToplamDogruCevap.ToString();
                                lblToplamYanlisCevap.Text = "Toplam Yanlış Cevap : " + ToplamYanlisCevap.ToString();
                            }
                        }
                        else
                        {
                            lblGun.Text = "Gün Sayısı : " + fark.ToString();
                            ArrayList xveritabaniDuzenle = new ArrayList();

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnAnaliz WHERE 1", baglanti);
                            SQLiteDataReader drveri2 = cekme.ExecuteReader();
                            while (drveri2.Read())
                            {
                                xveritabaniDuzenle.Add(drveri2["id"].ToString());
                            }
                            cekme.Dispose();
                            drveri2.Close();
                            baglanti.Close();

                            int xveriuzunluk2 = xveritabaniDuzenle.Count;

                            DateTime dt2 = DateTime.Today; // bugünün tarihi

                            ArrayList Tarihler2 = new ArrayList(); // 7 gün eklenicek.

                            for (int i = 0; i <= fark - 1; i++) // bugun eklendıgı için 30 kere dönücek.
                            {
                                DateTime dt3 = dt2.AddDays(-i);
                                Tarihler2.Add(dt3.ToShortDateString());

                            }

                            DataTable tbl2 = new DataTable();

                            // column isimleri
                            tbl2.Columns.Add("Tarih");
                            tbl2.Columns.Add("Ders");
                            tbl2.Columns.Add("Soru Sayısı");
                            tbl2.Columns.Add("Doğru Cevap");
                            tbl2.Columns.Add("Yanlış Cevap");

                            bool eslestimi = false;
                            if (xveriuzunluk2 == 0)
                            {
                                MessageBox.Show("Veritabanında hiç kayıt yok!", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            }
                            else
                            {
                                DataRow[] rows = new DataRow[xveriuzunluk2]; // veri tabanındakı kayıt kadar sayı.
                                int adet = 0;
                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT Tarih,Ders,SoruSayisi,dCevap,yCevap FROM GvnAnaliz WHERE K_id="+Kid, baglanti);
                                SQLiteDataReader dr = cekme.ExecuteReader();
                                while (dr.Read())
                                {
                                    for (int i = 0; i <= fark - 1; i++) // burdaki -1 nedeni 0 oldugundandır -1 yaparak 0 ı dahil etmiş oluruz.
                                    {
                                        if (dr["Tarih"].ToString() == Tarihler2[i].ToString())
                                        {
                                            eslestimi = true;
                                            DataRow row = tbl2.NewRow();
                                            row["Tarih"] = dr["Tarih"].ToString();
                                            row["Ders"] = dr["Ders"].ToString();
                                            row["Soru Sayısı"] = dr["SoruSayisi"].ToString();
                                            row["Doğru Cevap"] = dr["dCevap"].ToString();
                                            row["Yanlış Cevap"] = dr["yCevap"].ToString();

                                            rows[adet] = row;
                                            adet++;
                                            break;

                                        }
                                    }

                                }
                                cekme.Dispose();
                                dr.Close();
                                baglanti.Close();


                                if (eslestimi == true)
                                {
                                    tbl2 = rows.CopyToDataTable();

                                }
                                else
                                {
                                    MessageBox.Show("Kayıt Yok.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                                }

                                dataGridView2.DataSource = tbl2;

                               

                                ArrayList dersSayisi = new ArrayList();
                                int DersSayisi = 0;
                                int ToplamSoru = 0;
                                int ToplamDogruCevap = 0;
                                int ToplamYanlisCevap = 0;
                                // 1,2,3,4 sebebi ayri kendim manuel olarak listeledim bu yuzden id olmadigi icin 01234 seklinde oldu almamiz gereknelerde 1234 dur
                                foreach (DataGridViewRow r in dataGridView2.Rows)
                                {

                                    if (r.Cells[1].Value == null)
                                    {
                                        // boş geç
                                    }
                                    else
                                    {
                                        dersSayisi.Add(Convert.ToString(r.Cells[1].Value));
                                    }
                                    ToplamSoru += Convert.ToInt32(r.Cells[2].Value);
                                    ToplamDogruCevap += Convert.ToInt32(r.Cells[3].Value);
                                    ToplamYanlisCevap += Convert.ToInt32(r.Cells[4].Value);
                                }

                                ArrayList ToplamDers = new ArrayList();

                                if (dersSayisi.Count == 0)
                                {
                                    DersSayisi = 0;
                                }
                                else
                                {
                                    int dersSayiCount = dersSayisi.Count;
                                    int ilkEkleme = 0;
                                    for (int i = 0; i < dersSayiCount; i++)
                                    {
                                        if (ilkEkleme == 0)
                                        {
                                            ToplamDers.Add(dersSayisi[i]);
                                            ilkEkleme++;
                                        }
                                        else
                                        {
                                            int dersCount2 = ToplamDers.Count;
                                            bool aynikelimeVarmi = false;
                                            for (int d = 0; d < dersCount2; d++)
                                            {
                                                if (ToplamDers[d].ToString() == dersSayisi[i].ToString()) // eğer listedeki ile eşleşirse türkçe ve türkçe ise
                                                {
                                                    aynikelimeVarmi = true;
                                                    break; // aynısı var zaten ve çık.
                                                }
                                                else
                                                {
                                                    aynikelimeVarmi = false; //eğer eşleşmezse hiç ekle.
                                                }
                                            }

                                            if (aynikelimeVarmi == true)
                                            {
                                                // eklenmedi.
                                            }
                                            else
                                            {
                                                // eklendi.
                                                ToplamDers.Add(dersSayisi[i]);
                                            }
                                        }
                                    }


                                }

                                DersSayisi = ToplamDers.Count;

                                lblToplamDers.Text = "Toplam Ders : " + DersSayisi;
                                lblToplamSoru.Text = "Toplam Soru : " + ToplamSoru.ToString();
                                lblToplamDogruCevap.Text = "Toplam Doğru Cevap : " + ToplamDogruCevap.ToString();
                                lblToplamYanlisCevap.Text = "Toplam Yanlış Cevap : " + ToplamYanlisCevap.ToString();
                            }
                        }

                    }

                }
            }
            else
            {
                MessageBox.Show("2 Tarih Seçilmemiş.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            
        }

        public int dersGuncellemeVarmi = 0;

        int AnaGuvenlik = -1;

        private void timer2_Tick(object sender, EventArgs e)
        {


            try
            {
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT dersGuncelleme FROM GvnGenel WHERE id=1", baglanti);
                SQLiteDataReader drDersGunc = cekme.ExecuteReader();
                while (drDersGunc.Read())
                {
                    dersGuncellemeVarmi = Convert.ToInt32(drDersGunc["dersGuncelleme"]);
                    break;
                }
                cekme.Dispose();
                drDersGunc.Close();
                baglanti.Close();


                if (dersGuncellemeVarmi == 1)
                {
                    
                    // güncelle.
                    //DerslerGuncelle();
                    richSoru.Enabled = false;
                    txtResimYol.Enabled = false;
                    richD.Enabled = false;
                    richC.Enabled = false;
                    richB.Enabled = false;
                    richA.Enabled = false;
                    cmbDers.Enabled = true;
                    cmbCevap.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    dataGridView1.Enabled = false;
                    button10.Enabled = false;
                    button12.Enabled = false;

                    //seçimi sıfırla
                    if (cmbSeviye.SelectedIndex == 0) cmbSeviye.SelectedIndex = 1;
                    else cmbSeviye.SelectedIndex = 0; 

                    baglanti.Open();
                    cmd = new SQLiteCommand("UPDATE GvnGenel SET dersGuncelleme=0", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();
                }

                // Ana Güvenlik Kontrolü

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT AnaGuvenlik FROM GvnGenel WHERE id=1", baglanti);
                SQLiteDataReader drAnaGuvenlik = cekme.ExecuteReader();
                while (drAnaGuvenlik.Read())
                {
                    AnaGuvenlik = Convert.ToInt32(drAnaGuvenlik["AnaGuvenlik"]);
                    break;
                }
                cekme.Dispose();
                drAnaGuvenlik.Close();
                baglanti.Close();


                bugunTarih = DateTime.Today.ToLocalTime().ToString();
            }
            catch
            {
                if (baglanti.State == ConnectionState.Open)
                {
                    baglanti.Close();
                }
            }

        }

        // silme bozuluyor :D
        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            DersEkle drsEkle = new DersEkle();
            drsEkle.ShowDialog();
        }
        /*
        ArrayList Dersler = new ArrayList();
        ArrayList soruid = new ArrayList();

        void DerslerGuncelle()
        {
            cmbDers.Items.Clear();
            Dersler.Clear();
            soruid.Clear();
            // Dersleri gir.
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT SoruTipi, SoruID FROM GvnDersler WHERE 1", baglanti);
            SQLiteDataReader drderscek = cekme.ExecuteReader();
            while (drderscek.Read())
            {
                cmbDers.Items.Add(drderscek["SoruTipi"].ToString()); // combobox yazdır
                Dersler.Add(drderscek["SoruTipi"].ToString()); // dersler dizesine gönder
                soruid.Add(drderscek["SoruID"]); // Soru id leri al.
                cmbPzt.Items.Add(drderscek["SoruTipi"].ToString());
                cmbSali.Items.Add(drderscek["SoruTipi"].ToString());
                cmbCarsamba.Items.Add(drderscek["SoruTipi"].ToString());
                cmbPersembe.Items.Add(drderscek["SoruTipi"].ToString());
                cmbCuma.Items.Add(drderscek["SoruTipi"].ToString());
                cmbCumartesi.Items.Add(drderscek["SoruTipi"].ToString());
                cmbPazar.Items.Add(drderscek["SoruTipi"].ToString());
            }
            drderscek.Close();
            cekme.Dispose();
            baglanti.Close();
            Dersler.Add("Karışık");
            Dersler.Add("Otomatik");
            Dersler.Add("Hazır");
        }
        */

        private void button10_Click(object sender, EventArgs e)
        {
            secilenResimAdi = "";
            pictureBox1.Image = null;
            txtResimYol.Text = "";
        }

        int Kid = -1;

        private void button11_Click(object sender, EventArgs e)
        {

            DialogResult buton = MessageBox.Show("Analiz Temizlemek istediğinize emin misiniz ?", "Yönetici Paneli", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (buton == DialogResult.Yes)
            {
                try
                {
                    baglanti.Open();
                    cmd = new SQLiteCommand("DELETE FROM GvnAnaliz WHERE K_id=" + Kid, baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();
                }
                catch
                {
                    MessageBox.Show("Analiz Temizlenirken hata oluştu.","Hata");
                }

                // Hepsi Olarak Veritabanını yazdır.
                adp2 = new SQLiteDataAdapter("SELECT id,Tarih,Ders,SoruSayisi,dCevap,yCevap,K_id FROM GvnAnaliz WHERE K_id="+Kid, baglanti);
                dt2 = new DataTable();
                adp2.Fill(dt2);
                dataGridView2.DataSource = dt2;
                dataGridView2.Columns[0].Visible = false; // id
                dataGridView2.Columns[1].HeaderText = "Tarih";
                dataGridView2.Columns[2].HeaderText = "Ders";
                dataGridView2.Columns[3].HeaderText = "Soru Sayısı";
                dataGridView2.Columns[4].HeaderText = "Doğru Cevap";
                dataGridView2.Columns[5].HeaderText = "Yanlış Cevap";
                dataGridView2.Columns[6].Visible = false; // k_id
                cmbSurec.SelectedIndex = 3; // hepsi ayarla.
            }
            else
            {
                // işlem iptal
            }
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            try
            {
                if (fontDialog1.ShowDialog() == DialogResult.OK)
                {
                    richSoru.SelectionFont = fontDialog1.Font;
                    richA.SelectionFont = fontDialog1.Font;
                    richB.SelectionFont = fontDialog1.Font;
                    richC.SelectionFont = fontDialog1.Font;
                    richD.SelectionFont = fontDialog1.Font;
                }
            }
            catch ( Exception exx )
            {
                MessageBox.Show("Fontu biçimlendirirken hata oluştu :\n\n"+ exx.ToString());
            }
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            try
            {
                if (colorDialog1.ShowDialog() == DialogResult.OK)
                {
                    richSoru.SelectionColor = colorDialog1.Color;
                    richA.SelectionColor = colorDialog1.Color;
                    richB.SelectionColor = colorDialog1.Color;
                    richC.SelectionColor = colorDialog1.Color;
                    richD.SelectionColor = colorDialog1.Color;
                }
            }
            catch ( Exception exx )
            {
                MessageBox.Show("Rengi biçimlendirirken hata oluştu :\n\n"+ exx.ToString());
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            try
            {
                richSoru.SelectionFont = new Font(richSoru.SelectionFont.FontFamily, 8, FontStyle.Regular);
                richA.SelectionFont = new Font(richSoru.SelectionFont.FontFamily, 8, FontStyle.Regular);
                richB.SelectionFont = new Font(richSoru.SelectionFont.FontFamily, 8, FontStyle.Regular);
                richC.SelectionFont = new Font(richSoru.SelectionFont.FontFamily, 8, FontStyle.Regular);
                richD.SelectionFont = new Font(richSoru.SelectionFont.FontFamily, 8, FontStyle.Regular);
                richSoru.SelectionColor = Color.Black;
                richA.SelectionColor = Color.Black;
                richB.SelectionColor = Color.Black;
                richC.SelectionColor = Color.Black;
                richD.SelectionColor = Color.Black;
            }
            catch ( Exception exx )
            {
                MessageBox.Show("Sıfırlama işlemi yaparken hata oluştu :\n\n"+ exx.ToString());
            }
        }

        private void programıDevreDışıBırakToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult buton = MessageBox.Show("Programı devre dışı bırakmak istediğinizden emin misiniz ?", "Yönetici Paneli", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (buton == DialogResult.Yes)
            {
                Application.Exit(); // programı tamamen kapat.
            }
            else
            {
                // işlem iptal
            }
        }

        int doubleClick = 0;

        private void bilgisayarıKapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            doubleClick++;
            if (doubleClick == 2)
            {
                // Bilgisayar Kapatılıyor.
                DialogResult buton = MessageBox.Show("Bilgisayarı kapatmak istediğinizden emin misiniz ?", "Yönetici Paneli", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (buton == DialogResult.Yes)
                {
                    Process.Start("shutdown", "-s -f -t 0");
                }
                else
                {
                    // işlem iptal
                }


            }
        }

        private void yenidenBaşlatToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Bilgisayar Yeniden başlatılıyor.
            DialogResult buton = MessageBox.Show("Bilgisayarı yeniden başlatmak istediğinizden emin misiniz ?", "LCD Pano", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (buton == DialogResult.Yes)
            {
                Process.Start("shutdown", "-r -f -t 0");
            }
            else
            {
                // işlem iptal
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string sifre = "";

            baglanti.Open();
            cekme = new SQLiteCommand("SELECT sifre FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drsifre = cekme.ExecuteReader();
            while (drsifre.Read())
            {
                sifre = Convert.ToString(drsifre["sifre"].ToString());
                break;
            }
            cekme.Dispose();
            drsifre.Close();
            baglanti.Close();
            if (txtsifre.Text == "" || txtYeniSifre1.Text == "" || txtYeniSifre2.Text == "") 
            {
                MessageBox.Show("Lütfen Gerekli alanları doldurunuz.","Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (sifre == txtEskiSifre.Text)
                {
                    if (txtYeniSifre1.Text == txtYeniSifre2.Text)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET sifre='" + txtYeniSifre1.Text.Replace("'", "''") + "'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        MessageBox.Show("Şifreniz Başarıyla Değiştirilmiştir.\n\n Yeni Şifreniz : " + txtYeniSifre1.Text + "", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                    else
                    {
                        MessageBox.Show("Yeni şifreniz aynı değil. Lütfen doğru girdiğinizden emin olun.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }


                }
                else
                {
                    MessageBox.Show("Şifre Yanlış !", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }

        }

        private void button8_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text;
            int code = -1;
            if (txtEmail.Text == "" || cmbmail.Text == "")
            {
                email = "";
            }
            else
            {
                if (cmbmail.Text == "hotmail.com")
                {
                    code = 0;
                    email = txtEmail.Text;
                }
                else if (cmbmail.Text == "gmail.com")
                {
                    code = 1;
                    email = txtEmail.Text;
                }
                
            }

            /*
            if (sonuc == false)
            {
                MessageBox.Show("İnternet bağlantınız yok. Bu yüzden email kaydı yapılmadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
            }*/

            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=1", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();

            bool eksikBilgi = false;

            string sifre = txtEmailSifre.Text;
            if (txtEmail.Text != "" && sifre == "")
            {
                eksikBilgi = true;
            }
            else if (sifre != "" && txtEmail.Text == "")
            {
                eksikBilgi = true;
            }
            else if (sifre != "" && txtEmail.Text != "" && cmbmail.Text == "")
            {
                eksikBilgi = true;
            }


            if (sifre != "" && txtEmail.Text != "" && cmbmail.Text != "" && cmbSoruSeviye.Text != "")
            {
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET TestdgrCevapDk='" + numTestDogruCvpDk.Value.ToString() + "', TestSoruBit='" + numTestKacSoruBit.Value.ToString() + "', Email='" + email.Replace("'", "''") + "', EmailSifre='" + sifre.Replace("'", "''") + "', EmailKodu=" + code + ", OneriYazisi='" + txtOneri.Text.Replace("'", "''") + "', OneriYazisi2='" + txtOneriYazisi2.Text.Replace("'", "''") + "', Seviye='"+cmbSoruSeviye.Text+"' WHERE id=1", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                MessageBox.Show("Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else if (sifre == "" || txtEmail.Text == "" || cmbmail.Text == "")
            {
                sifre = "";
                email = "";
                code = -1;

                if (eksikBilgi == true)
                {
                    MessageBox.Show("Email'da eksik bilgi olduğu için kayıt edilmedi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }

                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET TestdgrCevapDk='" + numTestDogruCvpDk.Value.ToString() + "', TestSoruBit='" + numTestKacSoruBit.Value.ToString() + "', Email='" + email.Replace("'", "''") + "', EmailSifre='" + sifre.Replace("'", "''") + "', EmailKodu=" + code + ", OneriYazisi='" + txtOneri.Text.Replace("'", "''") + "', OneriYazisi2='" + txtOneriYazisi2.Text.Replace("'", "''") + "', Seviye='" + cmbSoruSeviye.Text + "' WHERE id=1", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                MessageBox.Show("Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            
            
            
        }

        int duzenlenecekiD = -1;

        string gecici = "";
        private void button12_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            // seçileni düzelt.
            try
            {
                duzenlenecekiD = Convert.ToInt32(dataGridView1.CurrentRow.Cells[1].Value); // seçilen satırın id değerini al.
            }
            catch
            { }
            if (duzenlenecekiD == -1)
            {
                // boş değer. yani seçilmemiş.
                MessageBox.Show("Lütfen soruyu listeden seçiniz.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                
                string Cevap = "";
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT Soru,Cevap,Ask,Bsk,Csk,Dsk,ResimYol FROM GvnSorular WHERE id="+duzenlenecekiD+"", baglanti);
                SQLiteDataReader drduzenle = cekme.ExecuteReader();
                while (drduzenle.Read())
                {
                    richSoru.Rtf = drduzenle["Soru"].ToString();
                    Cevap = drduzenle["Cevap"].ToString();
                    richA.Rtf = drduzenle["Ask"].ToString();
                    richB.Rtf = drduzenle["Bsk"].ToString();
                    richC.Rtf = drduzenle["Csk"].ToString();
                    richD.Rtf = drduzenle["Dsk"].ToString();
                    txtResimYol.Text = drduzenle["ResimYol"].ToString();
                    secilenResimAdi = drduzenle["ResimYol"].ToString();
                    if (drduzenle["ResimYol"].ToString() == "")
                    {
                        // resim yok boş değer.
                    }
                    else
                    {
                        // resim var.
                        
                        pictureBox1.ImageLocation = fnk.systemYol().ToString() + "\\" + drduzenle["ResimYol"].ToString();
                    }
                }
                cekme.Dispose();
                drduzenle.Close();
                baglanti.Close();

                if (Cevap == "A")
                {
                    cmbCevap.SelectedIndex = 0;
                }
                else if (Cevap == "B")
                {
                    cmbCevap.SelectedIndex = 1;
                }
                else if (Cevap == "C")
                {
                    cmbCevap.SelectedIndex = 2;
                }
                else if (Cevap == "D")
                {
                    cmbCevap.SelectedIndex = 3;
                }

                button1.Visible = false; // görünmez yap.
                button13.Visible = true;
                button14.Visible = true;

                gecici = txtResimYol.Text;

            }

        }

        private void button14_Click(object sender, EventArgs e)
        {
            // düzenleme işlemi iptal
            duzenlenecekiD = -1;
            button1.Visible = true;
            button13.Visible = false;
            button14.Visible = false;
            cmbDers.Enabled = true;
            richSoru.Text = "";
            richA.Text = "";
            richB.Text = "";
            richC.Text = "";
            richD.Text = "";
            txtResimYol.Text = "";
            pictureBox1.Image = null;
            secilenResimAdi = "";
            cmbCevap.Items.Clear();
            cmbCevap.Items.Add("A");
            cmbCevap.Items.Add("B");
            cmbCevap.Items.Add("C");
            cmbCevap.Items.Add("D");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            // düzenle güncelle.
            silinenResim.Clear(); // güncelle.
            if (richSoru.Text == "" || richA.Text == "" || richB.Text == "" || richC.Text == "" || richD.Text == "" || cmbCevap.Text == "" || cmbSeviye.Text == "" || cmbDers.Text == "" || cmbKonu.Text == "")
            {
                MessageBox.Show("Lütfen gerekli alanları doldurunuz.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                // resim güncelleme
                string resYol = "";
                string xgecici = gecici.Replace(@"\", "/"); // geçici resim image/resim.jpg örneğin.
                
                if (xgecici != "" && txtResimYol.Text == "") // resmi vardı ve resmi silmiş ise buraya girecek.
                {// resim yoksa.
                    try
                    {
                        File.Delete(fnk.systemYol().ToString()+ "\\" + gecici); // xgecici resmi sil.
                    }
                    catch { }
                }
                else if (gecici != txtResimYol.Text) // ilk yol ile aynı ise resim değiştirilmemiş demektir eğer değilse değiştirilmiş demektir.
                {
                    // Eğer resim varsa yeni resim ile değiştiriliyorsa buraya girecek
                    //Eski resmi sil.
                    try
                    {
                        File.Delete(fnk.systemYol().ToString() + "\\" + gecici);
                    }
                    catch { }
                    //Yeni Resmi Ekle
                    try
                    {
                        secilenResimAdi += "-" + fnk.dateformat(DateTime.Now.ToShortDateString().ToString()) + "-" + fnk.dateformat(DateTime.Now.ToLongTimeString().ToString());
                        pictureBox1.Image.Save(fnk.systemYol().ToString() + "\\image\\" + secilenResimAdi + "" + DosyaUzantisi);
                        resYol = @"image\" + secilenResimAdi + "" + DosyaUzantisi;
                    }
                    catch
                    { MessageBox.Show("Resmi Eklerken Hata Oluştu. Lütfen konum değişikliği yapmadığınızdan emin olun."); }
                }
                else if (gecici == txtResimYol.Text)
                {
                    // eğer aynı resim ise hiçbir şey yapılmayacak.
                    // aynı resim yolu verilcek
                    resYol = txtResimYol.Text;
                }
                else if(txtResimYol.Text != "") // Hiçbiri değilse soruda resim yokmuş yeni bir resim eklemiş.
                {
                    try
                    {
                        //Yeni Resmi Ekle
                        secilenResimAdi += "-" + fnk.dateformat(DateTime.Now.ToShortDateString().ToString()) + "-" + fnk.dateformat(DateTime.Now.ToLongTimeString().ToString());
                        pictureBox1.Image.Save(fnk.systemYol().ToString() + "\\image\\" + secilenResimAdi + "" + DosyaUzantisi);
                        resYol = @"image\" + secilenResimAdi + "" + DosyaUzantisi;
                    }
                    catch { MessageBox.Show("Resmi Eklerken Hata Oluştu. Lütfen konum değişikliği yapmadığınızdan emin olun."); }
                }

                // Soruyu güncelle.
                try
                {

                    baglanti.Open();
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Soru='" + fnk.strtext(richSoru.Rtf) + "', Ask='" + fnk.strtext(richA.Rtf) + "', Bsk='" + fnk.strtext(richB.Rtf) + "', Csk='" + fnk.strtext(richC.Rtf) + "', Dsk='" + fnk.strtext(richD.Rtf) + "', ResimYol='" + resYol + "', Cevap='" + cmbCevap.Text + "' WHERE id=" + duzenlenecekiD + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();

                    secilenResimAdi = "";
                    gecici = "";

                    duzenlenecekiD = -1;
                    button1.Visible = true;
                    button13.Visible = false;
                    button14.Visible = false;
                    cmbDers.Enabled = true;
                    richSoru.Text = "";
                    richA.Text = "";
                    richB.Text = "";
                    richC.Text = "";
                    richD.Text = "";
                    txtResimYol.Text = "";
                    pictureBox1.Image = null;
                    cmbCevap.Items.Clear();
                    cmbCevap.Items.Add("A");
                    cmbCevap.Items.Add("B");
                    cmbCevap.Items.Add("C");
                    cmbCevap.Items.Add("D");
                    datagridViewx();

                    baglanti.Open();
                    cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=1", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    baglanti.Close();

                    MessageBox.Show("Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                catch ( Exception exx )
                {
                    MessageBox.Show("Güncellerken Hata Oluştu.\n\n"+exx.ToString()+"", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

            }
        }

        
        // OTOMATİK

        SQLiteDataAdapter adp3;

        int secilenDersSoruID = -1;

        int gun = -1; // sıralaması pzt = 1, salı = 2, crsmba = 3, persembe = 4, cuma = 5, cumartesi = 6, pazar = 7 şeklinde.

        int inttex = -1;
        private void cmbPzt_SelectedIndexChanged(object sender, EventArgs e)
        {
            gun = 1;
            cmbKPzt.Items.Clear();

            int secilenDers = cmbPzt.SelectedIndex;
            if (secilenDers == -1)
            {
                
            }
            else
            {
                int dersid = Convert.ToInt32(Otodersid[secilenDers]); // seçilen dersin id sini alır
                secilenDersSoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id=" + dersid + "", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    secilenDersSoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (secilenDersSoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Otokonu_adlari = new ArrayList();
                    Otokonu_idleri = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + secilenDersSoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {

                        cmbKPzt.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        Otokonu_idleri.Add(drveri["Konu_id"]);
                        Otokonu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();

                    inttex = cmbKPzt.FindString(pztK);
                    if (inttex != -1) cmbKPzt.SelectedIndex = inttex;


                }
            }
        }

        DataTable dt3 = new DataTable();

        void datagridViewDers()
        {
            dt3 = new DataTable();
            if (dt3 == null)
            {
            }
            else
            {
                adp3.Fill(dt3);
                dataGridView3.DataSource = dt3;

                dataGridView3.Columns[0].DisplayIndex = 8;
                dataGridView3.Columns[0].ReadOnly = false; // Checkbox
                dataGridView3.Columns[1].ReadOnly = true;
                dataGridView3.Columns[2].ReadOnly = true;
                dataGridView3.Columns[3].ReadOnly = true;
                dataGridView3.Columns[4].ReadOnly = true;
                dataGridView3.Columns[5].ReadOnly = true;
                dataGridView3.Columns[6].ReadOnly = true;
                dataGridView3.Columns[7].ReadOnly = true;
                dataGridView3.Columns[8].ReadOnly = true;

                dataGridView3.Columns[3].Width = 86;
                dataGridView3.Columns[4].Width = 86;
                dataGridView3.Columns[5].Width = 86;
                dataGridView3.Columns[6].Width = 86;
                dataGridView3.Columns[7].Width = 86;
                dataGridView3.Columns[8].Width = 86;

                dataGridView3.Columns[1].Visible = false; // id
                dataGridView3.Columns[2].HeaderText = "Soru";
                dataGridView3.Columns[3].HeaderText = "A Şıkkı";
                dataGridView3.Columns[4].HeaderText = "B Şıkkı";
                dataGridView3.Columns[5].HeaderText = "C Şıkkı";
                dataGridView3.Columns[6].HeaderText = "D Şıkkı";
                dataGridView3.Columns[7].HeaderText = "Resim";
                dataGridView3.Columns[8].HeaderText = "Cevap";
                // rich convert normal text

                foreach (DataGridViewRow c in dataGridView3.Rows)
                {
                    if (c.Cells[2].Value != null)
                    {
                        richGorunmez.Rtf = c.Cells[2].Value.ToString();
                        c.Cells[2].Value = richGorunmez.Text;

                        richGorunmez.Rtf = c.Cells[3].Value.ToString();
                        c.Cells[3].Value = richGorunmez.Text;

                        richGorunmez.Rtf = c.Cells[4].Value.ToString();
                        c.Cells[4].Value = richGorunmez.Text;

                        richGorunmez.Rtf = c.Cells[5].Value.ToString();
                        c.Cells[5].Value = richGorunmez.Text;

                        richGorunmez.Rtf = c.Cells[6].Value.ToString();
                        c.Cells[6].Value = richGorunmez.Text;
                    }
                }
                // temizle.
                richGorunmez.Text = "";
            }
        }

        private void cmbSali_SelectedIndexChanged(object sender, EventArgs e)
        {
            gun = 2;
            cmbKSali.Items.Clear();
            
            int secilenDers = cmbSali.SelectedIndex;
            if (secilenDers == -1)
            {

            }
            else
            {
                int dersid = Convert.ToInt32(Otodersid[secilenDers]);
                secilenDersSoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id=" + dersid + "", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    secilenDersSoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (secilenDersSoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Otokonu_adlari = new ArrayList();
                    Otokonu_idleri = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + secilenDersSoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {

                        cmbKSali.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        Otokonu_idleri.Add(drveri["Konu_id"]);
                        Otokonu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();

                    inttex = cmbKSali.FindString(saliK);
                    if (inttex != -1) cmbKSali.SelectedIndex = inttex;

                }
            }
        }

        private void cmbCarsamba_SelectedIndexChanged(object sender, EventArgs e)
        {
            gun = 3;
            cmbKCarsamba.Items.Clear();

            int secilenDers = cmbCarsamba.SelectedIndex;
            if (secilenDers == -1)
            {

            }
            else
            {
                int dersid = Convert.ToInt32(Otodersid[secilenDers]);
                secilenDersSoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id=" + dersid + "", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    secilenDersSoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (secilenDersSoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Otokonu_adlari = new ArrayList();
                    Otokonu_idleri = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + secilenDersSoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {

                        cmbKCarsamba.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        Otokonu_idleri.Add(drveri["Konu_id"]);
                        Otokonu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();

                    inttex = cmbKCarsamba.FindString(carsambaK);
                    if (inttex != -1) cmbKCarsamba.SelectedIndex = inttex;

                }
            }
        }

        private void cmbPersembe_SelectedIndexChanged(object sender, EventArgs e)
        {
            gun = 4;
            cmbKPersembe.Items.Clear();

            int secilenDers = cmbPersembe.SelectedIndex;
            if (secilenDers == -1)
            {

            }
            else
            {
                int dersid = Convert.ToInt32(Otodersid[secilenDers]);
                secilenDersSoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id=" + dersid + "", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    secilenDersSoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (secilenDersSoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Otokonu_adlari = new ArrayList();
                    Otokonu_idleri = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + secilenDersSoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {

                        cmbKPersembe.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        Otokonu_idleri.Add(drveri["Konu_id"]);
                        Otokonu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();

                    inttex = cmbKPersembe.FindString(persembeK);
                    if (inttex != -1) cmbKPersembe.SelectedIndex = inttex;

                }
            }

        }

        private void cmbCuma_SelectedIndexChanged(object sender, EventArgs e)
        {
            gun = 5;
            cmbKCuma.Items.Clear();

            int secilenDers = cmbCuma.SelectedIndex;
            if (secilenDers == -1)
            {

            }
            else
            {
                int dersid = Convert.ToInt32(Otodersid[secilenDers]);
                secilenDersSoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id=" + dersid + "", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    secilenDersSoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (secilenDersSoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Otokonu_adlari = new ArrayList();
                    Otokonu_idleri = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + secilenDersSoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {

                        cmbKCuma.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        Otokonu_idleri.Add(drveri["Konu_id"]);
                        Otokonu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();

                    inttex = cmbKCuma.FindString(cumaK);
                    if (inttex != -1) cmbKCuma.SelectedIndex = inttex;

                }
            }
        }

        private void cmbCumartesi_SelectedIndexChanged(object sender, EventArgs e)
        {
            gun = 6;

            cmbKCumartesi.Items.Clear();

            int secilenDers = cmbCumartesi.SelectedIndex;
            if (secilenDers == -1)
            {

            }
            else
            {
                int dersid = Convert.ToInt32(Otodersid[secilenDers]);
                secilenDersSoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id=" + dersid + "", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    secilenDersSoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (secilenDersSoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Otokonu_adlari = new ArrayList();
                    Otokonu_idleri = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + secilenDersSoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {

                        cmbKCumartesi.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        Otokonu_idleri.Add(drveri["Konu_id"]);
                        Otokonu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();

                    inttex = cmbKCumartesi.FindString(cumartesiK);
                    if (inttex != -1) cmbKCumartesi.SelectedIndex = inttex;

                }
            }

        }

        private void cmbPazar_SelectedIndexChanged(object sender, EventArgs e)
        {
            gun = 7;
            cmbKPazar.Items.Clear();

            int secilenDers = cmbPazar.SelectedIndex;
            if (secilenDers == -1)
            {

            }
            else
            {
                int dersid = Convert.ToInt32(Otodersid[secilenDers]);
                secilenDersSoruID = -1;
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT SoruTipi,SoruID FROM GvnDersler WHERE id=" + dersid + "", baglanti);
                SQLiteDataReader drcek = cekme.ExecuteReader();
                while (drcek.Read())
                {
                    secilenDersSoruID = Convert.ToInt32(drcek["SoruID"]);
                    break;
                }
                cekme.Dispose();
                drcek.Close();
                baglanti.Close();

                if (secilenDersSoruID == -1)
                {
                    MessageBox.Show("Ders Bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    Otokonu_adlari = new ArrayList();
                    Otokonu_idleri = new ArrayList();
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT Konu_Adi,Konu_id FROM Konular WHERE Konu_SoruID='" + secilenDersSoruID + "'", baglanti);
                    SQLiteDataReader drveri = cekme.ExecuteReader();
                    while (drveri.Read())
                    {

                        cmbKPazar.Items.Add(drveri["Konu_Adi"].ToString());//seçilen dersin konuları yazdır
                        Otokonu_idleri.Add(drveri["Konu_id"]);
                        Otokonu_adlari.Add(drveri["Konu_Adi"].ToString());
                    }
                    cekme.Dispose();
                    drveri.Close();
                    baglanti.Close();

                    inttex = cmbKPazar.FindString(pazarK);
                    if (inttex != -1) cmbKPazar.SelectedIndex = inttex;

                }
            }
        }
        // açıldıgında checkedli olanları checkedliyecek.
        void chked()
        {
            ArrayList isaretlenmisidler = new ArrayList();

            if (gun == 1)
            {

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE 1", baglanti);
                SQLiteDataReader drchck = cekme.ExecuteReader();
                while (drchck.Read())
                {
                    if (drchck["Pzt"].ToString() == Convert.ToString(gun))
                    {
                        isaretlenmisidler.Add(Convert.ToInt32(drchck["id"]));
                    }
                }
                cekme.Dispose();
                drchck.Close();
                baglanti.Close();

                foreach (DataGridViewRow r in dataGridView3.Rows)
                {
                    foreach (int itm in isaretlenmisidler)
                    {
                        if (Convert.ToInt32(r.Cells[1].Value) == itm) // eğer isaretlenmiş dizinin içinersindeki pzt'tesiye ait olan checkler. doğruysa idler eşleşirse 
                        {
                            r.Cells[0].Value = true; // checkedla.
                        }
                    }
                }
            }
            else if (gun == 2)
            {
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE 1", baglanti);
                SQLiteDataReader drchck = cekme.ExecuteReader();
                while (drchck.Read())
                {
                    if (drchck["Sali"].ToString() == Convert.ToString(gun))
                    {
                        isaretlenmisidler.Add(Convert.ToInt32(drchck["id"]));
                    }
                }
                cekme.Dispose();
                drchck.Close();
                baglanti.Close();

                foreach (DataGridViewRow r in dataGridView3.Rows)
                {
                    foreach (int itm in isaretlenmisidler)
                    {
                        if (Convert.ToInt32(r.Cells[1].Value) == itm) // eğer isaretlenmiş dizinin içinersindeki pzt'tesiye ait olan checkler. doğruysa idler eşleşirse 
                        {
                            r.Cells[0].Value = true; // checkedla.
                        }
                    }
                }
            }
            else if (gun == 3)
            {
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE 1", baglanti);
                SQLiteDataReader drchck = cekme.ExecuteReader();
                while (drchck.Read())
                {
                    if (drchck["Carsamba"].ToString() == Convert.ToString(gun))
                    {
                        isaretlenmisidler.Add(Convert.ToInt32(drchck["id"]));
                    }
                }
                cekme.Dispose();
                drchck.Close();
                baglanti.Close();

                foreach (DataGridViewRow r in dataGridView3.Rows)
                {
                    foreach (int itm in isaretlenmisidler)
                    {
                        if (Convert.ToInt32(r.Cells[1].Value) == itm) // eğer isaretlenmiş dizinin içinersindeki pzt'tesiye ait olan checkler. doğruysa idler eşleşirse 
                        {
                            r.Cells[0].Value = true; // checkedla.
                        }
                    }
                }
            }
            else if (gun == 4)
            {
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE 1", baglanti);
                SQLiteDataReader drchck = cekme.ExecuteReader();
                while (drchck.Read())
                {
                    if (drchck["Persembe"].ToString() == Convert.ToString(gun))
                    {
                        isaretlenmisidler.Add(Convert.ToInt32(drchck["id"]));
                    }
                }
                cekme.Dispose();
                drchck.Close();
                baglanti.Close();

                foreach (DataGridViewRow r in dataGridView3.Rows)
                {
                    foreach (int itm in isaretlenmisidler)
                    {
                        if (Convert.ToInt32(r.Cells[1].Value) == itm) // eğer isaretlenmiş dizinin içinersindeki pzt'tesiye ait olan checkler. doğruysa idler eşleşirse 
                        {
                            r.Cells[0].Value = true; // checkedla.
                        }
                    }
                }
            }
            else if (gun == 5)
            {
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE 1", baglanti);
                SQLiteDataReader drchck = cekme.ExecuteReader();
                while (drchck.Read())
                {
                    if (drchck["Cuma"].ToString() == Convert.ToString(gun))
                    {
                        isaretlenmisidler.Add(Convert.ToInt32(drchck["id"]));
                    }
                }
                cekme.Dispose();
                drchck.Close();
                baglanti.Close();

                foreach (DataGridViewRow r in dataGridView3.Rows)
                {
                    foreach (int itm in isaretlenmisidler)
                    {
                        if (Convert.ToInt32(r.Cells[1].Value) == itm) // eğer isaretlenmiş dizinin içinersindeki pzt'tesiye ait olan checkler. doğruysa idler eşleşirse 
                        {
                            r.Cells[0].Value = true; // checkedla.
                        }
                    }
                }
            }
            else if (gun == 6)
            {
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE 1", baglanti);
                SQLiteDataReader drchck = cekme.ExecuteReader();
                while (drchck.Read())
                {
                    if (drchck["Cumartesi"].ToString() == Convert.ToString(gun))
                    {
                        isaretlenmisidler.Add(Convert.ToInt32(drchck["id"]));
                    }
                }
                cekme.Dispose();
                drchck.Close();
                baglanti.Close();

                foreach (DataGridViewRow r in dataGridView3.Rows)
                {
                    foreach (int itm in isaretlenmisidler)
                    {
                        if (Convert.ToInt32(r.Cells[1].Value) == itm) // eğer isaretlenmiş dizinin içinersindeki pzt'tesiye ait olan checkler. doğruysa idler eşleşirse 
                        {
                            r.Cells[0].Value = true; // checkedla.
                        }
                    }
                }
            }
            else if (gun == 7)
            {
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE 1", baglanti);
                SQLiteDataReader drchck = cekme.ExecuteReader();
                while (drchck.Read())
                {
                    if (drchck["Pazar"].ToString() == Convert.ToString(gun))
                    {
                        isaretlenmisidler.Add(Convert.ToInt32(drchck["id"]));
                    }
                }
                cekme.Dispose();
                drchck.Close();
                baglanti.Close();

                foreach (DataGridViewRow r in dataGridView3.Rows)
                {
                    foreach (int itm in isaretlenmisidler)
                    {
                        if (Convert.ToInt32(r.Cells[1].Value) == itm) // eğer isaretlenmiş dizinin içinersindeki pzt'tesiye ait olan checkler. doğruysa idler eşleşirse 
                        {
                            r.Cells[0].Value = true; // checkedla.
                        }
                    }
                }
            }
            else
            {
                // -1 yani hiçbir ders seçilmemiş.
            }
        }


        private void button16_Click(object sender, EventArgs e)
        {
            if (gun == 1)
            {
                
                switch(cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPzt='" + cmbPzt.Text + "', OtoKPzt='"+cmbKPzt.Text+"' WHERE id=1", baglanti); // Dersi Adını ve konuyu Veritabanına Ekleyelim.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPzt2='" + cmbPzt.Text + "', OtoKPzt2='"+cmbKPzt.Text+"' WHERE id=1", baglanti); 
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPzt3='" + cmbPzt.Text + "', OtoKPzt3='"+cmbKPzt.Text+"' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }
                

                // eski seçilen id leri -1 değerine dönderip sonra yeni idleri ekliyoruz. bunu 1 kere yapmak zorunda.
                ArrayList chkecliidler = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
                SQLiteDataReader drchcked = cekme.ExecuteReader();
                while (drchcked.Read())
                {
                    if (drchcked["Pzt"].ToString() == Convert.ToString(gun))
                    {
                        chkecliidler.Add(Convert.ToInt32(drchcked["id"]));
                    }
                }
                cekme.Dispose();
                drchcked.Close();
                baglanti.Close();

                baglanti.Open();
                for (int i = 0; i < chkecliidler.Count; i++)
                {
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Pzt=-1 WHERE id=" + chkecliidler[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
            }
            else if (gun == 2)
            {

                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoSali='" + cmbSali.Text + "', OtoKSali='" + cmbKSali.Text + "' WHERE id=1", baglanti); // Dersi Adını Veritabanına Ekleyelim.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoSali2='" + cmbSali.Text + "', OtoKSali2='" + cmbKSali.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoSali3='" + cmbSali.Text + "', OtoKSali3='" + cmbKSali.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

                

                ArrayList chkecliidler = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
                SQLiteDataReader drchcked = cekme.ExecuteReader();
                while (drchcked.Read())
                {
                    if (drchcked["Sali"].ToString() == Convert.ToString(gun))
                    {
                        chkecliidler.Add(Convert.ToInt32(drchcked["id"]));
                    }
                }
                cekme.Dispose();
                drchcked.Close();
                baglanti.Close();

                baglanti.Open();
                for (int i = 0; i < chkecliidler.Count; i++)
                {
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Sali=-1 WHERE id=" + chkecliidler[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
            }
            else if (gun == 3)
            {

                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCarsamba='" + cmbCarsamba.Text + "', OtoKCarsamba='" + cmbKCarsamba.Text+"' WHERE id=1", baglanti); // Dersi Adını Veritabanına Ekleyelim.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCarsamba2='" + cmbCarsamba.Text + "', OtoKCarsamba2='" + cmbKCarsamba.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCarsamba3='" + cmbCarsamba.Text + "', OtoKCarsamba3='" + cmbKCarsamba.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

                

                ArrayList chkecliidler = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
                SQLiteDataReader drchcked = cekme.ExecuteReader();
                while (drchcked.Read())
                {
                    if (drchcked["Carsamba"].ToString() == Convert.ToString(gun))
                    {
                        chkecliidler.Add(Convert.ToInt32(drchcked["id"]));
                    }
                }
                cekme.Dispose();
                drchcked.Close();
                baglanti.Close();

                baglanti.Open();
                for (int i = 0; i < chkecliidler.Count; i++)
                {
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Carsamba=-1 WHERE id=" + chkecliidler[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
            }
            else if (gun == 4)
            {

                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPersembe='" + cmbPersembe.Text + "', OtoKPersembe='" + cmbKPersembe.Text +"' WHERE id=1", baglanti); // Dersi Adını Veritabanına Ekleyelim.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPersembe2='" + cmbPersembe.Text + "', OtoKPersembe2='" + cmbKPersembe.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPersembe3='" + cmbPersembe.Text + "', OtoKPersembe3='" + cmbKPersembe.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

                

                ArrayList chkecliidler = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
                SQLiteDataReader drchcked = cekme.ExecuteReader();
                while (drchcked.Read())
                {
                    if (drchcked["Persembe"].ToString() == Convert.ToString(gun))
                    {
                        chkecliidler.Add(Convert.ToInt32(drchcked["id"]));
                    }
                }
                cekme.Dispose();
                drchcked.Close();
                baglanti.Close();

                baglanti.Open();
                for (int i = 0; i < chkecliidler.Count; i++)
                {
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Persembe=-1 WHERE id=" + chkecliidler[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
            }
            else if (gun == 5)
            {

                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCuma='" + cmbCuma.Text + "', OtoKCuma='" + cmbKCuma.Text +"' WHERE id=1", baglanti); // Dersi Adını Veritabanına Ekleyelim.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCuma2='" + cmbCuma.Text + "', OtoKCuma2='" + cmbKCuma.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCuma3='" + cmbCuma.Text + "', OtoKCuma3='" + cmbKCuma.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

               

                ArrayList chkecliidler = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
                SQLiteDataReader drchcked = cekme.ExecuteReader();
                while (drchcked.Read())
                {
                    if (drchcked["Cuma"].ToString() == Convert.ToString(gun))
                    {
                        chkecliidler.Add(Convert.ToInt32(drchcked["id"]));
                    }
                }
                cekme.Dispose();
                drchcked.Close();
                baglanti.Close();

                baglanti.Open();
                for (int i = 0; i < chkecliidler.Count; i++)
                {
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Cuma=-1 WHERE id=" + chkecliidler[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
            }
            else if (gun == 6)
            {

                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCumartesi='" + cmbCumartesi.Text + "', OtoKCumartesi='" + cmbKCumartesi.Text +"' WHERE id=1", baglanti); // Dersi Adını Veritabanına Ekleyelim.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCumartesi2='" + cmbCumartesi.Text + "', OtoKCumartesi2='" + cmbKCumartesi.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoCumartesi3='" + cmbCumartesi.Text + "', OtoKCumartesi3='" + cmbKCumartesi.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

                

                ArrayList chkecliidler = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
                SQLiteDataReader drchcked = cekme.ExecuteReader();
                while (drchcked.Read())
                {
                    if (drchcked["Cumartesi"].ToString() == Convert.ToString(gun))
                    {
                        chkecliidler.Add(Convert.ToInt32(drchcked["id"]));
                    }
                }
                cekme.Dispose();
                drchcked.Close();
                baglanti.Close();

                baglanti.Open();
                for (int i = 0; i < chkecliidler.Count; i++)
                {
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Cumartesi=-1 WHERE id=" + chkecliidler[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
            }
            else if (gun == 7)
            {
                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPazar='" + cmbPazar.Text + "', OtoKPazar='"+cmbKPazar.Text+"' WHERE id=1", baglanti); // Dersi Adını Veritabanına Ekleyelim.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPazar2='" + cmbPazar.Text + "', OtoKPazar2='" + cmbKPazar.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET OtoPazar3='" + cmbPazar.Text + "', OtoKPazar3='" + cmbKPazar.Text + "' WHERE id=1", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

                

                ArrayList chkecliidler = new ArrayList();

                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
                SQLiteDataReader drchcked = cekme.ExecuteReader();
                while (drchcked.Read())
                {
                    if (drchcked["Pazar"].ToString() == Convert.ToString(gun))
                    {
                        chkecliidler.Add(Convert.ToInt32(drchcked["id"]));
                    }
                }
                cekme.Dispose();
                drchcked.Close();
                baglanti.Close();

                baglanti.Open();
                for (int i = 0; i < chkecliidler.Count; i++)
                {
                    cmd = new SQLiteCommand("UPDATE GvnSorular SET Pazar=-1 WHERE id=" + chkecliidler[i] + "", baglanti);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }
                baglanti.Close();
            }
            else
            {
                // -1 yani hiçbir ders seçilmemiş.
            }

            // şimdik checkedli olanları checkedliyelim.

            foreach (DataGridViewRow r in dataGridView3.Rows)
            {
                if (r.Cells[0].Value != null && (bool)r.Cells[0].Value)
                {
                    
                    if (gun == 1)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnSorular SET Pzt=" + gun + " WHERE id=" + Convert.ToInt32(r.Cells[1].Value) + "", baglanti); // id değerine göre update iştemi yapıyoruz yani o güne ait bir soru olucak bu.
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();

                    }
                    else if (gun == 2)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnSorular SET Sali=" + gun + " WHERE id=" + Convert.ToInt32(r.Cells[1].Value) + "", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                    else if (gun == 3)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnSorular SET Carsamba=" + gun + " WHERE id=" + Convert.ToInt32(r.Cells[1].Value) + "", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                    else if (gun == 4)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnSorular SET Persembe=" + gun + " WHERE id=" + Convert.ToInt32(r.Cells[1].Value) + "", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                    else if (gun == 5)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnSorular SET Cuma=" + gun + " WHERE id=" + Convert.ToInt32(r.Cells[1].Value) + "", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                    else if (gun == 6)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnSorular SET Cumartesi=" + gun + " WHERE id=" + Convert.ToInt32(r.Cells[1].Value) + "", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                    else if (gun == 7)
                    {
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnSorular SET Pazar=" + gun + " WHERE id=" + Convert.ToInt32(r.Cells[1].Value) + "", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                    else
                    {
                        // -1 yani hiçbir ders seçilmemiş.
                    }

                }
            }

            if (gun == 1)
            {
                MessageBox.Show("Pazartesi Günün Soruları Güncellendi.","Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else if (gun == 2)
            {
                MessageBox.Show("Salı Günün Soruları Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else if (gun == 3)
            {
                MessageBox.Show("Çarşamba Günün Soruları Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else if (gun == 4)
            { 
                MessageBox.Show("Perşembe Günün Soruları Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else if (gun == 5)
            {
                MessageBox.Show("Cuma Günün Soruları Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else if (gun == 6)
            {
                MessageBox.Show("Cumartesi Günün Soruları Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else if (gun == 7)
            {
                MessageBox.Show("Pazar Günün Soruları Güncellendi.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                MessageBox.Show("Ders Seçmediğiniz için Güncelleme Yapılmadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                // -1 yani hiçbir ders seçilmemiş.
            }

            // bug olasılığını kıralım diğer buton ilede kullanı bu durumdan emin olsun diye yapılmaktadır.

            ArrayList tut = new ArrayList();

            // 7 gün oldugu için sayarak bakabiliriz 7 güne ait sorular checkedlenmişmi diye.
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT Pzt,Sali,Carsamba,Persembe,Cuma,Cumartesi,Pazar FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti); // tüm soruların içinde 1 den 7 kadar checklenmiş soru varsa programlama tamamlanmış demektir.
            SQLiteDataReader drcek = cekme.ExecuteReader();
            while (drcek.Read())
            {
                tut.Add(drcek["Pzt"].ToString());
                tut.Add(drcek["Sali"].ToString());
                tut.Add(drcek["Carsamba"].ToString());
                tut.Add(drcek["Persembe"].ToString());
                tut.Add(drcek["Cuma"].ToString());
                tut.Add(drcek["Cumartesi"].ToString());
                tut.Add(drcek["Pazar"].ToString());
            }
            cekme.Dispose();
            drcek.Close();
            baglanti.Close();

            int say = 0;
            for (int i = 1; i <= 7; i++)
            {
                foreach (string itm in tut) // içinde 7 kadar sarı varmı diye taratıyoruz.
                {
                    if (itm == Convert.ToString(i))
                    {
                        say++;
                        break; // 1 kere eşleştinmi döngüden çık 7 kadar kontrol olucak toplam 7 kere girmesi gerekıyor.
                    }
                }
            }

            if (say == 7)
            {
                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HaftayaProgramlamaPA='Aktif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HPOrtaOkul='Aktif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HPLise='Aktif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

            }
            else
            {
                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HaftayaProgramlamaPA='Pasif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HPOrtaOkul='Pasif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HPLise='Pasif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }
            }

            // Form1 deki çıkışı sağla

            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=1", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();

        }

       
        // Haftayı Programla Button

        private void button15_Click(object sender, EventArgs e)
        {
            // Form1 çıkışı sağla.
            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=1", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();

            ArrayList tut = new ArrayList();
            
            // 7 gün oldugu için sayarak bakabiliriz 7 güne ait sorular checkedlenmişmi diye.
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT Pzt,Sali,Carsamba,Persembe,Cuma,Cumartesi,Pazar FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "'", baglanti); // tüm soruların içinde 1 den 7 kadar checklenmiş soru varsa programlama tamamlanmış demektir.
            SQLiteDataReader drcek = cekme.ExecuteReader();
            while (drcek.Read())
            {
                tut.Add(drcek["Pzt"].ToString());
                tut.Add(drcek["Sali"].ToString());
                tut.Add(drcek["Carsamba"].ToString());
                tut.Add(drcek["Persembe"].ToString());
                tut.Add(drcek["Cuma"].ToString());
                tut.Add(drcek["Cumartesi"].ToString());
                tut.Add(drcek["Pazar"].ToString());
            }
            cekme.Dispose();
            drcek.Close();
            baglanti.Close();

            int say = 0;
            for (int i = 1; i <= 7; i++)
            {
                foreach (string itm in tut) // içinde 7 kadar sarı varmı diye taratıyoruz.
                {
                    if (itm == Convert.ToString(i)) 
                    {
                        say++;
                        break; // 1 kere eşleştinmi döngüden çık 7 kadar kontrol olucak toplam 7 kere girmesi gerekıyor.
                    }
                }
            }


            if (say == 7)
            {
                switch (cmbSoruSeviye.Text)
                { 
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HaftayaProgramlamaPA='Aktif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        MessageBox.Show("İlk Okul Otomatik Programlama Tamamlandı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HPOrtaOkul='Aktif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        MessageBox.Show("Orta Okul Otomatik Programlama Tamamlandı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        break;
                    case "Lise":                            
                            baglanti.Open();
                            cmd = new SQLiteCommand("UPDATE GvnGenel SET HPLise='Aktif'", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();
                            MessageBox.Show("Lise Otomatik Programlama Tamamlandı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        break;
                }
                
            }
            else
            {
                switch (cmbSoruSeviye.Text)
                {
                    case "İlk Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HaftayaProgramlamaPA='Pasif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Orta Okul":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HPOrtaOkul='Pasif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                    case "Lise":
                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET HPLise='Pasif'", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                        break;
                }

                MessageBox.Show("Pazartesiden - Pazar Gününe kadar içindeki Soruları Tamamlayın.","Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET Seviye='" + cmbSoruSeviye.Text + "'", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();

        }

        private void button17_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dataGridView3.Rows)
            {
                r.Cells[0].Value = true; // Hepsini checkedle.
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in dataGridView3.Rows)
            {
                r.Cells[0].Value = false; // checked kaldır.
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            int engel = -1;

            baglanti.Open();
            cekme = new SQLiteCommand("SELECT AnaGuvenlik FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drAnaGuvenlik = cekme.ExecuteReader();
            while (drAnaGuvenlik.Read())
            {
                engel = Convert.ToInt32(drAnaGuvenlik["AnaGuvenlik"]);
                break;
            }
            cekme.Dispose();
            drAnaGuvenlik.Close();
            baglanti.Close();

            if (engel == 1) // eğer 1 ise zaten engel açık tekrar basıldı ise devre dışı bırakmak istiyor demektir.
            {
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET AnaGuvenlik=0 WHERE id=1", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                lblGuvenlikDurumu.Text = "Güvenlik Durumu : Pasif Engeller kaldırıldı.";

                try
                {
                    RegistryKey rkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies", true);
                    rkey.CreateSubKey("System", RegistryKeyPermissionCheck.Default);
                    rkey.Close();
                }
                catch
                {
                }

                try
                {
                    RegistryKey rkey2 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey2.SetValue("DisableTaskMgr", 0);
                    rkey2.Close();
                }
                catch
                {}
                try
                {
                    RegistryKey rkey3 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey3.SetValue("DisableLockWorkstation", 0);
                    rkey3.Close();
                }
                catch
                {}
                try
                {
                    RegistryKey rkey4 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey4.SetValue("DisableChangePassword", 0);
                    rkey4.Close();
                }
                catch
                {}
                try
                {
                    RegistryKey rkey5 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey5.SetValue("HideFastUserSwitching", 0);
                    rkey5.Close();
                }
                catch
                {}
                try
                {
                    RegistryKey rkey6 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                    rkey6.SetValue("NoLogoff", 0);
                    rkey6.Close();
                }
                catch
                {
                }
            }

            if (engel == 0) // eğer engel 0 ise bunu demektirki engelleme yok bu yüzden engelleme yapılcak.
            {
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET AnaGuvenlik=1 WHERE id=1", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                lblGuvenlikDurumu.Text = "Güvenlik Durumu : Aktif  Görev Yöneticisi,CTRL+ALT+DELETE,CTRL+F4 vb. tuşlar devre dışı.";

                try
                {
                    RegistryKey rkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies", true);
                    rkey.CreateSubKey("System", RegistryKeyPermissionCheck.Default);
                    rkey.Close();
                }
                catch
                {}
                try
                {
                    RegistryKey rkey2 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey2.SetValue("DisableTaskMgr", 1);
                    rkey2.Close();
                }
                catch
                { }

                try
                { 
                    RegistryKey rkey3 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey3.SetValue("DisableLockWorkstation", 1);
                    rkey3.Close();
                }
                catch
                { }

                try
                { 
                    RegistryKey rkey4 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey4.SetValue("DisableChangePassword", 1);
                    rkey4.Close();
                }
                catch
                { }
                
                try
                { 
                    RegistryKey rkey5 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey5.SetValue("HideFastUserSwitching", 1);
                    rkey5.Close();
                }
                catch
                { }

                try
                { 
                    RegistryKey rkey6 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                    rkey6.SetValue("NoLogoff", 1);
                    rkey6.Close();
                }
                catch
                {}
            }

        }

        private void button20_Click(object sender, EventArgs e)
        {
            ExcelSoruGiris exc = new ExcelSoruGiris();
            exc.ShowDialog();

        }

        WebClient webClient;
        WebClient webClient2;
        WebClient webClient3;
        bool updateControl = false;
        private void button21_Click(object sender, EventArgs e)
        {
            if (updateControl == false)
             {
                updateControl = true;
                // İnternet bağlantı testi.
                string url = "http://www.google.com";
                bool test = false;
                try
                {
                    WebRequest myRequest = WebRequest.Create(url);
                    WebResponse myResponse = myRequest.GetResponse();
                    test = true;
                    myResponse.Close();
                }
                catch (WebException)
                {
                    test = false;
                }

                if (test == false)
                {
                    MessageBox.Show("İnternet Bağlantınız Yok.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    try
                    {

                        string indirilecek = "http://www.rexsquad.com/imtem/Verimtem.zip";
                        string dosyaYolu = @"C:\Verimtem.zip"; // indirileceği yer.
                        // indirme işlemi.
                        webClient2 = new WebClient();
                        webClient2.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged2);
                        webClient2.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed2);
                        webClient2.DownloadFileAsync(new Uri(indirilecek), dosyaYolu);                       

                    }
                    catch (Exception exx)
                    {
                        MessageBox.Show("Version Kontrol edilirken hata oluştu." + exx.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        updateControl = false;
                        webClient2.CancelAsync();
                        webClient2.Dispose();
                    }


                }
            }
            else
            {
                MessageBox.Show("Lütfen işlemin bitmesini bekleyin...", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }

        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            lblDownloadSure.Text = Convert.ToString("%")+ e.ProgressPercentage;
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                File.Copy(@"C:\database.s3db", Application.StartupPath.ToString()+"\\database.s3db", true);
            }
            catch ( Exception ex)
            {
                MessageBox.Show("Database kopyalanırken hata oluştu.\n\n"+ ex.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            
            
            try
            {
                File.Delete(@"C:\database.s3db");
            }
            catch ( Exception exx)
            {
                MessageBox.Show("İndirilen database silinirken bir hata oluştu. Dosya bulunamıyor olabilir.\n\n"+ exx.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            
            lblDownloadSure.Text = "";
            timer2.Start();
            webClient.CancelAsync();
            webClient.Dispose();
            // yeni databasenin güncel versionunu yazıyoruz.
            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET Version='" + NetVer + "' WHERE id=1", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();

            // Resimleri güncelleme.
            try
            {
                string indirilecek2 = "http://www.rexsquad.com/imtem/Resimler.zip";
                string dosyaYolu2 = @"C:\Resimler.zip"; // indirileceği yer.
                // indirme işlemi.
                webClient3 = new WebClient();
                webClient3.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged3);
                webClient3.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed3);
                webClient3.DownloadFileAsync(new Uri(indirilecek2), dosyaYolu2);
            }
            catch (Exception exx)
            {
                MessageBox.Show("Resimler İndirilirken hata oluştu.\n\n" + exx.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                updateControl = false;
            }

            
        }

        private void ProgressChanged3(object sender, DownloadProgressChangedEventArgs e)
        {
            lblDownloadSure.Text = Convert.ToString("%") + e.ProgressPercentage;
        }

        private void Completed3(object sender, AsyncCompletedEventArgs e)
        {
            lblDownloadSure.Text = "";
            webClient3.CancelAsync();
            webClient3.Dispose();

            ZipFile zipFile = new ZipFile(@"C:\Resimler.zip");
            try
            {
                zipFile.ExtractAll(Application.StartupPath.ToString()+@"\image\", ExtractExistingFileAction.DoNotOverwrite);
                zipFile.Dispose();
                MessageBox.Show("Database Güncelleme İşlemi Tamamlandı.\n\nYönetici Paneli Kapatılacak Tekrar Giriş Yapınız.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            catch (Exception exx)
            {
                MessageBox.Show("Resimler Çıkartılırken Hata oluştu.\n\n" + exx.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                updateControl = false;
                zipFile.Dispose();
            }
            
            try
            {
                File.Delete(@"C:\Resimler.zip");
            }
            catch (Exception exp)
            {
                MessageBox.Show("İndirilen Resimler.zip silinirken Hata oluştu.\n\n" + exp.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            updateControl = false;
            this.Close();
        }

        private void ProgressChanged2(object sender, DownloadProgressChangedEventArgs e)
        {
            lblDownloadSure.Text = Convert.ToString("%") + e.ProgressPercentage;
        }

        string NetVer = "";

        private void Completed2(object sender, AsyncCompletedEventArgs e)
        {                    
            lblDownloadSure.Text = "";
            webClient2.CancelAsync();
            webClient2.Dispose();

            string ver = "";

            NetVer = "";

            baglanti.Open();
            cekme = new SQLiteCommand("SELECT Version FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader dr = cekme.ExecuteReader();
            while (dr.Read())
            {
                ver = dr["Version"].ToString();
            }
            cekme.Dispose();
            dr.Close();
            baglanti.Close();            

            bool kesinAlindiMiBilgi = false;
            try
            {
                
                ZipFile zipFile = new ZipFile(@"C:\Verimtem.zip");

                zipFile.ExtractAll(@"C:\");
                zipFile.Dispose(); // zip kapatma.

                
                StreamReader oku; //yol bilgisini akıştan okuyacak. yani dosyamızı okuyacak

                oku = File.OpenText(@"C:\Verimtem.txt");

                string yazi;

                while ((yazi = oku.ReadLine()) != null) //satır boş olana kadar satır satır okumaya devam eder
                {
                    NetVer = yazi.ToString();
                }

                oku.Close();//okumayı kapat

                File.Delete(@"C:\Verimtem.txt");
                File.Delete(@"C:\Verimtem.zip");
                kesinAlindiMiBilgi = true;
            }
            catch (Exception ep)
            {
                MessageBox.Show("Version kontrol edilemedi. Hedef Dosya Bulunamadı.\n\n" + ep.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            if (NetVer == ver)
            {
                MessageBox.Show("Güncel Version'u kullanıyorsunuz.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (kesinAlindiMiBilgi == true)
                {
                    // yeni versionu güncelliyoruz.


                    DialogResult result = MessageBox.Show("Database'yi Güncellemek istediğinizden emin misiniz ?\n\nGüncelleme yapıldığında şifreniz varsayılan şifre olarak değişecektir ve kayıtlarınız silinecektir.", "Yönetici Paneli", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.Yes)
                    {
                        // İnternet bağlantı testi.
                        string url2 = "http://www.google.com";
                        bool test2 = false;
                        try
                        {
                            WebRequest myRequest = WebRequest.Create(url2);
                            WebResponse myResponse = myRequest.GetResponse();
                            test2 = true;
                            myResponse.Close();
                        }
                        catch (WebException)
                        {
                            test2 = false;
                        }

                        if (test2 == false)
                        {
                            MessageBox.Show("İnternet Bağlantınız Yok.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        }
                        else
                        {
                            baglanti.Open();
                            cmd = new SQLiteCommand("UPDATE GvnGenel SET VeriTabaniGuncelleme=1 WHERE id=1", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            timer2.Stop();
                            try
                            {
                                string indirilecek2 = "http://www.rexsquad.com/imtem/database.s3db";
                                string dosyaYolu2 = @"C:\database.s3db"; // indirileceği yer.
                                // indirme işlemi.
                                webClient = new WebClient();
                                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);
                                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                                webClient.DownloadFileAsync(new Uri(indirilecek2), dosyaYolu2);
                            }
                            catch (Exception exx)
                            {
                                MessageBox.Show("Database İndirilirken hata oluştu." + exx.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                updateControl = false;
                                webClient.CancelAsync();
                                webClient.Dispose();
                            }


                        }

                    }
                }
            } 
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }
        Excel.Application excel;
        Excel.Workbook workbook;
        private void button22_Click(object sender, EventArgs e)
        {
            if (Kid == -1)
            {
                MessageBox.Show("Kullanıcı seçilmemiş!", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (aktarmaSecilen == ".txt/*.*")
                {
                    saveFileDialog1.Filter = "(*.txt)|*.txt|" + "Hepsi (*.*)|*.*";

                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string yol = saveFileDialog1.FileName;
                        StreamWriter yaz = new StreamWriter(saveFileDialog1.FileName);
                        foreach (DataGridViewRow r in dataGridView2.Rows)
                        {

                            yaz.WriteLine("Tarih: " + r.Cells[1].Value.ToString() + " Ders: " + r.Cells[2].Value.ToString() + " Soru Sayısı: " + r.Cells[3].Value.ToString() + " Doğru Cevap: " + r.Cells[4].Value.ToString() + " Yanlış Cevap: " + r.Cells[5].Value.ToString() + "");
                        }
                        yaz.Close();

                        MessageBox.Show("Dışarıya aktarım işlemi tamamlandı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                }
                else if (aktarmaSecilen == "Excel")
                {
                    try
                    {
                        excel = new Excel.Application();

                        excel.Visible = false;

                        workbook = excel.Workbooks.Add(System.Reflection.Missing.Value);

                        Excel.Worksheet sheet1 = (Excel.Worksheet)workbook.Sheets[1];

                        int StartCol = 1;

                        int StartRow = 1;

                        for (int j = 1; j < dataGridView2.Columns.Count; j++)
                        {

                            Excel.Range myRange = (Excel.Range)sheet1.Cells[StartRow, StartCol + j - 1];

                            myRange.Value2 = dataGridView2.Columns[j].HeaderText;

                        }
                        /*
                        Excel.Range myRange2 = (Excel.Range)sheet1.Cells[2, 1]; // excel deki 2 satır 1 sütun yazılcak. // sheet1.Cells[satır, sutün]
                        myRange2.Value2 = dataGridView2[1, 2].Value == null ? "" : dataGridView2[1, 2].Value; // datagridview deki 2 satırdaki 2 ci sundaki veri yazılcak

                        Excel.Range myRange3 = (Excel.Range)sheet1.Cells[5, 1]; // excel deki 3 satır 1 sütun na yazılcak
                        myRange3.Value2 = dataGridView2[1, 3].Value == null ? "" : dataGridView2[1, 3].Value;*/
                        // dataGridView2[sutün, satır]
                        //openFile1.Filter = "Resimler (*.BMP;*.JPG;*.PNG;*.GIF)|*.BMP;*.JPG;*.PNG;*.GIF|" + "All files (*.*)|*.*";
                        StartRow++;
                        for (int i = 0; i < dataGridView2.Rows.Count; i++)
                        {

                            for (int j = 0; j < dataGridView2.Columns.Count; j++)
                            {


                                try
                                {
                                    Excel.Range myRange = (Excel.Range)sheet1.Cells[StartRow + i, StartCol + j - 1];

                                    myRange.Value2 = dataGridView2[j, i].Value == null ? "" : dataGridView2[j, i].Value;

                                }

                                catch
                                {



                                }
                            }
                        }
                        saveFileDialog1.Filter = "(*.xls;*.xlsx)|*.xls;*.xlsx|" + "Hepsi (*.*)|*.*";
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {

                            workbook.SaveAs(saveFileDialog1.FileName);
                            workbook.Close(false);
                            excel.Quit();
                            MessageBox.Show("Dışarıya aktarım işlemi tamamlandı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        }
                        else
                        {
                            // reddedildi.
                            workbook.Close(false);
                            excel.Quit();
                        }


                    }
                    catch (Exception exx)
                    {
                        MessageBox.Show("Excel Kayıt Yaparken Hata Oluştu. \n\n" + exx.ToString(), "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        try
                        {
                            workbook.Close(false);
                            excel.Quit();
                        }
                        catch
                        {

                        }
                    }
                }
            }
            
        }

        string aktarmaSecilen = ".txt/*.*";

        private void cmbAktarmaSec_SelectedIndexChanged(object sender, EventArgs e)
        {
            aktarmaSecilen = cmbAktarmaSec.Text;
        }

        private void pictureBox5_MouseHover(object sender, EventArgs e)
        {
            
        }
        int formAktif = 0;
        private void YoneticiPaneli_Activated(object sender, EventArgs e)
        {
            if (formAktif == 0)
            {
                txtsifre.Focus();
                formAktif++;
            }
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void button23_Click(object sender, EventArgs e)
        {
            KullaniciAyarlari kA = new KullaniciAyarlari();
            kA.ShowDialog();
            ArrayList cmbsoruid = new ArrayList();
        }

        ArrayList cmbsoruid = new ArrayList();

        void cagir(string ders)
        {
            cmbDers.Items.Clear();
            cmbKonu.Items.Clear();
            cmbsoruid.Clear();
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT id,SoruTipi FROM GvnDersler WHERE Seviye='" + ders + "'", baglanti);
            SQLiteDataReader drcek = cekme.ExecuteReader();
            while (drcek.Read())
            {
                cmbDers.Items.Add(drcek["SoruTipi"].ToString());
                cmbsoruid.Add(drcek["id"]);
            }
            cekme.Dispose();
            drcek.Close();
            baglanti.Close();
        }

        void kapat()
        {
            richSoru.Enabled = false;
            txtResimYol.Enabled = false;
            richD.Enabled = false;
            richC.Enabled = false;
            richB.Enabled = false;
            richA.Enabled = false;
            cmbCevap.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            dataGridView1.Enabled = false;
            button10.Enabled = false;
            button12.Enabled = false;

            secilenResimAdi = "";
            pictureBox1.Image = null;
            txtResimYol.Text = "";
        }

        void ac()
        {
            richSoru.Enabled = true;
            txtResimYol.Enabled = true;
            richD.Enabled = true;
            richC.Enabled = true;
            richB.Enabled = true;
            richA.Enabled = true;
            cmbCevap.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            dataGridView1.Enabled = true;
            button10.Enabled = true;
            button12.Enabled = true;
        }

        private void cmbSeviye_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Sıfırlama İşlemleri
            Konu_id = -1;
            SoruID = -1;
            kapat();

            switch (cmbSeviye.SelectedIndex)
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


        int Konu_id = -1;
        private void cmbKonu_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            int secilenindex = cmbKonu.SelectedIndex;

            if (secilenindex == -1)
            {

            }
            else
            {
                Konu_id = -1;
                Konu_id = Convert.ToInt32(konuidleri[secilenindex]);

                if (Konu_id == -1)
                {
                    kapat();
                    MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    adp = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSeviye.Text + "' AND SoruID=" + SoruID + " AND Konu_id=" + Konu_id + "", baglanti);
                    datagridViewx(); // güncelle
                    ac();
                }
            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
        }

        private void button24_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show(fnk.systemYol());
        }

        string pzt = "", sali = "", carsamba = "", persembe = "", cuma = "", cumartesi = "", pazar = "", pztK = "", saliK = "", carsambaK = "", persembeK = "", cumaK = "", cumartesiK = "", pazarK = "";

        ArrayList Otokonu_adlari;
        ArrayList Otokonu_idleri;
        ArrayList Otodersid = new ArrayList();
        void otoCagir(string ders)
        {
            cmbPzt.Items.Clear();
            cmbSali.Items.Clear();
            cmbCarsamba.Items.Clear();
            cmbPersembe.Items.Clear();
            cmbCuma.Items.Clear();
            cmbCumartesi.Items.Clear();
            cmbPazar.Items.Clear();
            cmbKPzt.Items.Clear();
            cmbKSali.Items.Clear();
            cmbKCarsamba.Items.Clear();
            cmbKPersembe.Items.Clear();
            cmbKCuma.Items.Clear();
            cmbKCumartesi.Items.Clear();
            cmbKPazar.Items.Clear();
            Otodersid.Clear();
   

            baglanti.Open();
            cekme = new SQLiteCommand("SELECT id,SoruTipi FROM GvnDersler WHERE Seviye='" + ders + "'", baglanti);
            SQLiteDataReader drcek = cekme.ExecuteReader();
            while (drcek.Read())
            {
                cmbPzt.Items.Add(drcek["SoruTipi"].ToString());
                cmbSali.Items.Add(drcek["SoruTipi"].ToString());
                cmbCarsamba.Items.Add(drcek["SoruTipi"].ToString());
                cmbPersembe.Items.Add(drcek["SoruTipi"].ToString());
                cmbCuma.Items.Add(drcek["SoruTipi"].ToString());
                cmbCumartesi.Items.Add(drcek["SoruTipi"].ToString());
                cmbPazar.Items.Add(drcek["SoruTipi"].ToString());
                Otodersid.Add(drcek["id"]);
            }
            cekme.Dispose();
            drcek.Close();
            baglanti.Close();

            
            // Hatırlatmalar
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT * FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drhat = cekme.ExecuteReader();
            while (drhat.Read())
            {
                switch (ders) // ders
                {
                    case "İlk Okul":
                        pzt = drhat["OtoPzt"].ToString();
                        sali = drhat["OtoSali"].ToString();
                        carsamba = drhat["OtoCarsamba"].ToString();
                        persembe = drhat["OtoPersembe"].ToString();
                        cuma = drhat["OtoCuma"].ToString();
                        cumartesi = drhat["OtoCumartesi"].ToString();
                        pazar = drhat["OtoPazar"].ToString();
                        pztK = drhat["OtoKPzt"].ToString();
                        saliK = drhat["OtoKSali"].ToString();
                        carsambaK = drhat["OtoKCarsamba"].ToString();
                        persembeK = drhat["OtoKPersembe"].ToString();
                        cumaK = drhat["OtoKCuma"].ToString();
                        cumartesiK = drhat["OtoKCumartesi"].ToString();
                        pazarK = drhat["OtoKPazar"].ToString();
                        break;
                    case "Orta Okul":
                        pzt = drhat["OtoPzt2"].ToString();
                        sali = drhat["OtoSali2"].ToString();
                        carsamba = drhat["OtoCarsamba2"].ToString();
                        persembe = drhat["OtoPersembe2"].ToString();
                        cuma = drhat["OtoCuma2"].ToString();
                        cumartesi = drhat["OtoCumartesi2"].ToString();
                        pazar = drhat["OtoPazar2"].ToString();
                        pztK = drhat["OtoKPzt2"].ToString();
                        saliK = drhat["OtoKSali2"].ToString();
                        carsambaK = drhat["OtoKCarsamba2"].ToString();
                        persembeK = drhat["OtoKPersembe2"].ToString();
                        cumaK = drhat["OtoKCuma2"].ToString();
                        cumartesiK = drhat["OtoKCumartesi2"].ToString();
                        pazarK = drhat["OtoKPazar2"].ToString();
                        break;
                    case "Lise":
                        pzt = drhat["OtoPzt3"].ToString();
                        sali = drhat["OtoSali3"].ToString();
                        carsamba = drhat["OtoCarsamba3"].ToString();
                        persembe = drhat["OtoPersembe3"].ToString();
                        cuma = drhat["OtoCuma3"].ToString();
                        cumartesi = drhat["OtoCumartesi3"].ToString();
                        pazar = drhat["OtoPazar3"].ToString();
                        pztK = drhat["OtoKPzt3"].ToString();
                        saliK = drhat["OtoKSali3"].ToString();
                        carsambaK = drhat["OtoKCarsamba3"].ToString();
                        persembeK = drhat["OtoKPersembe3"].ToString();
                        cumaK = drhat["OtoKCuma3"].ToString();
                        cumartesiK = drhat["OtoKCumartesi3"].ToString();
                        pazarK = drhat["OtoKPazar3"].ToString();
                        break;
                }
                break;
            }
            cekme.Dispose();
            drhat.Close();
            baglanti.Close();

            //Burası Ders Leri hatırlatma bölmü konuları ise dersleri otomatik seçtikten sonra eklendiği için orda kontrolü yapılacak.
            int index = -1;

            index = cmbPzt.FindString(pzt);
            if (index != -1) cmbPzt.SelectedIndex = index;

            index = cmbSali.FindString(sali);
            if (index != -1) cmbSali.SelectedIndex = index;

            index = cmbCarsamba.FindString(carsamba);
            if (index != -1) cmbCarsamba.SelectedIndex = index;

            index = cmbPersembe.FindString(persembe);
            if (index != -1) cmbPersembe.SelectedIndex = index;

            index = cmbCuma.FindString(cuma);
            if (index != -1) cmbCuma.SelectedIndex = index;

            index = cmbCumartesi.FindString(cumartesi);
            if (index != -1) cmbCumartesi.SelectedIndex = index;

            index = cmbPazar.FindString(pazar);
            if (index != -1) cmbPazar.SelectedIndex = index;



        }

        private void cmbSoruSeviye_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(cmbSoruSeviye.SelectedIndex)
            {
                case 0: // İlk okul
                    otoCagir("İlk Okul");
                    break;
                case 1: // Orta Okul
                    otoCagir("Orta Okul");
                    break;
                case 2:// Lise
                    otoCagir("Lise");
                    break;
            }
        }

        int otokonu_id = -1;
        private void cmbKPzt_SelectedIndexChanged(object sender, EventArgs e)
        {

            string secilenKonu = cmbKPzt.SelectedItem.ToString();
            otokonu_id = -1;
            for (int i = 0; i < Otokonu_adlari.Count; i++)
			{
			    if(Otokonu_adlari[i].ToString() == secilenKonu) // seçilen isim ile konu daki isim eşleşirse konu_id bulundu demektir.
                {
                    otokonu_id = Convert.ToInt32(Otokonu_idleri[i]);
                    
                    break;
                }
			}

            if (otokonu_id == -1)
            {
                MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                adp3 = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "' AND SoruID=" + secilenDersSoruID + " AND Konu_id=" + otokonu_id + "", baglanti);
                datagridViewDers(); // güncelle
                chked();
                lblPzt.ForeColor = Color.Red;
                lblSali.ForeColor = Color.Black;
                lblCarsamba.ForeColor = Color.Black;
                lblPersembe.ForeColor = Color.Black;
                lblCuma.ForeColor = Color.Black;
                lblCumartesi.ForeColor = Color.Black;
                lblPazar.ForeColor = Color.Black;
            }
            
        }

        private void cmbKSali_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKonu = cmbKSali.SelectedItem.ToString();
            otokonu_id = -1;
            for (int i = 0; i < Otokonu_adlari.Count; i++)
            {
                if (Otokonu_adlari[i].ToString() == secilenKonu) // seçilen isim ile konu daki isim eşleşirse konu_id bulundu demektir.
                {
                    otokonu_id = Convert.ToInt32(Otokonu_idleri[i]);

                    break;
                }
            }

            if (otokonu_id == -1)
            {
                MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                adp3 = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "' AND SoruID=" + secilenDersSoruID + " AND Konu_id=" + otokonu_id + "", baglanti);
                datagridViewDers(); // güncelle
                chked();
                lblPzt.ForeColor = Color.Black;
                lblSali.ForeColor = Color.Red;
                lblCarsamba.ForeColor = Color.Black;
                lblPersembe.ForeColor = Color.Black;
                lblCuma.ForeColor = Color.Black;
                lblCumartesi.ForeColor = Color.Black;
                lblPazar.ForeColor = Color.Black;
            }
        }

        private void cmbKCarsamba_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKonu = cmbKCarsamba.SelectedItem.ToString();
            otokonu_id = -1;
            for (int i = 0; i < Otokonu_adlari.Count; i++)
            {
                if (Otokonu_adlari[i].ToString() == secilenKonu) // seçilen isim ile konu daki isim eşleşirse konu_id bulundu demektir.
                {
                    otokonu_id = Convert.ToInt32(Otokonu_idleri[i]);

                    break;
                }
            }

            if (otokonu_id == -1)
            {
                MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                adp3 = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "' AND SoruID=" + secilenDersSoruID + " AND Konu_id=" + otokonu_id + "", baglanti);
                datagridViewDers(); // güncelle
                chked();
                lblPzt.ForeColor = Color.Black;
                lblSali.ForeColor = Color.Black;
                lblCarsamba.ForeColor = Color.Red;
                lblPersembe.ForeColor = Color.Black;
                lblCuma.ForeColor = Color.Black;
                lblCumartesi.ForeColor = Color.Black;
                lblPazar.ForeColor = Color.Black;
            }
        }

        private void cmbKPersembe_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKonu = cmbKPersembe.SelectedItem.ToString();
            otokonu_id = -1;
            for (int i = 0; i < Otokonu_adlari.Count; i++)
            {
                if (Otokonu_adlari[i].ToString() == secilenKonu) // seçilen isim ile konu daki isim eşleşirse konu_id bulundu demektir.
                {
                    otokonu_id = Convert.ToInt32(Otokonu_idleri[i]);

                    break;
                }
            }

            if (otokonu_id == -1)
            {
                MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                adp3 = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "' AND SoruID=" + secilenDersSoruID + " AND Konu_id=" + otokonu_id + "", baglanti);
                datagridViewDers(); // güncelle
                chked();
                lblPzt.ForeColor = Color.Black;
                lblSali.ForeColor = Color.Black;
                lblCarsamba.ForeColor = Color.Black;
                lblPersembe.ForeColor = Color.Red;
                lblCuma.ForeColor = Color.Black;
                lblCumartesi.ForeColor = Color.Black;
                lblPazar.ForeColor = Color.Black;
            }
        }

        private void cmbKCuma_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKonu = cmbKCuma.SelectedItem.ToString();
            otokonu_id = -1;
            for (int i = 0; i < Otokonu_adlari.Count; i++)
            {
                if (Otokonu_adlari[i].ToString() == secilenKonu) // seçilen isim ile konu daki isim eşleşirse konu_id bulundu demektir.
                {
                    otokonu_id = Convert.ToInt32(Otokonu_idleri[i]);

                    break;
                }
            }

            if (otokonu_id == -1)
            {
                MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                adp3 = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "' AND SoruID=" + secilenDersSoruID + " AND Konu_id=" + otokonu_id + "", baglanti);
                datagridViewDers(); // güncelle
                chked();
                lblPzt.ForeColor = Color.Black;
                lblSali.ForeColor = Color.Black;
                lblCarsamba.ForeColor = Color.Black;
                lblPersembe.ForeColor = Color.Black;
                lblCuma.ForeColor = Color.Red;
                lblCumartesi.ForeColor = Color.Black;
                lblPazar.ForeColor = Color.Black;
            }
        }

        private void cmbKCumartesi_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKonu = cmbKCumartesi.SelectedItem.ToString();
            otokonu_id = -1;
            for (int i = 0; i < Otokonu_adlari.Count; i++)
            {
                if (Otokonu_adlari[i].ToString() == secilenKonu) // seçilen isim ile konu daki isim eşleşirse konu_id bulundu demektir.
                {
                    otokonu_id = Convert.ToInt32(Otokonu_idleri[i]);

                    break;
                }
            }

            if (otokonu_id == -1)
            {
                MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                adp3 = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "' AND SoruID=" + secilenDersSoruID + " AND Konu_id=" + otokonu_id + "", baglanti);
                datagridViewDers(); // güncelle
                chked();
                lblPzt.ForeColor = Color.Black;
                lblSali.ForeColor = Color.Black;
                lblCarsamba.ForeColor = Color.Black;
                lblPersembe.ForeColor = Color.Black;
                lblCuma.ForeColor = Color.Black;
                lblCumartesi.ForeColor = Color.Red;
                lblPazar.ForeColor = Color.Black;
            }
        }

        private void cmbKPazar_SelectedIndexChanged(object sender, EventArgs e)
        {
            string secilenKonu = cmbKPazar.SelectedItem.ToString();
            otokonu_id = -1;
            for (int i = 0; i < Otokonu_adlari.Count; i++)
            {
                if (Otokonu_adlari[i].ToString() == secilenKonu) // seçilen isim ile konu daki isim eşleşirse konu_id bulundu demektir.
                {
                    otokonu_id = Convert.ToInt32(Otokonu_idleri[i]);

                    break;
                }
            }

            if (otokonu_id == -1)
            {
                MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                adp3 = new SQLiteDataAdapter("SELECT id,Soru,Ask,Bsk,Csk,Dsk,ResimYol,Cevap FROM GvnSorular WHERE Seviye='" + cmbSoruSeviye.Text + "' AND SoruID=" + secilenDersSoruID + " AND Konu_id=" + otokonu_id + "", baglanti);
                datagridViewDers(); // güncelle
                chked();
                lblPzt.ForeColor = Color.Black;
                lblSali.ForeColor = Color.Black;
                lblCarsamba.ForeColor = Color.Black;
                lblPersembe.ForeColor = Color.Black;
                lblCuma.ForeColor = Color.Black;
                lblCumartesi.ForeColor = Color.Black;
                lblPazar.ForeColor = Color.Red;
            }
        }

        ArrayList kullaniciId = new ArrayList();
        ArrayList kullaniciadi = new ArrayList();

        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string scKadi = cmbKullanici.SelectedItem.ToString();
            Kid = -1;
            for (int i = 0; i < kullaniciadi.Count; i++)
            {
                if (scKadi == kullaniciadi[i].ToString())
                    Kid = Convert.ToInt32(kullaniciId[i]);
            }
            if (Kid != -1)
            {
                //////////////// ANALİZ /////////////////////////

                // Hepsi Olarak Veritabanını yazdır.
                adp2 = new SQLiteDataAdapter("SELECT id,Tarih,Ders,SoruSayisi,dCevap,yCevap,K_id FROM GvnAnaliz WHERE K_id=" + Kid, baglanti);
                dt2 = new DataTable();
                adp2.Fill(dt2);
                dataGridView2.DataSource = dt2;
                dataGridView2.Columns[0].Visible = false; // id            
                dataGridView2.Columns[1].HeaderText = "Tarih";
                dataGridView2.Columns[2].HeaderText = "Ders";
                dataGridView2.Columns[3].HeaderText = "Soru Sayısı";
                dataGridView2.Columns[4].HeaderText = "Doğru Cevap";
                dataGridView2.Columns[5].HeaderText = "Yanlış Cevap";
                dataGridView2.Columns[6].Visible = false; // k_id

                cmbSurec.Enabled = true;
                button22.Enabled = true;
                button11.Enabled = true;
                button7.Enabled = true;

                cmbSurec.SelectedIndex = 3;
            }
        }

        private void richD_TextChanged(object sender, EventArgs e)
        {

        }


        

    }

}
