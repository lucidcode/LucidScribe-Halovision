using System;
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
using lucidcode.LucidScribe.Plugin.Halovision.VLC;
using System.IO.Pipes;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.TcpMessagingSystem;
using Microsoft.VisualBasic;
using Emgu.CV;
using Emgu.CV.Structure;

namespace lucidcode.LucidScribe.Plugin.Halovision
{
    public partial class VisionForm : Form
    {
        public int Value = 0;
        public int vREM = 0;
        private string m_strPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\lucidcode\\Lucid Scribe\\";

        [DllImport("avicap32.dll")]
        protected static extern bool capGetDriverDescriptionA(short wDriverIndex, [MarshalAs(UnmanagedType.VBByRefStr)]ref String lpszName, int cbName, [MarshalAs(UnmanagedType.VBByRefStr)] ref String lpszVer, int cbVer);

        [DllImport("gdi32.dll")]
        protected static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        public delegate void ReconnectHanlder();
        public event ReconnectHanlder Reconnect;

        private Boolean loaded = false;
        private int PixelThreshold = 20;
        private int PixelsInARow = 2;
        private int FrameThreshold = 960;
        private int IgnorePercentage = 16;
        private int Sensitivity = 5;
        private Boolean TCMP = false;
        private Boolean RecordVideo = false;
        private Boolean feedChanged = true;
        private VideoCaptureDevice videoSource;
        private Boolean DetectFace = false;
        private Rectangle lastFaceRegion;
        CascadeClassifier cascadeClassifier;

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
                cmbDevices.Items.Add("lucidcode Halovision Device");
                foreach (FilterInfo filterInfo in videoDevices)
                {
                    cmbDevices.Items.Add(filterInfo.Name);
                }

                LoadSettings();
                loadingDevices = false;

                int deviceId = 0;
                if (videoDevices.Count > 1)
                {
                    deviceId = 1;
                }

                if (cmbDevices.Text == "") cmbDevices.Text = videoDevices[deviceId].Name;

                cascadeClassifier = new CascadeClassifier(@"haarcascade.xml");

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

