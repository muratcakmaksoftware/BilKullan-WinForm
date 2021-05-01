using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Windows.Forms;

namespace Guvenlik
{
    class fonk
    {
        internal SQLiteConnection bag()
        {
            SQLiteConnection baglanti = new SQLiteConnection("Data Source=" + Application.StartupPath.ToString() + "\\database.s3db;"); //  Password=1234

            return baglanti;
        }

        internal string strtext (string str)
        {
            return str.Replace("'", "''");
        }

        internal string systemYol()
        {
            return Application.StartupPath.ToString();
        }

        internal string dateformat(string date)
        {
            date = date.Replace(".", "_");
            date = date.Replace("/", "_");
            date = date.Replace(":", "_");
            return date;
        }
    }
}
