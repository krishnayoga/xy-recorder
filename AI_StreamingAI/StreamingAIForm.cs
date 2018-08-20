using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Web.UI.DataVisualization;
using System.Windows.Forms.DataVisualization;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms;
using Automation.BDaq;

namespace AI_StreamingAI
{
   public partial class StreamingAIForm : Form
   {
        #region fields  
        double[]        m_dataScaled;
        bool            m_isFirstOverRun = true;
        double          m_xInc;
        int             dataCount = 0;
        double          last_x_0;
        double          last_x_1;
        bool            firstChecked = true;
        string[]        arrAvgData;
        string[]        arrData;
        double[]        arrSumData;
        double[]        dataPrint;
        double          max_x_1 = 0;
        double          min_x_1 = 1000;
        double          max_x_2 = 0;
        double          min_x_2 = 1000;
        double          max_y = 0;
        double          min_y = 1000;
        int             factor_baca;
        int             max_x_chart;
        int             min_x_chart;
        int             max_y_chart;
        int             min_y_chart;

        #endregion

        public StreamingAIForm()
        {
            InitializeComponent();
        }

        public StreamingAIForm(int deviceNumber)
        {
            InitializeComponent();
	        waveformAiCtrl1.SelectedDevice = new DeviceInformation(deviceNumber);
        }
      
        private void StreamingBufferedAiForm_Load(object sender, EventArgs e)
        {
            if (!waveformAiCtrl1.Initialized)
            {
                MessageBox.Show("No device be selected or device open failed!", "StreamingAI");
                this.Close();
                return;
            }

	        int chanCount = waveformAiCtrl1.Conversion.ChannelCount;
		    int sectionLength = waveformAiCtrl1.Record.SectionLength;
		    m_dataScaled = new double[chanCount * sectionLength];

            dataPrint = new double[3];

		    this.Text = "Streaming AI(" + waveformAiCtrl1.SelectedDevice.Description + ")";

            button_start.Enabled = true;
            button_stop.Enabled = false;
            button_pause.Enabled = false;

            chartXY.Series[0].IsXValueIndexed = false;

        }

        private void HandleError(ErrorCode err)
        {
            if ((err >= ErrorCode.ErrorHandleNotValid) && (err != ErrorCode.Success))
            {
                MessageBox.Show("Sorry ! Some errors happened, the error code is: " + err.ToString(), "StreamingAI");
            }
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            ErrorCode err = ErrorCode.Success;

            err = waveformAiCtrl1.Prepare();
            m_xInc = 1.0 / waveformAiCtrl1.Conversion.ClockRate;

            if (err == ErrorCode.Success)
            {
                err = waveformAiCtrl1.Start();
            }

            if (err != ErrorCode.Success)
            {
       	        HandleError(err);
	            return;
            }
            
            button_start.Enabled = false;
            button_pause.Enabled = true;
            button_stop.Enabled = true;
            
            factor_baca = Convert.ToInt32(textBox1.Text);

            max_x_chart = Convert.ToInt32(textBox2.Text);
            min_x_chart = -max_x_chart;
            max_y_chart = Convert.ToInt32(textBox3.Text);
            min_y_chart = -max_y_chart;

            initChart();
        }

