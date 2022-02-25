using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;

namespace Gas_Monitoring
{
    public partial class Form1 : Form
    {
        public string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private byte[] Q = new byte[30];
        private int index = 0;

        double x = 0;

        bool firsttime = true;

        private string pre_DAY, read_DAY, str_data = "0.0";
        private double d_data = 0;

        private SerialPort serialPort1 = new SerialPort();

        public Form1()
        {
            InitializeComponent();
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(this.serialPort1_DataReceived);


            getAvailablePorts();
        }

        void getAvailablePorts()
        {
            string prev_port = "";

            string[] ports = SerialPort.GetPortNames();
            comboBox1.Items.Clear();
            foreach (string port in ports)
            {
                if(prev_port != port)
                {
                    comboBox1.Items.Add(port);
                    prev_port = port;
                }
                
            }
            


        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chart1.Series[0].ChartType = SeriesChartType.Line;
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.ChartAreas[0].AxisX.Title = "sec";

            textBox1.Text = "wait for";
            textBox2.Text = ".csv file";

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
            if (!serialPort1.IsOpen)
            {
                serialPort1.PortName = comboBox1.Text;

                serialPort1.BaudRate = 9600;
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.Parity = Parity.None;

                serialPort1.Open();
            }
            else
            {
                MessageBox.Show("해당포트가 이미 열려 있습니다.");
            }
            timer1.Enabled = true;
        }


        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] QBUF = new byte[30];
            int index_Get = serialPort1.Read(QBUF, 0, 30);

            for (int i = 0; i < index_Get; i++)
            {
                Q[index] = QBUF[i];

                if(Q[index] == '\n' && firsttime == true)
                {
                    firsttime = false;
                    return;
                }

                if (Q[index] == '\n')
                {
                    if ((Q[0] == 'A') && (Q[1] == 'T') && (Q[2] == '+'))
                    {
                        save_TXT(Q);

                    }
                    index = -1;
                }
                
                index++; if (index >= 30) index = 0;
            }
        }

        private void save_TXT(byte[] Q)
        {
            string Temp = Encoding.Default.GetString(Q);
            string[] Split1 = Temp.Split('=');
            string[] Split2 = Split1[1].Split('\r');
            

            d_data = Math.Round(double.Parse(Split2[0]) * 9700 / 1024 + 300, 3);
            str_data = d_data.ToString("F1");

            string file = MyDocuments + "/TEMP/" + System.DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss") + ".csv";
            System.IO.File.AppendAllText(file, str_data+"\r\n", Encoding.Default);
        }



        private void timer1_Tick(object sender, EventArgs e)
        {
            string read_str_data = "";
            double read_d_data = 0;

            read_DAY = System.DateTime.Now.ToString("yyyy-MM-dd_HH_mm_ss");

            if (System.IO.File.Exists(MyDocuments + "/TEMP/" + read_DAY + ".csv"))
            {
                if (pre_DAY == read_DAY) return;
                read_str_data = System.IO.File.ReadAllText(MyDocuments + "/TEMP/" + read_DAY + ".csv");

                pre_DAY = read_DAY;

                read_d_data = double.Parse(read_str_data);
            }


            
            


            if(read_d_data != 0)
            {
                double num = (read_d_data);
                double vol = Math.Round((read_d_data / 1000),2);
                string numString = num.ToString();
                string volString = vol.ToString();
                textBox1.Text = numString;
                textBox2.Text = volString;

                

                chart1.Series[0].Points.AddXY(x, num);

                if (chart1.Series[0].Points.Count > 100)
                    chart1.Series[0].Points.RemoveAt(0);

                chart1.ChartAreas[0].AxisX.Minimum = Math.Round(chart1.Series[0].Points[0].XValue, 3);
                chart1.ChartAreas[0].AxisX.Maximum = Math.Round(x, 3);

                chart1.ChartAreas[0].AxisY.Minimum = 1000;
                chart1.ChartAreas[0].AxisY.Maximum = 3000;

                x += 1.0;
            }

        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Stop();
                StopButton.Text = "Start";
            }
            else
            {
                timer1.Start();
                StopButton.Text = "Stop";
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            timer1.Start();
            x = 0;
            chart1.Series[0].Points.Clear();
        }
    }
}
