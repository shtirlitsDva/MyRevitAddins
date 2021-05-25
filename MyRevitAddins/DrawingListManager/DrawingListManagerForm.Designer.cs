namespace MEPUtils.DrawingListManager
{
    partial class DrawingListManagerForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textBox6 = new System.Windows.Forms.TextBox();
            this.textBox11 = new System.Windows.Forms.TextBox();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.textBox9 = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textBox7 = new System.Windows.Forms.TextBox();
            this.textBox8 = new System.Windows.Forms.TextBox();
            this.textBox10 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.dGV1 = new System.Windows.Forms.DataGridView();
            this.button4 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.textBox12 = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGV1)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.CausesValidation = false;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(2930, 1331);
            this.tabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.textBox6);
            this.tabPage1.Controls.Add(this.textBox11);
            this.tabPage1.Controls.Add(this.button5);
            this.tabPage1.Controls.Add(this.textBox5);
            this.tabPage1.Controls.Add(this.textBox9);
            this.tabPage1.Controls.Add(this.button2);
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Controls.Add(this.button1);
            this.tabPage1.Controls.Add(this.textBox3);
            this.tabPage1.Controls.Add(this.textBox2);
            this.tabPage1.Location = new System.Drawing.Point(8, 39);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage1.Size = new System.Drawing.Size(2914, 1284);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Folder and file";
            // 
            // textBox6
            // 
            this.textBox6.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox6.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox6.Location = new System.Drawing.Point(750, 34);
            this.textBox6.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox6.Name = "textBox6";
            this.textBox6.ReadOnly = true;
            this.textBox6.Size = new System.Drawing.Size(175, 24);
            this.textBox6.TabIndex = 18;
            this.textBox6.TabStop = false;
            this.textBox6.Text = "Staging found:";
            this.textBox6.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox11
            // 
            this.textBox11.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox11.Location = new System.Drawing.Point(936, 31);
            this.textBox11.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox11.Name = "textBox11";
            this.textBox11.ReadOnly = true;
            this.textBox11.Size = new System.Drawing.Size(72, 31);
            this.textBox11.TabIndex = 17;
            this.textBox11.TabStop = false;
            this.textBox11.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(7, 268);
            this.button5.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(374, 75);
            this.button5.TabIndex = 16;
            this.button5.Text = "Select folder with STAGING drawings:";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBox5
            // 
            this.textBox5.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox5.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MEPUtils.DrawingListManager.Properties.Settings.Default, "PathToStagingFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox5.Location = new System.Drawing.Point(7, 354);
            this.textBox5.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox5.Name = "textBox5";
            this.textBox5.ReadOnly = true;
            this.textBox5.Size = new System.Drawing.Size(1428, 31);
            this.textBox5.TabIndex = 15;
            this.textBox5.TabStop = false;
            this.textBox5.Text = global::MEPUtils.DrawingListManager.Properties.Settings.Default.PathToStagingFolder;
            // 
            // textBox9
            // 
            this.textBox9.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox9.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox9.Location = new System.Drawing.Point(468, 34);
            this.textBox9.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox9.Name = "textBox9";
            this.textBox9.ReadOnly = true;
            this.textBox9.Size = new System.Drawing.Size(175, 24);
            this.textBox9.TabIndex = 14;
            this.textBox9.TabStop = false;
            this.textBox9.Text = "Drawings found:";
            this.textBox9.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(7, 139);
            this.button2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(374, 75);
            this.button2.TabIndex = 13;
            this.button2.Text = "Select the drawing list Excel file:";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MEPUtils.DrawingListManager.Properties.Settings.Default, "PathToDwgList", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox1.Location = new System.Drawing.Point(7, 225);
            this.textBox1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(1428, 31);
            this.textBox1.TabIndex = 12;
            this.textBox1.TabStop = false;
            this.textBox1.Text = global::MEPUtils.DrawingListManager.Properties.Settings.Default.PathToDwgList;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(7, 9);
            this.button1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(374, 75);
            this.button1.TabIndex = 11;
            this.button1.Text = "Select folder with RELEASED drawings:";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox3
            // 
            this.textBox3.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox3.Location = new System.Drawing.Point(654, 31);
            this.textBox3.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(72, 31);
            this.textBox3.TabIndex = 8;
            this.textBox3.TabStop = false;
            this.textBox3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MEPUtils.DrawingListManager.Properties.Settings.Default, "PathToDwgFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox2.Location = new System.Drawing.Point(7, 96);
            this.textBox2.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(1428, 31);
            this.textBox2.TabIndex = 9;
            this.textBox2.TabStop = false;
            this.textBox2.Text = global::MEPUtils.DrawingListManager.Properties.Settings.Default.PathToDwgFolder;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.textBox12);
            this.tabPage2.Controls.Add(this.textBox7);
            this.tabPage2.Controls.Add(this.textBox8);
            this.tabPage2.Controls.Add(this.textBox10);
            this.tabPage2.Controls.Add(this.textBox4);
            this.tabPage2.Controls.Add(this.dGV1);
            this.tabPage2.Controls.Add(this.button6);
            this.tabPage2.Controls.Add(this.button4);
            this.tabPage2.Location = new System.Drawing.Point(8, 39);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage2.Size = new System.Drawing.Size(2914, 1284);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Manage";
            // 
            // textBox7
            // 
            this.textBox7.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.textBox7.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox7.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox7.ForeColor = System.Drawing.Color.Yellow;
            this.textBox7.Location = new System.Drawing.Point(1356, 0);
            this.textBox7.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox7.Name = "textBox7";
            this.textBox7.ReadOnly = true;
            this.textBox7.Size = new System.Drawing.Size(376, 37);
            this.textBox7.TabIndex = 18;
            this.textBox7.TabStop = false;
            this.textBox7.Text = "Warning (data mismatch)";
            // 
            // textBox8
            // 
            this.textBox8.BackColor = System.Drawing.Color.Thistle;
            this.textBox8.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox8.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox8.ForeColor = System.Drawing.Color.DarkRed;
            this.textBox8.Location = new System.Drawing.Point(882, 39);
            this.textBox8.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox8.Name = "textBox8";
            this.textBox8.ReadOnly = true;
            this.textBox8.Size = new System.Drawing.Size(397, 37);
            this.textBox8.TabIndex = 18;
            this.textBox8.TabStop = false;
            this.textBox8.Text = "Error (File or Excel missing)";
            // 
            // textBox10
            // 
            this.textBox10.BackColor = System.Drawing.Color.LemonChiffon;
            this.textBox10.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox10.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox10.ForeColor = System.Drawing.Color.Green;
            this.textBox10.Location = new System.Drawing.Point(1009, 0);
            this.textBox10.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox10.Name = "textBox10";
            this.textBox10.ReadOnly = true;
            this.textBox10.Size = new System.Drawing.Size(335, 37);
            this.textBox10.TabIndex = 18;
            this.textBox10.TabStop = false;
            this.textBox10.Text = "All okay (Meta missing)";
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.Color.GreenYellow;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.ForeColor = System.Drawing.Color.Green;
            this.textBox4.Location = new System.Drawing.Point(882, 0);
            this.textBox4.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(115, 37);
            this.textBox4.TabIndex = 18;
            this.textBox4.TabStop = false;
            this.textBox4.Text = "All okay";
            // 
            // dGV1
            // 
            this.dGV1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGV1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dGV1.Location = new System.Drawing.Point(4, 82);
            this.dGV1.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.dGV1.Name = "dGV1";
            this.dGV1.RowHeadersWidth = 72;
            this.dGV1.Size = new System.Drawing.Size(2906, 1198);
            this.dGV1.TabIndex = 17;
            this.dGV1.DataBindingComplete += new System.Windows.Forms.DataGridViewBindingCompleteEventHandler(this.dGV1_DataBindingComplete);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(7, 9);
            this.button4.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(164, 44);
            this.button4.TabIndex = 16;
            this.button4.Text = "(Re-) Scan";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(379, 9);
            this.button6.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(164, 44);
            this.button6.TabIndex = 16;
            this.button6.Text = "Consolidate";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // textBox12
            // 
            this.textBox12.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox12.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox12.Location = new System.Drawing.Point(190, 19);
            this.textBox12.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.textBox12.Name = "textBox12";
            this.textBox12.ReadOnly = true;
            this.textBox12.Size = new System.Drawing.Size(175, 24);
            this.textBox12.TabIndex = 19;
            this.textBox12.TabStop = false;
            this.textBox12.Text = "Staging found:";
            this.textBox12.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // DrawingListManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2930, 1331);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DrawingListManagerForm";
            this.Text = "Drawing List Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DrawingListManagerForm_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGV1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox textBox9;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.DataGridView dGV1;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.TextBox textBox7;
        private System.Windows.Forms.TextBox textBox8;
        private System.Windows.Forms.TextBox textBox10;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox textBox5;
        private System.Windows.Forms.TextBox textBox6;
        private System.Windows.Forms.TextBox textBox11;
        private System.Windows.Forms.TextBox textBox12;
        private System.Windows.Forms.Button button6;
    }
}

