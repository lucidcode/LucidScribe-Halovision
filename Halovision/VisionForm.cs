using System;
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
using System.Configuration;
using System.Runtime.InteropServices;
using TensorFlow;
using Emgu.CV.Dnn;
using Emgu.CV.CvEnum;
using System.Runtime.ExceptionServices;
using System.Windows.Threading;
using Emgu.CV.Util;
using System.Collections.Generic;
using System.Linq;
using static PoseNet;

namespace lucidcode.LucidScribe.Plugin.Halovision
{

    public partial class VisionForm : Form
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware();

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

        public int TossThreshold = 2000;
        public int TossHalfLife = 10;
        public int TossValue = 0;

        public int EyeMoveMin = 20;
        public int EyeMoveMax = 800;
        public int IdleTicks = 8;
        private int PixelSize = 4;

        private bool RecordVideo = false;
        private bool CopyFromScreen = false;

        public bool TCMP = false;
        public int DotThreshold = 200;
        public int DashThreshold = 600;

        public bool Auralize = false;
        public WaveType WaveForm = WaveType.Triangle;

        //private VideoCaptureDevice videoSource;

        internal VideoCapture cameraCapture;
        private Rectangle[] faceRegions;
        private CascadeClassifier cascadeClassifier;
        public bool DetectREM = true;

        private bool processing = false;
        private Bitmap currentBitmap = null;
        private PictureBox pictureBoxCurrent = new PictureBox();
        private Bitmap previousBitmap = null;

        private readonly string modelPath = "Models\\model-mobilenet_v1_101.pb";
        private readonly string deployPath = "Models\\model-mobilenet_v1_101.pbtxt";

        private int GPUCard;
        private bool UsesGpu;

        private readonly Mat frame = new Mat();

        private int resolutionX = 640;

        private int resolutionY = 480;

        System.Windows.Point eyeRight;
        System.Windows.Point eyeLeft;
        System.Windows.Point nose;
        private TFSession session;
        private TFGraph graph;
        private readonly PoseNet posenet = new PoseNet();
        private readonly int detectionSizeTFT = 337;

        private Image<Bgr, byte> originalFrame;
        private Image<Bgr, byte> Frame; //current Frame from camera
        private Image<Bgr, byte> Previous_Frame; //Previousframe aquired

        private Image<Bgr, byte> Previous_Rect_Frame; //Previousframe aquired

        private Image<Bgr, byte> ImageDifferenceLd { get; set; }
        private Image<Gray, byte> DifferenceImageRect; //Difference between the two frame detected rect
        private Image<Bgr, byte> resultImage; //Difference between the two frame detected rect
        private Image<Bgr, byte> additionFramediff; //Additional Difference
        private Image<Bgr, byte> additionFrameDiffRegion; //Additional Difference
        private readonly Image<Gray, byte> additionFrameDiffREM; //Additional Difference
        private Image<Bgr, byte> display;
        private Image<Bgr, byte> result;
        private Image<Bgr, byte> previousFrameRegion; //Additional Difference

        private float ratioXFrame;
        private float ratioYFrame;
        private bool ResetROI = false;
        private Rectangle rectROIFinal = Rectangle.Empty;
        private Rectangle rectROITESTDraw = Rectangle.Empty;
        private bool noseKeyLost;
        private bool FaceTracker, FindEyePositionROI;
        private double WidthMulFT = 1.5;
        private double HeighMulFT = 1;
        private int xROIFT, yROIFT, WidthROIFT, HeightROIFT;

        private bool detectedRect;

        private Rectangle lastFaceRegion;

        private Rectangle PreviouslastFaceRegion;
        internal int redPixel;
        private double diff;
        internal int changedPixels;


        internal List<double> m_arrHistory = new List<double>();
        internal List<double> m_arrHistoryAdvert = new List<double>();
        private readonly List<Rectangle> m_arrPreviouslastFaceRegionHistory = new List<Rectangle>();
        private readonly List<int> m_arrDetectedFaceMove = new List<int>();
        private readonly List<int> m_arrDetectedRectMove = new List<int>();
        private readonly List<int> m_arrDetectedRegionMove = new List<int>();
        private readonly List<int> m_arrDetectedFaceRectChangeInfo = new List<int>();
        private readonly List<int> m_arrDetectedVREM = new List<int>();
        private readonly List<double> m_arrDetectedAreaBelow = new List<double>();
        private readonly List<double> m_arrDetectedAverageNotification = new List<double>();
        private readonly List<double> m_AverageFaceTracker = new List<double>();

        public int FaceTrackerCount;
        public int FaceTrackerCountReset;
        private List<RectangleArea> rectArchAreaRed;
        private List<RectangleArea> rectArchAreaGreen;
        private List<double> ArchAreaGreen;
        private List<double> ArchAreaRed;
        private bool bypassFrame;
        public int vREM;
        public int vREMResult;
        public int vREMValidateResult;
        public int messageAdvertCount = 0;
        private bool vREMSlowMovement;
        private readonly int NoseXValue = 30;
        private readonly int NoseYValue = 40;
        private int AreaMinThreshold = 1;
        private int AreaMaxThreshold = 2500;
        private double widthEyeRatio = 15;
        private double heightEyeRatio = 20;
        private double yEyeRatio = 12;
        private double xEyeRatio = 13;
        private int vREMResetFull = 3;
        private decimal PixelThresholdDiff = 45;
        public int FaceRectChange { get; set; }
        public double Value;
        public bool MovementBreathDetected { get; set; }
        internal int FrameThresholdH = 3500;
        private bool vREMLogOnce;
        public bool Detected888 { get; set; }
        public int CountVrem { get; set; }
        private Mat resizedFrame = new Mat();
        private Net net;
        private Keypoint rightEye;
        private Keypoint leftEye;
        private Keypoint noseKey;
        private readonly Pen jointColorRightGreen = new Pen(System.Drawing.Color.Green, 2);
        private readonly Pen jointColorLeftBlue = new Pen(System.Drawing.Color.Blue, 5);
        private readonly Pen jointColorNose = new Pen(System.Drawing.Color.Cyan, 5);
        private decimal ScoreTF = (decimal)0.04;
        readonly string[,] jointPairs = new string[,]
        {
            { "rightEye", "leftEye" }, { "leftEye", "rightEye" }, { "nose", "nose" }
        };
        private String cmbAlgorithmText;

        public VisionForm()
        {
            InitializeComponent();
        }

        Boolean loadingDevices = true;

        private void PortForm_Load(object sender, EventArgs e)
        {
            try
            {
                SetProcessDPIAware();
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

                if (cmbDevices.Text == "lucidcode INSPEC")
                {
                    loaded = true;
                    ConnectInspecDevice();
                    return;
                }

                OpenVideoDevice();

                loaded = true;
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.StackTrace, "LucidScribe.InitializePlugin() 1", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            cmbDevices.Items.Add("lucidcode INSPEC");
            cmbDevices.Items.Add("lucidcode Halovision Device");
        }

        private void OpenVideoDevice()
        {
            FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo filterInfo in videoDevices)
            {
                if (cmbDevices.Text == filterInfo.Name)
                {

                    Previous_Frame = new Image<Bgr, byte>(resolutionX, resolutionY);

                    cameraCapture = new VideoCapture(0);
                    cameraCapture.SetCaptureProperty(CapProp.FrameWidth, resolutionX);
                    cameraCapture.SetCaptureProperty(CapProp.FrameHeight, resolutionY);
                    //cameraCapture.SetCaptureProperty(CapProp.Buffersize, 50); // internal buffer will now store only 50 frames
                    cameraCapture.ImageGrabbed -= Camera_ImageGrabbed;
                    cameraCapture.ImageGrabbed += Camera_ImageGrabbed;
                    //cameraCapture.SetCaptureProperty(CapProp.Fps, fpsThread);
                    cameraCapture.Start();

                    StartProcess();
                    return;
                }
            }
        }


        private int _statFrameCounter = 0;
        private int _statFramesTotal = 0;
        public bool IsLucidCamReady()
        {
            _statFramesTotal++;
            return _readyEvent.WaitOne(0);
        }
        private ManualResetEvent _gotNewFrameEvent = new ManualResetEvent(false);
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private ManualResetEvent _readyEvent = new ManualResetEvent(false);

        [HandleProcessCorruptedStateExceptions]
        private void Camera_ImageGrabbed(object sender, EventArgs arg)
        {
            if (IsLucidCamReady())
            {
                _gotNewFrameEvent.Set();
            }
        }


        private Thread _thread;

        public void StartProcess()
        {
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }

