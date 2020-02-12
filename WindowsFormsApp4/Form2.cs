using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace WindowsFormsApp4
{
    public partial class Form2 : Form
    {

        private string gFCode = "101Q3";
        private string TimeSelected = "1";
        private string TimeDistance = "Day";
        private int RowNum = 40;
        private int AccControl = 0;

        //messagebox auto closing
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
        System.Threading.Timer _timeoutTimer; //쓰레드 타이머 string _caption; const int WM_CLOSE = 0x0010; 
        string _caption;
        const int WM_CLOSE = 0X0010;//close 명령

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Login();
            TimeDistance_Changed(TimeDistance);
            Load_Data(gFCode, TimeSelected, TimeDistance);
            Delay(100);
            
            getAccount();
            FCode_comboBox.Text = FCode.Text;
            FCode_comboBox.Items.Add(FCode.Text);
        }

        public void Load_Data(string Fcode, string time, string distance)
        {
            gFCode = Fcode;
            
            Comm_Obj_DATA.SetQueryName("TR_FCHART");
            Comm_Obj_DATA.SetSingleData(0, gFCode);

            switch (distance)
            {
                case "Day":
                    Comm_Obj_DATA.SetSingleData(1, "D");
                    Comm_Obj_DATA.SetSingleData(2, time);
                    break;
                case "Week":
                    Comm_Obj_DATA.SetSingleData(1, "W");
                    Comm_Obj_DATA.SetSingleData(2, time);
                    break;
                case "Month":
                    Comm_Obj_DATA.SetSingleData(1, "M");
                    Comm_Obj_DATA.SetSingleData(2, time);
                    break;
                case "Year":
                    Comm_Obj_DATA.SetSingleData(1, "Y");
                    Comm_Obj_DATA.SetSingleData(2, time);
                    break;
                case "Min":
                    Comm_Obj_DATA.SetSingleData(1, "1");
                    Comm_Obj_DATA.SetSingleData(2, TimeSelected);
                    break;
                case "Tick":
                    Comm_Obj_DATA.SetSingleData(1, "T");
                    Comm_Obj_DATA.SetSingleData(2, TimeSelected);
                    break;


            }
            Comm_Obj_DATA.SetSingleData(3, "00000000");
            Comm_Obj_DATA.SetSingleData(4, "99999999");
            Comm_Obj_DATA.SetSingleData(5, RowNum.ToString());
            Comm_Obj_DATA.RequestData();

        }

        private DataTable Proc_TR_FCHART()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("일자");
            dt.Columns.Add("체결시간");
            dt.Columns.Add("시가");
            dt.Columns.Add("고가");
            dt.Columns.Add("저가");
            dt.Columns.Add("종가");
            dt.Columns.Add("WMA");
            dt.Columns.Add("기울기");


            short nRowSize = Convert.ToInt16(Comm_Obj_DATA.GetMultiRowCount());
            for (short i = 0; i < nRowSize; i++)
            {
                DataRow dr = dt.NewRow();
                
                for (short j = 0; j < 6; j++)
                {
                    dr[j] = (string)Comm_Obj_DATA.GetMultiData(i, j);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

       
        
        private void Login()
        {
            bool login;

            string idname;
            idname = "yooncs";
            string password;
            password = "Thisis12#$";
            string AccountPassword;
            AccountPassword = "";
            string path = "C:\\SHINHAN-ii\\indi\\giexpertstarter.exe";

            login = Comm_Obj_DATA.StartIndi(idname, password, AccountPassword, path);


            if (login)
            {
                MessageBox.Show("로그인 성공");
            }
            else
            {
                MessageBox.Show("실패");
            }

        }



        private void FCode_TextChanged(object sender, EventArgs e)
        {
            string fcode = FCode.Text;

            if (fcode.Length == 5)
            {
                gFCode = fcode;

                Load_Data(gFCode, TimeSelected, TimeDistance);
            }
        }


        private void Gi_FC_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_FC();
        }
        
        private void Gi_FC_ReceiveRTData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveRTDataEvent e)
        {
            Proc_FC();
        }
        
        private void Proc_FC()
        {

            FCGrid.DataSource = Proc_TR_FCHART();
            Comm_Obj_DATA_Real.RequestRTReg("TR_FCHART", gFCode);
        }

        private double[] prov_WMA(double[] endPirce, int day)
        {
            double[] outputarray = new double[RowNum];
            int out1, out2;

            TicTacTec.TA.Library.Core.Wma(0, RowNum - 1, endPirce, day, out out1, out out2, outputarray);

            return outputarray;

        }

        private void WMA_input_btn_Click(object sender, EventArgs e)
        {
            WMA_prov();
        }

        private void WMA_prov()
        {
            double[] WMA = new double[RowNum];
            double[] endPrice = new double[RowNum];
            double[] angle = new double[RowNum];
            angle.Initialize();
            double radians;

            if (!string.IsNullOrEmpty(WMA_input.Text) && !string.IsNullOrEmpty(whereText.Text))
            {
                int day = Convert.ToInt32(WMA_input.Text);
                int time = Convert.ToInt32(TimeSelected);
                int where = Convert.ToInt32(whereText.Text);

                for (int i = 0; i <= RowNum - 1; i++)
                {
                    endPrice[i] = Convert.ToDouble(FCGrid.Rows[i].Cells[4].Value);

                }

                WMA = prov_WMA(endPrice, day);

                for (int j = 0; j <= RowNum - 1; j++)
                {
                    FCGrid.Rows[j].Cells[6].Value = WMA[j];
                }
                for (int k = 1; k <= RowNum - 1; k++)
                {
                    if (WMA[k] == 0 || WMA[k - 1] == 0)
                    {
                        FCGrid.Rows[k - 1].Cells[6].Value = 0;
                    }
                    else
                    {
                        if (WMA[k + where - 1] == 0)
                        {
                            break;
                        }
                        else
                        {
                            radians = Math.Atan((WMA[k + where - 1] - WMA[k - 1]) / time);
                            angle[k - 1] = radians * 57.3;
                        }

                    }

                }
                for (int j = 0; j <= RowNum; j++)
                {
                    FCGrid.Rows[j].Cells[7].Value = Convert.ToInt32(angle[j]);
                }
                int index = getWMA_Index(Convert.ToInt32(startWma.Text), Convert.ToInt32(endWma.Text), Convert.ToInt32(intervalWma.Text)).Length;
                double[] aaa = new double[index];

                for (int i = 0; i < index; i++)
                {
                    aaa[i] = prov_WMA(endPrice, getWMA_Index(Convert.ToInt32(startWma.Text), Convert.ToInt32(endWma.Text), Convert.ToInt32(intervalWma.Text))[i])[0];
                }
                Mecro(aaa);
            }
            else
            {
                MessageBox.Show("cex");
            }

        }

        private int[] getWMA_Index(int WMA_Start, int WMA_End, int WMA_Interval)
        {
            int num = Convert.ToInt32((WMA_End - WMA_Start) / WMA_Interval + 1);
            int[] WMA = new int[num];
            for (int i = 0; i < num; i++)
            {
                WMA[i] = WMA_Start + WMA_Interval * i;
            }
            return WMA;
        }

        private void Mecro(double[] WMA)
        {
            double[] aa = new double[WMA.Length], bb = new double[WMA.Length], cc = new double[WMA.Length];
            aa = (double[])WMA.Clone(); // WMA 복사
            //bb  // WMA sort
            //cc  // WMA reverse sort
            Array.Sort(WMA);
            Array.Copy(WMA, bb, WMA.Length);

            Array.Reverse(WMA);
            Array.Copy(WMA, cc, WMA.Length);

            if (checkSameArray(aa, bb) == true)
            {
                AutoClosingMessageBox("매수", "알림", 1000);
            }
            else if (checkSameArray(aa, cc) == true)
            {
                AutoClosingMessageBox("매도", "알림", 1000);
            }
        }

        public bool checkSameArray(double[] arr1, double[] arr2)
        {
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }

        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow(null, _caption);
            if (mbWnd != IntPtr.Zero) SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }

        private void AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption; _timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, System.Threading.Timeout.Infinite);
            MessageBox.Show(text, caption);

        }

        //주기

        private void TimeDistance_Changed(string distance)
        {
            TimeDistance = distance;

            string set_time = "1";
            
            Day_btn.BackColor = SystemColors.Control;
            Day_btn.ForeColor = SystemColors.ControlText;
            Week_btn.BackColor = SystemColors.Control;
            Week_btn.ForeColor = SystemColors.ControlText;
            Month_btn.BackColor = SystemColors.Control;
            Month_btn.ForeColor = SystemColors.ControlText;
            Year_btn.BackColor = SystemColors.Control;
            Year_btn.ForeColor = SystemColors.ControlText;
            Min_btn.BackColor = SystemColors.Control;
            Min_btn.ForeColor = SystemColors.ControlText;
            Tick_btn.BackColor = SystemColors.Control;
            Tick_btn.ForeColor = SystemColors.ControlText;

            switch (TimeDistance)
            {
                case "Day":
                    Day_btn.BackColor = SystemColors.Highlight;
                    Day_btn.ForeColor = SystemColors.Control;
                    set_time = "1";
                    Time_btn_Disabled();
                    break;
                case "Week":
                    Week_btn.BackColor = SystemColors.Highlight;
                    Week_btn.ForeColor = SystemColors.Control;
                    set_time = "7";
                    Time_btn_Disabled();
                    break;
                case "Month":
                    Month_btn.BackColor = SystemColors.Highlight;
                    Month_btn.ForeColor = SystemColors.Control;
                    set_time = "30";
                    Time_btn_Disabled();
                    break;
                case "Year":
                    Year_btn.BackColor = SystemColors.Highlight;
                    Year_btn.ForeColor = SystemColors.Control;
                    set_time = "365";
                    Time_btn_Disabled();
                    break;
                case "Min":
                    Min_btn.BackColor = SystemColors.Highlight;
                    Min_btn.ForeColor = SystemColors.Control;
                    set_time = TimeSelected;
                    Time_btn_Enabled();
                    Time_Changed(TimeSelected);
                    break;
                case "Tick":
                    Tick_btn.BackColor = SystemColors.Highlight;
                    Tick_btn.ForeColor = SystemColors.Control;
                    set_time = TimeSelected;
                    Time_btn_Enabled();
                    Time_Changed(TimeSelected);
                    break;
            }

            Load_Data(gFCode, set_time, TimeDistance);

        }

        private void Day_btn_Click(object sender, EventArgs e)
        {
            TimeDistance_Changed("Day");
        }

        private void Week_btn_Click(object sender, EventArgs e)
        {
            TimeDistance_Changed("Week");
        }

        private void Month_btn_Click(object sender, EventArgs e)
        {
            TimeDistance_Changed("Month");
        }

        private void Year_btn_Click(object sender, EventArgs e)
        {
            TimeDistance_Changed("Year");
        }

        private void Min_btn_Click(object sender, EventArgs e)
        {
            TimeDistance_Changed("Min");
        }

        private void Tick_btn_Click(object sender, EventArgs e)
        {
            TimeDistance_Changed("Tick");
        }

        //시간 단위

        private void Time_Changed(string time)
        {
            Time_btn_1.BackColor = SystemColors.Control;
            Time_btn_1.ForeColor = SystemColors.ControlText;
            Time_btn_3.BackColor = SystemColors.Control;
            Time_btn_3.ForeColor = SystemColors.ControlText;
            Time_btn_5.BackColor = SystemColors.Control;
            Time_btn_5.ForeColor = SystemColors.ControlText;
            Time_btn_10.BackColor = SystemColors.Control;
            Time_btn_10.ForeColor = SystemColors.ControlText;
            Time_btn_15.BackColor = SystemColors.Control;
            Time_btn_15.ForeColor = SystemColors.ControlText;
            Time_btn_30.BackColor = SystemColors.Control;
            Time_btn_30.ForeColor = SystemColors.ControlText;
            Time_btn_45.BackColor = SystemColors.Control;
            Time_btn_45.ForeColor = SystemColors.ControlText;
            Time_btn_60.BackColor = SystemColors.Control;
            Time_btn_60.ForeColor = SystemColors.ControlText;

            TimeSelected = time;
            Time_ComboBox.Text = TimeSelected;

            switch (time)
            {
                case "1":
                    Time_btn_1.BackColor = SystemColors.Highlight;
                    Time_btn_1.ForeColor = SystemColors.Control;
                    break;
                case "3":
                    Time_btn_3.BackColor = SystemColors.Highlight;
                    Time_btn_3.ForeColor = SystemColors.Control;
                    break;
                case "5":
                    Time_btn_5.BackColor = SystemColors.Highlight;
                    Time_btn_5.ForeColor = SystemColors.Control;
                    break;
                case "10":
                    Time_btn_10.BackColor = SystemColors.Highlight;
                    Time_btn_10.ForeColor = SystemColors.Control;
                    break;
                case "15":
                    Time_btn_15.BackColor = SystemColors.Highlight;
                    Time_btn_15.ForeColor = SystemColors.Control;
                    break;
                case "30":
                    Time_btn_30.BackColor = SystemColors.Highlight;
                    Time_btn_30.ForeColor = SystemColors.Control;
                    break;
                case "45":
                    Time_btn_45.BackColor = SystemColors.Highlight;
                    Time_btn_45.ForeColor = SystemColors.Control;
                    break;
                case "60":
                    Time_btn_60.BackColor = SystemColors.Highlight;
                    Time_btn_60.ForeColor = SystemColors.Control;
                    break;
            }

            Load_Data(gFCode, TimeSelected, TimeDistance);
        }

        private void Time_btn_1_Click(object sender, EventArgs e)
        {
            Time_Changed("1");
        }

        private void Time_btn_3_Click(object sender, EventArgs e)
        {
            Time_Changed("3");
        }

        private void Time_btn_5_Click(object sender, EventArgs e)
        {
            Time_Changed("5");
        }

        private void Time_btn_10_Click(object sender, EventArgs e)
        {
            Time_Changed("10");
        }

        private void Time_btn_15_Click(object sender, EventArgs e)
        {
            Time_Changed("15");
        }

        private void Time_btn_30_Click(object sender, EventArgs e)
        {
            Time_Changed("30");
        }

        private void Time_btn_45_Click(object sender, EventArgs e)
        {
            Time_Changed("45");
        }

        private void Time_btn_60_Click(object sender, EventArgs e)
        {
            Time_Changed("60");
        }

        private void Time_btn_Disabled()
        {
            Time_btn_1.Enabled = false;
            Time_btn_3.Enabled = false;
            Time_btn_5.Enabled = false;
            Time_btn_10.Enabled = false;
            Time_btn_15.Enabled = false;
            Time_btn_30.Enabled = false;
            Time_btn_45.Enabled = false;
            Time_btn_60.Enabled = false;
            Time_btn_1.BackColor = SystemColors.ControlLight;
            Time_btn_1.ForeColor = SystemColors.ControlDark;
            Time_btn_3.BackColor = SystemColors.ControlLight;
            Time_btn_3.ForeColor = SystemColors.ControlDark;
            Time_btn_5.BackColor = SystemColors.ControlLight;
            Time_btn_5.ForeColor = SystemColors.ControlDark;
            Time_btn_10.BackColor = SystemColors.ControlLight;
            Time_btn_10.ForeColor = SystemColors.ControlDark;
            Time_btn_15.BackColor = SystemColors.ControlLight;
            Time_btn_15.ForeColor = SystemColors.ControlDark;
            Time_btn_30.BackColor = SystemColors.ControlLight;
            Time_btn_30.ForeColor = SystemColors.ControlDark;
            Time_btn_45.BackColor = SystemColors.ControlLight;
            Time_btn_45.ForeColor = SystemColors.ControlDark;
            Time_btn_60.BackColor = SystemColors.ControlLight;
            Time_btn_60.ForeColor = SystemColors.ControlDark;
            Time_ComboBox.Enabled = false;
            Time_ComboBox.ForeColor = SystemColors.ControlDark;

        }
        private void Time_btn_Enabled()
        {
            Time_btn_1.Enabled = true;
            Time_btn_3.Enabled = true;
            Time_btn_5.Enabled = true;
            Time_btn_10.Enabled = true;
            Time_btn_15.Enabled = true;
            Time_btn_30.Enabled = true;
            Time_btn_45.Enabled = true;
            Time_btn_60.Enabled = true;
            Time_btn_1.BackColor = SystemColors.Highlight;
            Time_btn_1.ForeColor = SystemColors.Control;
            Time_btn_3.BackColor = SystemColors.Control;
            Time_btn_3.ForeColor = SystemColors.ControlText;
            Time_btn_5.BackColor = SystemColors.Control;
            Time_btn_5.ForeColor = SystemColors.ControlText;
            Time_btn_10.BackColor = SystemColors.Control;
            Time_btn_10.ForeColor = SystemColors.ControlText;
            Time_btn_15.BackColor = SystemColors.Control;
            Time_btn_15.ForeColor = SystemColors.ControlText;
            Time_btn_30.BackColor = SystemColors.Control;
            Time_btn_30.ForeColor = SystemColors.ControlText;
            Time_btn_45.BackColor = SystemColors.Control;
            Time_btn_45.ForeColor = SystemColors.ControlText;
            Time_btn_60.BackColor = SystemColors.Control;
            Time_btn_60.ForeColor = SystemColors.ControlText;
            Time_ComboBox.Enabled = true;
            Time_ComboBox.ForeColor = SystemColors.ControlText;
        }
        private void Time_ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Time_ComboBox.SelectedIndex >= 0)
            {
                this.TimeSelected = Time_ComboBox.SelectedItem as string;
            }
        }

        private void Time_ComboBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void Refresh_Data(object sender, EventArgs e)
        {
            
            Load_Data(gFCode, TimeSelected, TimeDistance);
            Delay(100);
            WMA_prov();
            
        }

        private static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }

        

        private void getAccount()
        {
            Comm_Obj_Account.SetQueryName("AccountList");
            Comm_Obj_Account.RequestData();
        }

        public void AccountInfo()
        {
            Comm_Obj_Account.SetQueryName("SABA655Q1");
            Comm_Obj_Account.SetSingleData(0, Account_Num.Text); //00311155910
            Comm_Obj_Account.SetSingleData(1, "01");
            Comm_Obj_Account.SetSingleData(2, "0000");
            Comm_Obj_Account.RequestData();
        }

        private void Proc_AccountList()
        {
            Account_Num.Text = (string)Comm_Obj_Account.GetMultiData(0, 0);
            Account_Name.Text = (string)Comm_Obj_Account.GetMultiData(0, 1);
            Account_Num2.Text = (string)Comm_Obj_Account.GetMultiData(0, 0);
            Account_Name2.Text = (string)Comm_Obj_Account.GetMultiData(0, 1);
            Account_GridView.Rows.Add();
            Account_GridView.Rows[0].HeaderCell.Value = "순자산";
            Account_GridView.Rows.Add();
            Account_GridView.Rows[1].HeaderCell.Value = "총자산";
            Account_GridView.Rows.Add();
            Account_GridView.Rows[2].HeaderCell.Value = "KOSPI";
            Account_GridView.Rows.Add();
            Account_GridView.Rows[3].HeaderCell.Value = "예수금";
            Account_GridView.Rows.Add();
            Account_GridView.Rows[4].HeaderCell.Value = "현금증거금";
            Account_GridView.Rows.Add();
            Account_GridView.Rows[5].HeaderCell.Value = "인출가능금액";
        }

        private void Proc_SABA655Q1()
        {
            Account_GridView.Rows[0].Cells[0].Value = Convert.ToInt32(Comm_Obj_Account.GetSingleData(0));
            Account_GridView.Rows[1].Cells[0].Value = Convert.ToInt32(Comm_Obj_Account.GetSingleData(1));
            Account_GridView.Rows[2].Cells[0].Value = Convert.ToInt32(Comm_Obj_Account.GetSingleData(5));
            Account_GridView.Rows[3].Cells[0].Value = Convert.ToInt32(Comm_Obj_Account.GetSingleData(18));
            Account_GridView.Rows[4].Cells[0].Value = Convert.ToInt32(Comm_Obj_Account.GetSingleData(19));
            Account_GridView.Rows[5].Cells[0].Value = Convert.ToInt32(Comm_Obj_Account.GetSingleData(20));
        }

        private void Comm_Obj_Account_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            if (AccControl == 0)
                Proc_AccountList();
            else if(AccControl == 1)
                Proc_SABA655Q1();
        }

        private void Lookup_btn_Click(object sender, EventArgs e)
        {
            AccControl = 1;
            AccountInfo();
        }

        private void getPrice()
        {
            Comm_Obj_Price.SetQueryName("SABC820Q1");
            Comm_Obj_Price.SetSingleData(0, "20200210");
            Comm_Obj_Price.SetSingleData(1, Account_Num2.Text); //00311155910
            Comm_Obj_Price.SetSingleData(2, "0000");
            Comm_Obj_Price.RequestData();
            
        }

        private void Proc_SABC820Q1()
        {
            Price_GridView.Rows[0].Cells[0].Value = (string)Comm_Obj_Price.GetMultiData(0,3);
            Price_GridView.Rows[0].Cells[1].Value = (string)Comm_Obj_Price.GetMultiData(0,4);
            Price_GridView.Rows[0].Cells[2].Value = (string)Comm_Obj_Price.GetMultiData(0,6);
            Price_GridView.Rows[0].Cells[3].Value = (string)Comm_Obj_Price.GetMultiData(0,11);
            Price_GridView.Rows[0].Cells[4].Value = (string)Comm_Obj_Price.GetMultiData(0,12);
            MessageBox.Show((string)Comm_Obj_Price.GetErrorMessage());
        }

        private void Comm_Obj_Price_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABC820Q1();
        }

        private void Price_Lookup_btn_Click(object sender, EventArgs e)
        {
            getPrice();
        }
    }
}
