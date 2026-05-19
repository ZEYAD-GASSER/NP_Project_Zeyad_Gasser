using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace NP_Project_Client_Zeyad_Gasser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();

            if (op.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = op.FileName;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtPath.Text == "")
            {
                MessageBox.Show("Choose a file first");
                return;
            }

            Thread t = new Thread(SendFile);
            t.IsBackground = true;
            t.Start();
        }

        void SendFile()
        {
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 5000);
                AddLog("Connected To Server");

                NetworkStream ns = client.GetStream();

                byte[] fileData = File.ReadAllBytes(txtPath.Text);

                byte[] fileSize = BitConverter.GetBytes((long)fileData.Length);
                ns.Write(fileSize, 0, fileSize.Length);

                ns.Write(fileData, 0, fileData.Length);

                AddLog("File Sent");

                byte[] sizeBytes = new byte[8];
                ns.Read(sizeBytes, 0, 8);

                long compressedSize = BitConverter.ToInt64(sizeBytes, 0);

                byte[] compressedData = new byte[compressedSize];

                int totalRead = 0;

                while (totalRead < compressedSize)
                {
                    int read = ns.Read(
                        compressedData,
                        totalRead,
                        (int)(compressedSize - totalRead));

                    totalRead += read;
                }

                AddLog("Compressed File Received");

                string newPath = txtPath.Text + ".gz";
                File.WriteAllBytes(newPath, compressedData);

                AddLog("File Saved");

                MessageBox.Show("Done");

                ns.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void AddLog(string msg)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    lstLogs.Items.Add(msg);
                }));
            }
            else
            {
                lstLogs.Items.Add(msg);
            }
        }

        private void txtPath_TextChanged(object sender, EventArgs e)
        {

        }

        private void lstLogs_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}