        private void video_NewFrame(object sender,
            NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pbDisplay.Image = bitmap;
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
        private NamedPipeServerStream myVideoPipe;
        private VlcInstance myVlcInstance;
        private IDuplexOutputChannel myVideoChannel;
        VlcMediaPlayer myPlayer;

        private void ConnectHalovisionDevice()
        {
            myVlcInstance = new VlcInstance("");

            // Use TCP messaging.
            // You can try to use UDP or WebSockets too.
            myVideoChannel = new TcpMessagingSystemFactory()
                //myVideoChannel = new UdpMessagingSystemFactory()
                // Note: Provide address of your service here.
                .CreateDuplexOutputChannel("tcp://" + txtDeviceIP.Text + ":8093/");
            myVideoChannel.ResponseMessageReceived += OnResponseMessageReceived;

            // Use unique name for the pipe.
            string aVideoPipeName = Guid.NewGuid().ToString();

            // Open pipe that will be read by VLC.
            myVideoPipe = new NamedPipeServerStream(@"\" + aVideoPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 32764);
            ManualResetEvent aVlcConnectedPipe = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(x =>
            {
                myVideoPipe.WaitForConnection();

                // Indicate VLC has connected the pipe.
                aVlcConnectedPipe.Set();
            });

            // VLC connects the pipe and starts playing.
            using (VlcMedia aMedia = new VlcMedia(myVlcInstance, @"stream://\\\.\pipe\" + aVideoPipeName))
            {
                // Setup VLC so that it can process raw h264 data (i.e. not in mp4 container)
                aMedia.AddOption(":demux=H264");

                myPlayer = new VlcMediaPlayer(aMedia);
                myPlayer.Drawable = pbDisplay.Handle; // VideoWindow.Child.Handle;

                // Note: This will connect the pipe and read the video.
                myPlayer.Play();
            }

            // Wait until VLC connects the pipe so that it is ready to receive the stream.
            if (!aVlcConnectedPipe.WaitOne(5000))
            {
                throw new TimeoutException("VLC did not open connection with the pipe.");
            }

            // Open connection with service running on Raspberry.
            myVideoChannel.OpenConnection();
        }

        public void DisconnectHalovisionHeadband()
        {
            try
            {
                myVideoChannel.CloseConnection();
                myVideoPipe.Close();
            }
            catch (Exception ex)
            {
            }
        }

        private void OnResponseMessageReceived(object sender, DuplexChannelMessageEventArgs e)
        {
            byte[] aVideoData = (byte[])e.Message;

            // Forward received data to the named pipe so that VLC can process it.
            myVideoPipe.Write(aVideoData, 0, aVideoData.Length);
        }

        Image previousImage = null;

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        public Bitmap CaptureControl(IntPtr handle, int width, int height)
        {
            Bitmap controlBmp;
            using (Graphics g1 = Graphics.FromHwnd(handle))
            {
                controlBmp = new Bitmap(width, height, g1);
                using (Graphics g2 = Graphics.FromImage(controlBmp))
                {
                    g2.CopyFromScreen(this.Location.X + 26, this.Location.Y + 71, 0, 0, pbDisplay.Size);

                    IntPtr dc1 = g1.GetHdc();
                    IntPtr dc2 = g2.GetHdc();

                    BitBlt(dc2, 0, 0, width, height, dc1, 0, 0, 13369376);
                    g1.ReleaseHdc(dc1);
                    g2.ReleaseHdc(dc2);
                }
            }

            return controlBmp;
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
            try
            {
                pbDisplay.Image = CaptureControl(pbDisplay.Handle, pbDisplay.Width, pbDisplay.Height);

                if (pbDisplay.Image != null)
                {
                    if (previousImage == null)
                    {
                        previousImage = pbDisplay.Image;
                    }

                    if (DetectFace)
                    {
                        using (Bitmap bitmap = new Bitmap(pbDisplay.Image))
                        {
                            Emgu.CV.Image<Bgr, byte> imageFrame = new Emgu.CV.Image<Bgr, byte>(bitmap);
                            Image<Gray, Byte> grayFrame = imageFrame.Convert<Gray, Byte>();
                            var detectedFaces = cascadeClassifier.DetectMultiScale(grayFrame);

                            foreach (var face in detectedFaces)
                            {
                                lastFaceRegion = face;
                            }
                        }
                    }

                    int diff = 0;
                    pbDifference.Image = Difference(previousImage, pbDisplay.Image, out diff);

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

                    previousImage = pbDisplay.Image;
                    Value = diff;

                    lblTime.Text = DateTime.Now.ToString("yyy-MM-dd hh:mm:ss - ") + Value;

                    bool boolDreaming = false;
                    if (cmbAlgorithm.Text == "Motion Detector")
                    {
                        if (Value >= FrameThreshold)
                        {
                            boolDreaming = true;
                        }
                    }
                    else if (cmbAlgorithm.Text == "REM Detector")
                    {
                        m_arrHistory.Add(Value);
                        if (m_arrHistory.Count > 128) { m_arrHistory.RemoveAt(0); }

                        int intBlinks = 0;
                        bool boolBlinking = false;

                        int intBelow = 0;
                        int intAbove = 0;

                        foreach (Double dblValue in m_arrHistory)
                        {
                            if (dblValue > FrameThreshold)
                            {
                                intAbove += 1;
                                intBelow = 0;
                            }
                            else
                            {
                                intBelow += 1;
                                intAbove = 0;
                            }

                            if (!boolBlinking)
                            {
                                if (intAbove >= 2)
                                {
                                    boolBlinking = true;
                                    intBlinks += 1;
                                    intAbove = 0;
                                    intBelow = 0;
                                }
                            }
                            else
                            {
                                if (intBelow >= 8)
                                {
                                    boolBlinking = false;
                                    intBlinks += 1;
                                    intBelow = 0;
                                    intAbove = 0;
                                }
                                else
                                {
                                    if (intAbove >= 12)
                                    {
                                        // reset
                                        boolBlinking = false;
                                        intBlinks = 0;
                                        intBelow = 0;
                                        intAbove = 0;
                                    }
                                }
                            }

                            if (intBlinks > 10)
                            {
                                boolDreaming = true;
                                break;
                            }

                            if (intAbove > 12)
                            { // reset
                                boolBlinking = false;
                                intBlinks = 0;
                                intBelow = 0;
                                intAbove = 0; ;
                            }
                            if (intBelow > 40)
                            { // reset
                                boolBlinking = false;
                                intBlinks = 0;
                                intBelow = 0;
                                intAbove = 0; ;
                            }
                        }
                    }

                    if (boolDreaming)
                    {
                        vREM = 888;
                    }
                    else
                    {
                        vREM = 0;
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
        List<int> m_arrHistory = new List<int>();

        Random random = new Random();
        String effect = "White";
        private Image Difference(Image image1, Image image2, out int diff)
        {
            Bitmap bitmap1 = new Bitmap(image1);
            BitmapData bmd1 = bitmap1.LockBits(new Rectangle(0, 0, image1.Width, image1.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap1.PixelFormat);

            Bitmap bitmap2 = new Bitmap(image2);
            BitmapData bmd2 = bitmap2.LockBits(new Rectangle(0, 0, image2.Width, image2.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap2.PixelFormat);

            int PixelSize = 4;

            if (image1.Width + image1.Height != image2.Width + image2.Height)
            {
                diff = 0;
                return image1;
            }

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

            diff = differences;

            double percentage = (Convert.ToDouble(changedPixels) / Convert.ToDouble(totalPixels)) * 100;
            if (percentage > IgnorePercentage)
            {
                diff = 0;
            }

            return bitmap2;
        }

        private void VisionForm_Resize(object sender, EventArgs e)
        {
            if (cmbDevices.Items.Count > 0)
            {
                cmbDevices.SelectedIndex = 0;
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
