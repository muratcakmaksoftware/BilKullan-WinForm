using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Diagnostics;
using System.Data.SQLite;
using System.Collections;
using System.Globalization;
using System.Net.Mail;
using System.Net;
using System.IO;
using IWshRuntimeLibrary;
using System.Threading;

namespace Guvenlik
{
    public partial class Form1 : Form
    {

        


        //  Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key Engelleme.

        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardDLLStruct
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        SQLiteCommand cekme;
        SQLiteCommand cmd;


        public Form1()
        {
            InitializeComponent();
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
        }

        public int engellensinMi = 1;
        public int engellensinMi2 = 1;

        SQLiteConnection baglanti;

        fonk fnk = new fonk();
        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (engellensinMi == 1 && engellensinMi2 == 1) // Yönetici Paneline geçiş kontrolü.
            {
                if (AnaGuvenlik == 1) // Yönetici Panelinden yapılan değişikler sonucunda ana güvenlik 1 ise engellenecektir.
                {
                    if (nCode >= 0)
                    {
                        KeyboardDLLStruct objKeyInfo = (KeyboardDLLStruct)Marshal.PtrToStructure(lp, typeof(KeyboardDLLStruct));

                        if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.Alt || objKeyInfo.key == Keys.Tab || objKeyInfo.key == Keys.Control || objKeyInfo.key == Keys.ControlKey || objKeyInfo.key == Keys.RControlKey || objKeyInfo.key == Keys.LControlKey || objKeyInfo.key == Keys.Delete || objKeyInfo.key == Keys.LMenu || objKeyInfo.key == Keys.Escape)
                        {
                            return (IntPtr)1;
                        }
                    }
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }


        // ALT + F4 Engelleme.
        protected override System.Boolean ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F2)
            {
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET ScreenEngel=0", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                this.TopMost = false;

                YoneticiPaneli yn = new YoneticiPaneli();
                yn.ShowDialog();
                return true;
            }
            else if (keyData == Keys.F1)
            {
                baglanti.Open();
                cmd = new SQLiteCommand("UPDATE GvnGenel SET ScreenEngel=0", baglanti);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                baglanti.Close();
                this.TopMost = false;

                Yardim y1 = new Yardim();
                y1.ShowDialog();
                return true;
            }

            if (engellensinMi == 1 && engellensinMi2 == 1)
            {
                if (AnaGuvenlik == 1)
                {
                    if ((msg.Msg == 0x104) || (((int)msg.LParam) == 0x203e0001))
                    {
                        return true;
                    }
                }
            }

