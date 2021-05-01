using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Runtime.InteropServices;

namespace Guvenlik
{
    public partial class Yardim : Form
    {
        public Yardim()
        {
            InitializeComponent();
        }

        fonk fnk = new fonk();

        private void Yardim_Load(object sender, EventArgs e)
        {
            baglanti = fnk.bag();
        }

        SQLiteConnection baglanti;

        SQLiteCommand cmd;

        protected override System.Boolean ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                this.Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Yardim_FormClosing(object sender, FormClosingEventArgs e)
        {
            baglanti.Open();
            cmd = new SQLiteCommand("UPDATE GvnGenel SET ScreenEngel=1", baglanti);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            baglanti.Close();
        }
    }
}
