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
        private string lucidScribeDataPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\lucidcode\\Lucid Scribe";

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
        public int IdleTicks = 8;
        private int PixelSize = 4;

        private bool RecordVideo = false;
        private bool feedChanged = true;

        private bool TCMP = false;
        public int DotThreshold = 100;
        public int DashThreshold = 500;

        private VideoCaptureDevice videoSource;
        private Rectangle[] faceRegions;
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
                LoadVideoDevices();
                LoadClassifiers();
                LoadSettings();
                loadingDevices = false;

                if (cmbDevices.Text == "") cmbDevices.Text = cmbDevices.Items[0].ToString();

                if (cmbDevices.Text == "lucidcode Halovision Device")
                {
                    loaded = true;
                    ConnectHalovisionDevice();
                    return;
                }

                OpenVideoDevice();

                loaded = true;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LucidScribe.InitializePlugin()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVideoDevices()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            cmbDevices.Items.Clear();
            foreach (FilterInfo filterInfo in videoDevices)
            {
                cmbDevices.Items.Add(filterInfo.Name);
            }
            cmbDevices.Items.Add("lucidcode Halovision Device");
        }

        private void OpenVideoDevice()
        {
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
        }
        private void LoadClassifiers()
        {
            cmbClassifier.Items.Add("None");
            foreach (string filename in Directory.EnumerateFiles($"{lucidScribeDataPath}\\Classifiers", "haarcascade*.xml", SearchOption.AllDirectories))
            {
                string classifierName = new FileInfo(filename).Name.Replace(".xml", "");
                cmbClassifier.Items.Add(classifierName);
            }
            if (cmbClassifier.Items.Count > 0)
            {
                cmbClassifier.Text = cmbClassifier.Items[0].ToString();
            }
        }

        private void LoadClassifier()
        {
            try
            {
                cascadeClassifier = null;
                if (cmbClassifier.Text != "" && cmbClassifier.Text != "None")
                {
                    cascadeClassifier = new CascadeClassifier($"{lucidScribeDataPath}\\Classifiers\\{cmbClassifier.Text}.xml");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LucidScribe.LoadClassifier()", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                videoSource.NewFrame -= video_NewFrame;
                Thread.Sleep(256);
                Application.DoEvents();
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
            }
        }

        private void LoadSettings()
        {
            XmlDocument xmlSettings = new XmlDocument();

            var settingsFilePath = $"{lucidScribeDataPath}\\Plugins\\Halovision.User.lsd";

            if (!File.Exists(settingsFilePath))
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
                defaultSettings += "<IdleTicks>8</IdleTicks>";
                defaultSettings += "<IgnorePercentage>16</IgnorePercentage>";
                defaultSettings += "<RecordVideo>0</RecordVideo>";
                defaultSettings += "<TCMP>0</TCMP>";
                defaultSettings += "<DotThreshold>100</DotThreshold>";
                defaultSettings += "<DashThreshold>500</DashThreshold>";
                defaultSettings += "<Classifier>None</Classifier>";
                defaultSettings += "</Plugin>";
                defaultSettings += "</LucidScribeData>";
                File.WriteAllText(settingsFilePath, defaultSettings);
            }

            xmlSettings.Load(settingsFilePath);

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

            if (xmlSettings.DocumentElement.SelectSingleNode("//Classifier") != null)
            {
                cmbClassifier.Text = xmlSettings.DocumentElement.SelectSingleNode("//Classifier").InnerText;
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

            if (xmlSettings.DocumentElement.SelectSingleNode("//IdleTicks") != null)
            {
                idleTicksInput.Value = Convert.ToDecimal(xmlSettings.DocumentElement.SelectSingleNode("//IdleTicks").InnerText);
            } 
            else
            {
                idleTicksInput.Value = IdleTicks;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//TCMP") != null && xmlSettings.DocumentElement.SelectSingleNode("//TCMP").InnerText == "1")
            {
                chkTCMP.Checked = true;
                TCMP = true;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//DotThreshold") != null)
            {
                dotThresholdInput.Value = Convert.ToDecimal(xmlSettings.DocumentElement.SelectSingleNode("//DotThreshold").InnerText);
            }
            else
            {
                dotThresholdInput.Value = DotThreshold;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//DashThreshold") != null)
            {
                dashThresholdInput.Value = Convert.ToDecimal(xmlSettings.DocumentElement.SelectSingleNode("//DashThreshold").InnerText);
            }
            else
            {
                dashThresholdInput.Value = DashThreshold;
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
            OpenVideoDevice();
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
                    libvlc.SetLogFile($"{lucidScribeDataPath}\\vlc.log");

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

            pbDisplay.DrawToBitmap(bmp, new Rectangle(0, 0, pbDisplay.Width, pbDisplay.Height));

            //using (Graphics graphics = Graphics.FromImage(bmp))
            //{
            //    graphics.CopyFromScreen(this.Location.X + Screen.PrimaryScreen.Bounds.X + 26, this.Location.Y + Screen.PrimaryScreen.Bounds.Y + 71, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
            //    graphics.Dispose();
            //}
        }

        private void CreateDirectories()
        {
            if (!Directory.Exists($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}"))
            {
                Directory.CreateDirectory($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}");
            }

            if (!Directory.Exists($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}"))
            {
                Directory.CreateDirectory($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}");
            }

            if (!Directory.Exists($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}"))
            {
                Directory.CreateDirectory($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}");
            }

            if (!Directory.Exists($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}\\{Strings.Format(DateTime.Now, "HH")}"))
            {
                Directory.CreateDirectory($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}\\{Strings.Format(DateTime.Now, "HH")}");
            }

            if (!Directory.Exists($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}\\{Strings.Format(DateTime.Now, "HH")}\\{Strings.Format(DateTime.Now, "mm")}"))
            {
                Directory.CreateDirectory($"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}\\{Strings.Format(DateTime.Now, "HH")}\\{Strings.Format(DateTime.Now, "mm")}");
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

                if (cascadeClassifier != null && currentBitmap != null)
                {
                    Image<Bgr, byte> imageFrame = new Image<Bgr, Byte>(currentBitmap);
                    Image<Gray, byte> grayFrame = imageFrame.Convert<Gray, byte>();
                    faceRegions = cascadeClassifier.DetectMultiScale(grayFrame);
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
                        String secondFile = $"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}\\{Strings.Format(DateTime.Now, "HH")}\\{Strings.Format(DateTime.Now, "mm")}\\{Strings.Format(DateTime.Now, "ss.")}{DateTime.Now.Millisecond}.jpg";
                        pbDifference.Image.Save(secondFile, System.Drawing.Imaging.ImageFormat.Jpeg);
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

            Rectangle[] regions = new Rectangle[] { new Rectangle(0, 0, bitmap1.Width, bitmap1.Height) };

            if (faceRegions != null && faceRegions.Length > 0)
            {
                regions = faceRegions;
            }

            unsafe
            {
                foreach (Rectangle region in regions)
                {
                    yStart = region.Y;
                    yEnd = region.Y + region.Height;
                    xStart = region.X;
                    xEnd = region.X + region.Width;

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

                    if (cascadeClassifier != null)
                    {
                        byte* row2 = (byte*)bmd2.Scan0 + (region.Y * bmd2.Stride);
                        for (int x = region.X; x <= region.X + region.Width; x++)
                        {
                            row2[x * PixelSize + 1] = (byte)255;
                        }
                        row2 = (byte*)bmd2.Scan0 + ((region.Y + region.Height) * bmd2.Stride);
                        for (int x = region.X; x <= region.X + region.Width; x++)
                        {
                            row2[x * PixelSize + 1] = (byte)255;
                        }
                        for (int y = region.Y; y <= region.Y + region.Height; y++)
                        {
                            row2 = (byte*)bmd2.Scan0 + ((y) * bmd2.Stride);
                            row2[region.X * PixelSize + 1] = (byte)255;
                            row2[(region.X + region.Width) * PixelSize + 1] = (byte)255;
                        }
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

            String settings = "<LucidScribeData>";
            settings += "<Plugin>";
            settings += "<Algorithm>" + cmbAlgorithm.Text + "</Algorithm>";
            settings += "<Camera>" + cmbDevices.Text + "</Camera>";
            settings += "<DeviceURL>" + txtDeviceURL.Text + "</DeviceURL>";
            settings += "<PixelThreshold>" + cmbPixelThreshold.Text + "</PixelThreshold>";
            settings += "<PixelsInARow>" + cmbPixelsInARow.Text + "</PixelsInARow>";
            settings += "<FrameThreshold>" + cmbFrameThreshold.Text + "</FrameThreshold>";
            settings += "<Sensitivity>" + cmbSensitivity.Text + "</Sensitivity>";
            settings += "<TossThreshold>" + tossThresholdInput.Value + "</TossThreshold>";
            settings += "<TossHalfLife>" + tossHalfLifeInput.Value + "</TossHalfLife>";
            settings += "<EyeMoveMin>" + eyeMoveMinInput.Value + "</EyeMoveMin>";
            settings += "<EyeMoveMax>" + eyeMoveMaxInput.Value + "</EyeMoveMax>";
            settings += "<IdleTicks>" + idleTicksInput.Value + "</IdleTicks>";
            settings += "<IgnorePercentage>" + cmbIgnorePercentage.Text + "</IgnorePercentage>";

            if (chkRecordVideo.Checked)
            {
                settings += "<RecordVideo>1</RecordVideo>";
            }
            else
            {
                settings += "<RecordVideo>0</RecordVideo>";
            }
            
            settings += "<Classifier>" + cmbClassifier.Text + "</Classifier>";

            if (chkTopMost.Checked)
            {
                settings += "<TopMost>1</TopMost>";
            }
            else
            {
                settings += "<TopMost>0</TopMost>";
            }

            if (chkTCMP.Checked)
            {
                settings += "<TCMP>1</TCMP>";
            }
            else
            {
                settings += "<TCMP>0</TCMP>";
            }

            settings += "<DotThreshold>" + dotThresholdInput.Value + "</DotThreshold>";
            settings += "<DashThreshold>" + dashThresholdInput.Value + "</DashThreshold>";

            settings += "</Plugin>";
            settings += "</LucidScribeData>";
            File.WriteAllText($"{lucidScribeDataPath}\\Plugins\\Halovision.User.lsd", settings);
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

        private void idleTicksInput_ValueChanged(object sender, EventArgs e)
        {
            IdleTicks = (int)idleTicksInput.Value;
            SaveSettings();
        }

        private void cmbClassifier_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadClassifier();
            SaveSettings();
        }

        private void dotThresholdInput_ValueChanged(object sender, EventArgs e)
        {
            DotThreshold = (int)dotThresholdInput.Value;
            SaveSettings();
        }

        private void dashThresholdInput_ValueChanged(object sender, EventArgs e)
        {
            DashThreshold = (int)dashThresholdInput.Value;
            SaveSettings();
        }
    }
}
