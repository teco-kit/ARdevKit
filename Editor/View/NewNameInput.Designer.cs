namespace ARdevKit.View
{
    partial class NewNameInput
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
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.OK_bttn = new System.Windows.Forms.Button();
            this.Cancel_bttn = new System.Windows.Forms.Button();
            this.Name_lbl = new System.Windows.Forms.Label();
            this.NewName_lbl = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // TextBox1
            // 
            this.TextBox1.Location = new System.Drawing.Point(16, 62);
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.Size = new System.Drawing.Size(346, 20);
            this.TextBox1.TabIndex = 0;
            this.TextBox1.Text = "Neuer Dateiname";
            this.TextBox1.Enter += new System.EventHandler(this.TextBox1_Enter);
            // 
            // OK_bttn
            // 
            this.OK_bttn.Location = new System.Drawing.Point(287, 88);
            this.OK_bttn.Name = "OK_bttn";
            this.OK_bttn.Size = new System.Drawing.Size(75, 23);
            this.OK_bttn.TabIndex = 1;
            this.OK_bttn.Text = "OK";
            this.OK_bttn.UseVisualStyleBackColor = true;
            this.OK_bttn.Click += new System.EventHandler(this.OK_bttn_Click);
            // 
            // Cancel_bttn
            // 
            this.Cancel_bttn.Location = new System.Drawing.Point(206, 88);
            this.Cancel_bttn.Name = "Cancel_bttn";
            this.Cancel_bttn.Size = new System.Drawing.Size(75, 23);
            this.Cancel_bttn.TabIndex = 1;
            this.Cancel_bttn.Text = "Abbrechen";
            this.Cancel_bttn.UseVisualStyleBackColor = true;
            this.Cancel_bttn.Click += new System.EventHandler(this.Cancel_bttn_Click);
            // 
            // Name_lbl
            // 
            this.Name_lbl.AutoSize = true;
            this.Name_lbl.Location = new System.Drawing.Point(13, 13);
            this.Name_lbl.Name = "Name_lbl";
            this.Name_lbl.Size = new System.Drawing.Size(360, 13);
            this.Name_lbl.TabIndex = 2;
            this.Name_lbl.Text = "Der Name der Datei ist identisch mit einem einer schon verwendeten Datei.";
            // 
            // NewName_lbl
            // 
            this.NewName_lbl.AutoSize = true;
            this.NewName_lbl.Location = new System.Drawing.Point(13, 36);
            this.NewName_lbl.Name = "NewName_lbl";
            this.NewName_lbl.Size = new System.Drawing.Size(258, 13);
            this.NewName_lbl.TabIndex = 3;
            this.NewName_lbl.Text = "Bitte geben Sie einen neuen Namen für die Datei ein.";
            // 
            // NewNameInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 116);
            this.Controls.Add(this.NewName_lbl);
            this.Controls.Add(this.Name_lbl);
            this.Controls.Add(this.Cancel_bttn);
            this.Controls.Add(this.OK_bttn);
            this.Controls.Add(this.TextBox1);
            this.Name = "NewNameInput";
            this.Text = "NewNameInput";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public System.Windows.Forms.TextBox TextBox1;
        private System.Windows.Forms.Button OK_bttn;
        private System.Windows.Forms.Button Cancel_bttn;
        private System.Windows.Forms.Label Name_lbl;
        private System.Windows.Forms.Label NewName_lbl;
    }
}