	    private void waveformAiCtrl1_DataReady(object sender, BfdAiEventArgs args)
        {
            try
            {
                if (waveformAiCtrl1.State == ControlState.Idle)
                {
	                return;
                }

                if (m_dataScaled.Length < args.Count)
                {
                    m_dataScaled = new double[args.Count];
                }

                ErrorCode err = ErrorCode.Success;
		        int chanCount = waveformAiCtrl1.Conversion.ChannelCount;
			    int sectionLength = waveformAiCtrl1.Record.SectionLength;
                err = waveformAiCtrl1.GetData(args.Count, m_dataScaled);

                if (err != ErrorCode.Success && err != ErrorCode.WarningRecordEnd)
                {
                    HandleError(err);
                    return;
                }

                this.Invoke(new Action(() =>
                {
                    arrSumData = new double[chanCount];

                    for (int i = 0; i < sectionLength; i++)
                    {
                        arrData = new string[chanCount];

                        for (int j = 0; j < chanCount; j++)
                        {
                            int cnt = i * chanCount + j;
                            arrData[j] = m_dataScaled[cnt].ToString("F1");
                            arrSumData[j] += m_dataScaled[cnt];
                            //Console.WriteLine("j ke " + j + " arrsumdata :" + arrSumData[j] + " m_datascaled: " + m_dataScaled[cnt] + " cnt: " + cnt + " chancount: " + chanCount);
                        }
                    }

                    arrAvgData = new string[arrSumData.Length];

                    for (int i = 0; i < arrSumData.Length; i++)
                    {
                        arrAvgData[i] = (arrSumData[i] / sectionLength).ToString("F1");
                        
                        //label3.Text = arrAvgData[2];
                        //Console.WriteLine("i ke " + i + " arrsumdata :" + arrSumData[i]);
                        dataCount++;

                    }

                    dataPrint[0] = Convert.ToDouble(arrAvgData[0]) * factor_baca;
                    dataPrint[1] = Convert.ToDouble(arrAvgData[1]) * factor_baca;
                    dataPrint[2] = Convert.ToDouble(arrAvgData[2]) * factor_baca;

                    label1.Text = dataPrint[0].ToString();
                    label2.Text = dataPrint[1].ToString();

                    if (dataPrint[0] > max_x_1)
                    {
                        max_x_1 = dataPrint[0];
                    }

                    if (dataPrint[0] < min_x_1)
                    {
                        min_x_1 = dataPrint[0];
                    }

                    if (dataPrint[0] > max_x_2)
                    {
                        max_x_2 = dataPrint[1];
                    }

                    if (dataPrint[0] < min_x_2)
                    {
                        min_x_2 = dataPrint[1];
                    }

                    if (dataPrint[1] > max_y)
                    {
                        max_y = dataPrint[2];
                    }

                    if (dataPrint[1] < min_y)
                    {
                        min_y = dataPrint[2];
                    }

                    //chartXY.Series[0].Points.AddXY(arrAvgData[0], arrAvgData[1]);

                    label9.Text = max_x_1.ToString();
                    label10.Text = min_x_1.ToString();
                    label11.Text = max_y.ToString();
                    label12.Text = min_y.ToString();

                    if (checkBox_holdX.Checked && firstChecked)
                    {
                        last_x_0 = dataPrint[0];
                        last_x_1 = dataPrint[1];
                        //last_x = dataCount.ToString();
                        firstChecked = false;
                    }

                    plotChart(arrAvgData);
                }));

                Console.WriteLine(dataCount / 3);          
            }

            catch
            {
                MessageBox.Show("nilai x dan y salah!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);    
            }   
        }

        private void button_pause_Click(object sender, EventArgs e)
        {
            ErrorCode err = ErrorCode.Success;      
            err = waveformAiCtrl1.Stop();
            if (err != ErrorCode.Success)
            {
	            HandleError(err);
                return;
            }

            button_start.Enabled = true;
            button_pause.Enabled = false;
        }

        private void button_stop_Click(object sender, EventArgs e)
        {
	        ErrorCode err = ErrorCode.Success;
		    err = waveformAiCtrl1.Stop();
            if (err != ErrorCode.Success)
            {
			    HandleError(err);
                return;
            }   
          
            button_start.Enabled = true;
            button_pause.Enabled = false;
            button_stop.Enabled = false;
            Array.Clear(m_dataScaled, 0, m_dataScaled.Length);     
        }
     
	    private void waveformAiCtrl1_CacheOverflow(object sender, BfdAiEventArgs e)
        {
            MessageBox.Show("WaveformAiCacheOverflow");
        }

        private void waveformAiCtrl1_Overrun(object sender, BfdAiEventArgs e)
        {
            if (m_isFirstOverRun)
            {
                MessageBox.Show("WaveformAiOverrun");
                m_isFirstOverRun = false;
            }
        }

        private void initChart()
        {
            
            chartXY.Series.Clear();
            chartXY.Series.Add("Series 1");
            chartXY.Series.Add("Series 2");
            chartXY.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chartXY.Series[1].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

            chartXY.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.Gainsboro;
            chartXY.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.Gainsboro;

            chartXY.ChartAreas[0].AxisX.Crossing = 0;
            chartXY.ChartAreas[0].AxisY.Crossing = 0;

            this.chartXY.Titles.Add("pt. B2TKS - BPPT");
            
            chartXY.ChartAreas[0].AxisX.Maximum = max_x_chart;
            chartXY.ChartAreas[0].AxisX.Minimum = min_x_chart;
            chartXY.ChartAreas[0].AxisY.Maximum = max_y_chart;
            chartXY.ChartAreas[0].AxisY.Minimum = min_y_chart;
            chartXY.ChartAreas[0].AxisX.Interval = max_x_chart/10;
            chartXY.ChartAreas[0].AxisY.Interval = max_y_chart/10;

            chartXY.ChartAreas[0].AxisX.Title = "waktu";
            chartXY.ChartAreas[0].AxisY.Title = "nilai";

            
        }

        #region not used
        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
        #endregion

        private void plotChart(string[] data)
        {
            

            if (checkBox3.Checked)
            {
                dataPrint[0] = -(Convert.ToDouble(arrAvgData[0]));
                Console.WriteLine("halo" + dataPrint[0]);
                last_x_0 = -last_x_0;
            }
            if (checkBox4.Checked)
            {
                dataPrint[1] = -(Convert.ToDouble(arrAvgData[1]));
                last_x_1 = -last_x_1;
            }
            if (checkBox5.Checked)
            {
                dataPrint[2] = -(Convert.ToDouble(arrAvgData[2]));
            }

            if (!checkBox_holdX.Checked)
            {
                if (checkBox1.Checked)
                {
                    chartXY.Series[0].Points.AddXY(dataPrint[0], dataPrint[2]);
                }
                if (checkBox2.Checked)
                {
                    chartXY.Series[1].Points.AddXY(dataPrint[1], dataPrint[2]);
                }
                firstChecked = true;
            }

            if (checkBox_holdX.Checked)
            {
                if (checkBox1.Checked)
                {
                    chartXY.Series[0].Points.AddXY(last_x_0, dataPrint[2]);
                }
                if (checkBox2.Checked)
                {
                    chartXY.Series[1].Points.AddXY(last_x_1, dataPrint[2]);
                }
            }
        }

        private void button_save_Click(object sender, EventArgs e)
        {
            this.chartXY.SaveImage("D:\\chart.png", ChartImageFormat.Png);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}