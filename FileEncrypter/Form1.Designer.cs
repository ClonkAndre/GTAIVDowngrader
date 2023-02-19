namespace FileEncrypter {
    partial class Form1 {
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
            if (disposing && (components != null)) {
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
            this.EncryptFile = new System.Windows.Forms.Button();
            this.DecryptFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // EncryptFile
            // 
            this.EncryptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EncryptFile.Location = new System.Drawing.Point(12, 17);
            this.EncryptFile.Name = "EncryptFile";
            this.EncryptFile.Size = new System.Drawing.Size(381, 54);
            this.EncryptFile.TabIndex = 0;
            this.EncryptFile.Text = "Encrypt file";
            this.EncryptFile.UseVisualStyleBackColor = true;
            this.EncryptFile.Click += new System.EventHandler(this.EncryptFile_Click);
            // 
            // DecryptFile
            // 
            this.DecryptFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.DecryptFile.Location = new System.Drawing.Point(12, 78);
            this.DecryptFile.Name = "DecryptFile";
            this.DecryptFile.Size = new System.Drawing.Size(381, 54);
            this.DecryptFile.TabIndex = 1;
            this.DecryptFile.Text = "Decrypt file";
            this.DecryptFile.UseVisualStyleBackColor = true;
            this.DecryptFile.Click += new System.EventHandler(this.DecryptFile_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(405, 145);
            this.Controls.Add(this.DecryptFile);
            this.Controls.Add(this.EncryptFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "File Encrypter for the GTA IV Downgrader";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button EncryptFile;
        private System.Windows.Forms.Button DecryptFile;
    }
}

