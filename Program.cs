using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace thermo_test_1
{
    static class Program
    {
        public static DAQ_1 DAQ_1;
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            DAQ_1 = new DAQ_1();
            Application.Run(DAQ_1);
        }
    }
}