            if (tmrZamanAsimi.Enabled == true)
            {
                // Zaman aşımı sıfırla
                zamanAsimiSifirla();

            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public int ScreenX;
        public int ScreenY;

        public string tarih = "";

        ArrayList dersler = new ArrayList();
        ArrayList derslerid = new ArrayList();


        private void Form1_Load(object sender, EventArgs e)
        {
            baglanti = fnk.bag();
            // PROGRAMIN ÖNCEDEN ACILIP AÇILMADIGINI KONTROL ETME

            Mutex Mtx = new Mutex(false, "SINGLE_INSTANCE_APP_MUTEX");
            if (Mtx.WaitOne(0, false) == false)
            {

                Mtx.Close();

                Mtx = null;

                MessageBox.Show("Program Daha Önce Açılmış.", "BilKullan", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);

                System.Environment.Exit(0);

            }
            
            button2.Visible = false; // Başla buttonu kapat kullanıcı girişi için
            button1.Visible = false; // oturum kapat gizle

            try
            {
                RegistryKey rkey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                if (Convert.ToInt32(rkey.GetValue("EnableLUA", null)) == 1) // ebeveny denetimi aktif mi devre dışımı kontrolü. // 1 = aktif 0 = pasif
                {
                    // ebeveny denetimi devre dışı bırak.
                    rkey.SetValue("EnableLUA", 0);
                    rkey.Close();
                    DialogResult buton = MessageBox.Show("Programın düzgün çalışması için bilgisayarı yeniden başlatması gerekiyor. Yeniden Başlatılsın mı ?","Yönetici Paneli", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
                    if (buton == DialogResult.Yes)
                    {
                        Process.Start("shutdown", "-r -f -t 0");
                    }
                }
                else
                {

                }
                
            }
            catch
            { }

            /*  Çalışıyor
            try
            {
                Process.Start("attrib", "+r +h +s /s /d" + @"C:\Users\" + SystemInformation.UserName + @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\BilKullan.lnk"); // kısayolu gizle
            }
            catch
            { }


            try
            {
                // Program Başlangıç Kısayol Oluşturma

                WshShell shell = new WshShell();
                IWshShortcut kisayol = (IWshShortcut)shell.CreateShortcut(@"C:\test.lnk");
                kisayol.TargetPath = Application.ExecutablePath;
                kisayol.Save();

            }
            catch (Exception exx)
            {
                MessageBox.Show(exx.ToString());
            }*/
            

            // Başlangıçta Açılması için regedit kodu.

            try
            {
                RegistryKey runKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                runKey.SetValue("BilKullan.exe", "\""+Application.ExecutablePath+"\"");
                runKey.Close();
            }
            catch
            { } // Çalışmazsa Yönetici olarak çalıştırmadığı için regedit kayıt işlenmemiş demektir.

            
            picArkaPlan.Image = Properties.Resources.arkaplan_fw;
            
            tarih = DateTime.Today.ToShortDateString();

            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET EngellensinMi=1, eklemeVarmi=0, dersGuncelleme=0, ScreenEngel=1, Dakika=0", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();
            
            panel1.Visible = false;
            panel4.Visible = false;
            lblScore.Text = "";
            ScreenX = Screen.PrimaryScreen.Bounds.Width; // Ekran Genişliği
            ScreenY = Screen.PrimaryScreen.Bounds.Height; // Ekran Yüksekliği
            
            this.MinimumSize = new Size(ScreenX, ScreenY); // Form Max Genişlikleri.
            this.WindowState = FormWindowState.Maximized;// FullScreen

            picArkaPlan.Size = new Size(ScreenX, ScreenY);

            picArkaPlan.Controls.Add(pictureBox2);
            pictureBox2.Location = new Point(ScreenX / 2 - 115 , ScreenY / 2 - 143);
            pictureBox2.BackColor = Color.Transparent;




            // RichTextBox Renk Mavi : 200, 216, 226
            richSoru.BackColor = Color.FromArgb(255, 255, 255);
            richA.BackColor = Color.FromArgb(200, 216, 226);
            richB.BackColor = Color.FromArgb(200, 216, 226);
            richC.BackColor = Color.FromArgb(200, 216, 226);
            richD.BackColor = Color.FromArgb(200, 216, 226);
            lblScore.ForeColor = Color.FromArgb(163, 195, 218);


            // MASK
            picArkaPlan.Controls.Add(panel1);
            panel1.BackColor = Color.Transparent;
            pictureBox3.Controls.Add(panel2);
            panel2.BackColor = Color.Transparent;
            pictureBox3.Controls.Add(pictureBox1);
            pictureBox1.BackColor = Color.Transparent;


            picArkaPlan.Controls.Add(panel4);
            panel4.BackColor = Color.Transparent;            
            pictureBox4.Controls.Add(panel5);
            panel5.BackColor = Color.Transparent;



            // Text Mask
            pictureBox4.Controls.Add(panel6);
            panel6.BackColor = Color.Transparent;
            pictureBox4.Controls.Add(panel61);
            panel61.BackColor = Color.Transparent;
            pictureBox4.Controls.Add(panel7);
            panel7.BackColor = Color.Transparent;
            pictureBox4.Controls.Add(Panel8);
            Panel8.BackColor = Color.Transparent;
            pictureBox4.Controls.Add(lblBasariOrani);
            lblBasariOrani.BackColor = Color.Transparent;
            
            // Login Mask
            
            picArkaPlan.Controls.Add(lblKadi);
            lblKadi.BackColor = Color.Transparent;
            picArkaPlan.Controls.Add(lblSifre);
            lblSifre.BackColor = Color.Transparent;
           
           
            
            //Buttonlar
            pictureBox3.Controls.Add(panel9);
            panel9.BackColor = Color.Transparent;
            pictureBox3.Controls.Add(panel10);
            panel10.BackColor = Color.Transparent;
            pictureBox3.Controls.Add(panel11);
            panel11.BackColor = Color.Transparent;
            pictureBox3.Controls.Add(panel12);
            panel12.BackColor = Color.Transparent;

            button2.Location = new Point(pictureBox2.Location.X, pictureBox2.Location.Y + 286 + 6); // başlat butonu


            panel1.Location = new Point((ScreenX / 2) - panel1.Size.Width / 2 + 20, (ScreenY / 2) - panel1.Size.Height / 2);
            // Soru Bitiş bölmü
            panel4.Location = new Point((ScreenX / 2) - panel4.Size.Width / 2 + 20, (ScreenY / 2) - panel4.Size.Height / 2);


            // Kullanıcı Login Panel
            lblKadi.Location = new Point(pictureBox2.Location.X -40,pictureBox2.Location.Y +  286);
            lblSifre.Location = new Point(lblKadi.Location.X, lblKadi.Location.Y + 33);
            txtKadi.Location = new Point(lblKadi.Location.X + 85, lblKadi.Location.Y);
            txtSifre.Location = new Point(txtKadi.Location.X, txtKadi.Location.Y + 30);
            button4.Location = new Point(txtSifre.Location.X + 33, txtSifre.Location.Y + 40);
            
            // oturum kapat
            button1.Location = new Point(ScreenX - 122 - 20, 10);

            // login 
            //panelKullaniciGiris.Location = new Point(pictureBox2.Location.X - 277, pictureBox2.Location.Y + pictureBox2.Height + 10);
            try
            {
                

                // Ekle. Bilgisayarı Kilitle

                if (AnaGuvenlik == 1)
                {
                    RegistryKey rkey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies", true);
                    rkey.CreateSubKey("System", RegistryKeyPermissionCheck.Default);
                    rkey.Close();

                    // Görev Yöneticisi Devre Dışı bırak

                    RegistryKey rkey2 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey2.SetValue("DisableTaskMgr", 0);
                    rkey2.Close();

                    // Bilgisayarı kilitlemeyi devre dışı bırak.

                    RegistryKey rkey3 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey3.SetValue("DisableLockWorkstation", 1);
                    rkey3.Close();

                    // Parolayı değiştiri devre dışı bırak.

                    RegistryKey rkey4 = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey4.SetValue("DisableChangePassword", 1);
                    rkey4.Close();

                    // Kullanıcı değiştiri devre dışı bırak

                    RegistryKey rkey5 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\System", true);
                    rkey5.SetValue("HideFastUserSwitching", 1);
                    rkey5.Close();

                    // Oturumu kapatmayı devre dışı bırak.

                    RegistryKey rkey6 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                    rkey6.SetValue("NoLogoff", 1);
                    rkey6.Close();
                }
            }
            catch
            {
            }
          
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            this.Show();
            
            // Engelle.
            engellensinMi = 1;

            

            try
            {
                // Ekle. Bilgisayarı Kilitle
                if (AnaGuvenlik == 1)
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

                    RegistryKey rkey6 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                    rkey6.SetValue("NoLogoff", 1);
                    rkey6.Close();
                }
            }
            catch
            {
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        

        private void button3_Click(object sender, EventArgs e)
        {
            // Zaman aşımı sıfırla
            zamanAsimiSifirla();
            
            button2.Visible = true; // başla butonu
            pictureBox2.Visible = true; // baykuş logomuz 
            picArkaPlan.Image = Properties.Resources.arkaplan_fw; // ana arka plan resmi
            panel1.Visible = false; // soru gösterme
            panel4.Visible = false; // soru bitiş gösterme
            KazanilanDk = 0;
            timergiris = 0;
            button2.Enabled = true;

            //SoruSayisi = 0;
            //SoruSayiBelirleme = 0;
            ilkSoru = false;
            dogruCevap = 0;
            YanlisCevap = 0;
            soruBas = 0;
            soruBit = 0;
            CikanSorular.Clear();
            lblScore.Text = "";
            pictureBox1.Image = null;

            // sıfırlama işlemleri.

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        bool baglantiVarMi = true;

        int ScreenEngel = 1;

        int AnaGuvenlik = -1;

        int guvenlikControl = 0;

        int veritabaniGuncelleme = -1;

        int saniye = 0;

        private void timer2_Tick(object sender, EventArgs e)
        {
            // Ana Güvenlik Kontrolü
            try
            {
                if (saniye == 0)
                {
                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT VeriTabaniGuncelleme FROM GvnGenel WHERE id=1", baglanti);
                    SQLiteDataReader drgunc = cekme.ExecuteReader();
                    while (drgunc.Read())
                    {
                        veritabaniGuncelleme = Convert.ToInt32(drgunc["VeriTabaniGuncelleme"]);
                        break;
                    }
                    cekme.Dispose();
                    drgunc.Close();
                    baglanti.Close();
                }

                if (veritabaniGuncelleme == 1)
                {
                    saniye++;
                    if (saniye == 4) // 4 saniye sonra işlev devam edecek.
                    {
                        veritabaniGuncelleme = 0;
                        saniye = 0;
                    }
                }
                else
                {
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


                    if (AnaGuvenlik == 1)
                    {
                        try
                        {

                            // Engelle
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

                            RegistryKey rkey6 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                            rkey6.SetValue("NoLogoff", 1);
                            rkey6.Close();
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        try
                        {
                            // ENGELİ KALDIR

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

                            RegistryKey rkey6 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", true);
                            rkey6.SetValue("NoLogoff", 0);
                            rkey6.Close();
                        }
                        catch
                        {
                        }
                    }



                    string eng = "";

                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT EngellensinMi, ScreenEngel FROM GvnGenel WHERE id=1", baglanti);
                    SQLiteDataReader drx = cekme.ExecuteReader();
                    while (drx.Read())
                    {
                        eng = (drx["EngellensinMi"].ToString());
                        ScreenEngel = Convert.ToInt32(drx["ScreenEngel"]);
                    }
                    cekme.Dispose();
                    drx.Close();
                    baglanti.Close();


                    engellensinMi2 = Convert.ToInt32(eng);

                    if (ScreenEngel == 1)
                    {
                        if (AnaGuvenlik == 1)
                        {
                            this.TopMost = true;
                            guvenlikControl = 0;
                        }
                        else
                        {
                            if (guvenlikControl == 0)
                            {
                                //for (int i = 0; i < 5; i++) { this.TopMost = false; } // bug varsa önle
                                this.TopMost = false;
                                guvenlikControl++;
                            }
                        }
                    }
                    else
                    {
                        //this.TopMost = false;
                    }


                    int eklemeVarmi = 0;

                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT eklemeVarmi FROM GvnGenel WHERE id=1", baglanti);
                    SQLiteDataReader drc = cekme.ExecuteReader();
                    while (drc.Read())
                    {
                        eklemeVarmi = Convert.ToInt32(drc["eklemeVarmi"]);
                    }
                    cekme.Dispose();
                    drc.Close();
                    baglanti.Close();

                    if (eklemeVarmi == 1)
                    {
                        // çıkış işlemini yap güncelleme yapılmış.
                        button3.PerformClick();

                        baglanti.Open();
                        cmd = new SQLiteCommand("UPDATE GvnGenel SET eklemeVarmi=0", baglanti);
                        cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        baglanti.Close();
                    }
                }

            }
            catch
            {
                // database güncellemesi.
                if (baglanti.State == ConnectionState.Open)
                {
                    baglanti.Close();
                }
            }
            //Tarihi Güncelle.
            tarih = DateTime.Today.ToShortDateString();

            

            gun = CultureInfo.GetCultureInfo("tr-TR").DateTimeFormat.DayNames[(int)DateTime.Now.DayOfWeek];


        }
        // Normal
        ArrayList veritabaniDuzenle = new ArrayList();
        
        string SoruTipi = "";

        ArrayList rndbaslst = new ArrayList();

        int rsgCikanid = 0; // rasgele çıkan id.
        
        // Karışık
        ArrayList soruidleri = new ArrayList();

        // Seçilen Seçenek 
        string secenek = "";

        // Gün Pazartesi, Sali ... 
        string gun = "";

        // otomatik sırası

        int otosira = -1;
        // gun olarak otosirabelirlemek için.
        string otogun = "";

        // Soru Seviyesi
        string soruSeviyesi = "";


        void zamanAsimiSifirla()
        {
            // Zaman aşımı sıfırla
           
            tmrZamanAsimi.Stop();
            tmrZamanAsimi.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {            
            // Zaman aşımı sıfırla
            zamanAsimiSifirla();
            
            // Ders Seçme ileri.

            // ders soru id ve dersin adını al.
            secenek = "";
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT TestSoruTuru, Seviye FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader drdrs = cekme.ExecuteReader();
            while (drdrs.Read())
            {
                secenek = drdrs["TestSoruTuru"].ToString();
                soruSeviyesi = drdrs["Seviye"].ToString();
                break;
            }
            cekme.Dispose();
            drdrs.Close();
            baglanti.Close();

            if (KSeviye == "")
            {
                MessageBox.Show("Kullanıcı Seviye Belirli Değil.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (secenek == "Otomatik")
                {
                    // otomatik

                    string programlamaAktifMi = "";

                    baglanti.Open();
                    cekme = new SQLiteCommand("SELECT HaftayaProgramlamaPA,HPOrtaOkul,HPLise FROM GvnGenel WHERE id=1", baglanti);
                    SQLiteDataReader drprogram = cekme.ExecuteReader();
                    while (drprogram.Read())
                    {
                        switch(KSeviye)
                        {
                            case "İlk Okul":
                                programlamaAktifMi = drprogram["HaftayaProgramlamaPA"].ToString();
                                break;
                            case "Orta Okul":
                                programlamaAktifMi = drprogram["HPOrtaOkul"].ToString();
                                break;
                            case "Lise":
                                programlamaAktifMi = drprogram["HPLise"].ToString();
                                break;

                        }
                        
                    }
                    cekme.Dispose();
                    drprogram.Close();
                    baglanti.Close();
                    
                    

                    if (programlamaAktifMi == "Aktif")
                    {
                        button2.Enabled = false;
                        if (gun == "Pazartesi")
                        {
                            otosira = 1;
                            otogun = "Pzt";
                            rndbaslst.Clear(); // sıfırla.
                            // Analiz için soru tipini aktaralım.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OtoPzt FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader drders = cekme.ExecuteReader();
                            while (drders.Read())
                            {
                                SoruTipi = drders["OtoPzt"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            drders.Close();
                            baglanti.Close();

                            // Soruları aktarıyoruz.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Pzt=" + otosira + " AND Seviye='" + KSeviye + "'", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                rndbaslst.Add(drveri["id"]);
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            soruBit = rndbaslst.Count; // max soruyu sorubit ayarlıyoruz.

                            soruBas++;
                            lblScore.Text = "" + soruBas + " / " + soruBit + "";
                            //panel2.Visible = false;
                            button2.Visible = false;
                            panel1.Visible = true;

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount); // dizi arasındaki rasgele bir sayı çıkar

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);  // çıkan rasgele sayısının dizinin içindeki id sini al.
                            CikanSorular.Add(rsgCikanid); // çıkan sorulara ekle

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                            SQLiteDataReader drd = cekme.ExecuteReader();
                            while (drd.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drd["Soru"].ToString();
                                richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                                xCevaps = drd["Cevap"].ToString();
                                try
                                {
                                    if (drd["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            cekme.Dispose();
                            drd.Close();
                            baglanti.Close();

                            pictureBox2.Visible = false;
                            picArkaPlan.Image = Properties.Resources.SoruArkaPlan;

                        }
                        else if (gun == "Salı")
                        {
                            otosira = 2;
                            otogun = "Sali";
                            rndbaslst.Clear(); // sıfırla.
                            // Analiz için soru tipini aktaralım.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OtoSali FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader drders = cekme.ExecuteReader();
                            while (drders.Read())
                            {
                                SoruTipi = drders["OtoSali"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            drders.Close();
                            baglanti.Close();

                            // Soruları aktarıyoruz.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Sali=" + otosira + " AND Seviye='" + KSeviye + "'", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                rndbaslst.Add(drveri["id"]);
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            soruBit = rndbaslst.Count; // max soruyu sorubit ayarlıyoruz.

                            soruBas++;
                            lblScore.Text = "" + soruBas + " / " + soruBit + "";
                            //panel2.Visible = false;
                            button2.Visible = false;
                            panel1.Visible = true;

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount); // dizi arasındaki rasgele bir sayı çıkar

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);  // çıkan rasgele sayısının dizinin içindeki id sini al.
                            CikanSorular.Add(rsgCikanid); // çıkan sorulara ekle

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                            SQLiteDataReader drd = cekme.ExecuteReader();
                            while (drd.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drd["Soru"].ToString();
                                richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                                xCevaps = drd["Cevap"].ToString();
                                try
                                {
                                    if (drd["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            cekme.Dispose();
                            drd.Close();
                            baglanti.Close();
                            pictureBox2.Visible = false;
                            picArkaPlan.Image = Properties.Resources.SoruArkaPlan;
                        }
                        else if (gun == "Çarşamba")
                        {
                            otosira = 3;
                            otogun = "Carsamba";
                            rndbaslst.Clear(); // sıfırla.
                            // Analiz için soru tipini aktaralım.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OtoCarsamba FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader drders = cekme.ExecuteReader();
                            while (drders.Read())
                            {
                                SoruTipi = drders["OtoCarsamba"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            drders.Close();
                            baglanti.Close();

                            // Soruları aktarıyoruz.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Carsamba=" + otosira + " AND Seviye='" + KSeviye + "'", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                rndbaslst.Add(drveri["id"]);
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            soruBit = rndbaslst.Count; // max soruyu sorubit ayarlıyoruz.

                            soruBas++;
                            lblScore.Text = "" + soruBas + " / " + soruBit + "";
                            //panel2.Visible = false;
                            button2.Visible = false;
                            panel1.Visible = true;

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount); // dizi arasındaki rasgele bir sayı çıkar

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);  // çıkan rasgele sayısının dizinin içindeki id sini al.
                            CikanSorular.Add(rsgCikanid); // çıkan sorulara ekle

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                            SQLiteDataReader drd = cekme.ExecuteReader();
                            while (drd.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drd["Soru"].ToString();
                                richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                                xCevaps = drd["Cevap"].ToString();
                                try
                                {
                                    if (drd["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            cekme.Dispose();
                            drd.Close();
                            baglanti.Close();

                            pictureBox2.Visible = false;
                            picArkaPlan.Image = Properties.Resources.SoruArkaPlan;
                        }
                        else if (gun == "Perşembe")
                        {
                            otosira = 4;
                            otogun = "Persembe";
                            rndbaslst.Clear(); // sıfırla.
                            // Analiz için soru tipini aktaralım.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OtoPersembe FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader drders = cekme.ExecuteReader();
                            while (drders.Read())
                            {
                                SoruTipi = drders["OtoPersembe"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            drders.Close();
                            baglanti.Close();

                            // Soruları aktarıyoruz.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Persembe=" + otosira + " AND Seviye='" + KSeviye + "'", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                rndbaslst.Add(drveri["id"]);
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            soruBit = rndbaslst.Count; // max soruyu sorubit ayarlıyoruz.

                            soruBas++;
                            lblScore.Text = "" + soruBas + " / " + soruBit + "";
                            //panel2.Visible = false;
                            button2.Visible = false;
                            panel1.Visible = true;

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount); // dizi arasındaki rasgele bir sayı çıkar

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);  // çıkan rasgele sayısının dizinin içindeki id sini al.
                            CikanSorular.Add(rsgCikanid); // çıkan sorulara ekle

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                            SQLiteDataReader drd = cekme.ExecuteReader();
                            while (drd.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drd["Soru"].ToString();
                                richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                                xCevaps = drd["Cevap"].ToString();
                                try
                                {
                                    if (drd["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            cekme.Dispose();
                            drd.Close();
                            baglanti.Close();

                            pictureBox2.Visible = false;
                            picArkaPlan.Image = Properties.Resources.SoruArkaPlan;
                        }
                        else if (gun == "Cuma")
                        {
                            otosira = 5;
                            otogun = "Cuma";
                            rndbaslst.Clear(); // sıfırla.
                            // Analiz için soru tipini aktaralım.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OtoCuma FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader drders = cekme.ExecuteReader();
                            while (drders.Read())
                            {
                                SoruTipi = drders["OtoCuma"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            drders.Close();
                            baglanti.Close();

                            // Soruları aktarıyoruz.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Cuma=" + otosira + " AND Seviye='" + KSeviye + "'", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                rndbaslst.Add(drveri["id"]);
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            soruBit = rndbaslst.Count; // max soruyu sorubit ayarlıyoruz.

                            soruBas++;
                            lblScore.Text = "" + soruBas + " / " + soruBit + "";
                            //panel2.Visible = false;
                            button2.Visible = false;
                            panel1.Visible = true;

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount); // dizi arasındaki rasgele bir sayı çıkar

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);  // çıkan rasgele sayısının dizinin içindeki id sini al.
                            CikanSorular.Add(rsgCikanid); // çıkan sorulara ekle

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                            SQLiteDataReader drd = cekme.ExecuteReader();
                            while (drd.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drd["Soru"].ToString();
                                richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                                xCevaps = drd["Cevap"].ToString();
                                try
                                {
                                    if (drd["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            cekme.Dispose();
                            drd.Close();
                            baglanti.Close();

                            pictureBox2.Visible = false;
                            picArkaPlan.Image = Properties.Resources.SoruArkaPlan;
                        }
                        else if (gun == "Cumartesi")
                        {
                            otosira = 6;
                            otogun = "Cumartesi";
                            rndbaslst.Clear(); // sıfırla.
                            // Analiz için soru tipini aktaralım.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OtoCumartesi FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader drders = cekme.ExecuteReader();
                            while (drders.Read())
                            {
                                SoruTipi = drders["OtoCumartesi"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            drders.Close();
                            baglanti.Close();

                            // Soruları aktarıyoruz.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Cumartesi=" + otosira + " AND Seviye='" + KSeviye + "'", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                rndbaslst.Add(drveri["id"]);
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            soruBit = rndbaslst.Count; // max soruyu sorubit ayarlıyoruz.

                            soruBas++;
                            lblScore.Text = "" + soruBas + " / " + soruBit + "";
                            //panel2.Visible = false;
                            button2.Visible = false;
                            panel1.Visible = true;

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount); // dizi arasındaki rasgele bir sayı çıkar

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);  // çıkan rasgele sayısının dizinin içindeki id sini al.
                            CikanSorular.Add(rsgCikanid); // çıkan sorulara ekle

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                            SQLiteDataReader drd = cekme.ExecuteReader();
                            while (drd.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drd["Soru"].ToString();
                                richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                                xCevaps = drd["Cevap"].ToString();
                                try
                                {
                                    if (drd["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            cekme.Dispose();
                            drd.Close();
                            baglanti.Close();

                            pictureBox2.Visible = false;
                            picArkaPlan.Image = Properties.Resources.SoruArkaPlan;
                        }
                        else if (gun == "Pazar")
                        {
                            otosira = 7;
                            otogun = "Pazar";
                            rndbaslst.Clear(); // sıfırla.
                            // Analiz için soru tipini aktaralım.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OtoPazar FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader drders = cekme.ExecuteReader();
                            while (drders.Read())
                            {
                                SoruTipi = drders["OtoPazar"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            drders.Close();
                            baglanti.Close();

                            // Soruları aktarıyoruz.
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE Pazar=" + otosira + " AND Seviye='" + KSeviye + "'", baglanti);
                            SQLiteDataReader drveri = cekme.ExecuteReader();
                            while (drveri.Read())
                            {
                                rndbaslst.Add(drveri["id"]);
                            }
                            cekme.Dispose();
                            drveri.Close();
                            baglanti.Close();

                            soruBit = rndbaslst.Count; // max soruyu sorubit ayarlıyoruz.

                            soruBas++;
                            lblScore.Text = "" + soruBas + " / " + soruBit + "";
                            //panel2.Visible = false;
                            button2.Visible = false;
                            panel1.Visible = true;

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount); // dizi arasındaki rasgele bir sayı çıkar

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);  // çıkan rasgele sayısının dizinin içindeki id sini al.
                            CikanSorular.Add(rsgCikanid); // çıkan sorulara ekle

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                            SQLiteDataReader drd = cekme.ExecuteReader();
                            while (drd.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drd["Soru"].ToString();
                                richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                                xCevaps = drd["Cevap"].ToString();
                                try
                                {
                                    if (drd["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }

                            cekme.Dispose();
                            drd.Close();
                            baglanti.Close();
                            pictureBox2.Visible = false;
                            picArkaPlan.Image = Properties.Resources.SoruArkaPlan;
                        }
                        else
                        {
                            // hiçbirgün değil.
                        }

                    }
                    else
                    {
                        MessageBox.Show("Haftayı Programlama '"+KSeviye+"' için Aktif Değil. Lütfen Haftayı Programla Butonuyla Kontrol Ediniz.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    }

                }
            }
            
        }


        // Doğru ve yanlış cevap sayılarını global olarak tutma.
        int dogruCevap = 0;
        int YanlisCevap = 0;

        ArrayList Cevaplar = new ArrayList();

        // Çıkan soruları diziye aktarma.
        ArrayList CikanSorular = new ArrayList();


        // Soru id global ayarlama.
        public int soruID = 0;

        //int SoruSayiBelirleme = 0; // Soru sayısını belirleme.
        bool ilkSoru = false; // ilk soru girişi ayrı belirleme.


        // Random Ayarlama
        public int rndBit = 0;


        // Sorunun cevabını global ayarlama.
        public string xCevaps = "";

        // Başlangıç ve bitiş ayarlama.
        public int soruBas = 0;
        public int soruBit = 0;

        string KulSoruCevap = "";

        void emailGonder()
        {
            string email = "";
            string sifre = "";
            int emailCode = -1;
            baglanti.Open();
            cekme = new SQLiteCommand("SELECT Email, EmailSifre, EmailKodu FROM GvnGenel WHERE id=1", baglanti);
            SQLiteDataReader dremail = cekme.ExecuteReader();
            while (dremail.Read())
            {
                email = dremail["Email"].ToString();
                sifre = dremail["EmailSifre"].ToString();
                emailCode = Convert.ToInt32(dremail["EmailKodu"]);
            }

            cekme.Dispose();
            dremail.Close();
            baglanti.Close();

            if (email == "" || sifre == "" || emailCode == -1) // email yok veya şifre yok code yok.
            {

            }
            else
            {
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


                if (test == true)
                {
                    baglantiVarMi = true;
                }
                else
                {
                    baglantiVarMi = false;
                }


                if (baglantiVarMi == false) // bağlantı yok bu yüzden email yollanmayacak.
                {
                    MessageBox.Show("İnternet Bağlantınız yok. Email Yollanamadı.", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
                else
                {
                    try
                    {
                        SmtpClient sc = new SmtpClient();

                        sc.Port = 587;

                        if (emailCode == 0)
                        {
                            sc.Host = "smtp.live.com";
                            email = email + "@hotmail.com";
                        }
                        else if (emailCode == 1)
                        {
                            sc.Host = "smtp.gmail.com";
                            email = email + "@gmail.com";
                        }



                        sc.EnableSsl = true;

                        sc.Timeout = 50000;

                        MailMessage mail = new MailMessage();

                        mail.From = new MailAddress("" + email + "", "Analiz Sonuçları");

                        mail.To.Add("" + email + "");

                        mail.Subject = "Analiz Sonuçları";

                        mail.IsBodyHtml = true;

                        mail.Body = "Tarih : " + tarih + "\n\nSoru Tipi : " + SoruTipi + "\n\nDoğru Cevap : " + dogruCevap + "\n\nYanlış Cevap : " + YanlisCevap + "";

                        sc.Credentials = new NetworkCredential("" + email + "", "" + sifre + "");

                        sc.Send(mail);


                        mail.Body = ""; //reset
                        MessageBox.Show("Analiz Sonuçları Email'inize Gönderildi.", "Mail", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    }
                    catch (Exception exx)
                    {
                        MessageBox.Show("Mail Gönderirken hata oluştu lütfen mail kontrol ediniz.\n\n" + exx.ToString() + "", "Yönetici Paneli", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                    }
                }
            }
        }

        int KazanilanDk = 0;
        int timergiris = 0;
        void SoruGiris()
        {


            lblScore.Text = ""+soruBas+" / "+soruBit+"";


            if (CikanSorular.Count == 0) // liste boş ise.
            {
                MessageBox.Show("Eklenen bir soru yok.","Hata",  MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }
            else
            {
                if (ilkSoru == false)
                {


                    if (soruBit == 1)
                    {
                        if (KulSoruCevap == xCevaps)
                        {
                            dogruCevap++;
                        }
                        else
                        {
                            YanlisCevap++;
                        }


                        string cvr = "";
                        baglanti.Open();
                        cekme = new SQLiteCommand("SELECT TestdgrCevapDk FROM GvnGenel WHERE id=1", baglanti);
                        SQLiteDataReader drdk = cekme.ExecuteReader();
                        while (drdk.Read())
                        {
                            cvr = drdk["TestdgrCevapDk"].ToString();
                        }

                        cekme.Dispose();
                        drdk.Close();
                        baglanti.Close();

                        int dk = Convert.ToInt32(cvr);

                        int timer = dk * 60 * dogruCevap * 1000;

                        if (timer == 0)
                        {
                            panel1.Visible = false; // soruları kapat
                            panel4.Visible = true; // Sonuçları ekranda göster.
                            timergiris = 1;
                            buttom1.Text = "Tekrar Deneyin.";
                            KazanilanDk = 0;
                            lblDogruCevap.Text = Convert.ToString(dogruCevap);
                            lblYanlisCevap.Text = Convert.ToString(YanlisCevap);
                            lblKazanilanDk.Text = "0";
                            lblYuzde.Text = "0";

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OneriYazisi FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader droneri = cekme.ExecuteReader();
                            while (droneri.Read())
                            {
                                lblOneri.Text = droneri["OneriYazisi"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            droneri.Close();
                            baglanti.Close();

                            // ANALİZ
                            baglanti.Open();
                            cmd = new SQLiteCommand("INSERT INTO GvnAnaliz (Tarih,Ders,SoruSayisi,dCevap,yCevap,K_id) VALUES ('" + tarih + "', '" + SoruTipi + "', " + soruBit + ", " + dogruCevap + ", " + YanlisCevap + ", "+Kid+")", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            emailGonder(); // email gönder.

                            //SoruSayiBelirleme = 1;
                            ilkSoru = false;
                            dogruCevap = 0;
                            YanlisCevap = 0;
                            soruBas = 1;
                            soruBit = 0;
                            rndbaslst.Clear();
                            CikanSorular.Clear();

                            // TEKRARLANIYOR
                            // Rasgele Soru Seç

                            if (secenek == "Otomatik")
                            {
                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE "+otogun+"="+otosira+"", baglanti);
                                SQLiteDataReader drveri = cekme.ExecuteReader();
                                while (drveri.Read())
                                {
                                     rndbaslst.Add(drveri["id"]);
                                }
                                cekme.Dispose();
                                drveri.Close();
                                baglanti.Close();
                            }
                            

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount);

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);
                            CikanSorular.Add(rsgCikanid);

                            // sorubit
                            if (secenek == "Otomatik")
                            {
                                soruBit = rndbaslst.Count;
                            }
                            else
                            {
                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT TestSoruBit FROM GvnGenel WHERE id=1", baglanti); // döngüden sağlandı.
                                SQLiteDataReader drsorubit = cekme.ExecuteReader();
                                while (drsorubit.Read())
                                {
                                    soruBit = Convert.ToInt32(drsorubit["TestSoruBit"]);
                                    break;
                                }

                                cekme.Dispose();
                                drsorubit.Close();
                                baglanti.Close();
                            }

                            lblScore.Text = "" + soruBas + " / " + soruBit + "";

                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti);
                            SQLiteDataReader drsira = cekme.ExecuteReader();
                            while (drsira.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drsira["Soru"].ToString();
                                richA.Rtf = drsira["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drsira["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drsira["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drsira["Dsk"].ToString(); // D Şıkkı
                                xCevaps = (drsira["Cevap"].ToString());
                                try
                                {
                                    if (drsira["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drsira["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }
                            cekme.Dispose();
                            drsira.Close();
                            baglanti.Close();

                            engellensinMi = 1; // engelleme devam.

                        }
                        else
                        {
                            int zaman = timer / 1000 / 60;
                            timergiris = timer;
                            panel1.Visible = false; // soruları kapat
                            panel4.Visible = true; // Sonuçları ekranda göster.
                            KazanilanDk = zaman;
                            buttom1.Text = "Masaüstüne Erişmek için Tıklayın.";
                            lblDogruCevap.Text = dogruCevap.ToString();
                            lblYanlisCevap.Text = YanlisCevap.ToString();
                            lblKazanilanDk.Text = zaman.ToString();
                            double yuzde = 0;

                            if (YanlisCevap == 0)
                            {
                                yuzde = 100;
                            }
                            else if (dogruCevap == 0)
                            {
                                yuzde = 0;
                            }
                            else if (YanlisCevap == dogruCevap)
                            {
                                yuzde = 50;
                            }
                            else
                            {
                                int soruSayisi = dogruCevap + YanlisCevap; // 2 sinin toplamı soru sayısıdır.
                                yuzde = 100 / soruSayisi;
                                yuzde = yuzde * dogruCevap;
                            }

                            lblYuzde.Text = yuzde.ToString();
                            if (yuzde < 50)
                            {

                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT OneriYazisi FROM GvnGenel WHERE id=1", baglanti);
                                SQLiteDataReader droneri = cekme.ExecuteReader();
                                while (droneri.Read())
                                {
                                    lblOneri.Text = droneri["OneriYazisi"].ToString();
                                    break;
                                }
                                cekme.Dispose();
                                droneri.Close();
                                baglanti.Close();
                            }
                            else
                            {
                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT OneriYazisi2 FROM GvnGenel WHERE id=1", baglanti);
                                SQLiteDataReader droneri = cekme.ExecuteReader();
                                while (droneri.Read())
                                {
                                    lblOneri.Text = droneri["OneriYazisi2"].ToString();
                                    break;
                                }
                                cekme.Dispose();
                                droneri.Close();
                                baglanti.Close();
                            }

                            // ANALİZ
                            baglanti.Open();
                            cmd = new SQLiteCommand("INSERT INTO GvnAnaliz (Tarih,Ders,SoruSayisi,dCevap,yCevap,K_id) VALUES ('" + tarih + "', '" + SoruTipi + "', " + soruBit + ", " + dogruCevap + ", " + YanlisCevap + ", "+Kid+")", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            emailGonder(); // email gönder.

                            //SoruSayiBelirleme = 1;
                            ilkSoru = false;
                            dogruCevap = 0;
                            YanlisCevap = 0;
                            soruBas = 1;
                            soruBit = 0;
                            rndbaslst.Clear();
                            CikanSorular.Clear();

                            baglanti.Open();
                            cmd = new SQLiteCommand("UPDATE GvnGenel SET Dakika="+zaman+"", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            

                            engellensinMi = 0; // engelleri kaldır. // regedit hariç

                            
                           
                        }
                    }
                    else // sorbit == false durumu.
                    {
                        ilkSoru = true;
                        string xCevabi = "";
                        // ilk sorumuzun cevaını kontrol etmek için baştan yazdırıp xcevabını alıyoruz.
                        baglanti.Open();
                        cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti); // döngüden sağlandı.
                        SQLiteDataReader drd = cekme.ExecuteReader();
                        while (drd.Read())
                        {
                            lblSoru.Text = "" + soruBas + "";
                            xCevabi = (drd["Cevap"].ToString());
                            richSoru.Rtf = drd["Soru"].ToString();
                            richA.Rtf = drd["Ask"].ToString(); // A Şıkkı
                            richB.Rtf = drd["Bsk"].ToString(); // B Şıkkı
                            richC.Rtf = drd["Csk"].ToString(); // C Şıkkı
                            richD.Rtf = drd["Dsk"].ToString(); // D Şıkkı
                            /*try // resme gerek yok
                            {
                                if (drd["ResimYol"].ToString() == "")
                                {
                                    //pictureBox1.Image = Properties.Resources.kuş_fw;
                                }
                                else
                                {
                                    //pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drd["ResimYol"].ToString();
                                }
                            }
                            catch
                            {
                            }*/
                        }

                        cekme.Dispose();
                        drd.Close();
                        baglanti.Close();

                        // ilk sorumuzu kontrol ediyoruz. ve doğru ve yanlış cevap olarak ekleme yapmak için
                        if (KulSoruCevap == xCevaps)
                        {
                            dogruCevap++;
                        }
                        else
                        {
                            YanlisCevap++;
                        }
                        // ikinci Sorumuza geçiş.
                        

                        ////////////////////////////////
                        // 2 ci tıklamaya geçiş. ||  2 ci soruya geçiş

                        //SoruSayiBelirleme++;
                        soruBas++; // ilk soru artımı 1 / 2 oluyor tıklandıgında ise 2 / 2 olucak.
                        // 2. Sorumuzu ekrana yazdırcaz.
                        lblScore.Text = "" + soruBas + " / " + soruBit + "";

                        int rndcountx = rndbaslst.Count;

                        int x = 1;
                        while (x != 0)
                        {
                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcountx);

                            int xuzunluk = CikanSorular.Count;
                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]); // 2
                            bool durumCikan = false;
                            for (int i = 0; i < xuzunluk; i++)
                            {
                                if (Convert.ToInt32(CikanSorular[i]) == rsgCikanid) // eğer item eşit ise rasgele sayı ile bu sayı önceden atılmış demektir yani. tekrar random çekilmelidir.
                                {
                                    x = 1; // rasgeleye devam.
                                    durumCikan = true;
                                    break;
                                }
                                else // eğer böyle bir sayı önceden çekilmediyse. devam et.
                                {
                                    // x = 0; // yukardaki döngümüzün şartına göre i eşit değilse 0  döngüye devam edecek eşit yaparak döngüyü bitirip yeni rakamımızı almış olduk. ve bu rakamı ekleyelım.
                                    durumCikan = false;
                                }
                            }

                            if (durumCikan == false) // true ise aynı sayı vardır boş geçer durum devam eder tekrarı. false yanı eşleşmemiş yeni bir sayı.
                            {
                                CikanSorular.Add(rsgCikanid);
                                // yeni değerde geldiği için tekrar break diyerek bu döngümüzdende çıkalım.
                                break;
                            }
                        }


                        //  2 ci soru yazdırılıyor.
                        baglanti.Open();
                        cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti);
                        SQLiteDataReader drx = cekme.ExecuteReader();
                        while (drx.Read())
                        {
                            lblSoru.Text = "" + soruBas + "";
                            richSoru.Rtf = drx["Soru"].ToString();
                            richA.Rtf = drx["Ask"].ToString(); // A Şıkkı
                            richB.Rtf = drx["Bsk"].ToString(); // B Şıkkı
                            richC.Rtf = drx["Csk"].ToString(); // C Şıkkı
                            richD.Rtf = drx["Dsk"].ToString(); // D Şıkkı

                            xCevaps = (drx["Cevap"].ToString());
                            /*try
                            {*/

                                if (drx["ResimYol"].ToString() == "")
                                {
                                    pictureBox1.Image = null;//////////////////////////////////////////----------------------------------------------------------------------------------
                                    pictureBox1.Image = Properties.Resources.kuş_fw;
                                }
                                else
                                {
                                    pictureBox1.Image = null;
                                    pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drx["ResimYol"].ToString();
                                }
                            /*}
                            catch
                            {
                            }*/
                        }
                        cekme.Dispose();
                        drx.Close();
                        baglanti.Close();
                        

                    }
                }
                else // ilk soru false = artık true olduğu için burdan diğer sorular devam edecek.
                {
                    //SoruSayiBelirleme++;
                    //soruBas++; // işte problem burası. 2/2 oldugu için 3 / 2 olur bu yüzden artımı burda yapmayacağız.
                    //lblScore.Text = "" + soruBas + " / " + soruBit + "";

                    if (KulSoruCevap == xCevaps)
                    {
                        dogruCevap++;
                    }
                    else
                    {
                        YanlisCevap++;
                    }

                    if (soruBas == soruBit) // 2 / 2 eşit ve bitti.
                    {

                        string cvr = "";
                        baglanti.Open();
                        cekme = new SQLiteCommand("SELECT TestdgrCevapDk FROM GvnGenel WHERE id=1", baglanti);
                        SQLiteDataReader drdk = cekme.ExecuteReader();
                        while (drdk.Read())
                        {
                            cvr = drdk["TestdgrCevapDk"].ToString();
                        }

                        cekme.Dispose();
                        drdk.Close();
                        baglanti.Close();

                        int dk = Convert.ToInt32(cvr);

                        int timer = dk * 60 * dogruCevap * 1000; // doğru cevap sıfır ise kaçı çarparsan çarp 0 sonuçu gelicektir.

                        if (timer == 0)
                        {
                            // TEKRARLANIYOR


                            panel1.Visible = false; // soruları kapat
                            panel4.Visible = true; // Sonuçları ekranda göster.
                            timergiris = 1;
                            buttom1.Text = "Tekrar Deneyin.";
                            KazanilanDk = 0;
                            lblDogruCevap.Text = Convert.ToString(dogruCevap);
                            lblYanlisCevap.Text = Convert.ToString(YanlisCevap);
                            lblKazanilanDk.Text = "0";
                            lblYuzde.Text = "0";
                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT OneriYazisi FROM GvnGenel WHERE id=1", baglanti);
                            SQLiteDataReader droneri = cekme.ExecuteReader();
                            while (droneri.Read())
                            {
                                lblOneri.Text = droneri["OneriYazisi"].ToString();
                                break;
                            }
                            cekme.Dispose();
                            droneri.Close();
                            baglanti.Close();

                            // ANALİZ
                            baglanti.Open();
                            cmd = new SQLiteCommand("INSERT INTO GvnAnaliz (Tarih,Ders,SoruSayisi,dCevap,yCevap,K_id) VALUES ('" + tarih + "', '" + SoruTipi + "', " + soruBit + ", " + dogruCevap + ", " + YanlisCevap + ", "+Kid+")", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            emailGonder();

                            ilkSoru = false;
                            dogruCevap = 0;
                            YanlisCevap = 0;
                            soruBas = 1;
                            soruBit = 0;
                            rndbaslst.Clear();
                            CikanSorular.Clear();

                            //Rasgele Soru seç
                            if (secenek == "Otomatik")
                            {

                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE " + otogun + "=" + otosira + "", baglanti);
                                SQLiteDataReader drveri = cekme.ExecuteReader();
                                while (drveri.Read())
                                {
                                    rndbaslst.Add(drveri["id"]);
                                }
                                cekme.Dispose();
                                drveri.Close();
                                baglanti.Close();
                            }

                            int rndcount = rndbaslst.Count;

                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount);

                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]);
                            CikanSorular.Add(rsgCikanid);

                            // sorubit
                            if (secenek == "Otomatik")
                            {
                                soruBit = rndbaslst.Count;
                            }
                            else
                            {
                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT TestSoruBit FROM GvnGenel WHERE id=1", baglanti); // döngüden sağlandı.
                                SQLiteDataReader drsorubit = cekme.ExecuteReader();
                                while (drsorubit.Read())
                                {
                                    soruBit = Convert.ToInt32(drsorubit["TestSoruBit"]);
                                    break;
                                }

                                cekme.Dispose();
                                drsorubit.Close();
                                baglanti.Close();
                            }

                            lblScore.Text = "" + soruBas + " / " + soruBit + "";


                            baglanti.Open();
                            cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti);
                            SQLiteDataReader drsira = cekme.ExecuteReader();
                            while (drsira.Read())
                            {
                                lblSoru.Text = "" + soruBas + "";
                                richSoru.Rtf = drsira["Soru"].ToString();
                                richA.Rtf = drsira["Ask"].ToString(); // A Şıkkı
                                richB.Rtf = drsira["Bsk"].ToString(); // B Şıkkı
                                richC.Rtf = drsira["Csk"].ToString(); // C Şıkkı
                                richD.Rtf = drsira["Dsk"].ToString(); // D Şıkkı
                                xCevaps = (drsira["Cevap"].ToString());
                                try
                                {
                                    if (drsira["ResimYol"].ToString() == "")
                                    {
                                        pictureBox1.Image = Properties.Resources.kuş_fw;
                                    }
                                    else
                                    {
                                        pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drsira["ResimYol"].ToString();
                                    }
                                }
                                catch
                                {
                                }
                            }
                            cekme.Dispose();
                            drsira.Close();
                            baglanti.Close();


                            engellensinMi = 1; // engelleme devam.
                        }
                        else
                        {
                            int zaman = timer / 1000 / 60;
                            timergiris = timer;
                            panel1.Visible = false; // soruları kapat
                            panel4.Visible = true; // Sonuçları ekranda göster.
                            KazanilanDk = zaman;
                            buttom1.Text = "Masaüstüne Erişmek için Tıklayın.";
                            lblDogruCevap.Text = dogruCevap.ToString();
                            lblYanlisCevap.Text = YanlisCevap.ToString();
                            lblKazanilanDk.Text = zaman.ToString();

                            double yuzde = 0;

                            if (YanlisCevap == 0)
                            {
                                yuzde = 100;
                            }
                            else if (dogruCevap == 0)
                            {
                                yuzde = 0;
                            }
                            else if (YanlisCevap == dogruCevap)
                            {
                                yuzde = 50;
                            }
                            else
                            {
                                int soruSayisi = dogruCevap + YanlisCevap; // 2 sinin toplamı soru sayısıdır.
                                yuzde = 100 / soruSayisi;
                                yuzde = yuzde * dogruCevap;
                            }
                           
                            

                            lblYuzde.Text = yuzde.ToString();

                            if (yuzde < 50)
                            {

                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT OneriYazisi FROM GvnGenel WHERE id=1", baglanti);
                                SQLiteDataReader droneri = cekme.ExecuteReader();
                                while (droneri.Read())
                                {
                                    lblOneri.Text = droneri["OneriYazisi"].ToString();
                                    break;
                                }
                                cekme.Dispose();
                                droneri.Close();
                                baglanti.Close();
                            }
                            else
                            {
                                baglanti.Open();
                                cekme = new SQLiteCommand("SELECT OneriYazisi2 FROM GvnGenel WHERE id=1", baglanti);
                                SQLiteDataReader droneri = cekme.ExecuteReader();
                                while (droneri.Read())
                                {
                                    lblOneri.Text = droneri["OneriYazisi2"].ToString();
                                    break;
                                }
                                cekme.Dispose();
                                droneri.Close();
                                baglanti.Close();
                            }
                            
                            // ANALİZ
                            baglanti.Open();
                            cmd = new SQLiteCommand("INSERT INTO GvnAnaliz (Tarih,Ders,SoruSayisi,dCevap,yCevap,K_id) VALUES ('" + tarih + "', '" + SoruTipi + "', " + soruBit + ", " + dogruCevap + ", " + YanlisCevap + ","+Kid+")", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            emailGonder();

                            //SoruSayiBelirleme = 1;
                            ilkSoru = false;
                            dogruCevap = 0;
                            YanlisCevap = 0;
                            soruBas = 1;
                            soruBit = 0;
                            rndbaslst.Clear();
                            CikanSorular.Clear();


                            baglanti.Open();
                            cmd = new SQLiteCommand("UPDATE GvnGenel SET Dakika=" + zaman + "", baglanti);
                            cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            baglanti.Close();

                            engellensinMi = 0; // engelleri kaldır. // regedit hariç                            
                        }
                        

                    } // sorubas ve sorubit eşleşmiyorsa else durumundan yeni bir soru yazdırıyoruz.
                    else
                    {

                        soruBas++; // artıralım ve yazdıralm.
                        lblScore.Text = "" + soruBas + " / " + soruBit + "";

                        int rndcount = rndbaslst.Count;

                        int x = 1;
                        while (x != 0)
                        {
                            Random rasgele = new Random();

                            int rsg = rasgele.Next(0, rndcount);

                            int xuzunluk = CikanSorular.Count;
                            rsgCikanid = Convert.ToInt32(rndbaslst[rsg]); // 2
                            bool durumCikan = false;
                            for (int i = 0; i < xuzunluk; i++)
                            {
                                if (Convert.ToInt32(CikanSorular[i]) == rsgCikanid) // eğer item eşit ise rasgele sayı ile bu sayı önceden atılmış demektir yani. tekrar random çekilmelidir.
                                {
                                    x = 1; // rasgeleye devam.
                                    durumCikan = true;
                                    break;
                                }
                                else // eğer böyle bir sayı önceden çekilmediyse. devam et.
                                {
                                    // x = 0; // yukardaki döngümüzün şartına göre i eşit değilse 0  döngüye devam edecek eşit yaparak döngüyü bitirip yeni rakamımızı almış olduk. ve bu rakamı ekleyelım.
                                    durumCikan = false;
                                }
                            }

                            if (durumCikan == false) // true ise aynı sayı vardır boş geçer durum devam eder tekrarı. false yanı eşleşmemiş yeni bir sayı.
                            {
                                CikanSorular.Add(rsgCikanid);
                                // yeni değerde geldiği için tekrar break diyerek bu döngümüzdende çıkalım.
                                break;
                            }
                        }


                        baglanti.Open();
                        cekme = new SQLiteCommand("SELECT * FROM GvnSorular WHERE id=" + rsgCikanid + "", baglanti);
                        SQLiteDataReader drsira = cekme.ExecuteReader();
                        while (drsira.Read())
                        {
                            lblSoru.Text = "" + soruBas + "";
                            richSoru.Rtf = drsira["Soru"].ToString();
                            richA.Rtf = drsira["Ask"].ToString(); // A Şıkkı
                            richB.Rtf = drsira["Bsk"].ToString(); // B Şıkkı
                            richC.Rtf = drsira["Csk"].ToString(); // C Şıkkı
                            richD.Rtf = drsira["Dsk"].ToString(); // D Şıkkı
                            xCevaps = (drsira["Cevap"].ToString());
                            try
                            {

                                if (drsira["ResimYol"].ToString() == "")
                                {
                                    pictureBox1.Image = Properties.Resources.kuş_fw;
                                }
                                else
                                {
                                    pictureBox1.ImageLocation = Application.StartupPath.ToString() + "\\" + drsira["ResimYol"].ToString();
                                }
                            }
                            catch
                            {
                            }
                        }
                        cekme.Dispose();
                        drsira.Close();
                        baglanti.Close();

                        lblScore.Text = "" + soruBas + " / " + soruBit + "";
                    }
                }
            }

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // Zaman aşımı sıfırla
            zamanAsimiSifirla();
            
            // Masaüstüne geçiş

            if (KazanilanDk == 0)
            {
                // Hepsi yanlışsa tekrar yapılcak sorularda.
                timer1.Interval = 1;
                timer1.Start();
                panel1.Visible = true;
                panel4.Visible = false;
            }
            else
            {

                

                timer1.Interval = timergiris;
                timer1.Start();

                // Başla butonuna al

                button2.Visible = true; // başla butonu
                pictureBox2.Visible = true; // baykuş logomuz 
                picArkaPlan.Image = Properties.Resources.arkaplan_fw; // ana arka plan resmi
                panel1.Visible = false; // soru gösterme
                panel4.Visible = false; // soru bitiş gösterme
                button2.Enabled = true;
                KazanilanDk = 0;
                timergiris = 0;

                //SoruSayisi = 0;
                //SoruSayiBelirleme = 0;
                ilkSoru = false;
                dogruCevap = 0;
                YanlisCevap = 0;
                soruBas = 0;
                soruBit = 0;
                CikanSorular.Clear();
                lblScore.Text = "";
                pictureBox1.Image = null;
                
                GeriSayim grsym = new GeriSayim();
                grsym.Show();
                                               

                this.Hide();

            }
        }

        private void pictureBox5_MouseHover(object sender, EventArgs e)
        {
            //A
            pictureBox5.Image = Properties.Resources.buttonA;
        }

        private void pictureBox5_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Image = Properties.Resources.buttonANor;
        }

        private void pictureBox6_MouseHover(object sender, EventArgs e)
        {
            //B
            pictureBox6.Image = Properties.Resources.buttonB;
        }

        private void pictureBox6_MouseLeave(object sender, EventArgs e)
        {
            
            pictureBox6.Image = Properties.Resources.buttonBNor;
        }

        private void pictureBox7_MouseHover(object sender, EventArgs e)
        {
            //C
            pictureBox7.Image = Properties.Resources.buttonC;
        }

        private void pictureBox7_MouseLeave(object sender, EventArgs e)
        {
            
            pictureBox7.Image = Properties.Resources.buttonCNor;
        }

        private void pictureBox8_MouseHover(object sender, EventArgs e)
        {
            // D
            pictureBox8.Image = Properties.Resources.buttonD;
        }

        private void pictureBox8_MouseLeave(object sender, EventArgs e)
        {
            pictureBox8.Image = Properties.Resources.buttonDNor;
        }

        private void pictureBox5_MouseDown(object sender, MouseEventArgs e)
        {
            // Zaman aşımı sıfırla
            zamanAsimiSifirla();
            // A Şıkkı
            KulSoruCevap = "A";
            SoruGiris();
        }

        private void pictureBox6_MouseDown(object sender, MouseEventArgs e)
        {
            // Zaman aşımı sıfırla
            zamanAsimiSifirla();
            // B Şıkkı
            KulSoruCevap = "B";
            SoruGiris();
        }

        private void pictureBox7_MouseDown(object sender, MouseEventArgs e)
        {
            // Zaman aşımı sıfırla
            zamanAsimiSifirla();
            // C Şıkkı
            KulSoruCevap = "C";
            SoruGiris();
        }

        private void pictureBox8_MouseDown(object sender, MouseEventArgs e)
        {
            // Zaman aşımı sıfırla
            zamanAsimiSifirla();
            // D Şıkkı
            KulSoruCevap = "D";
            SoruGiris();
        }

        string KSeviye = "";
        string KAdi = "";
        int Kid = -1;

        private void button4_Click(object sender, EventArgs e)
        {
            if (txtKadi.Text == "" || txtSifre.Text == "")
            {
                MessageBox.Show("Gerekli Yerleri Doldurunuz.", "BilKullan", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1);
            }
            else
            {
                string kadi = "";
                string sifre = "";
                baglanti.Open();
                cekme = new SQLiteCommand("SELECT * FROM GvnKullanici WHERE Kadi='"+txtKadi.Text+"' AND Sifre='"+txtSifre.Text+"'", baglanti);
                SQLiteDataReader veri = cekme.ExecuteReader();
                while(veri.Read())
                {
                    kadi = veri["Kadi"].ToString();
                    sifre = veri["Sifre"].ToString();
                    KSeviye = veri["Seviye"].ToString();
                    KAdi = veri["Adi"].ToString();
                    Kid = Convert.ToInt32(veri["id"]);
                }
                cekme.Dispose();
                veri.Close();                               
                baglanti.Close();

                if(kadi == txtKadi.Text && sifre == txtSifre.Text)
                {
                    //logged in
                    button1.Visible = true; // Oturum kapatma butonunu aç.
                    button2.Visible = true; // Başla buttonunu aç.
                    // login paneli yoket.
                    lblKadi.Visible = false;
                    lblSifre.Visible = false;
                    txtKadi.Visible = false;
                    txtSifre.Visible = false;
                    button4.Visible = false;
                    tmrZamanAsimi.Start();

                    txtKadi.Text = "";
                    txtSifre.Text = "";
                }
                else
                {
                    MessageBox.Show("Böyle bir kullanıcı bulunmamakta.", "BilKullan", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                }

            }
        }

        void baslangicMenuGit()
        {
            button2.Visible = false; // başla butonu
            pictureBox2.Visible = true; // baykuş logomuz 
            picArkaPlan.Image = Properties.Resources.arkaplan_fw; // ana arka plan resmi
            panel1.Visible = false; // soru gösterme
            panel4.Visible = false; // soru bitiş gösterme
            KazanilanDk = 0;
            timergiris = 0;
            button2.Enabled = true;

            ilkSoru = false;
            dogruCevap = 0;
            YanlisCevap = 0;
            soruBas = 0;
            soruBit = 0;
            CikanSorular.Clear();
            lblScore.Text = "";
            pictureBox1.Image = null;


            //Login Paneli Getir
            lblKadi.Visible = true;
            lblSifre.Visible = true;
            txtKadi.Visible = true;
            txtSifre.Visible = true;
            button4.Visible = true;

            button1.Visible = false;

            KSeviye = "";
            KAdi = "";
            Kid = -1;

            //Zaman aşımını devre dışı bırak.
            tmrZamanAsimi.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            baslangicMenuGit();
        }

        

        private void tmrZamanAsimi_Tick(object sender, EventArgs e)
        {
            baslangicMenuGit();
        }

        private void tmrKontrol_Tick(object sender, EventArgs e)
        {
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (tmrZamanAsimi.Enabled == true)
            {
                // Zaman aşımı sıfırla
                zamanAsimiSifirla();
                
            }
        }

        private void picArkaPlan_MouseDown(object sender, MouseEventArgs e)
        {
            if (tmrZamanAsimi.Enabled == true)
            {
                // Zaman aşımı sıfırla
                zamanAsimiSifirla();
            }
        }



    }
}
