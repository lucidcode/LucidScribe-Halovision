﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Threading;
using AForge.Video.DirectShow;
using AForge.Video;
using System.IO.Pipes;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.TcpMessagingSystem;
using Microsoft.VisualBasic;
using Emgu.CV;
using Emgu.CV.Structure;
using LibVLCSharp.Shared;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    public partial class VisionForm : Form
    {
        public int Value = 0;
        public int vREM = 0;
        private string m_strPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\lucidcode\\Lucid Scribe\\";

        public delegate void ReconnectHanlder();
        public event ReconnectHanlder Reconnect;

        private bool loaded = false;
        private int PixelThreshold = 20;
        private int PixelsInARow = 2;
        private int FrameThreshold = 960;
        private int IgnorePercentage = 16;
        private int Sensitivity = 5;
        private int PixelSize = 4;
        private bool TCMP = false;
        private bool RecordVideo = false;
        private bool feedChanged = true;
        private VideoCaptureDevice videoSource;
        private bool DetectFace = false;
        private Rectangle lastFaceRegion;
        private CascadeClassifier cascadeClassifier;

        private bool processing = false;
        private Bitmap currentBitmap = null;
        private PictureBox pictureBoxCurrent = new PictureBox();
        private Bitmap previousBitmap = null;

        public VisionForm()
        {
            InitializeComponent();
        }

        Boolean loadingDevices = true;

        private void PortForm_Load(object sender, EventArgs e)
        {
            try
            {
                // enumerate video devices
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                cmbDevices.Items.Clear();
                foreach (FilterInfo filterInfo in videoDevices)
                {
                    cmbDevices.Items.Add(filterInfo.Name);
                }
                cmbDevices.Items.Add("lucidcode Halovision Device");

                LoadSettings();
                loadingDevices = false;

                int deviceId = 0;
                if (videoDevices.Count > 1)
                {
                    deviceId = 1;
                }

                if (cmbDevices.Text == "") cmbDevices.Text = videoDevices[deviceId].Name;

                cascadeClassifier = new CascadeClassifier("haarcascade.xml");

                if (cmbDevices.Text == "lucidcode Halovision Device")
                {
                    loaded = true;
                    ConnectHalovisionDevice();
                    return;
                }

                foreach (FilterInfo filterInfo in videoDevices)
                {
                    if (cmbDevices.Text == filterInfo.Name)
                    {
                        // create video source
                        videoSource = new VideoCaptureDevice(filterInfo.MonikerString);
                        // set NewFrame event handler
                        videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                        // start the video source
                        videoSource.Start();
                        loaded = true;
                        return;
                    }
                }

                loaded = true;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LucidScribe.InitializePlugin()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Disconnect()
        {
            if (cmbDevices.Text == "lucidcode Halovision Device")
            {
                DisconnectHalovisionHeadband();
            }
            else
            {
                videoSource.Stop();
            }
        }

        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap videoBitmap = (Bitmap)eventArgs.Frame.Clone();
            if (pbDisplay.Image != null)
            {
                pbDisplay.Image.Dispose();
            }
            pbDisplay.Image = videoBitmap;
        }

        private void LoadSettings()
        {
            XmlDocument xmlSettings = new XmlDocument();

            if (!File.Exists(m_strPath + "Plugins\\Halovision.User.lsd"))
            {
                String defaultSettings = "<LucidScribeData>";
                defaultSettings += "<Plugin>";
                defaultSettings += "<Algorithm>REM Detector</Algorithm>";
                defaultSettings += "<PixelThreshold>16</PixelThreshold>";
                defaultSettings += "<PixelsInARow>4</PixelsInARow>";
                defaultSettings += "<FrameThreshold>960</FrameThreshold>";
                defaultSettings += "<Sensitivity>0</Sensitivity>";
                defaultSettings += "<IgnorePercentage>16</IgnorePercentage>";
                defaultSettings += "<RecordVideo>0</RecordVideo>";
                defaultSettings += "<TCMP>0</TCMP>";
                defaultSettings += "<DetectFace>0</DetectFace>";
                defaultSettings += "</Plugin>";
                defaultSettings += "</LucidScribeData>";
                File.WriteAllText(m_strPath + "Plugins\\Halovision.User.lsd", defaultSettings);
            }

            xmlSettings.Load(m_strPath + "Plugins\\Halovision.User.lsd");

            cmbAlgorithm.Text = xmlSettings.DocumentElement.SelectSingleNode("//Algorithm").InnerText;
            cmbPixelThreshold.Text = xmlSettings.DocumentElement.SelectSingleNode("//PixelThreshold").InnerText;
            cmbPixelsInARow.Text = xmlSettings.DocumentElement.SelectSingleNode("//PixelsInARow").InnerText;
            cmbFrameThreshold.Text = xmlSettings.DocumentElement.SelectSingleNode("//FrameThreshold").InnerText;
            cmbSensitivity.Text = "1"; // xmlSettings.DocumentElement.SelectSingleNode("//Sensitivity").InnerText;

            if (xmlSettings.DocumentElement.SelectSingleNode("//IgnorePercentage") != null)
            {
                cmbIgnorePercentage.Text = xmlSettings.DocumentElement.SelectSingleNode("//IgnorePercentage").InnerText;
            }
            else
            {
                cmbIgnorePercentage.Text = "50";
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//Camera") != null)
            {
                cmbDevices.Text = xmlSettings.DocumentElement.SelectSingleNode("//Camera").InnerText;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//DeviceIP") != null)
            {
                txtDeviceIP.Text = xmlSettings.DocumentElement.SelectSingleNode("//DeviceIP").InnerText;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//RecordVideo") != null && xmlSettings.DocumentElement.SelectSingleNode("//RecordVideo").InnerText == "1")
            {
                chkRecordVideo.Checked = true;
                RecordVideo = true;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//DetectFace") != null && xmlSettings.DocumentElement.SelectSingleNode("//DetectFace").InnerText == "1")
            {
                chkDetectFace.Checked = true;
                DetectFace = true;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//TCMP") != null && xmlSettings.DocumentElement.SelectSingleNode("//TCMP").InnerText == "1")
            {
                chkTCMP.Checked = true;
                TCMP = true;
            }
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDevices.Text == "lucidcode Halovision Device")
            {
                txtDeviceIP.Visible = true;
                lblDeviceIP.Visible = true;
            }
            else
            {
                txtDeviceIP.Visible = false;
                lblDeviceIP.Visible = false;
            }

            if (loadingDevices) { return; }
            SaveSettings();
            if (!loaded) { return; }

            if (cmbDevices.SelectedText == "lucidcode Halovision Device")
            {
                ConnectHalovisionDevice();
                return;
            }
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in videoDevices)
            {
                if (cmbDevices.Text == filterInfo.Name)
                {
                    // create video source
                    videoSource = new VideoCaptureDevice(filterInfo.MonikerString);
                    // set NewFrame event handler
                    videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
                    // start the video source
                    videoSource.Start();
                    return;
                }
            }
            //}
        }

        // VLC will read video from the named pipe.
        private NamedPipeServerStream videoPipe;
        private IDuplexOutputChannel videoChannel;
        MediaPlayer player;
        LibVLC libvlc;

        private void ConnectHalovisionDevice()
        {
            try
            {
                Core.Initialize();
                libvlc = new LibVLC(enableDebugLogs: false, "--rtsp-tcp");

                // Use TCP messaging.
                videoChannel = new TcpMessagingSystemFactory().CreateDuplexOutputChannel("tcp://" + txtDeviceIP.Text + ":8093/");
                videoChannel.ResponseMessageReceived += OnResponseMessageReceived;

                // Use unique name for the pipe.
                string aVideoPipeName = Guid.NewGuid().ToString();

                // Open pipe that will be read by VLC.
                videoPipe = new NamedPipeServerStream(@"\" + aVideoPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 65528);
                ManualResetEvent aVlcConnectedPipe = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(x =>
                {
                    videoPipe.WaitForConnection();

                    // Indicate VLC has connected the pipe.
                    aVlcConnectedPipe.Set();
                });

                // VLC connects the pipe and starts playing.
                using (Media aMedia = new Media(libvlc, @"stream://\\\.\pipe\" + aVideoPipeName, FromType.FromLocation))
                {
                    // Setup VLC so that it can process raw h264 data
                    aMedia.AddOption(":demux=H264");

                    player = new MediaPlayer(aMedia);
                    player.Hwnd = pbDisplay.Handle;

                    // Note: This will connect the pipe and read the video.
                    player.Play();
                }

                // Wait until VLC connects the pipe so that it is ready to receive the stream.
                if (!aVlcConnectedPipe.WaitOne(5000))
                {
                    throw new TimeoutException($"VLC did not open connection with {txtDeviceIP.Text}.");
                }

                // Open connection with service running on Raspberry.
                videoChannel.OpenConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LucidScribe.InitializePlugin()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DisconnectHalovisionHeadband()
        {
            try
            {
                videoChannel.CloseConnection();
                videoPipe.Close();
            }
            catch (Exception ex)
            {
            }
        }

        private void OnResponseMessageReceived(object sender, DuplexChannelMessageEventArgs e)
        {
            byte[] aVideoData = (byte[])e.Message;

            // Forward received data to the named pipe so that VLC can process it.
            videoPipe.WriteAsync(aVideoData, 0, aVideoData.Length);
        }

        public Bitmap CaptureControl(int width, int height)
        {
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(this.Location.X + 26, this.Location.Y + 71, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            }
            return bmp;
        }

        private void CreateDirectories()
        {
            if (!Directory.Exists(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy")))
            {
                Directory.CreateDirectory(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy"));
            }

            if (!Directory.Exists(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM")))
            {
                Directory.CreateDirectory(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM"));
            }

            if (!Directory.Exists(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM") + "\\" + Strings.Format(DateTime.Now, "dd")))
            {
                Directory.CreateDirectory(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM") + "\\" + Strings.Format(DateTime.Now, "dd"));
            }

            if (!Directory.Exists(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM") + "\\" + Strings.Format(DateTime.Now, "dd") + "\\" + Strings.Format(DateTime.Now, "HH")))
            {
                Directory.CreateDirectory(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM") + "\\" + Strings.Format(DateTime.Now, "dd") + "\\" + Strings.Format(DateTime.Now, "HH"));
            }

            if (!Directory.Exists(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM") + "\\" + Strings.Format(DateTime.Now, "dd") + "\\" + Strings.Format(DateTime.Now, "HH") + "\\" + Strings.Format(DateTime.Now, "mm")))
            {
                Directory.CreateDirectory(m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM") + "\\" + Strings.Format(DateTime.Now, "dd") + "\\" + Strings.Format(DateTime.Now, "HH") + "\\" + Strings.Format(DateTime.Now, "mm"));
            }
        }
        private void tmrDiff_Tick(object sender, EventArgs e)
        {
            if (processing)
            {
                return;
            }

            processing = true;

            try
            {
                int diff = 0;
                currentBitmap = CaptureControl(pbDisplay.Width, pbDisplay.Height);

                if (DetectFace && currentBitmap != null)
                {
                    Image<Bgr, byte> imageFrame = currentBitmap.ToImage<Bgr, byte>(); ;
                    Image<Gray, byte> grayFrame = imageFrame.Convert<Gray, byte>();
                    var detectedFaces = cascadeClassifier.DetectMultiScale(grayFrame);

                    if (detectedFaces.Length > 0)
                    {
                        lastFaceRegion = detectedFaces[0];
                    }
                }

                Difference(ref previousBitmap, ref currentBitmap, out diff);

                if (pbDifference.Image != null)
                {
                    pbDifference.Image.Dispose();
                }
                pbDifference.Image = currentBitmap;

                previousBitmap = CaptureControl(pbDisplay.Width, pbDisplay.Height);

                if (pictureBoxCurrent.Image != null)
                {
                    pictureBoxCurrent.Image.Dispose();
                }
                pictureBoxCurrent.Image = previousBitmap;

                if (RecordVideo)
                {
                    if (feedChanged | diff > 0)
                    {
                        CreateDirectories();
                        String secondFile = m_strPath + "Days\\" + Strings.Format(DateTime.Now, "yyyy") + "\\" + Strings.Format(DateTime.Now, "MM") + "\\" + Strings.Format(DateTime.Now, "dd") + "\\" + Strings.Format(DateTime.Now, "HH") + "\\" + Strings.Format(DateTime.Now, "mm") + "\\" + Strings.Format(DateTime.Now, "ss.") + DateTime.Now.Millisecond + ".png";
                        pbDifference.Image.Save(secondFile, System.Drawing.Imaging.ImageFormat.Png);
                        if (diff == 0)
                        {
                            feedChanged = false;
                        }
                    }
                }

                if (diff > 0)
                {
                    feedChanged = true;
                }

                Value = diff;

                lblTime.Text = DateTime.Now.ToString("yyy-MM-dd hh:mm:ss - ") + Value;
            }
            catch (Exception ex)
            {
            }
            processing = false;
        }
        List<int> m_arrHistory = new List<int>();

        Random random = new Random();
        String effect = "White";
        private void Difference(ref Bitmap bitmap1, ref Bitmap bitmap2, out int diff)
        {
            diff = 0;
            if (bitmap1 == null) return;
            if (bitmap2 == null) return;

            if (bitmap1.Width + bitmap1.Height != bitmap2.Width + bitmap2.Height)
            {
                return;
            }

            BitmapData bmd1 = bitmap1.LockBits(new Rectangle(0, 0, bitmap1.Width, bitmap1.Height), ImageLockMode.ReadOnly, bitmap1.PixelFormat);
            BitmapData bmd2 = bitmap2.LockBits(new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), ImageLockMode.ReadOnly, bitmap2.PixelFormat);

            int differences = 0;
            int size = bmd2.Height * bmd2.Width;

            int totalPixels = 0;
            int changedPixels = 0;

            int yStart = 0;
            int yEnd = bmd2.Height;
            int xStart = 0;
            int xEnd = bmd2.Width;

            if (DetectFace && lastFaceRegion != null)
            {
                yStart = lastFaceRegion.Y;
                yEnd = lastFaceRegion.Y + lastFaceRegion.Height;
                xStart = lastFaceRegion.X;
                xEnd = lastFaceRegion.X + lastFaceRegion.Width;
            }

            unsafe
            {

                for (int y = yStart; y < yEnd; y++)
                {
                    byte* row1 = (byte*)bmd1.Scan0 + (y * bmd1.Stride);
                    byte* row2 = (byte*)bmd2.Scan0 + (y * bmd2.Stride);

                    int rowDifferences = 0;

                    for (int x = xStart; x < xEnd; x++)
                    {
                        int totalpixelDiff = 0;

                        int pixelDiff = row1[x * PixelSize] - row2[x * PixelSize];
                        if (pixelDiff < 0) { pixelDiff *= -1; }
                        totalpixelDiff = pixelDiff;

                        pixelDiff = row1[x * PixelSize + 1] - row2[x * PixelSize + 1];
                        if (pixelDiff < 0) { pixelDiff *= -1; }
                        totalpixelDiff += pixelDiff;

                        pixelDiff = row1[x * PixelSize + 2] - row2[x * PixelSize + 2];
                        if (pixelDiff < 0) { pixelDiff *= -1; }
                        totalpixelDiff += pixelDiff;

                        if (pixelDiff >= PixelThreshold)
                        {
                            rowDifferences++;
                            changedPixels++;
                        }
                        else
                        {
                            rowDifferences = 0;
                        }

                        totalPixels++;
                        if (rowDifferences >= PixelsInARow)
                        {
                            differences += (Sensitivity);
                            int r = row2[x * PixelSize + 2];
                            int g = row2[x * PixelSize + 1];
                            int b = row2[x * PixelSize];

                            if (effect == "Psychedelic")
                            {
                                int ran = random.Next(1, 4);
                                if (ran == 1)
                                {
                                    r = r + (totalpixelDiff * 2);
                                }
                                else if (ran == 2)
                                {
                                    g = g + (totalpixelDiff * 2);
                                }
                                else if (ran == 3)
                                {
                                    b = b + (totalpixelDiff * 2);
                                }
                                else
                                {
                                    r = r + (totalpixelDiff * 2);
                                    g = g + (totalpixelDiff * 2);
                                    b = b + (totalpixelDiff * 2);
                                }
                            }
                            else
                            {
                                r = r + (totalpixelDiff * 2);
                                g = g + (totalpixelDiff * 2);
                                b = b + (totalpixelDiff * 2);
                            }

                            if (r > 255) r = 255;
                            if (g > 255) g = 255;
                            if (b > 255) b = 255;

                            row2[x * PixelSize + 2] = (byte)r;
                            row2[x * PixelSize + 1] = (byte)g;
                            row2[x * PixelSize] = (byte)b;
                        }
                    }
                }

                if (DetectFace && lastFaceRegion != null)
                {
                    byte* row2 = (byte*)bmd2.Scan0 + (lastFaceRegion.Y * bmd2.Stride);
                    for (int x = lastFaceRegion.X; x <= lastFaceRegion.X + lastFaceRegion.Width; x++)
                    {
                        row2[x * PixelSize + 2] = (byte)255;
                        row2[x * PixelSize + 1] = (byte)255;
                        row2[x * PixelSize] = (byte)225;
                    }
                    row2 = (byte*)bmd2.Scan0 + ((lastFaceRegion.Y + lastFaceRegion.Height) * bmd2.Stride);
                    for (int x = lastFaceRegion.X; x <= lastFaceRegion.X + lastFaceRegion.Width; x++)
                    {
                        row2[x * PixelSize + 2] = (byte)255;
                        row2[x * PixelSize + 1] = (byte)255;
                        row2[x * PixelSize] = (byte)225;
                    }

                    for (int y = lastFaceRegion.Y; y <= lastFaceRegion.Y + lastFaceRegion.Height; y++)
                    {
                        row2 = (byte*)bmd2.Scan0 + ((y) * bmd2.Stride);
                        row2[lastFaceRegion.X * PixelSize + 2] = (byte)255;
                        row2[lastFaceRegion.X * PixelSize + 1] = (byte)255;
                        row2[lastFaceRegion.X * PixelSize] = (byte)225;

                        row2[(lastFaceRegion.X + lastFaceRegion.Width) * PixelSize + 2] = (byte)255;
                        row2[(lastFaceRegion.X + lastFaceRegion.Width) * PixelSize + 1] = (byte)255;
                        row2[(lastFaceRegion.X + lastFaceRegion.Width) * PixelSize] = (byte)225;
                    }
                }
            }

            bitmap1.UnlockBits(bmd1);
            bitmap2.UnlockBits(bmd2);

            bmd1 = null;
            bmd2 = null;

            diff = differences;

            double percentage = (Convert.ToDouble(changedPixels) / Convert.ToDouble(totalPixels)) * 100;
            if (percentage > IgnorePercentage)
            {
                diff = 0;
            }
        }

        private void cmbAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }
            SaveSettings();
        }

        private void cmbPixelThreshold_SelectedIndexChanged(object sender, EventArgs e)
        {
            PixelThreshold = Convert.ToInt32(cmbPixelThreshold.Text);

            if (!loaded) { return; }
            SaveSettings();
        }

        private void cmbPixelsInARow_SelectedIndexChanged(object sender, EventArgs e)
        {
            PixelsInARow = Convert.ToInt32(cmbPixelsInARow.Text);

            if (!loaded) { return; }
            SaveSettings();
        }

        private void cmbFrameThreshold_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }

            FrameThreshold = Convert.ToInt32(cmbFrameThreshold.Text);
            SaveSettings();
        }

        private void cmbSensitivity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }

            Sensitivity = Convert.ToInt32(cmbSensitivity.Text);
            SaveSettings();
        }

        private void SaveSettings()
        {
            String defaultSettings = "<LucidScribeData>";
            defaultSettings += "<Plugin>";
            defaultSettings += "<Algorithm>" + cmbAlgorithm.Text + "</Algorithm>";
            defaultSettings += "<Camera>" + cmbDevices.Text + "</Camera>";
            defaultSettings += "<DeviceIP>" + txtDeviceIP.Text + "</DeviceIP>";
            defaultSettings += "<PixelThreshold>" + cmbPixelThreshold.Text + "</PixelThreshold>";
            defaultSettings += "<PixelsInARow>" + cmbPixelsInARow.Text + "</PixelsInARow>";
            defaultSettings += "<FrameThreshold>" + cmbFrameThreshold.Text + "</FrameThreshold>";
            defaultSettings += "<Sensitivity>" + cmbSensitivity.Text + "</Sensitivity>";
            defaultSettings += "<IgnorePercentage>" + cmbIgnorePercentage.Text + "</IgnorePercentage>";

            if (chkTCMP.Checked)
            {
                defaultSettings += "<TCMP>1</TCMP>";
            }
            else
            {
                defaultSettings += "<TCMP>0</TCMP>";
            }

            if (chkRecordVideo.Checked)
            {
                defaultSettings += "<RecordVideo>1</RecordVideo>";
            }
            else
            {
                defaultSettings += "<RecordVideo>0</RecordVideo>";
            }

            if (chkDetectFace.Checked)
            {
                defaultSettings += "<DetectFace>1</DetectFace>";
            }
            else
            {
                defaultSettings += "<DetectFace>0</DetectFace>";
            }

            defaultSettings += "</Plugin>";
            defaultSettings += "</LucidScribeData>";
            File.WriteAllText(m_strPath + "Plugins\\Halovision.User.lsd", defaultSettings);
        }

        private void mnuReconnectCamera_Click(object sender, EventArgs e)
        {
            if (Reconnect != null)
            {
                Reconnect();
            }
        }

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            if (Reconnect != null)
            {
                Reconnect();
            }
        }

        private void VisionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (videoSource != null)
            {
                videoSource.Stop();
            }
        }

        private void chkTCMP_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }

            TCMP = chkTCMP.Checked;
            SaveSettings();
        }

        private void txtDeviceIP_TextChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }
            SaveSettings();
        }

        private void chkRecordVideo_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }

            RecordVideo = chkRecordVideo.Checked;
            SaveSettings();
        }

        private void cmbIgnorePercentage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }

            IgnorePercentage = Convert.ToInt32(cmbIgnorePercentage.Text);
            SaveSettings();
        }

        private void chkDetectFace_CheckedChanged(object sender, EventArgs e)
        {
            if (!loaded) { return; }

            DetectFace = chkDetectFace.Checked;
            SaveSettings();
        }
    }
}