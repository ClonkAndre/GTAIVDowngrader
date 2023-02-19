using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace FileEncrypter {
    public partial class Form1 : Form {

        #region Constructor
        public Form1()
        {
            InitializeComponent();
        }
        #endregion

        private void EncryptFile_Click(object sender, EventArgs e)
        {
            try {
                using (OpenFileDialog ofd = new OpenFileDialog()) {
                    if (ofd.ShowDialog() == DialogResult.OK) {

                        string fileName = Path.GetFileName(ofd.FileName);
                        switch (MessageBox.Show(string.Format("Encrypt file {0}?", fileName), "Confirm Encryption", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
                            case DialogResult.Yes:

                                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read)) {
                                    byte[] arr = Helper.GetByteArray(fs);
                                    File.WriteAllBytes(string.Format(".\\Output\\{0}", fileName), Helper.DataCompression.CompressByteArray(arr, CompressionLevel.Fastest));
                                }

                                GC.Collect();
                                MessageBox.Show("File should be encrypted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                        }

                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Encrypt error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void DecryptFile_Click(object sender, EventArgs e)
        {
            try {
                using (OpenFileDialog ofd = new OpenFileDialog()) {
                    if (ofd.ShowDialog() == DialogResult.OK) {

                        string fileName = Path.GetFileName(ofd.FileName);
                        switch (MessageBox.Show(string.Format("Decrypt file {0}?", fileName), "Confirm Decryption", MessageBoxButtons.YesNo, MessageBoxIcon.Question)) {
                            case DialogResult.Yes:

                                using (FileStream fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read)) {
                                    byte[] arr = Helper.GetByteArray(fs);
                                    File.WriteAllBytes(string.Format(".\\Decrypted\\{0}", fileName), Helper.DataCompression.DecompressByteArray(arr));
                                }

                                GC.Collect();
                                MessageBox.Show("File should be decrypted!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                break;
                        }

                    }
                }
            }
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Decrypt error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