            var _threadStart = new ThreadStart(ProcessFrames);
            _thread = new Thread(_threadStart);
            _thread.Priority = ThreadPriority.Highest;
            _thread.Start();
            _readyEvent.Set();
        }

        public void StopProcess()
        {
            _stopEvent.Set();
            _stopEvent.Reset();
            _readyEvent.Reset();
            _gotNewFrameEvent.Reset();
            if (_thread != null && !_thread.Join(100))
            {
                _thread.Abort();
            }
        }

        private void ProcessFrames()
        {
            WaitHandle[] exitOrRetry = new WaitHandle[] { _stopEvent, _gotNewFrameEvent };

            while (!_stopEvent.WaitOne(0))
            {
                if (_gotNewFrameEvent.WaitOne(0))
                {
                    _readyEvent.Reset();
                    _gotNewFrameEvent.Reset();

                    _statFrameCounter++;

                    if (_statFrameCounter > 24)
                    {
                        DateTime now = DateTime.Now;
                        _statFrameCounter = 0;
                        _statFramesTotal = 0;
                    }

                    // Process the frame
                    ProcessFramesStream();

                    _readyEvent.Set();
                }
                else
                {
                    WaitHandle.WaitAny(exitOrRetry, 100);
                }
            }
        }

        public int CurrentFrameCount { get; set; }
        public int CurrentFrameAverageCount { get; set; }
        public int CurrentFrameAverageCountToClear { get; set; }
        public int CurrentFrameChangedROICount { get; set; }


        private void ProcessFramesStream()
        {
            try
            {
                CurrentFrameCount++;
                CurrentFrameAverageCount++;
                CurrentFrameAverageCountToClear++;
                CurrentFrameChangedROICount++;
                Application.DoEvents();




                using (Frame = new Image<Bgr, byte>(resolutionX, resolutionY))
                using (originalFrame = new Image<Bgr, byte>(resolutionX, resolutionY))
                using (DifferenceImageRect = new Image<Gray, byte>(resolutionX, resolutionY))
                using (additionFramediff = new Image<Bgr, byte>(resolutionX, resolutionY))
                using (additionFrameDiffRegion = new Image<Bgr, byte>(resolutionX, resolutionY))
                {

                    cameraCapture.Retrieve(originalFrame);
                    Frame = originalFrame.Resize(resolutionX, resolutionY, Inter.Linear);

                    // Release to avoid memory leak create exception
                    if (ImageDifferenceLd != null && ImageDifferenceLd.Ptr != IntPtr.Zero)
                    {
                        ImageDifferenceLd?.Dispose();
                        ImageDifferenceLd = null;
                    }

                    // Release to avoid memory leak
                    if (resultImage != null)
                    {
                        resultImage?.Dispose();
                        resultImage = null;
                    }

                    // Release to avoid memory leak
                    if (Previous_Rect_Frame != null)
                    {
                        Previous_Rect_Frame?.Dispose();
                        Previous_Rect_Frame = null;
                        Previous_Rect_Frame = new Image<Bgr, byte>(resolutionX, resolutionY);
                    }

                    using (var imgROI = new Image<Bgr, byte>(resolutionX, resolutionY))
                    {
                        Frame.CopyTo(imgROI);
                        imgROI.ROI = rectROIFinal;
                        Frame = imgROI.Resize(resolutionX, resolutionY, Inter.Linear);
                    }


                    ratioXFrame = (float)resolutionX / (float)pbDifference.Width;
                    ratioYFrame = (float)resolutionY / (float)pbDifference.Height;

                    noseKeyLost = false;
                    FindEyePositionROI = false;

                    if (nose.X != 0 && Math.Abs(Math.Abs(nose.X / ratioXFrame) - Math.Abs(eyeRight.X / ratioXFrame)) < 10 ||
                       (nose.X != 0 && Math.Abs(Math.Abs(nose.X / ratioXFrame) - Math.Abs(eyeLeft.X / ratioXFrame)) < 10))
                    {
                        WidthMulFT = 1.2;
                        HeighMulFT = 0.8;
                    }
                    else
                    {
                        WidthMulFT = 1.8;
                        HeighMulFT = 0.8;
                    }

                    //if (foundROI)
                    //{
                    //  vREMResetFull = vREMResetFull * 2;
                    //}
                    //else
                    //{
                    //  vREMResetFull = vREMResetFull;
                    //}

                    // debug
                    FaceTrackerModeDraw();


                    if (detectedRect)
                    {
                        if (lastFaceRegion.X + lastFaceRegion.Width > resolutionX ||
                            lastFaceRegion.Y + lastFaceRegion.Height > resolutionY || lastFaceRegion.Y < 0 ||
                            lastFaceRegion.X < 0)
                        {
                            if (lastFaceRegion.Y < 0)
                            {
                                lastFaceRegion.Y = 0;
                            }

                            if (lastFaceRegion.X < 0)
                            {
                                lastFaceRegion.X = 0;
                            }

                            detectedRect = false;
                        }
                    }



                    // Detection Code
                    diff = 0.0;
                    redPixel = 0;
                    changedPixels = 0;
                    {
                        using (var toGrayScaleImg = new Image<Bgr, byte>(resolutionX, resolutionY))
                        {
                            Previous_Frame.AbsDiff(Frame).CopyTo(toGrayScaleImg);

                            //copy the frame to act as the previous frame
                            Frame.CopyTo(Previous_Frame);

                            // Calcul pixel count
                            // Code DNN Threshold
                            /*Play with the value 60 to set a threshold for movement*/

                            //// OK RTSP and Video
                            //// for debugging purpose (displaying threshold diff on source)
                            ////display the image using thread safe call
                            //if (cameraCapture != null && pbSourceCheckBox.Checked)
                            //{
                            //  Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                            //  {
                            //    //// displaying only threshold diff
                            //    //pbSource.Image = DifferenceImageRect.ToBitmap();

                            //    // displaying only threshold diff
                            //    pbSource.Image = Frame.ToBitmap();
                            //  }));
                            //}

                            //// Call garbage collector
                            //GC.Collect();
                            //Application.DoEvents();
                            //return;

                            //if (modeColor)
                            //{
                            //    // checked Gray
                            //    // trying to filter pixel using gray scale
                            //    toGrayScaleImg.Convert<Gray, byte>().CopyTo(DifferenceImageRect);
                            //    DifferenceImageRect.SmoothMedian(Sensitivity).CopyTo(DifferenceImageRect); // Sensitivity to 9
                            //    var totalPixels = DifferenceImageRect.CountNonzero()[0];
                            //    DifferenceImageRect.ThresholdBinary(new Gray(PixelThreshold), new Gray(255)).CopyTo(DifferenceImageRect);
                            //    changedPixels = DifferenceImageRect.CountNonzero()[0];
                            //}
                            //else
                            {
                                // color Brg
                                // trying to filter pixel using color bgr
                                toGrayScaleImg.SmoothMedian(Sensitivity).CopyTo(toGrayScaleImg); // Sensitivity to 3
                                var totalPixels = toGrayScaleImg.CountNonzero()[0];
                                toGrayScaleImg.ThresholdBinary(new Bgr(PixelThreshold, PixelThreshold, PixelThreshold), new Bgr(255, 255, 255)).CopyTo(toGrayScaleImg);
                                changedPixels = toGrayScaleImg.CountNonzero()[0];
                                toGrayScaleImg.Convert<Gray, byte>().CopyTo(DifferenceImageRect);
                            }
                        }

                        // for debugging purpose (displaying threshold diff on source)
                        //display the image using thread safe call
                        if (cameraCapture != null)
                        {
                            Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                            {
                                //// displaying only threshold diff
                                //pbSource.Image = DifferenceImageRect.ToBitmap();

                                // displaying only threshold diff
                                pbDisplay.Image = Frame.ToBitmap();
                            }));
                        }
                    }



                    // detection area green and red
                    var contourMultiFrameDetected = FindContours(changedPixels);
                    //var contourMultiFrameDetected = false;

                    #region Detection average

                    if (m_arrDetectedFaceMove.Count > 5)
                    {
                        //var average = m_arrDetectedFaceMove.Average();
                        var average = m_arrDetectedFaceMove.Where(predicate: x => { return x > FrameThreshold; })
                          .DefaultIfEmpty().Average();

                        if (Convert.ToInt32(average) > 20000)
                        {
                            // Reset eye position after big movement
                            eyeRight = new System.Windows.Point(0, 0);
                            eyeLeft = new System.Windows.Point(0, 0);
                            contourMultiFrameDetected = false;
                        }
                        m_arrDetectedFaceMove.Clear();
                    }
                    #endregion


                    #region Notification trigger average

                    // check all 2 seconsd (for 10 FPS)
                    //if (m_arrDetectedAverageNotification.Count > 20)
                    //{
                    //    // count how pass has past
                    //    FaceTrackerCountReset++;

                    //    var average = m_arrDetectedAverageNotification.Where(predicate: x => { return x > FrameThreshold; })
                    //      .DefaultIfEmpty().Average();

                    //    int averageTrigger = AverageTriggerONOFF;

                    //    if (foundROI)
                    //    {
                    //        if (PixelThreshold <= 2)
                    //        {
                    //            averageTrigger = 200000;
                    //        }
                    //        else
                    //        {
                    //            averageTrigger = AverageTriggerONOFF;
                    //        }
                    //    }

                    //    if (Convert.ToInt32(average) > averageTrigger && !firstDetectedAverageNotification && NotificationCheck)
                    //    {
                    //        FaceTrackerCount++;

                    //        if (FaceTrackerCount >= 5)
                    //        {
                    //            if (checkBoxVerboseLog.Checked)
                    //            {
                    //                WriteOutput("vREM: " + vREMDigit + " detected higher notification average limit value: " + Convert.ToInt32(average), false);
                    //            }
                    //            SendUdpPacketNotification(Convert.ToInt32(average));
                    //            FaceTrackerCount = 0;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        // Reset FaceTrackerCount after 10 pass so 20 seconds
                    //        if (FaceTrackerCountReset >= 10)
                    //        {
                    //            FaceTrackerCountReset = 0;
                    //            FaceTrackerCount = 0;
                    //        }
                    //    }

                    //    m_arrDetectedAverageNotification.Clear();
                    //    firstDetectedAverageNotification = false;
                    //}

                    #endregion

                    // displaying image
                    if (contourMultiFrameDetected)
                    {
                        additionFramediff = DisplayingContour(DifferenceImageRect.Convert<Bgr, byte>(), display);
                    }
                    else
                    {
                        // Display with background image
                        // add previous frame to addition frame
                        CvInvoke.Add(DifferenceImageRect.Convert<Bgr, byte>(), Frame, additionFramediff);
                    }

                    // setup final image to display with rect area
                    ImageDifferenceLd = additionFramediff;
                    //using (ImageDifferenceLd = new Image<Bgr, byte>(resolutionX, resolutionY)) // not working ?
                    //{
                    //  additionFramediff.CopyTo(ImageDifferenceLd);
                    //}

                    VREMAnalyse(diff, contourMultiFrameDetected);

                    // execute vREM blinks analyse
                    vREMCodeExecution(false, diff);

                }

            }
            catch (AccessViolationException ex)
            {
                // release TFT
                if (session != null)
                {
                    session.Dispose();
                }
                if (graph != null)
                {
                    graph.Dispose();
                }

                Console.WriteLine(ex);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void LoadClassifiers()
        {
            cmbClassifier.Items.Add("None");

            cmbClassifier.Items.Add("TensorFlow");

            if (!Directory.Exists($"{lucidScribeDataPath}\\Classifiers"))
            {
                return;
            }

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
                DetectREM = true;
                Console.WriteLine($"DetectREM:{DetectREM}");
                cascadeClassifier = null;
                if (cmbClassifier.Text != "" && cmbClassifier.Text != "None")
                {
                    if (cmbClassifier.Text == "TensorFlow")
                    {
                        LoadTensorFlow();
                        return;
                    }
                    cascadeClassifier = new CascadeClassifier($"{lucidScribeDataPath}\\Classifiers\\{cmbClassifier.Text}.xml");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LucidScribe.LoadClassifier()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadTensorFlow()
        {
            GPUCard = Convert.ToInt32(ConfigurationManager.AppSettings.Get("GPU"));
            CreateSession(modelPath, "/device:GPU:" + GPUCard);

            var devices = session.ListDevices();

            foreach (var item in devices)
            {
                if (item.DeviceType.ToString().Contains("GPU"))
                {
                    UsesGpu = true;
                    break;
                }
                else
                {
                    UsesGpu = false;
                }
            }
        }

        public TFSession CreateSession(string graphPath, string deviceName)
        {
            if (session != null)
            {
                session.Dispose();
            }

            var graph = LoadGraph(modelPath, deviceName);

            byte[] configBuffer = Convert.FromBase64String("OAFAAQ==");

            var options = new TFSessionOptions();

            unsafe
            {
                fixed (byte* pConfigBuffer = configBuffer)
                {
                    options.SetConfig((IntPtr)pConfigBuffer, configBuffer.Length);
                }
            }

            return session = new TFSession(graph, options);
        }

        private TFGraph LoadGraph(string path, string deviceName)
        {
            if (graph != null)
            {
                graph.Dispose();
            }

            graph = new TFGraph();

            var options = new TFImportGraphDefOptions();

            try
            {
                SetDevice(options, deviceName);
            }
            catch
            {
                UsesGpu = false;
            }

            graph.Import(File.ReadAllBytes(path), options);

            return graph;
        }


        [DllImport("libtensorflow")]
        private static extern void TF_ImportGraphDefOptionsSetDefaultDevice(IntPtr opts, string device);

        private static void SetDevice(TFImportGraphDefOptions options, string device)
        {
            TF_ImportGraphDefOptionsSetDefaultDevice(options.Handle, device);
        }

        public void Disconnect()
        {
            tmrDiff.Enabled = false;
            processing = false;

            if (cmbDevices.Text == "lucidcode Halovision Device")
            {
                DisconnectHalovisionDevice();
            }
            else if (cmbDevices.Text == "lucidcode INSPEC")
            {
                DisconnectInspecDevice();
            }
            else
            {
                cameraCapture.Stop();
                Thread.Sleep(256);
                Application.DoEvents();
                cameraCapture = null;
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
                    if (cmbRotateFlip.Text != "RotateNoneFlipNone")
                    {
                        RotateFlipType rotateFlipType;
                        if (Enum.TryParse(cmbRotateFlip.Text, out rotateFlipType))
                        {
                            videoBitmap.RotateFlip(rotateFlipType);
                        }
                    }
                    pbDisplay.Image = videoBitmap;
                }));
            }
            catch (Exception ex)
            {
            }
        }

        private void FaceTrackerModeDraw()
        {
            try
            {
                // Find eye position
                WidthROIFT = (int)(resolutionX / WidthMulFT / ratioXFrame);
                HeightROIFT = (int)(resolutionY / HeighMulFT / ratioYFrame);
                xROIFT = (int)((nose.X / ratioXFrame) - WidthROIFT / 2);
                yROIFT = (int)((nose.Y / ratioYFrame) - HeightROIFT / 2);

                // debug
                rectROITESTDraw = new Rectangle((int)(xROIFT), (int)(yROIFT), (int)(WidthROIFT), (int)(HeightROIFT)); // x, y, width, height

                if (xROIFT > 0 && yROIFT > 0)
                {
                    faceRegions = new Rectangle[] { rectROITESTDraw };
                }
            }
            catch (Exception exception)
            {
                WidthMulFT = 1.5;
                HeighMulFT = 1;
                rectROITESTDraw = new Rectangle(0, 0, resolutionX, resolutionY);
                Console.WriteLine(exception.Message);
            }
        }

        private bool FindContours(int changedPixels)
        {
            bool debuggingPerf = false;
            var contourMultiFrameDetected = false;
            if (!debuggingPerf)
            {
                #region Find contour on frame diff

                // init List here
                rectArchAreaRed = new List<RectangleArea>();
                rectArchAreaGreen = new List<RectangleArea>();

                if (DifferenceImageRect != null && DifferenceImageRect.Ptr != IntPtr.Zero)
                {
                    using (display = new Image<Bgr, byte>(DifferenceImageRect.Width, DifferenceImageRect.Height))
                    using (Mat m = new Mat())
                    {
                        // for reading log
                        var vREMDigit = $"{vREM:000}";

                        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
                        Dictionary<int, double> shapes = new Dictionary<int, double>();

                        if (DifferenceImageRect != null)
                        {
                            // reset bypassFrame
                            bypassFrame = false;

                            // Find eye position
                            using (var tempImage = new Mat())
                            {
                                if (!FindEyePositionROI)
                                {
                                    Frame.Mat.CopyTo(tempImage);
                                    FindEyePosition(tempImage, posenet.DNN);
                                }
                            }

                            try
                            {
                                //CvInvoke.FindContours(DifferenceImageRect.Convert<Gray, byte>(), contours, m,
                                //Emgu.CV.CvEnum.RetrType.Tree,
                                //Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                                CvInvoke.FindContours(DifferenceImageRect, contours, m,
                                  Emgu.CV.CvEnum.RetrType.Tree,
                                  Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                                ArchAreaRed = new List<double>();
                                ArchAreaGreen = new List<double>();

                                var xRateEye = (float)DifferenceImageRect.Width / (float)detectionSizeTFT;
                                var yRateEye = (float)DifferenceImageRect.Height / (float)detectionSizeTFT;
                                var yRatioNose = (float)DifferenceImageRect.Width / (float)DifferenceImageRect.Height; ///*nose.Y * detectionSizeTFT **/ yRate; // eyeLeft.Y;
                                var xRatioNose = (float)DifferenceImageRect.Width / (float)DifferenceImageRect.Height; ///*nose.X * detectionSizeTFT **/ xRate; // eyeLeft.X;

                                bool logStop = false;
                                bool logStart = false;

                                contourMultiFrameDetected = false;

                                // Selecting largest contour
                                if (contours.Size > 0)
                                {
                                    //WriteOutput("Contour all detected " + contours.Size, false);

                                    if (display != null)
                                    {
                                        display?.Dispose();
                                        display = null;
                                    }
                                    display = new Image<Bgr, byte>(DifferenceImageRect.Width, DifferenceImageRect.Height);

                                    for (int i = 0; i < contours.Size; i++)
                                    {
                                        // Testing
                                        double perimeter = CvInvoke.ArcLength(contours[i], true);
                                        VectorOfPoint approx = new VectorOfPoint();
                                        CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);

                                        // set pixel to 0 for new count
                                        int n_green_pix_rect = 0;
                                        int n_red_pix_rect = 0;

                                        var area = CvInvoke.ContourArea(contours[i]);
                                        var rect = CvInvoke.BoundingRectangle(contours[i]);
                                        double ar = (double)rect.Width / rect.Height;

                                        //moments  center of the shape
                                        var moments = CvInvoke.Moments(contours[i]);
                                        int X = (int)(moments.M10 / moments.M00);
                                        int Y = (int)(moments.M01 / moments.M00);
                                        var WeightedCentroid = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00));
                                        //var distLeft = CvInvoke.PointPolygonTest(contours[i], new PointF((float)eyeLeft.X, (float)eyeLeft.Y), true);
                                        //var distRight = CvInvoke.PointPolygonTest(contours[i], new PointF((float)eyeRight.X, (float)eyeRight.Y), true);

                                        //CvInvoke.Circle(display, new System.Drawing.Point((int)eyeLeft.X, (int)eyeLeft.Y), 10, new MCvScalar(255, 0, 0), 2);
                                        //CvInvoke.Circle(display, new System.Drawing.Point((int)eyeRight.X, (int)eyeRight.Y), 10, new MCvScalar(255, 0, 0), 2);

                                        // correct area
                                        if (area == 0.5)
                                        {
                                            area = 1;
                                        }

                                        if (area > 0)//FrameThreshold && area < FrameThresholdH)
                                        {
                                            // check if it's REM eye detected
                                            if (/*area >= AreaMinThreshold && */eyeRight != new System.Windows.Point(0, 0) || eyeLeft != new System.Windows.Point(0, 0) && area < AreaMaxThreshold)
                                            {
                                                // nose
                                                if (Math.Abs(X - nose.X) < NoseXValue && Math.Abs(Y - nose.Y) < NoseYValue)
                                                {
                                                    // Draw rectangle in red for no matched REM eye position for area higher than "AreaMinThreshold"
                                                    /*CvInvoke.Rectangle(display, rect, new MCvScalar(0, 0, 255), 2);
                                                    //CvInvoke.DrawContours(display, contours, i, new MCvScalar(0, 0, 255), 2);
                                                    RectangleArea rectAreaRed = new RectangleArea(rect.X, rect.Y, rect.Width, rect.Height, area, n_green_pix_rect, ar);
                                                    rectArchAreaRed.Add(rectAreaRed);
                                                    ArchAreaRed.Add(area);
                                                    //DisplayingContour(Frame, display, true);  // Debugging

                                                    // add information to tell rectangle detected
                                                    contourMultiFrameDetected = true;

                                                    // debugging
                                                    if (checkBoxVerboseLog.Checked)
                                                    {
                                                      if (!logStart)
                                                        WriteOutput("vREM: " + vREMDigit + " - Start red over nose 0", false);

                                                      // debugging need to be enable manually
                                                      //WriteOutput("vREM: " + vREMDigit + " - rect red - eye right center: " + eyeRight +
                                                      //" - eye left center: " + eyeLeft + " - moment center: " + WeightedCentroid + " - diff: " + diff + " - area: " + area, false);
                                                      logStop = true;
                                                      logStart = true;
                                                    }

                                                    // Get red pixel into contour
                                                    using (Image<Gray, byte> temp = DifferenceImageRect.Copy(rect))
                                                    {
                                                      n_red_pix_rect = temp.CountNonzero()[0];
                                                    }
                                                    redPixel += n_red_pix_rect;*/
                                                }
                                                // right eye
                                                else if (eyeRight.X != 0 && (Math.Abs(X - eyeRight.X) < xEyeRatio * xRateEye && Math.Abs(Y - eyeRight.Y) < yEyeRatio * yRateEye) ||
                                                  (eyeRight.X != 0 && Math.Abs(X - eyeRight.X) < xEyeRatio * xRateEye && Y - eyeRight.Y >= 0 && Y - eyeRight.Y < yEyeRatio * 3 * yRateEye))
                                                {
                                                    if (area >= AreaMinThreshold)
                                                    {
                                                        // Get green pixel into contour
                                                        using (Image<Gray, byte> temp = DifferenceImageRect.Copy(rect))
                                                        {
                                                            n_green_pix_rect = temp.CountNonzero()[0];
                                                        }

                                                        // Draw rectangle green
                                                        CvInvoke.Rectangle(display, rect, new MCvScalar(0, 255, 0), 2);
                                                        //CvInvoke.DrawContours(display, contours, i, new MCvScalar(0, 255, 0), 2);
                                                        diff += n_green_pix_rect;

                                                        // add information to tell rectangle detected
                                                        contourMultiFrameDetected = true;

                                                        // Debugging
                                                        //WriteOutput("Diff: " + Value);

                                                        // added rect detected
                                                        RectangleArea rectAreaGreen = new RectangleArea(rect.X, rect.Y, rect.Width, rect.Height, area, n_green_pix_rect, ar);
                                                        rectArchAreaGreen.Add(rectAreaGreen);
                                                        ArchAreaGreen.Add(area);
                                                        //DisplayingContour(Frame, display, true);  // Debugging
                                                    }
                                                    else
                                                    {
                                                        // Get red pixel into contour
                                                        /*using (Image<Gray, byte> temp = DifferenceImageRect.Copy(rect))
                                                        {
                                                          n_red_pix_rect = temp.CountNonzero()[0];
                                                        }
                                                        redPixel += n_red_pix_rect;

                                                        // Draw rectangle in red for no matched REM eye position for area lower than "AreaMinThreshold"
                                                        CvInvoke.Rectangle(display, rect, new MCvScalar(0, 0, 255), 2);
                                                        RectangleArea rectAreaRed = new RectangleArea(rect.X, rect.Y, rect.Width, rect.Height, area, n_red_pix_rect, ar);
                                                        rectArchAreaRed.Add(rectAreaRed);
                                                        ArchAreaRed.Add(area);*/
                                                        //DisplayingContour(Frame, display, true);  // Debugging

                                                        // add information to tell rectangle detected
                                                        contourMultiFrameDetected = true;
                                                    }
                                                }
                                                // left eye
                                                else if (eyeLeft.X != 0 && (Math.Abs(X - eyeLeft.X) < xEyeRatio * xRateEye && Math.Abs(Y - eyeLeft.Y) < yEyeRatio * yRateEye) ||
                                                   (eyeLeft.X != 0 && Math.Abs(X - eyeLeft.X) < xEyeRatio * xRateEye && Y - eyeLeft.Y >= 0 && Y - eyeLeft.Y < yEyeRatio * 3 * yRateEye))/* ||
                               (X - eyeLeft.X >= 0 && X - eyeLeft.X < xEyeRatio * 3 * xRateEye && Y - eyeLeft.Y >= 0 && Y - eyeLeft.Y < yEyeRatio * 3 * yRateEye))*/

                                                /*||
                                                Math.Abs(X - eyeLeft.X) < xEyeRatio * xRateEye || X - eyeLeft.X > 0 && X - eyeLeft.X < xEyeRatio * 3 * xRateEye &&
                                                Math.Abs(Y - eyeLeft.Y) < yEyeRatio * yRateEye && Y - eyeLeft.Y > 0 && Y - eyeLeft.Y < yEyeRatio * 2 * yRateEye)*/
                                                {
                                                    if (area >= AreaMinThreshold)
                                                    {
                                                        // Get green pixel into contour
                                                        using (Image<Gray, byte> temp = DifferenceImageRect.Copy(rect))
                                                        {
                                                            n_green_pix_rect = temp.CountNonzero()[0];
                                                        }

                                                        // Draw rectangle green
                                                        CvInvoke.Rectangle(display, rect, new MCvScalar(0, 255, 0), 2);
                                                        //CvInvoke.DrawContours(display, contours, i, new MCvScalar(0, 255, 0), 2);
                                                        diff += n_green_pix_rect;

                                                        // add information to tell rectangle detected
                                                        contourMultiFrameDetected = true;

                                                        // added rect detected
                                                        RectangleArea rectAreaGreen = new RectangleArea(rect.X, rect.Y, rect.Width, rect.Height, area, n_red_pix_rect, ar);
                                                        rectArchAreaGreen.Add(rectAreaGreen);
                                                        ArchAreaGreen.Add(area);
                                                        //DisplayingContour(Frame, display, true);  // Debugging
                                                    }
                                                    else
                                                    {
                                                        // Get red pixel into contour
                                                        /*using (Image<Gray, byte> temp = DifferenceImageRect.Copy(rect))
                                                        {
                                                          n_red_pix_rect = temp.CountNonzero()[0];
                                                        }
                                                        redPixel += n_red_pix_rect;

                                                        // Draw rectangle in red for no matched REM eye position for area lower than "AreaMinThreshold"
                                                        CvInvoke.Rectangle(display, rect, new MCvScalar(0, 0, 255), 2);
                                                        RectangleArea rectAreaRed = new RectangleArea(rect.X, rect.Y, rect.Width, rect.Height, area, n_red_pix_rect, ar);
                                                        rectArchAreaRed.Add(rectAreaRed);
                                                        ArchAreaRed.Add(area);
                                                        //DisplayingContour(Frame, display, true);  // Debugging
                                                        */
                                                        // add information to tell rectangle detected
                                                        contourMultiFrameDetected = true;
                                                    }
                                                }
                                                else if (Math.Abs(X - nose.X) < widthEyeRatio * xRateEye * 10 &&
                                                  (nose.Y - Y) > 0 && (nose.Y - Y) < heightEyeRatio * yRateEye * 10 ||
                                                  (Math.Abs(X - nose.X) < widthEyeRatio * xRateEye * 10 &&
                                                  (Y - nose.Y) > 0 && (Y - nose.Y) < heightEyeRatio * yRateEye * 5))
                                                // cancel area video timecode detected on image and outside the face based on nose point
                                                {
                                                    // Get red pixel into contour
                                                    /* using (Image<Gray, byte> temp = DifferenceImageRect.Copy(rect))
                                                     {
                                                       n_red_pix_rect = temp.CountNonzero()[0];
                                                     }
                                                     redPixel += n_red_pix_rect;

                                                     // Draw rectangle in red for no matched REM eye position for area higher than "AreaMinThreshold"
                                                     CvInvoke.Rectangle(display, rect, new MCvScalar(0, 0, 255), 2);
                                                     //CvInvoke.DrawContours(display, contours, i, new MCvScalar(0, 0, 255), 2);
                                                     RectangleArea rectAreaRed = new RectangleArea(rect.X, rect.Y, rect.Width, rect.Height, area, n_red_pix_rect, ar);
                                                     rectArchAreaRed.Add(rectAreaRed);
                                                     ArchAreaRed.Add(area);*/
                                                    //DisplayingContour(Frame, display, true);  // Debugging

                                                    // add information to tell rectangle detected
                                                    contourMultiFrameDetected = true;
                                                }
                                            }
                                            else if (area > 0 && Math.Abs(X - nose.X) < widthEyeRatio * xRateEye * 10 &&
                                            (nose.Y - Y) > 0 && (nose.Y - Y) < heightEyeRatio * yRateEye * 10 ||
                                            (Math.Abs(X - nose.X) < widthEyeRatio * xRateEye * 10 &&
                                            (Y - nose.Y) > 0 && (Y - nose.Y) < heightEyeRatio * yRateEye * 5))
                                            {

                                                // Get red pixel into contour
                                                /* using (Image<Gray, byte> temp = DifferenceImageRect.Copy(rect))
                                                 {
                                                   n_red_pix_rect = temp.CountNonzero()[0];
                                                 }
                                                 redPixel += n_red_pix_rect;

                                                 // Draw rectangle in red for no matched REM eye position for area lower than "AreaMinThreshold"
                                                 CvInvoke.Rectangle(display, rect, new MCvScalar(0, 0, 255), 2);
                                                 //CvInvoke.DrawContours(display, contours, i, new MCvScalar(0, 0, 255), 2);
                                                 RectangleArea rectAreaRed = new RectangleArea(rect.X, rect.Y, rect.Width, rect.Height, area, n_red_pix_rect, ar);
                                                 rectArchAreaRed.Add(rectAreaRed);
                                                 ArchAreaRed.Add(area);*/
                                                //DisplayingContour(Frame, display, true);  // Debugging

                                                // add information to tell rectangle detected
                                                contourMultiFrameDetected = true;
                                            }
                                        }
                                    }
                                }


                                // for average detection
                                m_arrDetectedFaceMove.Add((int)changedPixels);
                                m_arrDetectedAverageNotification.Add((int)changedPixels);
                                m_AverageFaceTracker.Add((int)changedPixels);

                                // for debugging
                                //foreach (var item in rectArchAreaRed)
                                //{
                                //  WriteOutput("rect detected count: " + rectArchAreaRed.Count + " - area: " + item.Area + " - X: " + item.X + " - Y: " + item.Y + " - ratio: " + item.Ar, false);
                                //}

                                //// for debugging
                                //int rectDiff = Math.Abs(rectArchAreaRed.Count - rectArchAreaGreen.Count);

                                // set higher value for vREM analyse code (to lower blinks)
                                if (rectArchAreaRed.Count >= vREMResetFull)
                                {
                                    diff += 0.555;
                                }
                                else if (ArchAreaRed.Count >= vREMResetFull)
                                {
                                    diff += 0.555;
                                }
                                else if (Math.Abs(diff - changedPixels) >= (int)PixelThresholdDiff && Math.Abs(diff - changedPixels) <= 150 && diff < (int)PixelThresholdDiff && diff > 0)
                                {
                                    // this code is for very little movement
                                    diff += 0.555;
                                    logStop = true;
                                }

                                Console.WriteLine(diff);
                            }

                            catch (Exception Ex)
                            {
                                Console.WriteLine(Ex.Message);
                            }
                        }
                    }
                }

                #endregion
            }
            return contourMultiFrameDetected;
        }

        private Image<Bgr, byte> DisplayingContour(Image<Bgr, byte> DifferenceImage, Image<Bgr, byte> display, bool displayOnSource = false)
        {
            try
            {
                if (result != null)
                {
                    result?.Dispose();
                    result = null;
                }
                result = new Image<Bgr, byte>(DifferenceImageRect.Width, DifferenceImageRect.Height);

                if (additionFramediff != null)
                {
                    additionFramediff?.Dispose();
                    additionFramediff = null;
                }
                additionFramediff = new Image<Bgr, byte>(resolutionX, resolutionY);

                if (display != null)
                {
                    CvInvoke.Add(DifferenceImage, display, result);
                }
                // Display with background image and detect hole
                // add previous frame to addition frame
                CvInvoke.Add(result, Frame, additionFramediff);

                // return mixed frame
                return additionFramediff;
            }
            catch
            {
                // return mixed frame
                return additionFramediff;
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private void VREMAnalyse(double diff, bool multiFrame)
        {
            try
            {
                FaceRectChange = 0;

                //Display stats
                var displayStats = Convert.ToInt32(diff);
                //if (EnableStats)
                //{
                //    SetTextTimer(DateTime.Now.ToString("yyy-MM-dd HH:mm:ss - ") + displayStats);
                //    SetTextLblMov("Detected movement: " + displayStats);
                //}

                //display the previous image using thread safe call
                //if (cameraCapture != null && pbDifferenceCheckBox.Checked)
                //{
                //    Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                //    {
                //        pbDifference.Image = Previous_Frame.ToBitmap();
                //    }));
                //}
                //DisplayImageDifference(Previous_Frame.ToBitmap(), pbDifference);

                if (ImageDifferenceLd != null && ImageDifferenceLd.Ptr != IntPtr.Zero)
                {
                    //display the absolute difference
                    if (cameraCapture != null)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                        {
                            using (resultImage = new Image<Bgr, byte>(resolutionX, resolutionY))
                            {
                                ImageDifferenceLd.CopyTo(resultImage);
                                pbDifference.Image = resultImage.ToBitmap();
                            }
                        }));
                    }
                    //DisplayImageResult(resultImage.ToBitmap(), resultbox);
                }

                //if (CurrentDevices.Equals("Video File"))
                //{
                //    // offset information
                //    if ((int)numericUpDownOffsetH.Value != 0 || (int)numericUpDownOffsetM.Value != 0 || (int)numericUpDownOffsetS.Value != 0)
                //    {
                //        SetTextVideo("Video: " + displayTime + " - Recording offset: " + displayTimeOffsetString);
                //    }
                //    else
                //    {
                //        SetTextVideo("Video: " + displayTime);
                //    }
                //}
            }
            catch (AccessViolationException ex)
            {
                //WriteOutput("VREMAnalyse: " + ex.Message);
                Console.WriteLine(ex);
            }
            catch (Exception ex)
            {
                //WriteOutput("VREMAnalyse: " + ex.Message);
                Console.WriteLine(ex);
            }
        }


        internal int vREMCodeExecution(bool validate, double diff)
        {
            // Notification in progress don't fill vREM algo
            //while (notificationInProgress)
            //{
            //    // wait until sound if finished
            //    if (vREMLogOnce)
            //    {
            //        WriteOutput("Notification in progress clear and restore vREM when finished", true, false);
            //        vREMLogOnce = false;
            //    }

            //    // Clear current REM algo
            //    m_arrHistory.Clear();
            //    vREM = 0;
            //    Value = 0;
            //    return 0;
            //}

            // set checkBoxSlowMovement to true to use new vREM algo
            int vREMRange = 256;
            int intBelowAlgo1 = 6; // idleTicks
            int intBelowAlgo2 = 256;

            if (vREMSlowMovement)
            {
                vREMRange = 1024;
                intBelowAlgo1 = 2; // idleTicks
                intBelowAlgo2 = 64;
            }

            bool boolDreaming = false;
            Value = diff;
            if (cmbAlgorithmText == "Motion Detector")
            {
                if (Value >= FrameThreshold)
                {
                    boolDreaming = true;
                }
            }
            else if (cmbAlgorithmText == "REM Detector" && !bypassFrame)
            {
                // Update the mem list
                //m_arrHistory.Add(Convert.ToInt32(Device.GetVision()));

                if (MovementBreathDetected)
                {
                    Value = FrameThresholdH + 10;
                }

                //if (bypassFrame)
                //{
                //  Value = 10500;
                //}

                var ValueDB = Value;
                var decPart = (int)(((decimal)Value % 1) * 1000);
                if (decPart == 555)
                {
                    ValueDB = 10500;
                }

                // Notification advert in progress freeze vREM algo (default 10s)
                //while (DateTime.Now.Subtract(m_dtNowSpeechAdvert).TotalSeconds <= AdvertTimeOut)
                //{
                //    int vREMValidate = vREMCodeValidate(true, ValueDB);
                //    if (vREMValidate != 888)
                //    {
                //        m_arrHistoryAdvert.Add(ValueDB);
                //        if (m_arrHistoryAdvert.Count > vREMRange)
                //        {
                //            m_arrHistoryAdvert.RemoveAt(0);
                //        }
                //        vREMValidateLogOnce = true;
                //    }
                //    else
                //    {
                //        if (vREMValidateLogOnce)
                //        {
                //            WriteOutput("vREMValidate: " + vREMValidate + " - ADVERT count: " + messageAdvertCount, true, true);
                //            vREMValidateLogOnce = false;
                //        }
                //    }

                //    // reset trigger delay notification after 60 secondes
                //    if (vREMValidateResult == 888 && messageAdvertCount > 0 &&
                //      DateTime.Now.Subtract(m_dtNowSpeechFirstAdvert).TotalSeconds >= 60 && m_triggerDreamingREM)
                //    {
                //        m_triggerDreamingREM = false;
                //        m_triggerAdvertDreamingREM = true;
                //    }

                //    //// execute analysing code to log vREMValidateResult change
                //    //TmrUpdateDevice(true);

                //    return 0;
                //}

                // trigger the vREM = 888
                if (vREMValidateResult == 888)
                {
                    boolDreaming = true;
                }

                //int eyeMoveMin = Device.GetEyeMoveMin();
                //int eyeMoveMax = Device.GetEyeMoveMax();
                int idleTicks = IdleTicks;

                // add false detection to vREM algo (default 10s)
                //if (notificationAdvertInProgress && DateTime.Now.Subtract(m_dtNowSpeechAdvert).TotalSeconds > AdvertTimeOut)
                //{
                //    notificationAdvertInProgress = false;
                //    messageAdvertCount = 0;
                //    WriteOutput("vREM restore current detection Notification", true, false);

                //    m_arrHistory = new List<double>(m_arrHistoryAdvert);
                //    m_arrHistoryAdvert.Clear();

                //    // clear
                //    m_arrHistory.Clear();

                //    // restore current detection
                //    m_arrHistory.Add(ValueDB);
                //    if (m_arrHistory.Count > vREMRange)
                //    {
                //        m_arrHistory.RemoveAt(0);
                //    }
                //}
                //else
                //{
                //    if (Device.GetTossValue() > 0)
                //    {
                //        // clear
                //        m_arrHistory.Clear();
                //        WriteOutput("Toss clear history with red pixels value: " + Convert.ToInt32(redPixel), true, false);
                //        return 0;
                //    }

                //    m_arrHistory.Add(ValueDB);
                //    if (m_arrHistory.Count > vREMRange)
                //    {
                //        m_arrHistory.RemoveAt(0);
                //    }
                //}

                if (!bypassFrame && !boolDreaming)
                {
                    // Check for blinks
                    int intBlinks = 0;
                    bool boolBlinking = false;

                    int intBelow = 0;
                    int intAbove = 0;

                    for (int i = 0; i < m_arrHistory.Count; i++)
                    {
                        double dblValue = m_arrHistory[i];
                        //bool overMax = false;

                        //// Check if the last 10 or next 10 were 1000
                        //// it's reset below blink or reset it to 0
                        int lastOrNextOver4000 = 0;
                        for (int l = i; l > 0 & l > i - 10; l--)
                        {
                            if (m_arrHistory[l] > 9999 || m_arrHistory[l] > FrameThresholdH)
                            //if (m_arrHistory[l] > eyeMoveMax || m_arrHistory[l] > FrameThresholdH)
                            {
                                //overMax = true;
                                //break;
                                lastOrNextOver4000++;
                            }
                        }

                        for (int n = i; n < m_arrHistory.Count & n < i + 10; n++)
                        {
                            if (m_arrHistory[n] > 9999 || m_arrHistory[n] > FrameThresholdH)
                            //if (m_arrHistory[n] > eyeMoveMax || m_arrHistory[n] > FrameThresholdH)
                            {
                                //overMax = true;
                                //break;
                                lastOrNextOver4000++;
                            }
                        }

                        //// Check if the last 10 or next 10 were over FrameThresholdH
                        //// it's reset below blink or reset it to 0
                        //for (int l = i; l > 0 & l > i - 10; l--)
                        //{
                        //  if (m_arrHistory[l] > FrameThresholdH)
                        //  {
                        //    lastOrNextOver1000++;
                        //    //WriteOutput("l - " + l);
                        //  }
                        //}

                        //for (int n = i; n < m_arrHistory.Count & n < i + 10; n++)
                        //{
                        //  if (m_arrHistory[n] > FrameThresholdH)
                        //  {
                        //    lastOrNextOver1000++;
                        //    //WriteOutput("n - " + n);
                        //  }
                        //}

                        if (lastOrNextOver4000 == 0) //if (!overMax)
                        {
                            //if (dblValue > eyeMoveMin & dblValue < eyeMoveMax)
                            if (dblValue > FrameThreshold & dblValue < FrameThresholdH)
                            {
                                intAbove += 1;
                                intBelow = 0;
                                //WriteOutput("intAbove - " + intAbove + "dblValue - " + dblValue);
                            }
                            else
                            {
                                intBelow += 1;
                                intAbove = 0;
                                //if (intBelow > 0)
                                //WriteOutput("intBelow - " + intBelow);
                            }
                        }

                        if (lastOrNextOver4000 > 10)
                        {
                            intBlinks = 0;
                            //WriteOutput("lastOrNextOver1000 > 10");
                        }

                        if (!boolBlinking)
                        {
                            if (intAbove >= 1)
                            {
                                boolBlinking = true;
                                intBlinks += 1;
                                intAbove = 0;
                                intBelow = 0;
                                //WriteOutput("boolBlinking - intBlinks - "+ intBlinks);
                            }
                        }
                        else
                        {
                            // speed detection - reduce will get faster and value updated faster goes to 0 more quickly
                            if (intBelow >= idleTicks) //intBelowAlgo1) // default Algo1 = 6
                            {
                                boolBlinking = false;
                                intBelow = 0;
                                intAbove = 0;
                                //WriteOutput("intBelow >= 24 - " + intBelow);
                            }
                            else
                            {
                                // speed detection - reduce will get faster and value updated faster goes to 0 more quickly
                                if (intAbove >= 24)
                                {
                                    // reset
                                    boolBlinking = false;
                                    intBlinks = 0;
                                    intBelow = 0;
                                    intAbove = 0; // Todo
                                                  //WriteOutput("intAbove >= 12 - " + intBelow);
                                }
                            }
                        }

                        if (intBlinks > 8)
                        {
                            if (bypassFrame)
                            {
                                intBlinks = 8;
                            }
                            else
                            {
                                //WriteOutput("boolDreaming - " + intBlinks);
                                boolDreaming = true;
                                break;
                            }
                        }

                        if (intAbove > 24)
                        {
                            // reset
                            boolBlinking = false;
                            intBlinks = 0;
                            intBelow = 0;
                            intAbove = 0;
                            //WriteOutput("intAbove > 12 - " + intAbove);
                        }

                        // Reset to 0 (higher value will delay it)
                        // 8 seconds for 64 when "m_arrHistory.Count = 256"
                        // 16 seconds for 128 when "m_arrHistory.Count = 256"
                        if (intBelow > intBelowAlgo2) // default algo1 = 256
                        {
                            // reset
                            //WriteOutput("intBelow > " + " intBelowAlgo2 - " + intBelow);
                            boolBlinking = false;
                            if (!vREMSlowMovement)
                            {
                                intBlinks = 0;
                            }
                            intBelow = 0;
                            intAbove = 0;
                        }
                    }

                    if (boolDreaming && !MovementBreathDetected)
                    {
                        if (validate)
                        {
                            vREMResult = 888;
                            vREMLogOnce = true;
                        }
                        else
                        {
                            vREM = 888;
                            m_arrHistory.Clear();
                            vREMLogOnce = true;
                            //detectedRectAreaMaxPass = 0;
                            //detectedRectAreaMinPass = 0;
                        }
                    }
                    else
                    {
                        // For counting vREM at 888
                        Detected888 = false;
                        intBlinks = Math.Min(10, intBlinks);

                        if (validate)
                        {
                            vREMResult = intBlinks * 100;
                        }
                        else
                        {
                            vREM = intBlinks * 100;
                        }
                    }

                    if (MovementBreathDetected)
                    {
                        // Reset bool movement
                        MovementBreathDetected = false;
                    }
                }
            }

            // for Motion Detector
            if (boolDreaming)
            {
                if (validate)
                {
                    vREMResult = 888;
                    vREMLogOnce = true;
                }
                else
                {
                    vREM = 888;
                    if (!Detected888)
                    {
                        CountVrem++;
                        Detected888 = true;
                    }

                    // reset vREMValidateResult when notification is trigger
                    if (vREMValidateResult == 888)
                    {
                        vREMValidateResult = 0;
                    }
                    vREMLogOnce = true;
                }
            }

            return vREMResult;
        }

        private void FindEyePosition(Mat Image, bool DNN = false)
        {
            try
            {
                // TFT
                using (Image)
                {
                    if (resizedFrame != null)
                    {
                        resizedFrame.Dispose();
                        resizedFrame = null;
                        resizedFrame = new Mat();
                    }
                    else
                    {
                        resizedFrame = new Mat();
                    }

                    CvInvoke.Resize(Image, resizedFrame, new System.Drawing.Size(detectionSizeTFT, detectionSizeTFT), 0, 0);

                    if (!DNN)
                    {
                        if (session != null && graph != null)
                        {
                            //// code to test execution delay
                            //var timer = Stopwatch.StartNew();
                            using (TFTensor tensor = TransformInput(resizedFrame.ToBitmap()))
                            {
                                TFSession.Runner runner = session.GetRunner();
                                runner.AddInput(graph["image"][0], tensor);
                                runner.Fetch(
                                    graph["heatmap"][0],
                                    graph["offset_2"][0],
                                    graph["displacement_fwd_2"][0],
                                    graph["displacement_bwd_2"][0]
                                );

                                var resultTFT = runner.Run();

                                //// log execution delay
                                //timer.Stop();
                                //Debug.WriteLine($"Detected boxes in {timer.ElapsedMilliseconds} ms");

                                var heatmap = (float[,,,])resultTFT[0].GetValue(jagged: false);
                                var offsets = (float[,,,])resultTFT[1].GetValue(jagged: false);
                                var displacementsFwd = (float[,,,])resultTFT[2].GetValue(jagged: false);
                                var displacementsBwd = (float[,,,])resultTFT[3].GetValue(jagged: false);

                                Pose[] poses = posenet.DecodeMultiplePoses(
                                           heatmap, offsets,
                                           displacementsFwd,
                                           displacementsBwd,
                                           outputStride: 16, maxPoseDetections: 2,
                                           scoreThreshold: 0.5f, nmsRadius: 20);

                                Drawing(Frame.Mat, poses);

                                // release for avoid memory leak
                                tensor.Dispose();
                                // release output (result) tensor
                                foreach (var item in resultTFT)
                                {
                                    item.Dispose();
                                }
                            }
                        }
                    }
                    else
                    {
                        // not used
                        //var frameWidth = resizedFrame.Cols;
                        //var frameHeight = resizedFrame.Rows;
                        //var aspect_ratio = frameWidth / (double)frameHeight;
                        //var inHeight = frameHeight;
                        //var inWidth = System.Convert.ToDouble(aspect_ratio * inHeight * 8) / (double)8;
                        //using (Mat blobs = DnnInvoke.BlobFromImage(Frame, 1.0 / 255, new System.Drawing.Size((int)inWidth, inHeight), new MCvScalar(127.5, 127.5, 127.5), true, false))
                        //using (Mat blobs = DnnInvoke.BlobFromImage(resizedFrame/*.ToImage<Bgr,byte>()*/, 1.0 / 255, new System.Drawing.Size(resizedFrame.Width, resizedFrame.Height), new MCvScalar(127.5, 127.5, 127.5), true, false))

                        using (Mat blobs = DnnInvoke.BlobFromImage(resizedFrame, 1.0 / 255, new Size(detectionSizeTFT, detectionSizeTFT), default, false, false))
                        {
                            net.SetInput(blobs);

                            using (var output1 = net.Forward("heatmap"))
                            using (var output2 = net.Forward("Conv2D_1"))
                            using (var output3 = net.Forward("Conv2D_2"))
                            using (var output4 = net.Forward("Conv2D_3"))
                            {
                                // new test
                                float[,,,] heatmap = output1.GetData() as float[,,,];
                                float[,,,] offsets = output2.GetData() as float[,,,];
                                float[,,,] displacementsFwd = output3.GetData() as float[,,,];
                                float[,,,] displacementsBwd = output4.GetData() as float[,,,];

                                Pose[] poses = posenet.DecodeMultiplePoses(
                                                           heatmap, offsets,
                                                           displacementsFwd,
                                                           displacementsBwd,
                                                           outputStride: 16, maxPoseDetections: 100,
                                                           scoreThreshold: 0.5f, nmsRadius: 20);

                                Drawing(Frame.Mat, poses);
                                //DisplayImageSource(Frame.ToBitmap(), pbSource);
                            }
                        }

                    }

                    GC.Collect();
                    GC.Collect();
                    GC.Collect();
                    GC.Collect();
                }

            }
            catch (Exception ex)
            {
                // release memory leak
                if (resizedFrame != null)
                {
                    resizedFrame.Dispose();
                    resizedFrame = null;
                }

                if (!posenet.DNN)
                {   // load Tensor for new usage
                    LoadTensorFlow();
                }

                //GC.Collect();
                //GC.Collect();
                //GC.Collect();
                //GC.Collect();

                //GC.WaitForPendingFinalizers();
                Console.Write(ex.Message);
            }
        }
        private TFTensor TransformInput(Bitmap bitmap)
        {
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var length = bitmapData.Stride * bitmapData.Height;

            byte[] bytes = new byte[length];

            int strideWithoutReserved = bitmapData.Stride - bitmapData.Reserved;

            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bitmap.UnlockBits(bitmapData);

            float[] floatValues = new float[bitmap.Width * bitmap.Height * 3];

            int idx = 0;

            for (int i = 0; i < bytes.Length; i++)
            {
                if (i == strideWithoutReserved)
                {
                    //Reserved byte.
                    continue;
                }

                if ((i - strideWithoutReserved) % bitmapData.Stride == 0)
                {
                    //Reserved byte.
                    continue;
                }

                floatValues[idx] = bytes[i] * (2.0f / 255.0f) - 1.0f;
                idx++;
            }

            TFShape shape = new TFShape(1, bitmap.Width, bitmap.Height, 3);
            bitmap.Dispose();
            return TFTensor.FromBuffer(shape, floatValues, 0, floatValues.Length);
        }
        private void Drawing(Mat image, Pose[] poses, bool poseChain = false)
        {
            noseKeyLost = false;
            if (poses.Length > 0)
            {
                var xRateEye = image.Width / (float)detectionSizeTFT;
                var yRateEye = image.Height / (float)detectionSizeTFT;
                var ratio = (float)image.Width / (float)image.Height;

                using (Graphics g = Graphics.FromImage(image.ToBitmap()))
                {
                    for (int i = 0; i < 1; i++)
                    {
                        Pose pose = poses[i];

                        float score = (float)ScoreTF;

                        if (pose.score > score)
                        {
                            for (int j = 0;
                                j < (poseChain ? posenet.poseChain.GetLength(0) : jointPairs.GetLength(0));
                                j++)
                            {
                                rightEye = new Keypoint();
                                leftEye = new Keypoint();
                                noseKey = new Keypoint();

                                // detect correct point (eye left/right and nose) for Item1 and 2
                                // Item1
                                if (posenet.poseChain[j].Item1.Equals("rightEye"))
                                {
                                    rightEye = pose.keypoints.FirstOrDefault(item => item.part.Equals(
                                        posenet.poseChain[j].Item1));
                                }
                                else if (posenet.poseChain[j].Item1.Equals("leftEye"))
                                {
                                    leftEye = pose.keypoints.FirstOrDefault(item => item.part.Equals(
                                        posenet.poseChain[j].Item1));
                                }
                                else if (posenet.poseChain[j].Item1.Equals("nose"))
                                {
                                    noseKey = pose.keypoints.FirstOrDefault(item => item.part.Equals(
                                        posenet.poseChain[j].Item1));
                                }

                                // Item2
                                if (posenet.poseChain[j].Item2.Equals("rightEye"))
                                {
                                    rightEye = pose.keypoints.FirstOrDefault(item => item.part.Equals(
                                        posenet.poseChain[j].Item2));
                                }
                                else if (posenet.poseChain[j].Item2.Equals("leftEye"))
                                {
                                    leftEye = pose.keypoints.FirstOrDefault(item => item.part.Equals(
                                        posenet.poseChain[j].Item2));
                                }
                                else if (posenet.poseChain[j].Item2.Equals("nose"))
                                {
                                    noseKey = pose.keypoints.FirstOrDefault(item => item.part.Equals(
                                        posenet.poseChain[j].Item2));
                                }

                                // draw righteye
                                if (!rightEye.IsEmpty && rightEye.score >= score)
                                {
                                    eyeRight = new System.Windows.Point(rightEye.position.X * xRateEye, rightEye.position.Y * yRateEye);
                                    //g.DrawRectangle(jointColorRightGreen, rightEye.position.X * xRateEye - 16, rightEye.position.Y * yRateEye - 16, 32, 32);
                                    CurrentFrameCount = 0;
                                }
                                // draw lefteye
                                if (!leftEye.IsEmpty && leftEye.score >= score)
                                {
                                    eyeLeft = new System.Windows.Point(leftEye.position.X * xRateEye, leftEye.position.Y * yRateEye);
                                    //g.DrawRectangle(jointColorRightGreen, leftEye.position.X * xRateEye - 16, leftEye.position.Y * yRateEye - 16, 32, 32);
                                    CurrentFrameCount = 0;
                                }
                                // draw nose
                                if (!noseKey.IsEmpty && noseKey.score >= score)
                                {
                                    nose = new System.Windows.Point(noseKey.position.X * xRateEye, noseKey.position.Y * yRateEye);
                                    //g.DrawEllipse(jointColorNose, noseKey.position.X * xRateEye, noseKey.position.Y * yRateEye, 3, 3);
                                    if (nose.X > 0 && nose.Y > 0 && nose.X < image.Cols && nose.Y < image.Rows)
                                    {
                                        CurrentFrameAverageCountToClear = 0;
                                        noseKeyLost = false;
                                    }
                                    else
                                    {
                                        // Nose position lost
                                        noseKeyLost = true;
                                    }
                                }
                                else
                                {
                                    // Nose position lost
                                    noseKeyLost = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Nose position lost
                noseKeyLost = true;
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
                defaultSettings += "<TossThreshold>2000</TossThreshold>";
                defaultSettings += "<TossHalfLife>10</TossHalfLife>";
                defaultSettings += "<EyeMoveMin>20</EyeMoveMin>";
                defaultSettings += "<EyeMoveMax>800</EyeMoveMax>";
                defaultSettings += "<IdleTicks>8</IdleTicks>";
                defaultSettings += "<IgnorePercentage>100</IgnorePercentage>";
                defaultSettings += "<CopyFromScreen>0</CopyFromScreen>";
                defaultSettings += "<RecordVideo>0</RecordVideo>";
                defaultSettings += "<TCMP>0</TCMP>";
                defaultSettings += "<Auralize>0</Auralize>";
                defaultSettings += "<DotThreshold>200</DotThreshold>";
                defaultSettings += "<DashThreshold>600</DashThreshold>";
                defaultSettings += "<Classifier>None</Classifier>";
                defaultSettings += "<WaveForm>Triangle</WaveForm>";
                defaultSettings += "<Volume>0</Volume>";
                defaultSettings += "<RotateFlip>RotateNoneFlipNone</RotateFlip>";
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

            if (xmlSettings.DocumentElement.SelectSingleNode("//Volume") != null)
            {
                VolumeTrackBar.Value = Convert.ToInt32(xmlSettings.DocumentElement.SelectSingleNode("//Volume").InnerText);
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//CopyFromScreen") != null && xmlSettings.DocumentElement.SelectSingleNode("//CopyFromScreen").InnerText == "1")
            {
                chkCopyFromScreen.Checked = true;
                CopyFromScreen = true;
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

            if (xmlSettings.DocumentElement.SelectSingleNode("//WaveForm") != null)
            {
                cmbWaveForm.Text = xmlSettings.DocumentElement.SelectSingleNode("//WaveForm").InnerText;
            }
            else
            {
                cmbWaveForm.Text = "Triangle";
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//RotateFlip") != null)
            {
                cmbRotateFlip.Text = xmlSettings.DocumentElement.SelectSingleNode("//RotateFlip").InnerText;
            }
            else
            {
                cmbRotateFlip.Text = "RotateFlip";
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

            if (xmlSettings.DocumentElement.SelectSingleNode("//Auralize") != null && xmlSettings.DocumentElement.SelectSingleNode("//Auralize").InnerText == "1")
            {
                chkAuralize.Checked = true;
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//Width") != null)
            {
                this.Width = Convert.ToInt32(xmlSettings.DocumentElement.SelectSingleNode("//Width").InnerText);
            }

            if (xmlSettings.DocumentElement.SelectSingleNode("//Height") != null)
            {
                this.Height = Convert.ToInt32(xmlSettings.DocumentElement.SelectSingleNode("//Height").InnerText);
            }

            Console.WriteLine($"TossThreshold:{tossThresholdInput.Value}");
            Console.WriteLine($"DetectREM:{DetectREM}");
            Console.WriteLine($"TossHalfLife:{tossHalfLifeInput.Value}");
            Console.WriteLine($"EyeMoveMin:{eyeMoveMinInput.Value}");
            Console.WriteLine($"EyeMoveMax:{eyeMoveMaxInput.Value}");
            Console.WriteLine($"IdleTicks:{idleTicksInput.Value}");
            Console.WriteLine($"DashThreshold:{dashThresholdInput.Value}");
            Console.WriteLine($"DotThreshold:{dotThresholdInput.Value}");
            Console.WriteLine($"TCMP:{chkTCMP.Checked}");
            Console.WriteLine($"Auralize:{chkAuralize.Checked}");
            Console.WriteLine($"WaveForm:{WaveForm}");
        }

        private void cmbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDevices.Text == "lucidcode Halovision Device" ||
                cmbDevices.Text == "lucidcode INSPEC")
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
            if (cmbDevices.SelectedText == "lucidcode INSPEC")
            {
                ConnectInspecDevice();
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
                        player.Volume = VolumeTrackBar.Value;
                        
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
                    player.Volume = VolumeTrackBar.Value;

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

        private void ConnectInspecDevice()
        {
            try
            {
                Core.Initialize();

                libvlc = new LibVLC(enableDebugLogs: false);
                using (Media media = new Media(libvlc, txtDeviceURL.Text, FromType.FromLocation))
                {
                    player = new MediaPlayer(media);
                    player.Hwnd = pbDisplay.Handle;
                    player.Volume = VolumeTrackBar.Value;
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LucidScribe.InitializePlugin()", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DisconnectInspecDevice()
        {
            try
            {
                player.Stop();
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

            if (CopyFromScreen)
            {
                using (Graphics graphics = Graphics.FromImage(bmp))
                {
                    graphics.CopyFromScreen(this.Location.X + 26, this.Location.Y + 71, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
                    graphics.Dispose();
                }
            }
            else
            {
                pbDisplay.DrawToBitmap(bmp, new Rectangle(0, 0, pbDisplay.Width, pbDisplay.Height));
            }
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
            lblTime.Text = DateTime.Now.ToString("yyy-MM-dd hh:mm:ss - ") + diff;
            return;
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
                    //Image<Bgr, byte> imageFrame = new Image<Bgr, Byte>(Frame.ToBitmap());
                    //Image<Gray, byte> grayFrame = imageFrame.Convert<Gray, byte>();
                    //faceRegions = cascadeClassifier.DetectMultiScale(grayFrame);
                    //DetectREM = faceRegions != null && faceRegions.Length > 0;
                    //Console.WriteLine($"DetectREM:{DetectREM}");
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
                    bool saveSnapshot = cascadeClassifier == null && diff >= EyeMoveMin;
                    if (cascadeClassifier != null && faceRegions != null && faceRegions.Length > 0 && diff > 1)
                    {
                        saveSnapshot = true;
                    }
                    if (saveSnapshot)
                    {
                        CreateDirectories();
                        String secondFile = $"{lucidScribeDataPath}\\Days\\{Strings.Format(DateTime.Now, "yyyy")}\\{Strings.Format(DateTime.Now, "MM")}\\{Strings.Format(DateTime.Now, "dd")}\\{Strings.Format(DateTime.Now, "HH")}\\{Strings.Format(DateTime.Now, "mm")}\\{Strings.Format(DateTime.Now, "ss.")}{DateTime.Now.Millisecond}.jpg";
                        pbDifference.Image.Save(secondFile, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
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
        byte highlight = 200;
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
            bool staticBorders = true;

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
                        // top
                        byte* row2 = (byte*)bmd2.Scan0 + (region.Y * bmd2.Stride);
                        for (int x = region.X; x <= region.X + region.Width; x++)
                        {
                            if (!staticBorders || staticBorders && random.Next(2) == 1)
                            {
                                row2[x * PixelSize] = highlight;
                                row2[x * PixelSize + 1] = highlight;
                                row2[x * PixelSize + 2] = highlight;
                            }
                        }

                        // top highlight
                        if (region.Y > 0)
                        {
                            byte* row2Highlight = (byte*)bmd2.Scan0 + ((region.Y - 1) * bmd2.Stride);
                            for (int x = region.X + 1; x <= region.X + region.Width - 1; x++)
                            {
                                if (!staticBorders || staticBorders && random.Next(2) == 1)
                                {
                                    row2[x * PixelSize] = highlight;
                                    row2[x * PixelSize + 1] = highlight;
                                    row2[x * PixelSize + 2] = highlight;
                                }
                            }
                        }

                        // bottom
                        if (region.Y + region.Height < bitmap2.Height)
                        {
                            row2 = (byte*)bmd2.Scan0 + ((region.Y + region.Height) * bmd2.Stride);
                            for (int x = region.X; x <= region.X + region.Width; x++)
                            {
                                if (!staticBorders || staticBorders && random.Next(2) == 1)
                                {
                                    row2[x * PixelSize] = highlight;
                                    row2[x * PixelSize + 1] = highlight;
                                    row2[x * PixelSize + 2] = highlight;
                                }
                            }
                        }

                        // bottom highlight
                        if (region.Y + region.Height + 1 < bitmap2.Height)
                        {
                            row2 = (byte*)bmd2.Scan0 + ((region.Y + region.Height + 1) * bmd2.Stride);
                            for (int x = region.X + 1; x <= region.X + region.Width - 1; x++)
                            {
                                if (!staticBorders || staticBorders && random.Next(2) == 1)
                                {
                                    row2[x * PixelSize] = highlight;
                                    row2[x * PixelSize + 1] = highlight;
                                    row2[x * PixelSize + 2] = highlight;
                                }
                            }
                        }


                        for (int y = region.Y; y <= region.Y + region.Height; y++)
                        {
                            if (y < bitmap2.Height)
                            {
                                row2 = (byte*)bmd2.Scan0 + ((y) * bmd2.Stride);

                                // left
                                if (!staticBorders || staticBorders && random.Next(2) == 1)
                                {
                                    row2[region.X * PixelSize] = highlight;
                                    row2[region.X * PixelSize + 1] = highlight;
                                    row2[region.X * PixelSize + 2] = highlight;
                                }

                                // left highlight
                                if (y > region.Y && y < region.Y + region.Height)
                                {
                                    if (!staticBorders || staticBorders && random.Next(2) == 1)
                                    {
                                        row2[(region.X - 1) * PixelSize] = highlight;
                                        row2[(region.X - 1) * PixelSize + 1] = highlight;
                                        row2[(region.X - 1) * PixelSize + 2] = highlight;
                                    }
                                }

                                // right
                                if (!staticBorders || staticBorders && random.Next(2) == 1)
                                {
                                    row2[(region.X + region.Width) * PixelSize] = highlight;
                                    row2[(region.X + region.Width) * PixelSize + 1] = highlight;
                                    row2[(region.X + region.Width) * PixelSize + 2] = highlight;
                                }

                                // right highlight
                                if (y > region.Y && y < region.Y + region.Height)
                                {
                                    if (!staticBorders || staticBorders && random.Next(2) == 1)
                                    {
                                        row2[(region.X + region.Width) * PixelSize + 1] = highlight;
                                        row2[(region.X + region.Width + 1) * PixelSize + 2] = highlight;
                                        row2[(region.X + region.Width + 2) * PixelSize + 3] = highlight;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            bitmap1.UnlockBits(bmd1);
            bitmap2.UnlockBits(bmd2);

            bmd1 = null;
            bmd2 = null;

            diff = differences;

            double percentage = (changedPixels / totalPixels) * 100;
            if (percentage > IgnorePercentage)
            {
                diff = 0;
            }
        }

        private void cmbAlgorithm_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbAlgorithmText = cmbAlgorithm.Text;
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
            settings += "<Volume>" + VolumeTrackBar.Value + "</Volume>";
            settings += "<Width>" + this.Width + "</Width>";
            settings += "<Height>" + this.Height + "</Height>";
            settings += "<IgnorePercentage>" + cmbIgnorePercentage.Text + "</IgnorePercentage>";

            if (chkCopyFromScreen.Checked)
            {
                settings += "<CopyFromScreen>1</CopyFromScreen>";
            }
            else
            {
                settings += "<CopyFromScreen>0</CopyFromScreen>";
            }

            if (chkRecordVideo.Checked)
            {
                settings += "<RecordVideo>1</RecordVideo>";
            }
            else
            {
                settings += "<RecordVideo>0</RecordVideo>";
            }
            
            settings += "<Classifier>" + cmbClassifier.Text + "</Classifier>";
            settings += "<WaveForm>" + cmbWaveForm.Text + "</WaveForm>";
            settings += "<RotateFlip>" + cmbRotateFlip.Text + "</RotateFlip>";

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

            if (chkAuralize.Checked)
            {
                settings += "<Auralize>1</Auralize>";
            }
            else
            {
                settings += "<Auralize>0</Auralize>";
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
            StopProcess();
        }

        private void chkTCMP_CheckedChanged(object sender, EventArgs e)
        {
            TCMP = chkTCMP.Checked;
            Console.WriteLine($"TCMP:{TCMP}");
            SaveSettings();
        }

        private void chkAuralize_CheckedChanged(object sender, EventArgs e)
        {
            Auralize = chkAuralize.Checked;
            Console.WriteLine($"Auralize:{chkAuralize.Checked}");
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
            Console.WriteLine($"TossThreshold:{TossThreshold}");
            SaveSettings();
        }

        private void tossHalfLife_ValueChanged(object sender, EventArgs e)
        {
            TossHalfLife = (int)tossHalfLifeInput.Value;
            Console.WriteLine($"TossHalfLife:{TossHalfLife}");
            SaveSettings();
        }

        private void eyeMoveMinInput_ValueChanged(object sender, EventArgs e)
        {
            EyeMoveMin = (int)eyeMoveMinInput.Value;
            Console.WriteLine($"EyeMoveMin:{EyeMoveMin}");
            SaveSettings();
        }

        private void eyeMoveMaxInput_ValueChanged(object sender, EventArgs e)
        {
            EyeMoveMax = (int)eyeMoveMaxInput.Value;
            Console.WriteLine($"EyeMoveMax:{EyeMoveMax}");
            SaveSettings();
        }

        private void idleTicksInput_ValueChanged(object sender, EventArgs e)
        {
            IdleTicks = (int)idleTicksInput.Value;
            Console.WriteLine($"IdleTicks:{IdleTicks}");
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
            Console.WriteLine($"DotThreshold:{DotThreshold}");
            SaveSettings();
        }

        private void dashThresholdInput_ValueChanged(object sender, EventArgs e)
        {
            DashThreshold = (int)dashThresholdInput.Value;
            Console.WriteLine($"DashThreshold:{DashThreshold}");
            SaveSettings();
        }

        private void chkCopyFromScreen_CheckedChanged(object sender, EventArgs e)
        {
            CopyFromScreen = chkCopyFromScreen.Checked;
            SaveSettings();
        }

        private void cmbWaveForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cmbWaveForm.Text)
            {
                case "Sin":
                    WaveForm = WaveType.Sin;
                    break;
                case "SawTooth":
                    WaveForm = WaveType.SawTooth;
                    break;
                case "Sqaure":
                    WaveForm = WaveType.Square;
                    break;
                case "Triangle":
                    WaveForm = WaveType.Triangle;
                    break;
                case "Sweep":
                    WaveForm = WaveType.Sweep;
                    break;
                case "Pink":
                    WaveForm = WaveType.Pink;
                    break;
                default:
                    WaveForm = WaveType.Triangle;
                    break;
            }
            Console.WriteLine($"WaveForm:{WaveForm}");
            SaveSettings();
        }

        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            if (player != null)
            {
                player.Volume = VolumeTrackBar.Value;
            }
            SaveSettings();
        }

        private void cmbRotateFlip_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void VisionForm_ResizeEnd(object sender, EventArgs e)
        {
            SaveSettings();
        }
    }
}
