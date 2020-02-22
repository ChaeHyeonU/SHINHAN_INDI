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
        private string TimeSelected = "3";
        private string TimeDistance = "Min";
        private int RowNum = 200;
        private int control_buy_sell = 0;
        private int control_Enable_Angle = 0;

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
            //Login();
            TimeDistance_Changed(TimeDistance);
            Load_Data(gFCode, TimeSelected, TimeDistance);
            Delay(100);
            //WMA_prov();
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
                case "Min":
                    Comm_Obj_DATA.SetSingleData(1, "1");
                    Comm_Obj_DATA.SetSingleData(2, time);
                    break;
                case "Tick":
                    Comm_Obj_DATA.SetSingleData(1, "T");
                    Comm_Obj_DATA.SetSingleData(2, time);
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
            dt.Columns.Add("상태");
            dt.Columns.Add("매수/매도");


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

        private void WMA_input_btn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(startWma.Text) && !string.IsNullOrEmpty(endWma.Text) && !string.IsNullOrEmpty(intervalWma.Text))
                Get_GridData();
            else
                MessageBox.Show("입력값 오류");
        }

        private void Get_GridData()
        {
            if (string.IsNullOrEmpty(startWma.Text) || string.IsNullOrEmpty(endWma.Text) || string.IsNullOrEmpty(intervalWma.Text))
            {
                MessageBox.Show("입력값 오류");
            }
            else if (string.IsNullOrEmpty(WMA_input.Text))
            {
                MessageBox.Show("WMA 간격설정 오류");
            }
            else if (string.IsNullOrEmpty(Angle_input.Text))
            {
                MessageBox.Show("기울기 설정 오류");
            }
            else if (string.IsNullOrEmpty(whereText.Text))
            {
                MessageBox.Show("기울기 간격설정 오류");
            }
            else
            {
                int day = Convert.ToInt32(WMA_input.Text);
                int[] index = getWMA_Index(Convert.ToInt32(startWma.Text), Convert.ToInt32(endWma.Text), Convert.ToInt32(intervalWma.Text));
                int index_length = index.Length;
                double[] aaa = new double[index_length];

                for (int i = 0; i < RowNum; i++)
                {
                    FCGrid.Rows[i].Cells[6].Value = Prov_WMA(Get_EndPrice(), day, 0)[i];
                    FCGrid.Rows[i].Cells[7].Value = Get_Angle(Prov_WMA(Get_EndPrice(), day, 0))[i];
                    FCGrid.Rows[i].Cells[9].Value = "";
                    FCGrid.Rows[i].Cells[9].Style.BackColor = SystemColors.Window;
                    FCGrid.Rows[i].Cells[9].Style.ForeColor = SystemColors.WindowText;
                }

                for (int j = RowNum - index[index_length - 1]; j >= 0; j--)
                {
                    for (int i = 0; i < index_length; i++)
                    {
                        aaa[i] = Prov_WMA(Get_EndPrice(), index[i], 0)[j];
                    }
                    Mecro(aaa, j);
                }
                for (int j = RowNum - index[index_length - 1]; j >= 0; j--)
                {
                    for (int i = 0; i < index_length; i++)
                    {
                        aaa[i] = Prov_WMA(Get_EndPrice(), index[i], 0)[j];
                    }
                    set_Condition(aaa, j);
                }
            }
        }


        private int[] Get_Angle(double[] WMA)
        {
            int[] angle = new int[RowNum];

            int day = Convert.ToInt32(WMA_input.Text);
            int time = Convert.ToInt32(TimeSelected);
            int where = Convert.ToInt32(whereText.Text);

            double radians;

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
                        angle[k - 1] = Convert.ToInt32(radians * 57.3);
                    }
                }
            }
            return angle;
        }

        private double[] Get_EndPrice()
        {
            double[] EndPrice = new double[RowNum];
            for (int i = 0; i < RowNum; i++)
            {
                EndPrice[i] = Convert.ToDouble(FCGrid.Rows[i].Cells[5].Value);
            }

            return EndPrice;
        }

        private double[] Prov_WMA(double[] inputarray, int day, int startidx)
        {
            int arr_length = inputarray.Length;
            double[] outputarray = new double[arr_length];

            for (int i = startidx; i < arr_length; i++)
            {
                if (i > arr_length - day)
                {
                    outputarray[i] = 0;
                    continue;
                }
                else
                {
                    double arr_sum = 0.0;
                    int day_sum = 0;

                    for (int j = 0; j < day; j++)
                    {
                        arr_sum += inputarray[i + j] * (day - j);
                        day_sum += j + 1;
                    }
                    outputarray[i] = arr_sum / day_sum;
                }
            }
            return outputarray;
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

        private void set_Condition(double[] WMA, int index)
        {
            double[] aa = new double[WMA.Length], bb = new double[WMA.Length], cc = new double[WMA.Length];

            Array.Copy(WMA, aa,WMA.Length);
            Array.Sort(WMA);
            Array.Copy(WMA, bb, WMA.Length);

            Array.Reverse(WMA);
            Array.Copy(WMA, cc, WMA.Length);

            if(checkSameArray(aa,bb) == true)
            {
                FCGrid.Rows[index].Cells[8].Value = "역배열";
            }
            else if(checkSameArray(aa,cc) == true)
            {
                FCGrid.Rows[index].Cells[8].Value = "정배열";
            }
            else
            {
                FCGrid.Rows[index].Cells[8].Value = "혼조세";
            }
        }

        private void Mecro(double[] WMA, int index)
        {
            double[] aa = new double[WMA.Length], bb = new double[WMA.Length], cc = new double[WMA.Length];
            int angle = Convert.ToInt32(Angle_input.Text);

            Array.Copy(WMA, aa, WMA.Length); // WMA 복사
            //bb  // WMA sort
            //cc  // WMA reverse sort
            Array.Sort(WMA);
            Array.Copy(WMA, bb, WMA.Length);

            Array.Reverse(WMA);
            Array.Copy(WMA, cc, WMA.Length);

            //control_buy_sell // 0: 일반 1:역배 2: 정배 3:역배인데 기울기 x 4: 정배인데 기울기 x
            //control_Enable_Angle // 0:아무것도 없는 상태 1:매도 2:매수

            if (checkSameArray(aa, bb) == true && control_buy_sell != 1 && control_Enable_Angle != 2) //역배 && 전 상태 != 역배 && !매수
            {
                if (Math.Abs(Convert.ToInt32(FCGrid.Rows[index].Cells[7].Value)) > angle)
                {
                    FCGrid.Rows[index].Cells[9].Value = "매도";
                    FCGrid.Rows[index].Cells[9].Style.BackColor = Color.Tomato;
                    FCGrid.Rows[index].Cells[9].Style.ForeColor = Color.White;
                    control_buy_sell = 1;
                    control_Enable_Angle = 1;
                }
                else
                {
                    control_buy_sell = 3;
                }
            }
            if (checkSameArray(aa, bb) == true && control_buy_sell != 1 && control_Enable_Angle == 2) //역배 && 전 상태 != 역배 && 매수
            {
                 FCGrid.Rows[index].Cells[9].Value = "매도(청산)";
                 FCGrid.Rows[index].Cells[9].Style.BackColor = Color.Tomato;
                 FCGrid.Rows[index].Cells[9].Style.ForeColor = Color.White;
                 control_buy_sell = 1;
                 control_Enable_Angle = 0;
            }
            else if (checkSameArray(aa, bb) == true && control_buy_sell == 1)
            {
                control_buy_sell = 1;
            }
            else if (checkSameArray(aa, cc) == true && control_buy_sell != 2 && control_Enable_Angle !=1)//정배 && 전 상태 != 정배 && !매도
            {
                if (Math.Abs(Convert.ToInt32(FCGrid.Rows[index].Cells[7].Value)) > angle)
                {
                    FCGrid.Rows[index].Cells[9].Value = "매수";
                    FCGrid.Rows[index].Cells[9].Style.BackColor = SystemColors.Highlight;
                    FCGrid.Rows[index].Cells[9].Style.ForeColor = Color.White;
                    control_buy_sell = 2;
                    control_Enable_Angle = 2;
                }
                else
                {
                    control_buy_sell = 4;
                }
            }
            else if (checkSameArray(aa, cc) == true && control_buy_sell != 2 && control_Enable_Angle == 1)//정배 && 전 상태 !=정배 && 매도
            {
                FCGrid.Rows[index].Cells[9].Value = "매수(청산)";
                FCGrid.Rows[index].Cells[9].Style.BackColor = SystemColors.Highlight;
                FCGrid.Rows[index].Cells[9].Style.ForeColor = Color.White;
                control_buy_sell = 2;
                control_Enable_Angle = 0;
            }
            else if (checkSameArray(aa, cc) == true && control_buy_sell == 2)
            {
                control_buy_sell = 2;
            }
            else
            {
                control_buy_sell = 0;                
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
            TimeSelected = Time_ComboBox.Text;
            Time_Changed(TimeSelected);
        }

        private void Refresh_Data(object sender, EventArgs e)
        {
            Load_Data(gFCode, TimeSelected, TimeDistance);
            Delay(100);
            Get_GridData();
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
            axGiExpertControl1.SetQueryName("AccountList");
            axGiExpertControl1.RequestData();
        }

        public void AccountInfo()
        {
            if (AccountPW.Text != "0000")
            {
                MessageBox.Show("비밀번호 확인");
            }
            else
            {
                Comm_Obj_Account.SetQueryName("SABA655Q1");
                Comm_Obj_Account.SetSingleData(0, Account_Num.Text); //00311155910
                Comm_Obj_Account.SetSingleData(1, "01");
                Comm_Obj_Account.SetSingleData(2, AccountPW.Text);
                Comm_Obj_Account.RequestData();
            }
        }

        private void Proc_AccountList()
        {
            short nRowSize = axGiExpertControl1.GetMultiRowCount();
            for (short i = 0; i < nRowSize; i++)
            {
                Account_Num.Items.Add((string)axGiExpertControl1.GetMultiData(i, 0));
                Account_Num2.Items.Add((string)axGiExpertControl1.GetMultiData(i, 0));
            }
            //Account_Name.Text = (string)Comm_Obj_Account.GetMultiData(0, 1);
            //Account_Name2.Text = (string)Comm_Obj_Account.GetMultiData(0, 1);
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
            Proc_SABA655Q1();
        }
        private void Comm_Obj_AccountList_ReceivedData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_AccountList();
        }

        private void Lookup_btn_Click(object sender, EventArgs e)
        {
            AccountInfo();
        }

        private void getPrice()
        {
            Comm_Obj_Price.SetQueryName("SABC820Q1");
            Comm_Obj_Price.SetSingleData(0, "20200213");
            Comm_Obj_Price.SetSingleData(1, Account_Num2.Text); //00311155910
            Comm_Obj_Price.SetSingleData(2, "0000");
            Comm_Obj_Price.RequestData();
        }

        private void Proc_SABC820Q1()
        {
            Price_GridView.Rows[0].Cells[0].Value = (string)Comm_Obj_Price.GetMultiData(0, 3);
            Price_GridView.Rows[0].Cells[1].Value = (string)Comm_Obj_Price.GetMultiData(0, 4);
            Price_GridView.Rows[0].Cells[2].Value = (string)Comm_Obj_Price.GetMultiData(0, 6);
            Price_GridView.Rows[0].Cells[3].Value = (string)Comm_Obj_Price.GetMultiData(0, 11);
            Price_GridView.Rows[0].Cells[4].Value = (string)Comm_Obj_Price.GetMultiData(0, 12);
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

        private void Comm_Obj_Deal_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABC100U1();
        }

        private void Proc_SABC100U1()
        {
            string aa = (string)axGiExpertControl2.GetSingleData(0); //0.주문번호
            string bb = (string)axGiExpertControl2.GetSingleData(1); //1.ORC주문번호

            MessageBox.Show((string)axGiExpertControl2.GetErrorMessage());
            MessageBox.Show((string)axGiExpertControl2.GetErrorCode());
            MessageBox.Show(aa);
            MessageBox.Show(bb);
        }
        private void getDeal(string count, string control)
        {
            axGiExpertControl2.SetQueryName("SABC100U1");
            axGiExpertControl2.SetSingleData(0, "00311155910"); // 계좌번호
            axGiExpertControl2.SetSingleData(1, "0000"); //비밀번호
            axGiExpertControl2.SetSingleData(2, "101Q3"); //종목코드
            axGiExpertControl2.SetSingleData(3, count); // 주문수량 
            axGiExpertControl2.SetSingleData(4, "0"); //주문단가 -999.99 ~ 999.99
            axGiExpertControl2.SetSingleData(5, "0"); // 주문조건 0:일반(FAS) 3:IOC(FAK) 4:FOK
            axGiExpertControl2.SetSingleData(6, control); // 매매구분 01:매도 02:매수
            axGiExpertControl2.SetSingleData(7, "M"); //호가유형 L:지정가 M:시장가 C:조건부 B:최유리
            axGiExpertControl2.SetSingleData(8, "1"); //차익거래구분 1:차익 2:헷지 3:기타
            axGiExpertControl2.SetSingleData(9, "1"); //처리구분 1:신규 2:정정 3:취소
            axGiExpertControl2.SetSingleData(10, "0"); //정정취소수량구분 0:신규 2:정정 3:취소
            axGiExpertControl2.SetSingleData(11, ""); //원주문번호 (신규매도/매수시 생략)
            axGiExpertControl2.SetSingleData(12, ""); //예약주문여부 1:예약 (예약주문 어닌경우생략)
            axGiExpertControl2.RequestData();
        }

        private void Sell_btn_Click(object sender, EventArgs e)
        {
            string count = Convert.ToString(Stock_Count.Value);
            getDeal(count, "01");
        }

        private void Buy_btn_Click(object sender, EventArgs e)
        {
            string count = Convert.ToString(Stock_Count.Value);
            getDeal(count, "02");
        }

        private void AccountComboChange(object sender, EventArgs e)
        {
            short index = Convert.ToInt16(Account_Num.SelectedIndex);
            Account_Name.Text = Convert.ToString(axGiExpertControl1.GetMultiData(index, 1));
        }

        private void AccountComboChange2(object sender, EventArgs e)
        {
            short index = Convert.ToInt16(Account_Num2.SelectedIndex);
            Account_Name2.Text = Convert.ToString(axGiExpertControl1.GetMultiData(index, 1));
        }
        private void txtInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }
    }
}
