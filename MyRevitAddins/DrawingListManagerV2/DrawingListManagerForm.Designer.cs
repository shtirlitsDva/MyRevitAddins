namespace MEPUtils.DrawingListManagerV2
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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            textBox6 = new TextBox();
            textBox11 = new TextBox();
            button5 = new Button();
            textBox5 = new TextBox();
            textBox9 = new TextBox();
            button2 = new Button();
            textBox1 = new TextBox();
            button1 = new Button();
            textBox3 = new TextBox();
            textBox2 = new TextBox();
            tabPage2 = new TabPage();
            textBox13 = new TextBox();
            textBox12 = new TextBox();
            textBox7 = new TextBox();
            textBox8 = new TextBox();
            textBox10 = new TextBox();
            textBox4 = new TextBox();
            button6 = new Button();
            button4 = new Button();
            panel1 = new Panel();
            panel2 = new Panel();
            dGV1 = new DataGridView();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dGV1).BeginInit();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.CausesValidation = false;
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1281, 686);
            tabControl1.TabIndex = 8;
            // 
            // tabPage1
            // 
            tabPage1.BackColor = SystemColors.Control;
            tabPage1.Controls.Add(textBox6);
            tabPage1.Controls.Add(textBox11);
            tabPage1.Controls.Add(button5);
            tabPage1.Controls.Add(textBox5);
            tabPage1.Controls.Add(textBox9);
            tabPage1.Controls.Add(button2);
            tabPage1.Controls.Add(textBox1);
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(textBox3);
            tabPage1.Controls.Add(textBox2);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3, 3, 3, 3);
            tabPage1.Size = new Size(1018, 602);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Folder and file";
            // 
            // textBox6
            // 
            textBox6.BackColor = SystemColors.Menu;
            textBox6.BorderStyle = BorderStyle.None;
            textBox6.Location = new Point(500, 27);
            textBox6.Margin = new Padding(5, 5, 5, 5);
            textBox6.Name = "textBox6";
            textBox6.ReadOnly = true;
            textBox6.Size = new Size(117, 20);
            textBox6.TabIndex = 18;
            textBox6.TabStop = false;
            textBox6.Text = "Staging found:";
            textBox6.TextAlign = HorizontalAlignment.Right;
            // 
            // textBox11
            // 
            textBox11.BackColor = SystemColors.Menu;
            textBox11.Location = new Point(624, 25);
            textBox11.Margin = new Padding(5, 5, 5, 5);
            textBox11.Name = "textBox11";
            textBox11.ReadOnly = true;
            textBox11.Size = new Size(49, 27);
            textBox11.TabIndex = 17;
            textBox11.TabStop = false;
            textBox11.TextAlign = HorizontalAlignment.Center;
            // 
            // button5
            // 
            button5.Location = new Point(5, 215);
            button5.Margin = new Padding(5, 5, 5, 5);
            button5.Name = "button5";
            button5.Size = new Size(250, 60);
            button5.TabIndex = 16;
            button5.Text = "Select folder with STAGING drawings:";
            button5.UseVisualStyleBackColor = true;
            button5.Click += button5_Click;
            // 
            // textBox5
            // 
            textBox5.BackColor = SystemColors.Menu;
            textBox5.Location = new Point(5, 283);
            textBox5.Margin = new Padding(5, 5, 5, 5);
            textBox5.Name = "textBox5";
            textBox5.ReadOnly = true;
            textBox5.Size = new Size(953, 27);
            textBox5.TabIndex = 15;
            textBox5.TabStop = false;
            // 
            // textBox9
            // 
            textBox9.BackColor = SystemColors.Menu;
            textBox9.BorderStyle = BorderStyle.None;
            textBox9.Location = new Point(312, 27);
            textBox9.Margin = new Padding(5, 5, 5, 5);
            textBox9.Name = "textBox9";
            textBox9.ReadOnly = true;
            textBox9.Size = new Size(117, 20);
            textBox9.TabIndex = 14;
            textBox9.TabStop = false;
            textBox9.Text = "Drawings found:";
            textBox9.TextAlign = HorizontalAlignment.Right;
            // 
            // button2
            // 
            button2.Location = new Point(5, 111);
            button2.Margin = new Padding(5, 5, 5, 5);
            button2.Name = "button2";
            button2.Size = new Size(250, 60);
            button2.TabIndex = 13;
            button2.Text = "Select the drawing list Excel file:";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // textBox1
            // 
            textBox1.BackColor = SystemColors.Menu;
            textBox1.Location = new Point(5, 180);
            textBox1.Margin = new Padding(5, 5, 5, 5);
            textBox1.Name = "textBox1";
            textBox1.ReadOnly = true;
            textBox1.Size = new Size(953, 27);
            textBox1.TabIndex = 12;
            textBox1.TabStop = false;
            // 
            // button1
            // 
            button1.Location = new Point(5, 7);
            button1.Margin = new Padding(5, 5, 5, 5);
            button1.Name = "button1";
            button1.Size = new Size(250, 60);
            button1.TabIndex = 11;
            button1.Text = "Select folder with RELEASED drawings:";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox3
            // 
            textBox3.BackColor = SystemColors.Menu;
            textBox3.Location = new Point(436, 25);
            textBox3.Margin = new Padding(5, 5, 5, 5);
            textBox3.Name = "textBox3";
            textBox3.ReadOnly = true;
            textBox3.Size = new Size(49, 27);
            textBox3.TabIndex = 8;
            textBox3.TabStop = false;
            textBox3.TextAlign = HorizontalAlignment.Center;
            // 
            // textBox2
            // 
            textBox2.BackColor = SystemColors.Menu;
            textBox2.Location = new Point(5, 77);
            textBox2.Margin = new Padding(5, 5, 5, 5);
            textBox2.Name = "textBox2";
            textBox2.ReadOnly = true;
            textBox2.Size = new Size(953, 27);
            textBox2.TabIndex = 9;
            textBox2.TabStop = false;
            // 
            // tabPage2
            // 
            tabPage2.BackColor = SystemColors.Control;
            tabPage2.Controls.Add(panel2);
            tabPage2.Controls.Add(panel1);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3, 3, 3, 3);
            tabPage2.Size = new Size(1273, 653);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Manage";
            // 
            // textBox13
            // 
            textBox13.BackColor = Color.SpringGreen;
            textBox13.BorderStyle = BorderStyle.None;
            textBox13.Cursor = Cursors.IBeam;
            textBox13.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox13.ForeColor = Color.Green;
            textBox13.Location = new Point(867, 29);
            textBox13.Margin = new Padding(5, 5, 5, 5);
            textBox13.Name = "textBox13";
            textBox13.ReadOnly = true;
            textBox13.Size = new Size(145, 23);
            textBox13.TabIndex = 20;
            textBox13.TabStop = false;
            textBox13.Text = "Only excel data";
            // 
            // textBox12
            // 
            textBox12.BackColor = SystemColors.Menu;
            textBox12.BorderStyle = BorderStyle.None;
            textBox12.Location = new Point(132, 13);
            textBox12.Margin = new Padding(5, 5, 5, 5);
            textBox12.Name = "textBox12";
            textBox12.ReadOnly = true;
            textBox12.Size = new Size(117, 20);
            textBox12.TabIndex = 19;
            textBox12.TabStop = false;
            textBox12.Text = "Staging found:";
            textBox12.TextAlign = HorizontalAlignment.Right;
            // 
            // textBox7
            // 
            textBox7.BackColor = Color.DeepSkyBlue;
            textBox7.BorderStyle = BorderStyle.None;
            textBox7.Cursor = Cursors.IBeam;
            textBox7.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox7.ForeColor = Color.Yellow;
            textBox7.Location = new Point(909, -2);
            textBox7.Margin = new Padding(5, 5, 5, 5);
            textBox7.Name = "textBox7";
            textBox7.ReadOnly = true;
            textBox7.Size = new Size(251, 23);
            textBox7.TabIndex = 18;
            textBox7.TabStop = false;
            textBox7.Text = "Warning (data mismatch)";
            // 
            // textBox8
            // 
            textBox8.BackColor = Color.Thistle;
            textBox8.BorderStyle = BorderStyle.None;
            textBox8.Cursor = Cursors.IBeam;
            textBox8.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox8.ForeColor = Color.DarkRed;
            textBox8.Location = new Point(593, 29);
            textBox8.Margin = new Padding(5, 5, 5, 5);
            textBox8.Name = "textBox8";
            textBox8.ReadOnly = true;
            textBox8.Size = new Size(265, 23);
            textBox8.TabIndex = 18;
            textBox8.TabStop = false;
            textBox8.Text = "Error (File or Excel missing)";
            // 
            // textBox10
            // 
            textBox10.BackColor = Color.LemonChiffon;
            textBox10.BorderStyle = BorderStyle.None;
            textBox10.Cursor = Cursors.IBeam;
            textBox10.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox10.ForeColor = Color.Green;
            textBox10.Location = new Point(678, -2);
            textBox10.Margin = new Padding(5, 5, 5, 5);
            textBox10.Name = "textBox10";
            textBox10.ReadOnly = true;
            textBox10.Size = new Size(223, 23);
            textBox10.TabIndex = 18;
            textBox10.TabStop = false;
            textBox10.Text = "Revision Pending";
            // 
            // textBox4
            // 
            textBox4.BackColor = Color.GreenYellow;
            textBox4.BorderStyle = BorderStyle.None;
            textBox4.Cursor = Cursors.IBeam;
            textBox4.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point);
            textBox4.ForeColor = Color.Green;
            textBox4.Location = new Point(593, -2);
            textBox4.Margin = new Padding(5, 5, 5, 5);
            textBox4.Name = "textBox4";
            textBox4.ReadOnly = true;
            textBox4.Size = new Size(77, 23);
            textBox4.TabIndex = 18;
            textBox4.TabStop = false;
            textBox4.Text = "All okay";
            // 
            // button6
            // 
            button6.Location = new Point(258, 5);
            button6.Margin = new Padding(5, 5, 5, 5);
            button6.Name = "button6";
            button6.Size = new Size(109, 35);
            button6.TabIndex = 16;
            button6.Text = "Consolidate";
            button6.UseVisualStyleBackColor = true;
            button6.Click += button6_Click;
            // 
            // button4
            // 
            button4.Location = new Point(10, 5);
            button4.Margin = new Padding(5, 5, 5, 5);
            button4.Name = "button4";
            button4.Size = new Size(109, 35);
            button4.TabIndex = 16;
            button4.Text = "(Re-) Scan";
            button4.UseVisualStyleBackColor = true;
            button4.Click += button4_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(button4);
            panel1.Controls.Add(textBox13);
            panel1.Controls.Add(button6);
            panel1.Controls.Add(textBox12);
            panel1.Controls.Add(textBox4);
            panel1.Controls.Add(textBox7);
            panel1.Controls.Add(textBox10);
            panel1.Controls.Add(textBox8);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(3, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1267, 63);
            panel1.TabIndex = 21;
            // 
            // panel2
            // 
            panel2.Controls.Add(dGV1);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(3, 66);
            panel2.Name = "panel2";
            panel2.Size = new Size(1267, 584);
            panel2.TabIndex = 22;
            // 
            // dGV1
            // 
            dGV1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dGV1.Dock = DockStyle.Fill;
            dGV1.Location = new Point(0, 0);
            dGV1.Margin = new Padding(5);
            dGV1.Name = "dGV1";
            dGV1.RowHeadersWidth = 72;
            dGV1.Size = new Size(1267, 584);
            dGV1.TabIndex = 18;
            // 
            // DrawingListManagerForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1281, 686);
            Controls.Add(tabControl1);
            Name = "DrawingListManagerForm";
            Text = "Drawing List Manager";
            FormClosing += DrawingListManagerForm_FormClosing;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dGV1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TextBox textBox9;
        private Button button2;
        private TextBox textBox1;
        private Button button1;
        private TextBox textBox3;
        private TextBox textBox2;
        private TabPage tabPage2;
        private Button button4;
        private TextBox textBox4;
        private TextBox textBox7;
        private TextBox textBox8;
        private TextBox textBox10;
        private Button button5;
        private TextBox textBox5;
        private TextBox textBox6;
        private TextBox textBox11;
        private TextBox textBox12;
        private Button button6;
        private TextBox textBox13;
        private Panel panel2;
        private DataGridView dGV1;
        private Panel panel1;
    }
}

