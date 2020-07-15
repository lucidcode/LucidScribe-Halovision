namespace lucidcode.LucidScribe.Plugin.Halovision
{
  partial class VisionForm
  {
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
      if (disposing && (components != null))
      {
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
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VisionForm));
        this.pnlPlugins = new lucidcode.Controls.Panel3D();
        this.pbDisplay = new System.Windows.Forms.PictureBox();
        this.mnuReconnect = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.mnuReconnectCamera = new System.Windows.Forms.ToolStripMenuItem();
        this.Panel3D4 = new lucidcode.Controls.Panel3D();
        this.Label6 = new System.Windows.Forms.Label();
        this.lstImg = new System.Windows.Forms.ImageList(this.components);
        this.cmbDevices = new System.Windows.Forms.ComboBox();
        this.tmrDiff = new System.Windows.Forms.Timer(this.components);
        this.pbDifference = new System.Windows.Forms.PictureBox();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.panel3D1 = new lucidcode.Controls.Panel3D();
        this.panel3D2 = new lucidcode.Controls.Panel3D();
        this.lblTime = new System.Windows.Forms.Label();
        this.panel3D3 = new lucidcode.Controls.Panel3D();
        this.chkDetectFace = new System.Windows.Forms.CheckBox();
        this.textBox1 = new System.Windows.Forms.TextBox();
        this.label11 = new System.Windows.Forms.Label();
        this.cmbIgnorePercentage = new System.Windows.Forms.ComboBox();
        this.label2 = new System.Windows.Forms.Label();
        this.cmbPixelThreshold = new System.Windows.Forms.ComboBox();
        this.cmbPixelsInARow = new System.Windows.Forms.ComboBox();
        this.chkRecordVideo = new System.Windows.Forms.CheckBox();
        this.txtDeviceIP = new System.Windows.Forms.TextBox();
        this.lblDeviceIP = new System.Windows.Forms.Label();
        this.chkTCMP = new System.Windows.Forms.CheckBox();
        this.label10 = new System.Windows.Forms.Label();
        this.cmbSensitivity = new System.Windows.Forms.ComboBox();
        this.label9 = new System.Windows.Forms.Label();
        this.btnReconnect = new System.Windows.Forms.Button();
        this.label8 = new System.Windows.Forms.Label();
        this.cmbAlgorithm = new System.Windows.Forms.ComboBox();
        this.label7 = new System.Windows.Forms.Label();
        this.cmbFrameThreshold = new System.Windows.Forms.ComboBox();
        this.label5 = new System.Windows.Forms.Label();
        this.label1 = new System.Windows.Forms.Label();
        this.panel3D5 = new lucidcode.Controls.Panel3D();
        this.label3 = new System.Windows.Forms.Label();
        this.label4 = new System.Windows.Forms.Label();
        this.pnlPlugins.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pbDisplay)).BeginInit();
        this.mnuReconnect.SuspendLayout();
        this.Panel3D4.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.pbDifference)).BeginInit();
        this.tableLayoutPanel1.SuspendLayout();
        this.panel3D1.SuspendLayout();
        this.panel3D2.SuspendLayout();
        this.panel3D3.SuspendLayout();
        this.panel3D5.SuspendLayout();
        this.SuspendLayout();
        // 
        // pnlPlugins
        // 
        this.pnlPlugins.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.pnlPlugins.BackColor = System.Drawing.Color.White;
        this.pnlPlugins.Controls.Add(this.pbDisplay);
        this.pnlPlugins.Controls.Add(this.Panel3D4);
        this.pnlPlugins.Location = new System.Drawing.Point(3, 3);
        this.pnlPlugins.Name = "pnlPlugins";
        this.pnlPlugins.Size = new System.Drawing.Size(294, 279);
        this.pnlPlugins.TabIndex = 5;
        // 
        // pbDisplay
        // 
        this.pbDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.pbDisplay.ContextMenuStrip = this.mnuReconnect;
        this.pbDisplay.Cursor = System.Windows.Forms.Cursors.Default;
        this.pbDisplay.Location = new System.Drawing.Point(3, 25);
        this.pbDisplay.Name = "pbDisplay";
        this.pbDisplay.Size = new System.Drawing.Size(288, 251);
        this.pbDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        this.pbDisplay.TabIndex = 33;
        this.pbDisplay.TabStop = false;
        // 
        // mnuReconnect
        // 
        this.mnuReconnect.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuReconnectCamera});
        this.mnuReconnect.Name = "mnuPortsList";
        this.mnuReconnect.Size = new System.Drawing.Size(131, 26);
        // 
        // mnuReconnectCamera
        // 
        this.mnuReconnectCamera.Name = "mnuReconnectCamera";
        this.mnuReconnectCamera.Size = new System.Drawing.Size(130, 22);
        this.mnuReconnectCamera.Text = "&Reconnect";
        this.mnuReconnectCamera.Click += new System.EventHandler(this.mnuReconnectCamera_Click);
        // 
        // Panel3D4
        // 
        this.Panel3D4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.Panel3D4.BackColor = System.Drawing.Color.SteelBlue;
        this.Panel3D4.Controls.Add(this.Label6);
        this.Panel3D4.Location = new System.Drawing.Point(0, 0);
        this.Panel3D4.Name = "Panel3D4";
        this.Panel3D4.Size = new System.Drawing.Size(294, 24);
        this.Panel3D4.TabIndex = 4;
        // 
        // Label6
        // 
        this.Label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.Label6.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
        this.Label6.ForeColor = System.Drawing.Color.White;
        this.Label6.Location = new System.Drawing.Point(3, 3);
        this.Label6.Name = "Label6";
        this.Label6.Size = new System.Drawing.Size(267, 19);
        this.Label6.TabIndex = 3;
        this.Label6.Text = "Camera Feed";
        this.Label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // lstImg
        // 
        this.lstImg.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("lstImg.ImageStream")));
        this.lstImg.TransparentColor = System.Drawing.Color.Transparent;
        this.lstImg.Images.SetKeyName(0, "Graph.Plugin2.bmp");
        // 
        // cmbDevices
        // 
        this.cmbDevices.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbDevices.FormattingEnabled = true;
        this.cmbDevices.Location = new System.Drawing.Point(99, 30);
        this.cmbDevices.Name = "cmbDevices";
        this.cmbDevices.Size = new System.Drawing.Size(406, 21);
        this.cmbDevices.TabIndex = 33;
        this.cmbDevices.SelectedIndexChanged += new System.EventHandler(this.cmbDevices_SelectedIndexChanged);
        // 
        // tmrDiff
        // 
        this.tmrDiff.Enabled = true;
        this.tmrDiff.Tick += new System.EventHandler(this.tmrDiff_Tick);
        // 
        // pbDifference
        // 
        this.pbDifference.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.pbDifference.Cursor = System.Windows.Forms.Cursors.Default;
        this.pbDifference.Location = new System.Drawing.Point(3, 25);
        this.pbDifference.Name = "pbDifference";
        this.pbDifference.Size = new System.Drawing.Size(288, 251);
        this.pbDifference.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
        this.pbDifference.TabIndex = 34;
        this.pbDifference.TabStop = false;
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.tableLayoutPanel1.ColumnCount = 2;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel1.Controls.Add(this.pnlPlugins, 0, 0);
        this.tableLayoutPanel1.Controls.Add(this.panel3D1, 1, 0);
        this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 1;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(600, 285);
        this.tableLayoutPanel1.TabIndex = 35;
        // 
        // panel3D1
        // 
        this.panel3D1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                    | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.panel3D1.BackColor = System.Drawing.Color.White;
        this.panel3D1.Controls.Add(this.pbDifference);
        this.panel3D1.Controls.Add(this.panel3D2);
        this.panel3D1.Location = new System.Drawing.Point(303, 3);
        this.panel3D1.Name = "panel3D1";
        this.panel3D1.Size = new System.Drawing.Size(294, 279);
        this.panel3D1.TabIndex = 34;
        // 
        // panel3D2
        // 
        this.panel3D2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.panel3D2.BackColor = System.Drawing.Color.SteelBlue;
        this.panel3D2.Controls.Add(this.lblTime);
        this.panel3D2.Location = new System.Drawing.Point(0, 0);
        this.panel3D2.Name = "panel3D2";
        this.panel3D2.Size = new System.Drawing.Size(294, 24);
        this.panel3D2.TabIndex = 4;
        // 
        // lblTime
        // 
        this.lblTime.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.lblTime.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
        this.lblTime.ForeColor = System.Drawing.Color.White;
        this.lblTime.Location = new System.Drawing.Point(3, 3);
        this.lblTime.Name = "lblTime";
        this.lblTime.Size = new System.Drawing.Size(267, 19);
        this.lblTime.TabIndex = 3;
        this.lblTime.Text = "Differences";
        this.lblTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // panel3D3
        // 
        this.panel3D3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.panel3D3.BackColor = System.Drawing.Color.LightSteelBlue;
        this.panel3D3.Controls.Add(this.chkDetectFace);
        this.panel3D3.Controls.Add(this.textBox1);
        this.panel3D3.Controls.Add(this.label11);
        this.panel3D3.Controls.Add(this.cmbIgnorePercentage);
        this.panel3D3.Controls.Add(this.label2);
        this.panel3D3.Controls.Add(this.cmbPixelThreshold);
        this.panel3D3.Controls.Add(this.cmbPixelsInARow);
        this.panel3D3.Controls.Add(this.chkRecordVideo);
        this.panel3D3.Controls.Add(this.txtDeviceIP);
        this.panel3D3.Controls.Add(this.lblDeviceIP);
        this.panel3D3.Controls.Add(this.chkTCMP);
        this.panel3D3.Controls.Add(this.label10);
        this.panel3D3.Controls.Add(this.cmbSensitivity);
        this.panel3D3.Controls.Add(this.label9);
        this.panel3D3.Controls.Add(this.btnReconnect);
        this.panel3D3.Controls.Add(this.label8);
        this.panel3D3.Controls.Add(this.cmbAlgorithm);
        this.panel3D3.Controls.Add(this.label7);
        this.panel3D3.Controls.Add(this.cmbFrameThreshold);
        this.panel3D3.Controls.Add(this.label5);
        this.panel3D3.Controls.Add(this.label1);
        this.panel3D3.Controls.Add(this.panel3D5);
        this.panel3D3.Controls.Add(this.cmbDevices);
        this.panel3D3.Location = new System.Drawing.Point(9, 303);
        this.panel3D3.Name = "panel3D3";
        this.panel3D3.Size = new System.Drawing.Size(600, 153);
        this.panel3D3.TabIndex = 36;
        // 
        // chkDetectFace
        // 
        this.chkDetectFace.Location = new System.Drawing.Point(430, 119);
        this.chkDetectFace.Name = "chkDetectFace";
        this.chkDetectFace.Size = new System.Drawing.Size(151, 22);
        this.chkDetectFace.TabIndex = 291;
        this.chkDetectFace.Text = "Detect Face";
        this.chkDetectFace.UseVisualStyleBackColor = true;
        this.chkDetectFace.CheckedChanged += new System.EventHandler(this.chkDetectFace_CheckedChanged);
        // 
        // textBox1
        // 
        this.textBox1.Location = new System.Drawing.Point(99, 151);
        this.textBox1.Name = "textBox1";
        this.textBox1.Size = new System.Drawing.Size(154, 21);
        this.textBox1.TabIndex = 290;
        this.textBox1.Visible = false;
        // 
        // label11
        // 
        this.label11.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label11.Location = new System.Drawing.Point(6, 151);
        this.label11.Name = "label11";
        this.label11.Size = new System.Drawing.Size(85, 21);
        this.label11.TabIndex = 289;
        this.label11.Text = "Video Data ";
        this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.label11.Visible = false;
        // 
        // cmbIgnorePercentage
        // 
        this.cmbIgnorePercentage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbIgnorePercentage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbIgnorePercentage.FormattingEnabled = true;
        this.cmbIgnorePercentage.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100"});
        this.cmbIgnorePercentage.Location = new System.Drawing.Point(361, 120);
        this.cmbIgnorePercentage.Name = "cmbIgnorePercentage";
        this.cmbIgnorePercentage.Size = new System.Drawing.Size(54, 21);
        this.cmbIgnorePercentage.TabIndex = 287;
        this.cmbIgnorePercentage.SelectedIndexChanged += new System.EventHandler(this.cmbIgnorePercentage_SelectedIndexChanged);
        // 
        // label2
        // 
        this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.label2.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label2.Location = new System.Drawing.Point(259, 120);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(104, 21);
        this.label2.TabIndex = 288;
        this.label2.Text = "Ignore %";
        this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // cmbPixelThreshold
        // 
        this.cmbPixelThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbPixelThreshold.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbPixelThreshold.FormattingEnabled = true;
        this.cmbPixelThreshold.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100",
            "101",
            "102",
            "103",
            "104",
            "105",
            "106",
            "107",
            "108",
            "109",
            "110",
            "111",
            "112",
            "113",
            "114",
            "115",
            "116",
            "117",
            "118",
            "119",
            "120",
            "121",
            "122",
            "123",
            "124",
            "125",
            "126",
            "127",
            "128",
            "129",
            "130",
            "131",
            "132",
            "133",
            "134",
            "135",
            "136",
            "137",
            "138",
            "139",
            "140",
            "141",
            "142",
            "143",
            "144",
            "145",
            "146",
            "147",
            "148",
            "149",
            "150",
            "151",
            "152",
            "153",
            "154",
            "155",
            "156",
            "157",
            "158",
            "159",
            "160",
            "161",
            "162",
            "163",
            "164",
            "165",
            "166",
            "167",
            "168",
            "169",
            "170",
            "171",
            "172",
            "173",
            "174",
            "175",
            "176",
            "177",
            "178",
            "179",
            "180",
            "181",
            "182",
            "183",
            "184",
            "185",
            "186",
            "187",
            "188",
            "189",
            "190",
            "191",
            "192",
            "193",
            "194",
            "195",
            "196",
            "197",
            "198",
            "199",
            "200",
            "201",
            "202",
            "203",
            "204",
            "205",
            "206",
            "207",
            "208",
            "209",
            "210",
            "211",
            "212",
            "213",
            "214",
            "215",
            "216",
            "217",
            "218",
            "219",
            "220",
            "221",
            "222",
            "223",
            "224",
            "225",
            "226",
            "227",
            "228",
            "229",
            "230",
            "231",
            "232",
            "233",
            "234",
            "235",
            "236",
            "237",
            "238",
            "239",
            "240",
            "241",
            "242",
            "243",
            "244",
            "245",
            "246",
            "247",
            "248",
            "249",
            "250",
            "251",
            "252",
            "253",
            "254",
            "255"});
        this.cmbPixelThreshold.Location = new System.Drawing.Point(540, 58);
        this.cmbPixelThreshold.Name = "cmbPixelThreshold";
        this.cmbPixelThreshold.Size = new System.Drawing.Size(54, 21);
        this.cmbPixelThreshold.TabIndex = 34;
        this.cmbPixelThreshold.SelectedIndexChanged += new System.EventHandler(this.cmbPixelThreshold_SelectedIndexChanged);
        // 
        // cmbPixelsInARow
        // 
        this.cmbPixelsInARow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbPixelsInARow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbPixelsInARow.FormattingEnabled = true;
        this.cmbPixelsInARow.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32"});
        this.cmbPixelsInARow.Location = new System.Drawing.Point(540, 89);
        this.cmbPixelsInARow.Name = "cmbPixelsInARow";
        this.cmbPixelsInARow.Size = new System.Drawing.Size(54, 21);
        this.cmbPixelsInARow.TabIndex = 278;
        this.cmbPixelsInARow.SelectedIndexChanged += new System.EventHandler(this.cmbPixelsInARow_SelectedIndexChanged);
        // 
        // chkRecordVideo
        // 
        this.chkRecordVideo.Location = new System.Drawing.Point(99, 88);
        this.chkRecordVideo.Name = "chkRecordVideo";
        this.chkRecordVideo.Size = new System.Drawing.Size(151, 22);
        this.chkRecordVideo.TabIndex = 286;
        this.chkRecordVideo.Text = "Record Video";
        this.chkRecordVideo.UseVisualStyleBackColor = true;
        this.chkRecordVideo.CheckedChanged += new System.EventHandler(this.chkRecordVideo_CheckedChanged);
        // 
        // txtDeviceIP
        // 
        this.txtDeviceIP.Location = new System.Drawing.Point(99, 121);
        this.txtDeviceIP.Name = "txtDeviceIP";
        this.txtDeviceIP.Size = new System.Drawing.Size(154, 21);
        this.txtDeviceIP.TabIndex = 285;
        this.txtDeviceIP.Visible = false;
        this.txtDeviceIP.TextChanged += new System.EventHandler(this.txtDeviceIP_TextChanged);
        // 
        // lblDeviceIP
        // 
        this.lblDeviceIP.ForeColor = System.Drawing.Color.MidnightBlue;
        this.lblDeviceIP.Location = new System.Drawing.Point(6, 121);
        this.lblDeviceIP.Name = "lblDeviceIP";
        this.lblDeviceIP.Size = new System.Drawing.Size(85, 21);
        this.lblDeviceIP.TabIndex = 284;
        this.lblDeviceIP.Text = "Device IP";
        this.lblDeviceIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        this.lblDeviceIP.Visible = false;
        // 
        // chkTCMP
        // 
        this.chkTCMP.Location = new System.Drawing.Point(246, 25);
        this.chkTCMP.Name = "chkTCMP";
        this.chkTCMP.Size = new System.Drawing.Size(151, 22);
        this.chkTCMP.TabIndex = 282;
        this.chkTCMP.Text = "TCMP";
        this.chkTCMP.UseVisualStyleBackColor = true;
        this.chkTCMP.Visible = false;
        this.chkTCMP.CheckedChanged += new System.EventHandler(this.chkTCMP_CheckedChanged);
        // 
        // label10
        // 
        this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.label10.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label10.Location = new System.Drawing.Point(256, 89);
        this.label10.Name = "label10";
        this.label10.Size = new System.Drawing.Size(100, 21);
        this.label10.TabIndex = 281;
        this.label10.Text = "Amplification";
        this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // cmbSensitivity
        // 
        this.cmbSensitivity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbSensitivity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbSensitivity.FormattingEnabled = true;
        this.cmbSensitivity.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "31",
            "32",
            "33",
            "34",
            "35",
            "36",
            "37",
            "38",
            "39",
            "40",
            "41",
            "42",
            "43",
            "44",
            "45",
            "46",
            "47",
            "48",
            "49",
            "50",
            "51",
            "52",
            "53",
            "54",
            "55",
            "56",
            "57",
            "58",
            "59",
            "60",
            "61",
            "62",
            "63",
            "64",
            "65",
            "66",
            "67",
            "68",
            "69",
            "70",
            "71",
            "72",
            "73",
            "74",
            "75",
            "76",
            "77",
            "78",
            "79",
            "80",
            "81",
            "82",
            "83",
            "84",
            "85",
            "86",
            "87",
            "88",
            "89",
            "90",
            "91",
            "92",
            "93",
            "94",
            "95",
            "96",
            "97",
            "98",
            "99",
            "100",
            "101",
            "102",
            "103",
            "104",
            "105",
            "106",
            "107",
            "108",
            "109",
            "110",
            "111",
            "112",
            "113",
            "114",
            "115",
            "116",
            "117",
            "118",
            "119",
            "120",
            "121",
            "122",
            "123",
            "124",
            "125",
            "126",
            "127",
            "128",
            "129",
            "130",
            "131",
            "132",
            "133",
            "134",
            "135",
            "136",
            "137",
            "138",
            "139",
            "140",
            "141",
            "142",
            "143",
            "144",
            "145",
            "146",
            "147",
            "148",
            "149",
            "150",
            "151",
            "152",
            "153",
            "154",
            "155",
            "156",
            "157",
            "158",
            "159",
            "160",
            "161",
            "162",
            "163",
            "164",
            "165",
            "166",
            "167",
            "168",
            "169",
            "170",
            "171",
            "172",
            "173",
            "174",
            "175",
            "176",
            "177",
            "178",
            "179",
            "180",
            "181",
            "182",
            "183",
            "184",
            "185",
            "186",
            "187",
            "188",
            "189",
            "190",
            "191",
            "192",
            "193",
            "194",
            "195",
            "196",
            "197",
            "198",
            "199",
            "200",
            "201",
            "202",
            "203",
            "204",
            "205",
            "206",
            "207",
            "208",
            "209",
            "210",
            "211",
            "212",
            "213",
            "214",
            "215",
            "216",
            "217",
            "218",
            "219",
            "220",
            "221",
            "222",
            "223",
            "224",
            "225",
            "226",
            "227",
            "228",
            "229",
            "230",
            "231",
            "232",
            "233",
            "234",
            "235",
            "236",
            "237",
            "238",
            "239",
            "240",
            "241",
            "242",
            "243",
            "244",
            "245",
            "246",
            "247",
            "248",
            "249",
            "250",
            "251",
            "252",
            "253",
            "254",
            "255"});
        this.cmbSensitivity.Location = new System.Drawing.Point(361, 89);
        this.cmbSensitivity.Name = "cmbSensitivity";
        this.cmbSensitivity.Size = new System.Drawing.Size(54, 21);
        this.cmbSensitivity.TabIndex = 280;
        this.cmbSensitivity.SelectedIndexChanged += new System.EventHandler(this.cmbSensitivity_SelectedIndexChanged);
        // 
        // label9
        // 
        this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.label9.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label9.Location = new System.Drawing.Point(427, 89);
        this.label9.Name = "label9";
        this.label9.Size = new System.Drawing.Size(122, 21);
        this.label9.TabIndex = 279;
        this.label9.Text = "Pixels in a Row";
        this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // btnReconnect
        // 
        this.btnReconnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.btnReconnect.Location = new System.Drawing.Point(511, 29);
        this.btnReconnect.Name = "btnReconnect";
        this.btnReconnect.Size = new System.Drawing.Size(83, 23);
        this.btnReconnect.TabIndex = 277;
        this.btnReconnect.Text = "Connect";
        this.btnReconnect.UseVisualStyleBackColor = true;
        this.btnReconnect.Click += new System.EventHandler(this.btnReconnect_Click);
        // 
        // label8
        // 
        this.label8.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label8.Location = new System.Drawing.Point(6, 57);
        this.label8.Name = "label8";
        this.label8.Size = new System.Drawing.Size(85, 21);
        this.label8.TabIndex = 276;
        this.label8.Text = "Algorithm";
        this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // cmbAlgorithm
        // 
        this.cmbAlgorithm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbAlgorithm.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbAlgorithm.FormattingEnabled = true;
        this.cmbAlgorithm.Items.AddRange(new object[] {
            "Motion Detector",
            "REM Detector"});
        this.cmbAlgorithm.Location = new System.Drawing.Point(99, 57);
        this.cmbAlgorithm.Name = "cmbAlgorithm";
        this.cmbAlgorithm.Size = new System.Drawing.Size(154, 21);
        this.cmbAlgorithm.TabIndex = 275;
        this.cmbAlgorithm.SelectedIndexChanged += new System.EventHandler(this.cmbAlgorithm_SelectedIndexChanged);
        // 
        // label7
        // 
        this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.label7.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label7.Location = new System.Drawing.Point(256, 58);
        this.label7.Name = "label7";
        this.label7.Size = new System.Drawing.Size(107, 21);
        this.label7.TabIndex = 274;
        this.label7.Text = "Frame Threshold";
        this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // cmbFrameThreshold
        // 
        this.cmbFrameThreshold.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.cmbFrameThreshold.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbFrameThreshold.FormattingEnabled = true;
        this.cmbFrameThreshold.Items.AddRange(new object[] {
            "10",
            "20",
            "30",
            "40",
            "50",
            "60",
            "70",
            "80",
            "90",
            "100",
            "110",
            "120",
            "130",
            "140",
            "150",
            "160",
            "170",
            "180",
            "190",
            "200",
            "210",
            "220",
            "230",
            "240",
            "250",
            "260",
            "270",
            "280",
            "290",
            "300",
            "310",
            "320",
            "330",
            "340",
            "350",
            "360",
            "370",
            "380",
            "390",
            "400",
            "410",
            "420",
            "430",
            "440",
            "450",
            "460",
            "470",
            "480",
            "490",
            "500",
            "510",
            "520",
            "530",
            "540",
            "550",
            "560",
            "570",
            "580",
            "590",
            "600",
            "610",
            "620",
            "630",
            "640",
            "650",
            "660",
            "670",
            "680",
            "690",
            "700",
            "710",
            "720",
            "730",
            "740",
            "750",
            "760",
            "770",
            "780",
            "790",
            "800",
            "810",
            "820",
            "830",
            "840",
            "850",
            "860",
            "870",
            "880",
            "890",
            "900",
            "910",
            "920",
            "930",
            "940",
            "950",
            "960",
            "970",
            "980",
            "990",
            "1000"});
        this.cmbFrameThreshold.Location = new System.Drawing.Point(361, 58);
        this.cmbFrameThreshold.Name = "cmbFrameThreshold";
        this.cmbFrameThreshold.Size = new System.Drawing.Size(54, 21);
        this.cmbFrameThreshold.TabIndex = 273;
        this.cmbFrameThreshold.SelectedIndexChanged += new System.EventHandler(this.cmbFrameThreshold_SelectedIndexChanged);
        // 
        // label5
        // 
        this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
        this.label5.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label5.Location = new System.Drawing.Point(425, 58);
        this.label5.Name = "label5";
        this.label5.Size = new System.Drawing.Size(104, 21);
        this.label5.TabIndex = 272;
        this.label5.Text = "Pixel Threshold";
        this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label1
        // 
        this.label1.ForeColor = System.Drawing.Color.MidnightBlue;
        this.label1.Location = new System.Drawing.Point(6, 29);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(87, 21);
        this.label1.TabIndex = 271;
        this.label1.Text = "Device";
        this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // panel3D5
        // 
        this.panel3D5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.panel3D5.BackColor = System.Drawing.Color.SteelBlue;
        this.panel3D5.Controls.Add(this.label3);
        this.panel3D5.Controls.Add(this.label4);
        this.panel3D5.Location = new System.Drawing.Point(0, 0);
        this.panel3D5.Name = "panel3D5";
        this.panel3D5.Size = new System.Drawing.Size(600, 24);
        this.panel3D5.TabIndex = 4;
        // 
        // label3
        // 
        this.label3.Font = new System.Drawing.Font("Verdana", 10F, System.Drawing.FontStyle.Bold);
        this.label3.ForeColor = System.Drawing.Color.White;
        this.label3.Image = ((System.Drawing.Image)(resources.GetObject("label3.Image")));
        this.label3.Location = new System.Drawing.Point(3, 3);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(19, 19);
        this.label3.TabIndex = 4;
        this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // label4
        // 
        this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                    | System.Windows.Forms.AnchorStyles.Right)));
        this.label4.Font = new System.Drawing.Font("Verdana", 8F, System.Drawing.FontStyle.Bold);
        this.label4.ForeColor = System.Drawing.Color.White;
        this.label4.Location = new System.Drawing.Point(24, 3);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(573, 19);
        this.label4.TabIndex = 3;
        this.label4.Text = "Settings";
        this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // VisionForm
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackColor = System.Drawing.Color.LightSteelBlue;
        this.ClientSize = new System.Drawing.Size(624, 469);
        this.Controls.Add(this.tableLayoutPanel1);
        this.Controls.Add(this.panel3D3);
        this.Font = new System.Drawing.Font("Verdana", 8.25F);
        this.ForeColor = System.Drawing.Color.MidnightBlue;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "VisionForm";
        this.Text = "Halovision";
        this.TopMost = true;
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VisionForm_FormClosing);
        this.Load += new System.EventHandler(this.PortForm_Load);
        this.Resize += new System.EventHandler(this.VisionForm_Resize);
        this.pnlPlugins.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pbDisplay)).EndInit();
        this.mnuReconnect.ResumeLayout(false);
        this.Panel3D4.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.pbDifference)).EndInit();
        this.tableLayoutPanel1.ResumeLayout(false);
        this.panel3D1.ResumeLayout(false);
        this.panel3D2.ResumeLayout(false);
        this.panel3D3.ResumeLayout(false);
        this.panel3D3.PerformLayout();
        this.panel3D5.ResumeLayout(false);
        this.ResumeLayout(false);

    }

    #endregion

    internal lucidcode.Controls.Panel3D pnlPlugins;
    internal lucidcode.Controls.Panel3D Panel3D4;
    internal System.Windows.Forms.Label Label6;
    internal System.Windows.Forms.ImageList lstImg;
    private System.Windows.Forms.ContextMenuStrip mnuReconnect;
    private System.Windows.Forms.ToolStripMenuItem mnuReconnectCamera;
    private System.Windows.Forms.PictureBox pbDisplay;
    private System.Windows.Forms.ComboBox cmbDevices;
    private System.Windows.Forms.Timer tmrDiff;
    private System.Windows.Forms.PictureBox pbDifference;
    private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    internal lucidcode.Controls.Panel3D panel3D3;
    internal lucidcode.Controls.Panel3D panel3D5;
    internal System.Windows.Forms.Label label3;
    internal System.Windows.Forms.Label label4;
    private System.Windows.Forms.ComboBox cmbPixelThreshold;
    internal System.Windows.Forms.Label label8;
    private System.Windows.Forms.ComboBox cmbAlgorithm;
    internal System.Windows.Forms.Label label7;
    private System.Windows.Forms.ComboBox cmbFrameThreshold;
    internal System.Windows.Forms.Label label5;
    internal System.Windows.Forms.Label label1;
    private System.Windows.Forms.Button btnReconnect;
    internal System.Windows.Forms.Label label9;
    private System.Windows.Forms.ComboBox cmbPixelsInARow;
    internal System.Windows.Forms.Label label10;
    private System.Windows.Forms.ComboBox cmbSensitivity;
    internal lucidcode.Controls.Panel3D panel3D1;
    internal lucidcode.Controls.Panel3D panel3D2;
    internal System.Windows.Forms.Label lblTime;
    private System.Windows.Forms.CheckBox chkTCMP;
    private System.Windows.Forms.TextBox txtDeviceIP;
    internal System.Windows.Forms.Label lblDeviceIP;
    private System.Windows.Forms.CheckBox chkRecordVideo;
    private System.Windows.Forms.ComboBox cmbIgnorePercentage;
    internal System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBox1;
    internal System.Windows.Forms.Label label11;
    private System.Windows.Forms.CheckBox chkDetectFace;
  }
}