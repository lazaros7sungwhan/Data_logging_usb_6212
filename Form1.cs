using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NationalInstruments.DAQmx;
using System.Diagnostics;


namespace thermo_test_1
{
    
    public partial class DAQ_1 : Form
    {
        private NationalInstruments.DAQmx.Task mytask;
        private NationalInstruments.DAQmx.Task running_task;
        private AnalogMultiChannelReader analogreader;
        public double[,] Voltage_Data;
        private AsyncCallback analogCallback;

        private StreamWriter fileStreamWriter;

        System.Timers.Timer timerstimer = new System.Timers.Timer();//Timer 이용

        // Real data  "File Save Data"
        Stopwatch timerstimersw = new Stopwatch();
        private string filenamewrite = null;
        public double time_sec=0;
        public double Temp = 0;

        /*Public Values for graphs*/
        public double[,] value_for_graphs;

        public double PPPP;

        public Graphs graphs;
        public DAQ_1()
        {
            InitializeComponent();

            button5.Enabled = false;

            /*Timer Setting*/
            //timerstimer = new System.Timers.Timer();
            //timerstimer.Interval = 10;
            //timerstimer.Elapsed += Timerstimer_Elapsed;

            //timerstimer.AutoReset = true;
            //  timerstimer.Start();
            //  timerstimersw.Start();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            serialPort1.Close();
            backgroundWorker1.CancelAsync();
            running_task = null;
            if(mytask!=null)
            mytask.Dispose();
            Application.Exit();
        }

        private void DAQ_1_Load(object sender, EventArgs e)
        {
            backgroundWorker1.WorkerSupportsCancellation = true;
            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            comboBox2.Items.AddRange(DaqSystem.Local.GetPhysicalChannels(PhysicalChannelTypes.AI, PhysicalChannelAccess.External));
            if (comboBox2.Items.Count > 0)
            comboBox2.SelectedIndex = 0;
        }
        //Connect button .....
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = 9600;
                serialPort1.Open();
            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;

                string recdata;

                richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); richTextBox1.AppendText("reading...\n"); });

                //serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceivedEventHandler);            

                while (true)
                {
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        break;
                    }

                    recdata = serialPort1.ReadLine();
                    //richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); richTextBox1.AppendText(recdata); });

                    int position1 = recdata.IndexOf("index");
                    if (position1 != -1)
                    {
                        Invoke((MethodInvoker)delegate {
                            textBox2.Text = recdata.Substring(position1 + 13);
                            Temp = Convert.ToDouble((textBox2.Text));
                        });
                    }
                    recdata = null;
                }

            }
            catch
            {
                ;
            }

        }
        
        /*private void DataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e)
        {

            try
            {
                SerialPort sData = (SerialPort)sender;

                string recdata = sData.ReadLine();
                double temperature_sub;
                richTextBox1.Invoke((MethodInvoker)delegate { richTextBox1.ScrollToCaret(); richTextBox1.AppendText(recdata); });
                int position1 = recdata.IndexOf("index");
                if(position1!=-1)
                {
                    textBox2.Invoke((MethodInvoker)delegate { textBox2.Text = recdata.Substring(position1+13); });
                    temperature_sub=Convert.ToDouble(textBox2.Text);
                    textBox3.Invoke((MethodInvoker)delegate { textBox3.Text = temperature[0].ToString(); });

                    for(int i=0;i<sample_time-1; i++)
                    {
                        temperature[i] = temperature[i+1];
                    }

                    if(data_count<sample_time)
                    {
                        temperature[data_count] = temperature_sub;
                        data_count++;
                    }
                    else
                    {
                     for(int i=0;i<sample_time-1; i++)
                      {
                          temperature[i] = temperature[i+1];
                      }
                      temperature[sample_time - 1] = temperature_sub;
                    }

                    textBox4.Invoke((MethodInvoker)delegate { textBox4.Text = sample_time.ToString();});


                    Invoke((MethodInvoker)delegate {
                        easyChart1.Plot(temperature);
                    });
                }                
            }
            catch
            {
                Application.ExitThread();

            }
        }*/

        private void button4_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            Back_Save_worker.CancelAsync();
            Timer_Backup.CancelAsync();
            serialPort1.Close();
            if(mytask!=null)
            {
                running_task = null;
                mytask.Dispose();
            }

            //Timer stopwatch
            timerstimer.Stop();
            timerstimersw.Stop();
            timerstimersw.Reset();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                mytask = new NationalInstruments.DAQmx.Task();
                //voltage channel Configure AITerminalConfiguration -1  to Rse.. Check in NI MAX..
                mytask.AIChannels.CreateVoltageChannel(comboBox2.Text, "", AITerminalConfiguration.Rse, -10, 10, AIVoltageUnits.Volts);
                mytask.Timing.ConfigureSampleClock("", Convert.ToDouble(textBox7.Text), (SampleClockActiveEdge.Rising), SampleQuantityMode.ContinuousSamples, 1000);
                mytask.Control(TaskAction.Verify);
                running_task = mytask;
                analogreader = new AnalogMultiChannelReader(mytask.Stream);
                analogreader.SynchronizeCallbacks = true;
                analogCallback = new AsyncCallback(analogInCallback);
                analogreader.BeginReadMultiSample(Convert.ToInt32(textBox8.Text), analogCallback, mytask);
            }
            catch
            {
                MessageBox.Show("error_1");
                running_task = null;
                mytask.Dispose();
            }            
        }

        private void analogInCallback(IAsyncResult ar)
        {
            try
            {
                if(running_task!=null&&running_task==ar.AsyncState)
                {
                    Voltage_Data = analogreader.EndReadMultiSample(ar);
                    value_for_graphs = Voltage_Data;
                }
                Invoke((MethodInvoker)delegate {
                    textBox9.Text = Voltage_Data[0, 0].ToString("f3"); //1
                    textBox10.Text = Voltage_Data[1, 0].ToString("f3");//2
                    textBox11.Text = Voltage_Data[2, 0].ToString("f3");//3
                    textBox12.Text = Voltage_Data[3, 0].ToString("f3");//4
                    textBox13.Text = Voltage_Data[4, 0].ToString("f3");//5
                    textBox14.Text = Voltage_Data[5, 0].ToString("f3");//6
                    textBox15.Text = Voltage_Data[6, 0].ToString("f3");//7
                    textBox16.Text = Voltage_Data[7, 0].ToString("f3");//8
                    textBox17.Text = Voltage_Data[8, 0].ToString("f3");//9
                    textBox18.Text = Voltage_Data[9, 0].ToString("f3");//10
                    textBox19.Text = Voltage_Data[10, 0].ToString("f3");//11
                    textBox20.Text = Voltage_Data[11, 0].ToString("f3");//12
                    textBox21.Text = Voltage_Data[12, 0].ToString("f3");//13
                    textBox22.Text = Voltage_Data[13, 0].ToString("f3");//14
                    textBox23.Text = Voltage_Data[14, 0].ToString("f3");//15
                    textBox24.Text = Voltage_Data[15, 0].ToString("f3");//16

                    //Setting depending on the number of Data Acqusition port
                });                
                analogreader.BeginReadMultiSample(Convert.ToInt32(textBox8.Text), analogCallback, mytask);
            }
            catch
            {
                MessageBox.Show("Callback_Error");
                running_task = null;
                mytask.Dispose();
            }
        }


        private void button3_Click(object sender, EventArgs e)  // brouse file 
        {
            saveFileDialog1.DefaultExt = "*.txt";
            saveFileDialog1.FileName = "acquisitionData.txt";
            saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";

            DialogResult result = saveFileDialog1.ShowDialog();
            if(result==DialogResult.OK)
            {
                filenamewrite = saveFileDialog1.FileName;
                Invoke((MethodInvoker)delegate {
                    textBox1.Text = filenamewrite;
                    button5.Enabled = true;
                });
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if(!Back_Save_worker.IsBusy)
                Back_Save_worker.RunWorkerAsync();               
            }
            catch
            {
            }
        }
        private void Back_Save_worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker2 = sender as BackgroundWorker;
            try
            {
                int count = 0;
                while (true)
                {
                    if (worker2.CancellationPending == true)
                    {
                        e.Cancel = true;
                        break;
                    }
                    if (count != 0)
                    filenamewrite = saveFileDialog1.FileName;
                    FileStream fs = new FileStream(filenamewrite, FileMode.Append);
                    fileStreamWriter = new StreamWriter(fs);
                    //MessageBox.Show("saveing");
                    for(int j=0;j<3;j++)
                    {
                        for (int i = 0; i < 16; i++)
                        {
                            fileStreamWriter.Write(Voltage_Data[i, 0].ToString("f3"));
                            fileStreamWriter.Write("\t");
                        }
                        fileStreamWriter.Write((time_sec).ToString("f1"));
                        fileStreamWriter.Write("\t");
                        fileStreamWriter.Write(Temp);
                        fileStreamWriter.Write("\t");
                        fileStreamWriter.WriteLine();

                        Thread.Sleep(3000);
                    }
                    fileStreamWriter.Close();
                    count++;
                }
            }
            catch
            {
                MessageBox.Show("Saveing Error");
            }
        }

        // Timer 

        private void Time_button_Click(object sender, EventArgs e)
        {
            Timer_Backup.RunWorkerAsync();
        }
        private void Timer_Backup_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;         
               
            /*Timer Setting*/
           timerstimer = new System.Timers.Timer();
           timerstimer.Interval = 10;
           timerstimer.Elapsed += Timerstimer_Elapsed;

           timerstimer.AutoReset = true;
           timerstimer.Start();
           timerstimersw.Start();
          /*
            while(true)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                textBox3.Text = (time_sec++ / 100).ToString("f1");
                Thread.Sleep(10);
            }*/
        }
        
        private void Timerstimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate ()
            {
                //textBox3.Text = (time_sec++ / 100).ToString("f1");
                time_sec = timerstimersw.ElapsedMilliseconds / 1000; 
                textBox3.Text =(time_sec).ToString();
            }));
            //timerstimersw.Reset();
        }

        private void Timer_Reset_Btn_Click(object sender, EventArgs e)
        {
            try
            {
                timerstimersw.Reset();
            }
            catch
            {
                ;
            }
        }

        private void graphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    if (!Back_Worker_Form2.IsBusy)
                    {
                        Back_Worker_Form2.RunWorkerAsync();
                    }
                }
                catch
                {
                    ;
                }
            }
            catch
            {
                MessageBox.Show("error to open the form2");
            }
        }

        private void Back_Worker_Form2_DoWork(object sender, DoWorkEventArgs e)
        {
            Graphs gr= new Graphs(this);
            gr.ShowDialog();
        }
    }
}