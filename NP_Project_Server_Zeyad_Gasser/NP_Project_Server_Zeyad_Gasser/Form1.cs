using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace NP_Project_Server_Zeyad_Gasser
{
    public partial class Form1 : Form
    {
        TcpListener server;
        private void label1_Click(object sender, EventArgs e)
        {

        }
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            server = new TcpListener(IPAddress.Any, 5000);
            server.Start();

            AddLog("Server Started...");

            Thread t = new Thread(StartServer);
            t.IsBackground = true;
            t.Start();
        }

        void StartServer()
        {
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                AddLog("Client Connected");

                Thread t = new Thread(() => HandleClient(client));
                t.IsBackground = true;
                t.Start();
            }
        }

        void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream ns = client.GetStream();

                byte[] sizeBytes = new byte[8];
                ReadExact(ns, sizeBytes, 8);

                long fileSize = BitConverter.ToInt64(sizeBytes, 0);

                AddLog("Receiving File...");


                byte[] fileData = new byte[fileSize];
                ReadExact(ns, fileData, fileSize);

                AddLog("File Received");


                byte[] compressedData;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, true))
                    {
                        gzip.Write(fileData, 0, fileData.Length);
                    }

                    compressedData = ms.ToArray();
                }

                AddLog("File Compressed");


                byte[] compressedSize =
                    BitConverter.GetBytes((long)compressedData.Length);

                ns.Write(compressedSize, 0, compressedSize.Length);


                ns.Write(compressedData, 0, compressedData.Length);

                AddLog("Compressed File Sent");

                ns.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                AddLog("Error: " + ex.Message);
            }
        }


        void ReadExact(NetworkStream ns, byte[] buffer, long size)
        {
            int totalRead = 0;

            while (totalRead < size)
            {
                int read = ns.Read(buffer, totalRead, (int)(size - totalRead));

                if (read == 0)
                    throw new Exception("Client disconnected أثناء الإرسال");

                totalRead += read;
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
    }

}
