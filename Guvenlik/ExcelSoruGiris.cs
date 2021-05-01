using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Diagnostics;
using System.Data.SQLite;
using System.Collections;

namespace Guvenlik
{
    public partial class ExcelSoruGiris : Form
    {
        public ExcelSoruGiris()
        {
            InitializeComponent();
        }

        SQLiteCommand cmd;
        SQLiteCommand cekme;

        SQLiteConnection baglanti;
        fonk fnk = new fonk();
        Process process;
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                process = Process.Start(Application.StartupPath.ToString()+"\\TopluSoruEkle.xls");
            }
            catch ( Exception exx)
            {
                try
                {
                    process.Kill();
                }
                catch
                {

                }
                MessageBox.Show("Excel Dosyası Bulunamadı. Silinmiş Olabilir. \n\n"+exx.ToString()+"", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
             
        }


        ArrayList konuidleri;
        ArrayList konu_adlari;

        private void cmbExcelDers_SelectedIndexChanged(object sender, EventArgs e)
        {
            Konu_id = -1;
            SoruID = -1;
            cmbKonu.Items.Clear();
            int cmbdersindex = cmbExcelDers.SelectedIndex;
            if (cmbdersindex == -1)
            {

            }
            else
            {
                int cmbdersSoruid = -1;
                cmbdersSoruid = Convert.ToInt32(cmbsoruid[cmbdersindex]);

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


        //OleDbConnection baglantiOL = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source="+Application.StartupPath.ToString()+"\\TopluSoruEkle.xlsx; Extended Properties=Excel 8.0");
        
        private void ExcelSoruGiris_Load(object sender, EventArgs e)
        {
            baglanti = fnk.bag();
            dataGridView2.AllowUserToAddRows = false;
            dataGridView1.AllowUserToAddRows = false;            

            try
            {
                File.Copy(Application.StartupPath.ToString()+"\\TopluSoruEkleOrjinal.xls", Application.StartupPath.ToString()+"\\TopluSoruEkle.xls", true);
            }
            catch (Exception exx)
            {
                MessageBox.Show("Orjinal excel kopyalanırken hata oluştu.\n\n"+ exx.ToString(), "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            //Hatırlatma.
            /*
            veriKontrol();
            if (guncelleneBilirmi == false)
            {
                // hata mesajı verildi.
            }
            else
            {
                try
                {
                    OleDbDataAdapter adp = new OleDbDataAdapter("SELECT * FROM [Sorular$]", "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=TopluSoruEkle.xls; Extended Properties=Excel 8.0");
                    DataSet ds = new DataSet();
                    adp.Fill(ds, "ExcelSorulari");
                    dataGridView1.DataSource = ds.Tables["ExcelSorulari"].DefaultView;
                    dataGridView1.Columns[0].HeaderText = "Soru";
                    dataGridView1.Columns[1].HeaderText = "A Şıkkı";
                    dataGridView1.Columns[2].HeaderText = "B Şıkkı";
                    dataGridView1.Columns[3].HeaderText = "C Şıkkı";
                    dataGridView1.Columns[4].HeaderText = "D Şıkkı";
                    dataGridView1.Columns[5].HeaderText = "Resim Yol";
                    dataGridView1.Columns[6].HeaderText = "Cevap";
                }
                catch (Exception exx)
                {
                    MessageBox.Show("Excel Dosyası Bulunamadı. Silinmiş Olabilir. \n\n" + exx.ToString() + "", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }*/


        }

        int dersGuncellemeVarmi = 0;

        private void timer1_Tick(object sender, EventArgs e)
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
                    cmbSeviye.Text = "";
                    cmbExcelDers.Text = "";
                    cmbKonu.Text = "";
                }
            }
            catch
            {
                if (baglanti.State == ConnectionState.Open)
                {
                    baglanti.Close();
                }
            }
        }

        bool guncelleneBilirmi = false;

        void veriKontrol()
        {
            try
            {
                // Görünmez Datagridview ile kontrolerimizi yapalım duruma göre soruları kayıt edip edemiceğimize bakıcaz.

                OleDbDataAdapter adp = new OleDbDataAdapter("SELECT * FROM [Sorular$]", "Provider=Microsoft.Jet.OLEDB.4.0; Data Source="+Application.StartupPath.ToString()+"\\TopluSoruEkle.xls; Extended Properties=Excel 8.0");
                DataSet ds = new DataSet();
                adp.Fill(ds, "ExcelSorulari");
                dataGridView2.DataSource = ds.Tables["ExcelSorulari"].DefaultView;

                int dgv2ColumnsCount = dataGridView2.Columns.Count;
                if (dgv2ColumnsCount == 7)
                {
                    // eğer 7 adet girilmişse demektirki herşey doğru. fazlası istenmiyor azı istenmiyor girilen texti biz başlıklarını düzeltip atıcaz eğer önceden atıyorsak bu sorun şu şekilde oluyor 6 indexsı bulamadığı için adını değiştiremiyor hata alıyor. bu yüzden 7 tane oldugunden emin olduktan sonra değiştirme işlemini yapıyoruz.
                    dataGridView2.Columns[0].HeaderText = "Soru";
                    dataGridView2.Columns[1].HeaderText = "A Şıkkı";
                    dataGridView2.Columns[2].HeaderText = "B Şıkkı";
                    dataGridView2.Columns[3].HeaderText = "C Şıkkı";
                    dataGridView2.Columns[4].HeaderText = "D Şıkkı";
                    dataGridView2.Columns[5].HeaderText = "Resim Yol";
                    dataGridView2.Columns[6].HeaderText = "Cevap";

                    guncelleneBilirmi = false; // sıfırla
                    // kontrol bitirildikten sonra bir datatable aktarılcak bu datatabledeki verilerde datagridview1 aktarılcak.

                    foreach (DataGridViewRow r in dataGridView2.Rows) // satır satır kontrol edelim.
                    {
                        if (r.Cells[0].Value == null || r.Cells[1].Value == null || r.Cells[2].Value == null || r.Cells[3].Value == null || r.Cells[4].Value == null || r.Cells[6].Value == null)
                        {
                            guncelleneBilirmi = false;
                            break;
                        }
                        else
                        {
                            // Cevap tek karaktermi ve a,b,c,d şıklarına eşitmi kontrol edelim.
                            try
                            {
                                char cevapControl = Convert.ToChar(r.Cells[6].Value);
                                if (cevapControl == 'A' || cevapControl == 'a' || cevapControl == 'B' || cevapControl == 'b' || cevapControl == 'C' || cevapControl == 'c' || cevapControl == 'D' || cevapControl == 'd')
                                {
                                    // doğru sonuç
                                    guncelleneBilirmi = true;
                                }
                                else
                                {
                                    MessageBox.Show("Excel Cevap olarak verilen şık belirtilen a,b,c,d eşit değil veya metinsel bir ifade kullanılmış.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                    guncelleneBilirmi = false;
                                    break;
                                }


                            }
                            catch
                            {
                                MessageBox.Show("Excel Cevap olarak verilen şık belirtilen a,b,c,d eşit değil veya metinsel bir ifade kullanılmış.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                guncelleneBilirmi = false;
                                break;
                            }
                            // Resim Yolu Doğrumu kontrol edelim. 
                            try
                            {
                                string yol = r.Cells[5].Value.ToString();
                                if (yol != "")
                                {
                                    pictureBox1.Image = Image.FromFile(@"" + yol + "");
                                    guncelleneBilirmi = true;
                                }
                            }
                            catch
                            {
                                // resim yolu bulunamadı hata oluştu.
                                MessageBox.Show("" + r.Cells[5].Value.ToString() + " Belirtiniz Resim yolu bulunamadı.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                                guncelleneBilirmi = false;
                                break;
                            }

                        }

                    }

                    
                }
                else
                {
                    MessageBox.Show("Soru, A,B,C,D Şıkkı Ve Resim Yolu, Cevap Adlarından farklı olarak bir başlık daha girilmiş veya silinmiş.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    guncelleneBilirmi = false;
                }

            }
            catch (Exception exx)
            {
                MessageBox.Show("Excel Dosyası Bulunamadı. Silinmiş Olabilir. \n\n" + exx.ToString() + "", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                guncelleneBilirmi = false;
            }


            // Kontroler bitti
            if (guncelleneBilirmi == false)
            {
                MessageBox.Show("Belirtilen hataları düzeltin ve tekrar deneyin güncelleme yapılmayacaktır.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                // başarılı hata yok.
                guncelleneBilirmi = true;
            }
        
        }

        private void button2_Click(object sender, EventArgs e)
        {
            veriKontrol();
            if (guncelleneBilirmi == false)
            {
                // mesajı verildi.
            }
            else
            {
                try
                {
                    OleDbDataAdapter adp = new OleDbDataAdapter("SELECT * FROM [Sorular$]", "Provider=Microsoft.Jet.OLEDB.4.0; Data Source="+Application.StartupPath.ToString()+"\\TopluSoruEkle.xls; Extended Properties=Excel 8.0");
                    DataSet ds = new DataSet();
                    adp.Fill(ds, "ExcelSorulari");
                    dataGridView1.DataSource = ds.Tables["ExcelSorulari"].DefaultView;

                    dataGridView1.Columns[0].HeaderText = "Soru";
                    dataGridView1.Columns[1].HeaderText = "A Şıkkı";
                    dataGridView1.Columns[2].HeaderText = "B Şıkkı";
                    dataGridView1.Columns[3].HeaderText = "C Şıkkı";
                    dataGridView1.Columns[4].HeaderText = "D Şıkkı";
                    dataGridView1.Columns[5].HeaderText = "Resim Yol";
                    dataGridView1.Columns[6].HeaderText = "Cevap";

                    MessageBox.Show("Güncellendi.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                }
                catch (Exception exx)
                {
                    MessageBox.Show("Excel Dosyası Bulunamadı. Silinmiş Olabilir. \n\n" + exx.ToString() + "", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }
            }
            
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (cmbExcelDers.Text == "") MessageBox.Show("Ders Seçilmemiş.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            else if (cmbSeviye.Text == "")MessageBox.Show("Seviye Seçilmemiş.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);         
            else if (cmbKonu.Text == "") MessageBox.Show("Konu Seçilmemiş.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            else
            {
                veriKontrol();
                if (guncelleneBilirmi == false)
                {
                    // Mesaj verildi.
                }
                else
                {

                    bool kayitDurum = false;

                    foreach (DataGridViewRow r in dataGridView1.Rows)
                    {
                        richSoru.Text = r.Cells[0].Value.ToString();
                        richAsk.Text = r.Cells[1].Value.ToString();
                        richBsk.Text = r.Cells[2].Value.ToString();
                        richCsk.Text = r.Cells[3].Value.ToString();
                        richDsk.Text = r.Cells[4].Value.ToString();

                        char cvp = Convert.ToChar(r.Cells[6].Value);
                        switch(cvp)
                        {
                            case 'a':
                                cvp = 'A';
                                break;
                            case 'b':
                                cvp = 'B';
                                break;
                            case 'c':
                                cvp = 'C';
                                break;
                            case 'd':
                                cvp = 'D';
                                break;
                        }

                        // resim kopyala.
                        string copy = "";
                        try
                        {
                            string resYol = r.Cells[5].Value.ToString();
                            
                            if (resYol != "")
                            {
                                string urlfix = resYol.Replace(@"/", @"\");

                                int enson = urlfix.LastIndexOf(@"\");

                                int hsp = urlfix.Length - enson; // karakter sayısıyla en son slash geçtiği yer çıkartılacak. bu sayede kaç harfli karakterli olduğunu bulucaz ve substring işlemini yapıcaz.

                                string url = urlfix.Substring(enson, hsp);

                                copy = @"image" + url; // gerçek url. veri tabanına kayıt olucak string.
                                File.Copy(urlfix, Application.StartupPath.ToString()+"\\"+copy, true);
                            }
                           
                        }
                        catch (Exception exx)
                        {
                            copy = ""; // eğer hata varsa copy sıfırla. kayıt yaparken boş yapsın.
                            MessageBox.Show("Resim Kopyalama işlemi yaparken hata oluştu.\n\n"+exx.ToString() , "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        }

                        try
                        {
                            baglanti.Open();
                            cmd = new SQLiteCommand("INSERT INTO GvnSorular (Soru,Cevap,Ask,Bsk,Csk,Dsk,ResimYol,SoruID,SoruTipi,Konu_id,Konu_Adi,Seviye) VALUES ('" + richSoru.Rtf.Replace("'", "''") + "', '" + cvp + "', '" + richAsk.Rtf.Replace("'", "''") + "', '" + richBsk.Rtf.Replace("'", "''") + "', '" + richCsk.Rtf.Replace("'", "''") + "', '" + richDsk.Rtf.Replace("'", "''") + "', '" + copy.Replace("'", "''") + "', '" + SoruID + "','" + cmbExcelDers.Text + "', '" + Konu_id + "', '" + cmbKonu.Text + "' ,'" + cmbSeviye.Text + "')", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            baglanti.Open();
                            cmd = new SQLiteCommand("UPDATE GvnGenel SET dersGuncelleme=1", baglanti); // Dersleri güncelleyip tekrar dersi seçip güncelini görmesi.
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();
                            kayitDurum = true;
                        }
                        catch (Exception exx)
                        {
                            kayitDurum = false;
                            if (baglanti.State == ConnectionState.Open)
                            {
                                baglanti.Close();
                            }
                            MessageBox.Show("Kayıt Yaparken Hata oluştu.\n\n" + exx.ToString(), "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        }
                    }


                    richSoru.Text = "";
                    richAsk.Text = "";
                    richBsk.Text = "";
                    richCsk.Text = "";
                    richDsk.Text = "";
                    if (kayitDurum == true)
                    {
                        MessageBox.Show("Toplu Kayıt Yapıldı.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        try
                        {
                            File.Copy(""+Application.StartupPath.ToString()+"\\TopluSoruEkleOrjinal.xls", ""+Application.StartupPath.ToString()+"\\TopluSoruEkle.xls", true);
                            dataGridView1.DataSource = null;
                        }
                        catch (Exception exx)
                        {
                            dataGridView1.DataSource = null;
                            MessageBox.Show("Orjinal excel kopyalanırken hata oluştu.\n\n" + exx.ToString(), "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Toplu kayıt yapılırken hata oluştu. Eksik Kayıt olmuş olabilir Lütfen kontrol ediniz.", "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                        try
                        {
                            File.Copy("" + Application.StartupPath.ToString() + "\\TopluSoruEkleOrjinal.xls", "" + Application.StartupPath.ToString() + "\\TopluSoruEkle.xls", true);
                            dataGridView1.DataSource = null;
                        }
                        catch (Exception exx)
                        {
                            MessageBox.Show("Orjinal excel kopyalanırken hata oluştu.\n\n" + exx.ToString(), "Excel Toplu Soru Giriş", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                            dataGridView1.DataSource = null;
                        }
                    }
                    
                }
            }
        }

        int SoruID = -1;
        int Konu_id = -1;

        ArrayList cmbsoruid = new ArrayList();

        void cagir(string ders)
        {
            cmbExcelDers.Items.Clear();
            cmbKonu.Items.Clear();
            cmbsoruid.Clear();
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT id,SoruTipi FROM GvnDersler WHERE Seviye='" + ders + "'", baglanti);
            SQLiteDataReader drcek = cekme.ExecuteReader();
            while (drcek.Read())
            {
                cmbExcelDers.Items.Add(drcek["SoruTipi"].ToString());
                cmbsoruid.Add(drcek["id"]);
            }
            cekme.Dispose();
            drcek.Close();
            baglanti.Close();
        }

        private void cmbSeviye_SelectedIndexChanged(object sender, EventArgs e)
        {
            Konu_id = -1;
            SoruID = -1;
            

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
                    MessageBox.Show("Konu bulunamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    // konu var yazdırıldı.
                }
            }
        }
    }
}
