namespace Guvenlik
{
    partial class ExcelSoruGiris
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExcelSoruGiris));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.cmbExcelDers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.richSoru = new System.Windows.Forms.RichTextBox();
            this.richAsk = new System.Windows.Forms.RichTextBox();
            this.richBsk = new System.Windows.Forms.RichTextBox();
            this.richCsk = new System.Windows.Forms.RichTextBox();
            this.richDsk = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbSeviye = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbKonu = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(13, 72);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.Size = new System.Drawing.Size(745, 319);
            this.dataGridView1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 8);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 31);
            this.button1.TabIndex = 1;
            this.button1.Text = "Excel Aç";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 397);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(170, 38);
            this.button2.TabIndex = 2;
            this.button2.Text = "Listeyi Yenile";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(580, 397);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(178, 38);
            this.button3.TabIndex = 3;
            this.button3.Text = "Soruları Ekle";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // cmbExcelDers
            // 
            this.cmbExcelDers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbExcelDers.FormattingEnabled = true;
            this.cmbExcelDers.Location = new System.Drawing.Point(285, 45);
            this.cmbExcelDers.Name = "cmbExcelDers";
            this.cmbExcelDers.Size = new System.Drawing.Size(161, 21);
            this.cmbExcelDers.TabIndex = 4;
            this.cmbExcelDers.SelectedIndexChanged += new System.EventHandler(this.cmbExcelDers_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.Location = new System.Drawing.Point(249, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Ders";
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Location = new System.Drawing.Point(632, 4);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(117, 35);
            this.dataGridView2.TabIndex = 6;
            this.dataGridView2.Visible = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(506, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(120, 35);
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Visible = false;
            // 
            // richSoru
            // 
            this.richSoru.Location = new System.Drawing.Point(258, 8);
            this.richSoru.Name = "richSoru";
            this.richSoru.Size = new System.Drawing.Size(172, 17);
            this.richSoru.TabIndex = 8;
            this.richSoru.Text = "";
            this.richSoru.Visible = false;
            // 
            // richAsk
            // 
            this.richAsk.Location = new System.Drawing.Point(267, 8);
            this.richAsk.Name = "richAsk";
            this.richAsk.Size = new System.Drawing.Size(172, 17);
            this.richAsk.TabIndex = 9;
            this.richAsk.Text = "";
            this.richAsk.Visible = false;
            // 
            // richBsk
            // 
            this.richBsk.Location = new System.Drawing.Point(279, 8);
            this.richBsk.Name = "richBsk";
            this.richBsk.Size = new System.Drawing.Size(172, 17);
            this.richBsk.TabIndex = 10;
            this.richBsk.Text = "";
            this.richBsk.Visible = false;
            // 
            // richCsk
            // 
            this.richCsk.Location = new System.Drawing.Point(289, 8);
            this.richCsk.Name = "richCsk";
            this.richCsk.Size = new System.Drawing.Size(172, 17);
            this.richCsk.TabIndex = 11;
            this.richCsk.Text = "";
            this.richCsk.Visible = false;
            // 
            // richDsk
            // 
            this.richDsk.Location = new System.Drawing.Point(302, 8);
            this.richDsk.Name = "richDsk";
            this.richDsk.Size = new System.Drawing.Size(172, 17);
            this.richDsk.TabIndex = 12;
            this.richDsk.Text = "";
            this.richDsk.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.Location = new System.Drawing.Point(8, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Seviye";
            // 
            // cmbSeviye
            // 
            this.cmbSeviye.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSeviye.FormattingEnabled = true;
            this.cmbSeviye.Items.AddRange(new object[] {
            "İlk Okul",
            "Orta Okul",
            "Lise"});
            this.cmbSeviye.Location = new System.Drawing.Point(59, 45);
            this.cmbSeviye.Name = "cmbSeviye";
            this.cmbSeviye.Size = new System.Drawing.Size(161, 21);
            this.cmbSeviye.TabIndex = 14;
            this.cmbSeviye.SelectedIndexChanged += new System.EventHandler(this.cmbSeviye_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.Location = new System.Drawing.Point(470, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Konu";
            // 
            // cmbKonu
            // 
            this.cmbKonu.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKonu.FormattingEnabled = true;
            this.cmbKonu.Location = new System.Drawing.Point(506, 45);
            this.cmbKonu.Name = "cmbKonu";
            this.cmbKonu.Size = new System.Drawing.Size(161, 21);
            this.cmbKonu.TabIndex = 15;
            this.cmbKonu.SelectedIndexChanged += new System.EventHandler(this.cmbKonu_SelectedIndexChanged);
            // 
            // ExcelSoruGiris
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(761, 447);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbKonu);
            this.Controls.Add(this.cmbSeviye);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.richDsk);
            this.Controls.Add(this.richCsk);
            this.Controls.Add(this.richBsk);
            this.Controls.Add(this.richAsk);
            this.Controls.Add(this.richSoru);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.dataGridView2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbExcelDers);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExcelSoruGiris";
            this.Text = "Excel Toplu Soru Giriş";
            this.Load += new System.EventHandler(this.ExcelSoruGiris_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox cmbExcelDers;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.RichTextBox richSoru;
        private System.Windows.Forms.RichTextBox richAsk;
        private System.Windows.Forms.RichTextBox richBsk;
        private System.Windows.Forms.RichTextBox richCsk;
        private System.Windows.Forms.RichTextBox richDsk;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbSeviye;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbKonu;
    }
}