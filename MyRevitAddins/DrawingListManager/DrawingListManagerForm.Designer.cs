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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 15);
            this.button1.Margin = new System.Windows.Forms.Padding(6);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(343, 72);
            this.button1.TabIndex = 4;
            this.button1.Text = "Select the folder with drawings:";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(15, 139);
            this.button2.Margin = new System.Windows.Forms.Padding(6);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(343, 72);
            this.button2.TabIndex = 6;
            this.button2.Text = "Select the drawing list Excel file:";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MEPUtils.DrawingListManager.Properties.Settings.Default, "PathToDwgList", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox1.Location = new System.Drawing.Point(15, 222);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(1310, 29);
            this.textBox1.TabIndex = 5;
            this.textBox1.TabStop = false;
            this.textBox1.Text = global::MEPUtils.DrawingListManager.Properties.Settings.Default.PathToDwgList;
            // 
            // textBox2
            // 
            this.textBox2.BackColor = System.Drawing.SystemColors.Menu;
            this.textBox2.DataBindings.Add(new System.Windows.Forms.Binding("Text", global::MEPUtils.DrawingListManager.Properties.Settings.Default, "PathToDwgFolder", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox2.Location = new System.Drawing.Point(15, 98);
            this.textBox2.Margin = new System.Windows.Forms.Padding(6);
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(1310, 29);
            this.textBox2.TabIndex = 3;
            this.textBox2.TabStop = false;
            this.textBox2.Text = global::MEPUtils.DrawingListManager.Properties.Settings.Default.PathToDwgFolder;
            // 
            // DrawingListManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1340, 741);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox2);
            this.Name = "DrawingListManagerForm";
            this.Text = "Drawing List Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DrawingListManagerForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox textBox1;
    }
}

