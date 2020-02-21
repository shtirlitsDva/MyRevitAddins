namespace MEPUtils.SupportTools
{
    partial class SupportTools
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
            this.radioBox1 = new Shared.RadioBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.button1 = new System.Windows.Forms.Button();
            this.radioBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioBox1
            // 
            this.radioBox1.Controls.Add(this.radioButton2);
            this.radioBox1.Controls.Add(this.radioButton1);
            this.radioBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.radioBox1.Location = new System.Drawing.Point(0, 0);
            this.radioBox1.Name = "radioBox1";
            this.radioBox1.Size = new System.Drawing.Size(357, 75);
            this.radioBox1.TabIndex = 0;
            this.radioBox1.TabStop = false;
            this.radioBox1.Text = "Choose function";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(12, 43);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(210, 17);
            this.radioButton2.TabIndex = 0;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Calculate height by steel framing above";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(13, 20);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(140, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Calculate height by level";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Dock = System.Windows.Forms.DockStyle.Right;
            this.button1.Location = new System.Drawing.Point(363, 0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 75);
            this.button1.TabIndex = 1;
            this.button1.Text = "GO!";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // SupportTools
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(508, 75);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.radioBox1);
            this.Name = "SupportTools";
            this.Text = "SupportTools";
            this.Load += new System.EventHandler(this.SupportTools_Load);
            this.radioBox1.ResumeLayout(false);
            this.radioBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Shared.RadioBox radioBox1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.Button button1;
    }
}