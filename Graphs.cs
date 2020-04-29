using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using NationalInstruments.DAQmx;

namespace thermo_test_1
{
    public partial class Graphs : Form
    {
        DAQ_1 opener;
        //NationalInstruments.AnalogWaveform<double> waveforms = new NationalInstruments.AnalogWaveform<double>(30);
        public double[] time = new double[16];
        public double[,] volt_form2 = new double[17,1];

        public Graphs(DAQ_1 arg)
        {
            InitializeComponent();
            opener = arg;
        }
        private void Graphs_Load(object sender, EventArgs e)
        {
            try
            {
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
            catch
            {
                ;
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            
            try
            {
                while (true)
                {                    
                    if (backgroundWorker1.CancellationPending==true)
                    {
                        e.Cancel = true;
                        break;
                    }
                    for (int i = 0; i < 16; i++)
                    {
                        time[i] = opener.time_sec;
                        volt_form2[i,0] = opener.Voltage_Data[i, 0];
                    }
                    volt_form2[16,0] = opener.Temp;
                    Invoke((MethodInvoker)delegate {
                        scatterGraph1.PlotXYAppendMultiple(time,volt_form2);
                        //(time[0], opener.Temp);
                    });                    
                    Thread.Sleep(1000);
                }
            }
            catch
            {
             MessageBox.Show("Background Error..");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ;
        }
    }
}
