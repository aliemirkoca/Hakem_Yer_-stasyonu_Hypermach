using System;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System.Drawing;
using ZedGraph;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {


        private GMapOverlay markersOverlay;
        public Form1()
        {
            InitializeComponent();
        }

        private LineItem curve_irtifa;
        private LineItem curve_ivme;
        private LineItem curve_gpsIrtifa;
        private double zaman = 0;


        private void SetupAltitudeGraph(ZedGraphControl tablo, string özellik)
        {
            GraphPane pane = tablo.GraphPane;

            // Başlık ve eksen adları
            pane.Title.Text = "";
            pane.XAxis.Title.Text = "Zaman (sn)";
            pane.YAxis.Title.Text = özellik;

            // Dış zemin (panel): daha koyu
            pane.Fill = new Fill(Color.FromArgb(30, 30, 30));

            // İç çizim alanı (grafik): Form arka planına uygun
            pane.Chart.Fill = new Fill(Color.FromArgb(50, 50, 50));

            // Kenarlıklar kapalı
            pane.Border.IsVisible = false;
            pane.Chart.Border.IsVisible = false;

            // Yazı rengi: Açık gri
            var labelColor = Color.Gainsboro;
            pane.XAxis.Title.FontSpec.FontColor = labelColor;
            pane.YAxis.Title.FontSpec.FontColor = labelColor;
            pane.XAxis.Scale.FontSpec.FontColor = labelColor;
            pane.YAxis.Scale.FontSpec.FontColor = labelColor;
            pane.Title.FontSpec.FontColor = Color.White;

            // Grid çizgileri: Yumuşak gri
            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;
            pane.XAxis.MajorGrid.Color = Color.Gray;
            pane.YAxis.MajorGrid.Color = Color.Gray;

            // Örnek çizgi (yeşil, parlak)
            LineItem curve = pane.AddCurve("İrtifa", new PointPairList(), Color.LimeGreen, SymbolType.None);
            curve.Line.Width = 2.5f;
            curve.Line.IsSmooth = true;

            // Kenar boşluklarını azalt
            pane.Margin.All = 10;

            tablo.AxisChange();
            tablo.Invalidate();
        }





        private void Form1_Load(object sender, EventArgs e)
        {
            //tabloların ayarlanması
            SetupAltitudeGraph(zedGraphControl1, "İRTİFA");
            SetupAltitudeGraph(zedGraphControl2, "İVME");
            SetupAltitudeGraph(zedGraphControl3, "İRTİFA");

            // Curve’leri CurveList’ten al
            curve_irtifa = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            curve_ivme = zedGraphControl2.GraphPane.CurveList[0] as LineItem;
            curve_gpsIrtifa = zedGraphControl3.GraphPane.CurveList[0] as LineItem;

            
            /////
            buttonDisconnect.Enabled = false;
            buttonDisconnect1.Enabled = false;
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            comboBox2.Items.AddRange(SerialPort.GetPortNames());

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;
            else
                MessageBox.Show("Seri port bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            if (comboBox2.Items.Count > 0)
                comboBox2.SelectedIndex = 0;
            else
                MessageBox.Show("Seri port bulunamadı!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);


            // Harita ayarları
            GMaps.Instance.Mode = AccessMode.ServerAndCache; // Online + Cache
            gMapControl1.MapProvider = GMapProviders.GoogleSatelliteMap; // Google Harita
            gMapControl1.Position = new PointLatLng(38.4121782, 33.6502926); // Başlangıç noktası
            gMapControl1.MinZoom = 2;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 10;
            gMapControl1.ShowCenter = false;






            // Marker katmanı ekle
            markersOverlay = new GMapOverlay("markers");
            gMapControl1.Overlays.Add(markersOverlay);


            //Roket İkonu
            string imagePath = System.IO.Path.Combine(Application.StartupPath, "rocket.png");
            Bitmap rocketIcon = (Bitmap)Image.FromFile(imagePath);
            // Hisar Atış Alanı ROKETSAN
            GMapMarker marker = new GMarkerGoogle(
              new PointLatLng(38.4121782, 33.6502926),
              rocketIcon
            );
            markersOverlay.Markers.Add(marker);


            //hiz halkasi fake veri



        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir port seçin!", "Uyarı");
                return;
            }

            serialPort1.PortName = comboBox1.Text;
            serialPort1.BaudRate = 9600;
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;
            serialPort1.DataBits = 8;
            serialPort1.NewLine = "\n"; // ReadLine için gerekli

            try
            {
                serialPort1.Open();
                buttonConnect.Enabled = false;
                buttonDisconnect.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Seri port açılamadı.\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                buttonConnect.Enabled = true;
                buttonDisconnect.Enabled = false;
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string gelenVeri = serialPort1.ReadLine().Trim();
                if (!string.IsNullOrEmpty(gelenVeri))
                {
                    if (InvokeRequired)
                        Invoke(new Action(() => ParseAndDisplay(gelenVeri)));
                    else
                        ParseAndDisplay(gelenVeri);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Veri alma hatası: " + ex.Message);
            }
        }
        public static byte[] HexStringToByteArray(string[] hexArray)
        {
            byte[] byteArray = new byte[hexArray.Length - 1];

            for (int i = 0; i < hexArray.Length - 1; i++)
            {
                byteArray[i] = Convert.ToByte(hexArray[i], 16);
            }

            return byteArray;
        }


        private void ParseAndDisplay(string rawData)
        {

            string[] data = rawData.Split(',');

            textBoxMessages.Text = String.Join("", data);

            if (data.Length < 70)
            {
                // textBoxMessages.Text += "\nEksik veri alındı.";
                return;
            }
            /*
            try
            {*/
            // Örnek: takım ID ve sayaç
            takim_id.Text = ParseHexToUInt(data[4]).ToString();
            labelCounter.Text = ParseHexToUInt(data[5]).ToString();

            // GPS ve görev verileri

            float ivme = ParseHexToFloat(data, 62);
            irtifa.Text = ParseHexToFloat(data, 6).ToString();
            irtifa_halkasi.Value = ParseHexToFloat(data, 10);
            //ivme_halkasi.Value= ParseHexToFloat(data, 62);
            //hiz_halkasi.Value = 1000;
            gps_irtifa.Text = ParseHexToFloat(data, 10).ToString();
            float harita_enlem = ParseHexToFloat(data, 14);
            float harita_boylam = ParseHexToFloat(data, 18);
            gps_enlem.Text = harita_enlem.ToString();
            gps_boylam.Text = harita_boylam.ToString();
            enlem_form.Text = harita_enlem.ToString();
            boylam_form.Text = harita_boylam.ToString();
            gorev_irtifa.Text = ParseHexToFloat(data, 22).ToString();
            gorev_enlem.Text = ParseHexToFloat(data, 26).ToString();
            gorev_boylam.Text = ParseHexToFloat(data, 30).ToString();

            // Jiro verileri
            J_X.Text = ParseHexToFloat(data, 46).ToString();
            J_Y.Text = ParseHexToFloat(data, 50).ToString();
            J_Z.Text = ParseHexToFloat(data, 54).ToString();

            // İvme verileri
            İvme_X.Text = ParseHexToFloat(data, 58).ToString();
            İvme_Y.Text = ParseHexToFloat(data, 62).ToString();
            İvme_Z.Text = ParseHexToFloat(data, 66).ToString();
            if (serialPort2.IsOpen)
            {
                Console.WriteLine(String.Join("", data).Length);
                Console.WriteLine(HexStringToByteArray(data).Length);
                serialPort2.Write(HexStringToByteArray(data), 0, HexStringToByteArray(data).Length);
            }

            // GPS verisini haritaya marker olarak ekle
            if (harita_enlem != 0 && harita_boylam != 0)
            {
                markersOverlay.Markers.Clear(); // Önceki markerları temizle

                string imagePath = System.IO.Path.Combine(Application.StartupPath, "rocket.png");
                Bitmap rocketIcon = (Bitmap)Image.FromFile(imagePath);

                GMapMarker marker = new GMarkerGoogle(
                   new PointLatLng(harita_enlem, harita_boylam),
                   rocketIcon
                  );
                markersOverlay.Markers.Add(marker);
                gMapControl1.Position = new PointLatLng(harita_enlem, harita_boylam);
            }

            /*
            }
            catch (Exception ex)
            {
                textBoxMessages.Text += "\nVeri parse hatası: " + ex.Message;
            }*/


            // Grafik çizimi
            zaman += 0.1;

            float irtifaVerisi = ParseHexToFloat(data, 6);
            float ivmeVerisi = ParseHexToFloat(data, 62);
            float gpsIrtifaVerisi = ParseHexToFloat(data, 10);
            
            if (curve_irtifa != null)
            {
                curve_irtifa.AddPoint(zaman, irtifaVerisi);
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
            }

            if (curve_ivme != null)
            {
                curve_ivme.AddPoint(zaman, ivmeVerisi);
                zedGraphControl2.AxisChange();
                zedGraphControl2.Invalidate();
            }

            if (curve_gpsIrtifa != null)
            {
                curve_gpsIrtifa.AddPoint(zaman, gpsIrtifaVerisi);
                zedGraphControl3.AxisChange();
                zedGraphControl3.Invalidate();
            }

            
        }

        private uint ParseHexToUInt(string hex)
        {
            return uint.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out uint result)
                ? result
                : 0;
        }

        private float ParseHexToFloat(string[] data, int startIndex)
        {
            // 4 byte = 4 array elemanı (startIndex + 0,1,2,3)
            if (startIndex + 3 >= data.Length)
                return 0;

            string combinedHex = data[startIndex + 3].PadLeft(2, '0') +
                                 data[startIndex + 2].PadLeft(2, '0') +
                                 data[startIndex + 1].PadLeft(2, '0') +
                                 data[startIndex + 0].PadLeft(2, '0');

            try
            {
                uint num = Convert.ToUInt32(combinedHex, 16);
                return BitConverter.ToSingle(BitConverter.GetBytes(num), 0);
            }
            catch
            {
                return 0;
            }
        }

        private void buttonConnect1_Click(object sender, EventArgs e)
        {
            
            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir port seçin!", "Uyarı");
                return;
            }

            serialPort2.PortName = comboBox2.Text;
            serialPort2.BaudRate = 19200;
            serialPort2.Parity = Parity.None;
            serialPort2.StopBits = StopBits.One;
            serialPort2.DataBits = 8;
            serialPort2.NewLine = "\n"; // ReadLine için gerekli

            try
            {
                serialPort2.Open();
                buttonConnect1.Enabled = false;
                buttonDisconnect1.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Seri port açılamadı.\nHata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void buttonDisconnect1_Click(object sender, EventArgs e)
        {
            if (serialPort2.IsOpen)
            {
                serialPort2.Close();
                buttonConnect1.Enabled = true;
                buttonDisconnect1.Enabled = false;
            }
        }


        
    }
}
