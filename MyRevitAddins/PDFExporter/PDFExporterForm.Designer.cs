namespace PDFExporter
{
    partial class PDFExporterForm
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.radioBox1 = new Shared.RadioBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.button4 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.radioBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(22, 22);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(260, 22);
            this.textBox1.TabIndex = 0;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "Choose sheet set to export:";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(22, 59);
            this.comboBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(473, 32);
            this.comboBox1.TabIndex = 1;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(22, 153);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(183, 72);
            this.button1.TabIndex = 2;
            this.button1.Text = "Select folder where to export";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox2.Location = new System.Drawing.Point(22, 236);
            this.textBox2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(1150, 29);
            this.textBox2.TabIndex = 0;
            this.textBox2.TabStop = false;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(22, 284);
            this.button2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(183, 72);
            this.button2.TabIndex = 2;
            this.button2.Text = "Export!";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(216, 153);
            this.button3.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(183, 72);
            this.button3.TabIndex = 2;
            this.button3.Text = "Create print settings";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox3.Location = new System.Drawing.Point(537, 22);
            this.textBox3.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(638, 22);
            this.textBox3.TabIndex = 0;
            this.textBox3.TabStop = false;
            this.textBox3.Text = "Settings: ~Run (Create Print Settings) each time settings change!~";
            // 
            // radioBox1
            // 
            this.radioBox1.Controls.Add(this.radioButton3);
            this.radioBox1.Controls.Add(this.radioButton2);
            this.radioBox1.Controls.Add(this.radioButton1);
            this.radioBox1.Location = new System.Drawing.Point(537, 57);
            this.radioBox1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.radioBox1.Name = "radioBox1";
            this.radioBox1.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.radioBox1.Size = new System.Drawing.Size(169, 168);
            this.radioBox1.TabIndex = 3;
            this.radioBox1.TabStop = false;
            this.radioBox1.Text = "Colour:";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Checked = global::PDFExporter.Properties.Settings.Default.Setting_Colour_BlackWhite;
            this.radioButton3.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::PDFExporter.Properties.Settings.Default, "Setting_Colour_BlackWhite", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButton3.Location = new System.Drawing.Point(11, 116);
            this.radioButton3.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(142, 29);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "Black/White";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Checked = global::PDFExporter.Properties.Settings.Default.Setting_Colour_GrayScale;
            this.radioButton2.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::PDFExporter.Properties.Settings.Default, "Setting_Colour_GrayScale", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButton2.Location = new System.Drawing.Point(13, 78);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(129, 29);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "GrayScale";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = global::PDFExporter.Properties.Settings.Default.Setting_Colour_Colour;
            this.radioButton1.DataBindings.Add(new System.Windows.Forms.Binding("Checked", global::PDFExporter.Properties.Settings.Default, "Setting_Colour_Colour", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.radioButton1.Location = new System.Drawing.Point(13, 37);
            this.radioButton1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(95, 29);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Colour";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(216, 286);
            this.button4.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(183, 72);
            this.button4.TabIndex = 2;
            this.button4.Text = "Copy files";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.Button4_Click);
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox4.Location = new System.Drawing.Point(1302, 336);
            this.textBox4.Margin = new System.Windows.Forms.Padding(6);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(49, 29);
            this.textBox4.TabIndex = 0;
            this.textBox4.TabStop = false;
            this.textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox5.Location = new System.Drawing.Point(1255, 339);
            this.textBox5.Margin = new System.Windows.Forms.Padding(6);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(35, 22);
            this.textBox5.TabIndex = 0;
            this.textBox5.TabStop = false;
            this.textBox5.Text = "OF";
            this.textBox5.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox6.Location = new System.Drawing.Point(1194, 336);
            this.textBox6.Margin = new System.Windows.Forms.Padding(6);
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            this.textBox6.Size = new System.Drawing.Size(49, 29);
            this.textBox6.TabIndex = 0;
            this.textBox6.TabStop = false;
            this.textBox6.Text = "0";
            this.textBox6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox7
            // 
            this.textBox7.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.Location = new System.Drawing.Point(1046, 339);
            this.textBox7.Margin = new System.Windows.Forms.Padding(6);
            this.textBox7.Name = "textBox7";
            this.textBox7.ReadOnly = true;
            this.textBox7.Size = new System.Drawing.Size(136, 22);
            this.textBox7.TabIndex = 0;
            this.textBox7.TabStop = false;
            this.textBox7.Text = "Sheets created:";
            this.textBox7.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox8
            // 
            this.textBox8.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox8.Location = new System.Drawing.Point(1194, 298);
            this.textBox8.Margin = new System.Windows.Forms.Padding(6);
            this.textBox8.Name = "textBox8";
            this.textBox8.ReadOnly = true;
            this.textBox8.Size = new System.Drawing.Size(157, 29);
            this.textBox8.TabIndex = 0;
            this.textBox8.TabStop = false;
            this.textBox8.Text = "Setup";
            this.textBox8.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox9
            // 
            this.textBox9.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox9.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox9.Location = new System.Drawing.Point(1046, 301);
            this.textBox9.Margin = new System.Windows.Forms.Padding(6);
            this.textBox9.Name = "textBox9";
            this.textBox9.ReadOnly = true;
            this.textBox9.Size = new System.Drawing.Size(136, 22);
            this.textBox9.TabIndex = 0;
            this.textBox9.TabStop = false;
            this.textBox9.Text = "Status:";
            this.textBox9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // PDFExporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1366, 380);
            this.Controls.Add(this.radioBox1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.textBox6);
            this.Controls.Add(this.textBox5);
            this.Controls.Add(this.textBox8);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox9);
            this.Controls.Add(this.textBox7);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox1);
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "PDFExporterForm";
            this.Text = "PDFExporterForm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PDFExporterForm_FormClosing);
            this.radioBox1.ResumeLayout(false);
            this.radioBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox textBox3;
        private Shared.RadioBox radioBox1;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.TextBox textBox9;
    }
}