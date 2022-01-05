using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
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
        private string m_strPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\lucidcode\\Lucid Scribe\\";

        public delegate void ReconnectHanlder();
        public event ReconnectHanlder Reconnect;

        public delegate void ValueChangedHandler(int value);
        public event ValueChangedHandler ValueChanged;

        private bool loaded = false;
        private int PixelThreshold = 20;
        private int PixelsInARow = 2;
        private int FrameThreshold = 960;
        private int IgnorePercentage = 16;
        private int Sensitivity = 5;

        public int TossThreshold = 800;
        public int TossHalfLife = 10;
        public int TossValue = 0;

        public int EyeMoveMin = 4;
        public int EyeMoveMax = 200;

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
            tmrDiff.Enabled = false;
            processing = false;

            if (cmbDevices.Text == "lucidcode Halovision Device")
            {
                DisconnectHalovisionDevice();
            }
            else
            {
                Thread.Sleep(256);
                Application.DoEvents();
                videoSource.NewFrame -= new NewFrameEventHandler(video_NewFrame);
                videoSource.SignalToStop();
                videoSource = null;
            }
        }

        Bitmap videoBitmap;
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    if (pbDisplay.Image != null)
                    {
                        pbDisplay.Image.Dispose();
                    }
                    videoBitmap = (Bitmap)eventArgs.Frame.Clone();
                    pbDisplay.Image = videoBitmap;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                defaultSettings += "<TossThreshold>800</TossThreshold>";
                defaultSettings += "<TossHalfLife>10</TossHalfLife>";
                defaultSettings += "<EyeMoveMin>4</EyeMoveMin>";
                defaultSettings += "<EyeMoveMax>200</EyeMoveMax>";
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

            if (xmlSettings.DocumentElement.SelectSingleNode("//DeviceURL") != null)
            {
                txtDeviceURL.Text = xmlSettings.DocumentElement.SelectSingleNode("//DeviceURL").InnerText;
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

            if (xmlSettings.DocumentElement.SelectSingleNode("//TopMost") != null && xmlSettings.DocumentElement.SelectSingleNode("//TopMost").InnerText == "0")
            {
                chkTopMost.Checked = false;
                TopMost = false;
            } else
            {
                chkTopMost.Checked = true;
                TopMost = true;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//TossThreshold") != null)
            {
                tossThresholdInput.Value = Convert.ToDecimal(xmlSettings.DocumentElement.SelectSingleNode("//TossThreshold").InnerText);
            }
            else
            {
                tossThresholdInput.Value = TossThreshold;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//TossHalfLife") != null)
            {
                tossHalfLifeInput.Value = Convert.ToDecimal(xmlSettings.DocumentElement.SelectSingleNode("//TossHalfLife").InnerText);
            } 
            else
            {
                tossHalfLifeInput.Value = TossHalfLife;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//EyeMoveMin") != null)
            {
                eyeMoveMinInput.Value = Convert.ToDecimal(xmlSettings.DocumentElement.SelectSingleNode("//EyeMoveMin").InnerText);
            } 
            else
            {
                eyeMoveMinInput.Value = EyeMoveMin;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//EyeMoveMax") != null)
            {
                eyeMoveMaxInput.Value = Convert.ToDecimal(xmlSettings.DocumentElement.SelectSingleNode("//EyeMoveMax").InnerText);
            } 
            else
            {
                eyeMoveMaxInput.Value = EyeMoveMax;
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
                txtDeviceURL.Enabled = true;
                lblDeviceURL.Enabled = true;
            }
            else
            {
                txtDeviceURL.Enabled = false;
                lblDeviceURL.Enabled = false;
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

                if (txtDeviceURL.Text.Contains("://"))
                {
                    libvlc = new LibVLC(enableDebugLogs: true);
                    libvlc.SetLogFile(m_strPath + "vlc.log");

                    using (Media media = new Media(libvlc, txtDeviceURL.Text, FromType.FromLocation))
                    {
                        player = new MediaPlayer(media);
                        player.Hwnd = pbDisplay.Handle;
                        player.Play();
                    }
                    return;
                }

                libvlc = new LibVLC(enableDebugLogs: false, "--rtsp-tcp");

                // Use TCP messaging.
                videoChannel = new TcpMessagingSystemFactory().CreateDuplexOutputChannel("tcp://" + txtDeviceURL.Text + ":8093/");
                videoChannel.ResponseMessageReceived += OnResponseMessageReceived;

                // Use unique name for the pipe.
                string aVideoPipeName = Guid.NewGuid().ToString();

                // Open pipe that will be read by VLC.
                videoPipe = new NamedPipeServerStream(@"\" + aVideoPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 65528);
                ManualResetEvent vlcConnectedPipe = new ManualResetEvent(false);
                ThreadPool.QueueUserWorkItem(x =>
                {
                    videoPipe.WaitForConnection();

                    // Indicate VLC has connected the pipe.
                    vlcConnectedPipe.Set();
                });

                // VLC connects the pipe and starts playing.
                using (Media media = new Media(libvlc, @"stream://\\\.\pipe\" + aVideoPipeName, FromType.FromLocation))
                {
                    // Setup VLC so that it can process raw h264 data
                    media.AddOption(":demux=H264");

                    player = new MediaPlayer(media);
                    player.Hwnd = pbDisplay.Handle;

                    // Note: This will connect the pipe and read the video.
                    player.Play();
                }

                // Wait until VLC connects the pipe so that it is ready to receive the stream.
                if (!vlcConnectedPipe.WaitOne(5000))
                {
                    throw new TimeoutException($"VLC did not open connection with {txtDeviceURL.Text}.");
                }

                // Open connection with service running on Raspberry.
                videoChannel.OpenConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LucidScribe.InitializePlugin()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DisconnectHalovisionDevice()
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

        public void CaptureControl(ref Bitmap bmp, int width, int height)
        {
            bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.CopyFromScreen(this.Location.X + 26, this.Location.Y + 71, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                graphics.Dispose();
            }
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
                CaptureControl(ref currentBitmap, pbDisplay.Width, pbDisplay.Height);

                if (DetectFace && currentBitmap != null)
                {
                    Image<Bgr, byte> imageFrame = new Image<Bgr, Byte>(currentBitmap);
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

               CaptureControl(ref previousBitmap, pbDisplay.Width, pbDisplay.Height);

                if (pictureBoxCurrent.Image != null)
                {
                    pictureBoxCurrent.Image.Dispose();
                }
                pictureBoxCurrent.Image = previousBitmap;

                if (RecordVideo)
                {
                    if (feedChanged || diff > 0)
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

                //Value = diff;
                if (ValueChanged != null)
                {
                    ValueChanged(diff);
                }

                lblTime.Text = DateTime.Now.ToString("yyy-MM-dd hh:mm:ss - ") + diff;
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
                        int bDiff = Math.Abs(row1[x * PixelSize] - row2[x * PixelSize]);
                        int gDiff = Math.Abs(row1[x * PixelSize + 1] - row2[x * PixelSize + 1]);
                        int rDiff = Math.Abs(row1[x * PixelSize + 2] - row2[x * PixelSize + 2]);

                        int pixelDiff = rDiff + gDiff + bDiff;

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
                                    r = r + (pixelDiff * 2);
                                }
                                else if (ran == 2)
                                {
                                    g = g + (pixelDiff * 2);
                                }
                                else if (ran == 3)
                                {
                                    b = b + (pixelDiff * 2);
                                }
                                else
                                {
                                    r = r + (pixelDiff * 2);
                                    g = g + (pixelDiff * 2);
                                    b = b + (pixelDiff * 2);
                                }
                            }
                            else
                            {
                                r = r + (pixelDiff);
                                g = g + (pixelDiff);
                                b = b + (pixelDiff);
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
            SaveSettings();
        }

        private void cmbPixelThreshold_SelectedIndexChanged(object sender, EventArgs e)
        {
            PixelThreshold = Convert.ToInt32(cmbPixelThreshold.Text);
            SaveSettings();
        }

        private void cmbPixelsInARow_SelectedIndexChanged(object sender, EventArgs e)
        {
            PixelsInARow = Convert.ToInt32(cmbPixelsInARow.Text);
            SaveSettings();
        }

        private void cmbFrameThreshold_SelectedIndexChanged(object sender, EventArgs e)
        {
            FrameThreshold = Convert.ToInt32(cmbFrameThreshold.Text);
            SaveSettings();
        }

        private void cmbSensitivity_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sensitivity = Convert.ToInt32(cmbSensitivity.Text);
            SaveSettings();
        }

        private void SaveSettings()
        {
            if (!loaded) return;

            String defaultSettings = "<LucidScribeData>";
            defaultSettings += "<Plugin>";
            defaultSettings += "<Algorithm>" + cmbAlgorithm.Text + "</Algorithm>";
            defaultSettings += "<Camera>" + cmbDevices.Text + "</Camera>";
            defaultSettings += "<DeviceURL>" + txtDeviceURL.Text + "</DeviceURL>";
            defaultSettings += "<PixelThreshold>" + cmbPixelThreshold.Text + "</PixelThreshold>";
            defaultSettings += "<PixelsInARow>" + cmbPixelsInARow.Text + "</PixelsInARow>";
            defaultSettings += "<FrameThreshold>" + cmbFrameThreshold.Text + "</FrameThreshold>";
            defaultSettings += "<Sensitivity>" + cmbSensitivity.Text + "</Sensitivity>";
            defaultSettings += "<TossThreshold>" + tossThresholdInput.Value + "</TossThreshold>";
            defaultSettings += "<TossHalfLife>" + tossHalfLifeInput.Value + "</TossHalfLife>";
            defaultSettings += "<EyeMoveMin>" + eyeMoveMinInput.Value + "</EyeMoveMin>";
            defaultSettings += "<EyeMoveMax>" + eyeMoveMaxInput.Value + "</EyeMoveMax>";
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

            if (chkTopMost.Checked)
            {
                defaultSettings += "<TopMost>1</TopMost>";
            }
            else
            {
                defaultSettings += "<TopMost>0</TopMost>";
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
            Disconnect();
        }

        private void chkTCMP_CheckedChanged(object sender, EventArgs e)
        {
            TCMP = chkTCMP.Checked;
            SaveSettings();
        }

        private void txtDeviceURL_TextChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void chkRecordVideo_CheckedChanged(object sender, EventArgs e)
        {
            RecordVideo = chkRecordVideo.Checked;
            SaveSettings();
        }

        private void cmbIgnorePercentage_SelectedIndexChanged(object sender, EventArgs e)
        {
            IgnorePercentage = Convert.ToInt32(cmbIgnorePercentage.Text);
            SaveSettings();
        }

        private void chkDetectFace_CheckedChanged(object sender, EventArgs e)
        {
            DetectFace = chkDetectFace.Checked;
            SaveSettings();
        }

        private void chkTopMost_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = chkTopMost.Checked;
            SaveSettings();
        }

        private void tossThreshold_ValueChanged(object sender, EventArgs e)
        {
            TossThreshold = (int)tossThresholdInput.Value;
            SaveSettings();
        }

        private void tossHalfLife_ValueChanged(object sender, EventArgs e)
        {
            TossHalfLife = (int)tossHalfLifeInput.Value;
            SaveSettings();
        }

        private void eyeMoveMinInput_ValueChanged(object sender, EventArgs e)
        {
            EyeMoveMin = (int)eyeMoveMinInput.Value;
            SaveSettings();
        }

        private void eyeMoveMaxInput_ValueChanged(object sender, EventArgs e)
        {
            EyeMoveMax = (int)eyeMoveMaxInput.Value;
            SaveSettings();
        }
    }
}
