using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace SharpUpdate
{
    /// <summary>
    /// Form that download the update
    /// </summary>
    internal partial class SharpUpdateDownloadForm : Form
    {
        /// <summary>
        /// The web client to download the update
        /// </summary>
        private WebClient webClient;

        /// <summary>
        /// The thread to control each download
        /// </summary>
        private BackgroundWorker bgWorker;

        /// <summary>
        /// Flag to control the download turn
        private AutoResetEvent downloadComplete;

        /// <summary>
        /// A temp file name to download to
        /// </summary>
        private List<Uri> locations;

        /// <summary>
        /// A temp file name to download to
        /// </summary>
        private List<string> tempFiles;

        /// <summary>
        /// A temp folder name to download to
        /// </summary>
        private string tempFolder;

        /// <summary>
        /// The MD5 hash of the file to download
        /// </summary>
        private List<string> md5;

        /// <summary>
        /// Gets the temp file path for the downloaded files
        /// </summary>
        internal List<string> TempFilesPath
        {
            get { return this.tempFiles; }
        }

        /// <summary>
        /// Gets the temp folder for the downloaded files
        /// </summary>
        internal string TempFolder
        {
            get { return this.tempFolder; }
        }


        /// <summary>
        /// Creates a new SharpUpdateDownloadForm
        /// </summary>
        internal SharpUpdateDownloadForm(List<Uri> locations, List<string> md5, Icon programIcon)
        {
            InitializeComponent();

            if (programIcon != null)
                this.Icon = programIcon;

            this.locations = locations;
            this.tempFiles = new List<string>();
            this.md5 = md5;
            this.tempFolder = Guid.NewGuid().ToString();

            // Set up backgroundworker to hash file
            bgWorker = new BackgroundWorker();
            bgWorker.DoWork += new DoWorkEventHandler(bgWorkerDownload_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorkerDownload_RunWorkerCompleted);

            bgWorker.RunWorkerAsync(new List<string>[] { this.tempFiles, this.md5 });
        }

        private void bgWorkerDownload_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                foreach (Uri location in locations)
                {
                    downloadComplete = new AutoResetEvent(false);

                    // Set up WebClient to download file
                    webClient = new WebClient();
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(webClient_DownloadFileCompleted);

                    // Set the temp file name and create new 0-byte file
                    string tempFile = Path.GetTempFileName();
                    tempFiles.Add(tempFile);
                    webClient.DownloadFileAsync(location, tempFile);

                    // Wait until download is complete
                    downloadComplete.WaitOne();
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
                this.DialogResult = DialogResult.No; this.Close();
            }

            // Check Hashes
            int i = 0;
            foreach (string tempFile in tempFiles)
            {
                if (Hasher.HashFile(tempFile, HashType.MD5).ToUpper() != md5[i].ToUpper()) { 
                    e.Result = DialogResult.No; this.Close();
                }
                else
                    e.Result = DialogResult.OK;
                i++;
            }
            i = 0;
        }

        private void bgWorkerDownload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.DialogResult = (DialogResult)e.Result;
            this.Close();
        }

        /// <summary>
        /// Downloads file from server
        /// </summary>
        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Update progressbar on download
            lblProgress.Invoke((MethodInvoker)delegate {
                lblProgress.Text = String.Format("Descargados {0} de {1}", FormatBytes(e.BytesReceived, 1, true), FormatBytes(e.TotalBytesToReceive, 1, true));
            });
            progressBar.Invoke((MethodInvoker)delegate {
                progressBar.Value = e.ProgressPercentage;
            });
        }

        private void webClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.DialogResult = DialogResult.No;
                this.Close();
            }
            else if (e.Cancelled)
            {
                this.DialogResult = DialogResult.Abort;
                this.Close();
            }
            else
            {
                downloadComplete.Set();
            }
        }

        /// <summary>
        /// Formats the byte count to closest byte type
        /// </summary>
        /// <param name="bytes">The amount of bytes</param>
        /// <param name="decimalPlaces">How many decimal places to show</param>
        /// <param name="showByteType">Add the byte type on the end of the string</param>
        /// <returns>The bytes formatted as specified</returns>
        private string FormatBytes(long bytes, int decimalPlaces, bool showByteType)
        {
            double newBytes = bytes;
            string formatString = "{0";
            string byteType = "B";

            // Check if best size in KB
            if (newBytes > 1024 && newBytes < 1048576)
            {
                newBytes /= 1024;
                byteType = "KB";
            }
            else if (newBytes > 1048576 && newBytes < 1073741824)
            {
                // Check if best size in MB
                newBytes /= 1048576;
                byteType = "MB";
            }
            else
            {
                // Best size in GB
                newBytes /= 1073741824;
                byteType = "GB";
            }

            // Show decimals
            if (decimalPlaces > 0)
                formatString += ":0.";

            // Add decimals
            for (int i = 0; i < decimalPlaces; i++)
                formatString += "0";

            // Close placeholder
            formatString += "}";

            // Add byte type
            if (showByteType)
                formatString += byteType;

            return String.Format(formatString, newBytes);
        }

        private void SharpUpdateDownloadForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (webClient.IsBusy)
            {
                webClient.CancelAsync();
                this.DialogResult = DialogResult.Abort;
            }

            if (bgWorker.IsBusy)
            {
                bgWorker.CancelAsync();
                this.DialogResult = DialogResult.Abort;
            }
        }
    }
}