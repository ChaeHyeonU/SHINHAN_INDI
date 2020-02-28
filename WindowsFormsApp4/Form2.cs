using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{

    public partial class Form2 : Form
    {
        private int RowNum = 200;
        private string[] gFCode = new string[6] { "101Q3", "101Q3", "101Q3", "101Q3", "101Q3", "101Q3" };
        private string[] TimeSelected = new string[6] { "3", "3", "3", "3", "3", "3" };
        private string[] TimeDistance = new string[6] { "Min", "Min", "Min", "Min", "Min", "Min" };
        private int[] control_buy_sell = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Enable_Angle = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Mecro = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int control_num = 0;
        
        DataGridView[] FCGrid_sample;
        const int ARR_COUNT = 6;

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
            FCGrid_sample = new DataGridView[ARR_COUNT];
            FCode_1.Text = gFCode[0];
            FCode_2.Text = gFCode[1];
            FCode_3.Text = gFCode[2];
            FCode_4.Text = gFCode[3];
            FCode_5.Text = gFCode[4];
            FCode_6.Text = gFCode[5];
            FCGrid_sample[0] = FCGrid_1;
            FCGrid_sample[1] = FCGrid_2;
            FCGrid_sample[2] = FCGrid_3;
            FCGrid_sample[3] = FCGrid_4;
            FCGrid_sample[4] = FCGrid_5;
            FCGrid_sample[5] = FCGrid_6;

            for (int i = 0; i < 6; i++)
            {
                TimeDistance_Changed(TimeDistance[i], i + 1);
                //Load_Data(gFCode[i], TimeSelected[i], TimeDistance[i], i + 1);
            }

            //Login();

            getAccount();

            FCode_comboBox.Text = gFCode[0];
            FCode_comboBox.Items.Add(gFCode[0]);

            setGridView();
        }

        public void Load_Data(string Fcode, string time, string distance, int control)
        {

            control_num = control;
            gFCode[control - 1] = Fcode;
            
            Comm_Obj_DATA_1.SetQueryName("TR_FCHART");
            Comm_Obj_DATA_1.SetSingleData(0, gFCode[control - 1]);

            switch (distance)
            {
                case "Day":
                    Comm_Obj_DATA_1.SetSingleData(1, "D");
                    Comm_Obj_DATA_1.SetSingleData(2, time);
                    break;
                case "Min":
                    Comm_Obj_DATA_1.SetSingleData(1, "1");
                    Comm_Obj_DATA_1.SetSingleData(2, time);
                    break;
                case "Tick":
                    Comm_Obj_DATA_1.SetSingleData(1, "T");
                    Comm_Obj_DATA_1.SetSingleData(2, time);
                    break;


            }
            Comm_Obj_DATA_1.SetSingleData(3, "00000000");
            Comm_Obj_DATA_1.SetSingleData(4, "99999999");
            Comm_Obj_DATA_1.SetSingleData(5, RowNum.ToString());
            Comm_Obj_DATA_1.RequestData();

            Delay(100);

        }

        private DataTable Proc_TR_FCHART()
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("일자  ");
            dt.Columns.Add("시간");
            dt.Columns.Add("시가");
            dt.Columns.Add("고가");
            dt.Columns.Add("저가");
            dt.Columns.Add("종가");
            dt.Columns.Add("WMA");
            dt.Columns.Add("기울기");
            dt.Columns.Add("상태");
            dt.Columns.Add("매수매도");

            short nRowSize = Convert.ToInt16(Comm_Obj_DATA_1.GetMultiRowCount());
            for (short j = 0; j < nRowSize; j++)
            {
                DataRow dr = dt.NewRow();

                for (short k = 0; k < 6; k++)
                {
                    dr[k] = (string)Comm_Obj_DATA_1.GetMultiData(j, k);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private void FCode_TextChanged(object sender, EventArgs e)
        {
            TextBox Tmp = sender as TextBox;
            string fcode = Tmp.Text;
            int control = 0;
            if (Tmp.Name == "FCode_1")
            {
                control = 1;
            }
            else if (Tmp.Name == "FCode_2")
            {
                control = 2;
            }
            else if (Tmp.Name == "FCode_3")
            {
                control = 3;
            }
            else if (Tmp.Name == "FCode_4")
            {
                control = 4;
            }
            else if (Tmp.Name == "FCode_5")
            {
                control = 5;
            }
            else if (Tmp.Name == "FCode_6")
            {
                control = 6;
            }

            if (fcode.Length == 5)
            {
                gFCode[control - 1] = fcode;

                Load_Data(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
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
            FCGrid_sample[control_num-1].DataSource = Proc_TR_FCHART();
            Comm_Obj_DATA_Real.RequestRTReg("TR_FCHART", gFCode[control_num-1]);
        }

        private void setGridView()
        {
            FCGrid_1 = FCGrid_sample[0];
            FCGrid_2 = FCGrid_sample[1];
            FCGrid_3 = FCGrid_sample[2];
            FCGrid_4 = FCGrid_sample[3];
            FCGrid_5 = FCGrid_sample[4];
            FCGrid_6 = FCGrid_sample[5];
        }

        private void WMA_input_btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "WMA_input_btn_1")
            {
                control = 1;
            }
            else if (btn.Name == "WMA_input_btn_2")
            {
                control = 2;
            }
            else if (btn.Name == "WMA_input_btn_3")
            {
                control = 3;
            }
            else if (btn.Name == "WMA_input_btn_4")
            {
                control = 4;
            }
            else if (btn.Name == "WMA_input_btn_5")
            {
                control = 5;
            }
            else if (btn.Name == "WMA_input_btn_6")
            {
                control = 6;
            }

            Get_GridData(control);
        }

        private void Get_GridData(int control)
        {
            string startWma_name = "startWma_" + (control).ToString();
            string endWma_name = "endWma_" + (control).ToString();
            string intervalWma_name = "intervalWma_" + (control).ToString();
            string Wma_name = "WMA_input_" + (control).ToString();
            string Angle_name = "Angle_input_" + (control).ToString();
            string Distance_name = "Distance_input_" + (control).ToString();

            var tmpText_start = this.Controls.Find(startWma_name, true).FirstOrDefault();
            var tmpText_end = this.Controls.Find(endWma_name, true).FirstOrDefault();
            var tmpText_interval = this.Controls.Find(intervalWma_name, true).FirstOrDefault();
            var tmpText_wma = this.Controls.Find(Wma_name, true).FirstOrDefault();
            var tmpText_angle = this.Controls.Find(Angle_name, true).FirstOrDefault();
            var tmpText_distance = this.Controls.Find(Distance_name, true).FirstOrDefault();

            if (true)
            {
                if (string.IsNullOrEmpty(tmpText_start.Text) || string.IsNullOrEmpty(tmpText_end.Text) || string.IsNullOrEmpty(tmpText_interval.Text))
                {
                    MessageBox.Show("입력값 오류");
                }
                else if (string.IsNullOrEmpty(tmpText_wma.Text))
                {
                    MessageBox.Show("WMA 간격설정 오류");
                }
                else if (string.IsNullOrEmpty(tmpText_angle.Text))
                {
                    MessageBox.Show("기울기 설정 오류");
                }
                else if (string.IsNullOrEmpty(tmpText_distance.Text))
                {
                    MessageBox.Show("기울기 간격설정 오류");
                }
                else
                {
                    int day = Convert.ToInt32(tmpText_wma.Text);
                    int[] index = getWMA_Index(Convert.ToInt32(tmpText_start.Text), Convert.ToInt32(tmpText_end.Text), Convert.ToInt32(tmpText_interval.Text));
                    int index_length = index.Length;
                    double[] aaa = new double[index_length];

                    for (int i = 0; i < RowNum; i++)
                    {
                        FCGrid_sample[control - 1].Rows[i].Cells[6].Value = Prov_WMA(Get_EndPrice(control), day, 0)[i];
                        FCGrid_sample[control - 1].Rows[i].Cells[7].Value = Get_Angle(Prov_WMA(Get_EndPrice(control), day, 0), control)[i];
                        FCGrid_sample[control - 1].Rows[i].Cells[9].Value = "";
                        FCGrid_sample[control - 1].Rows[i].Cells[9].Style.BackColor = SystemColors.Window;
                        FCGrid_sample[control - 1].Rows[i].Cells[9].Style.ForeColor = SystemColors.WindowText;
                    }

                    control_buy_sell[control - 1] = 0;
                    control_Enable_Angle[control - 1] = 0;

                    for (int j = RowNum - index[index_length - 1]; j >= 0; j--)
                    {
                        for (int i = 0; i < index_length; i++)
                        {
                            aaa[i] = Prov_WMA(Get_EndPrice(control), index[i], 0)[j];
                        }
                        Mecro(aaa, j, control);
                    }
                    for (int j = RowNum - index[index_length - 1]; j >= 0; j--)
                    {
                        for (int i = 0; i < index_length; i++)
                        {
                            aaa[i] = Prov_WMA(Get_EndPrice(control), index[i], 0)[j];
                        }
                        set_Condition(aaa, j, control);
                    }
                }
                if(MecroSet.Checked)
                    Mecro_Deal(control);
                //setGridView();
            }
            /*else
            {
                MessageBox.Show("시간이 아닙니다");
            }*/
        }

        private bool setTimeDeal() //startTime endTime
        {
            if(startTime.Text.Length !=4 || endTime.Text.Length != 4)
            {
                MessageBox.Show("시간입력 다시 4자리로 입력");
                return false;
            }
            else
            {
                int start = Convert.ToInt32(startTime.Text);
                int end = Convert.ToInt32(endTime.Text);
                int nowTime = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
                if (start == 0000 && end == 0000)
                {
                    return true;
                }
                else if (start <0900 || end > 1545)
                {
                    MessageBox.Show("장마감시간");
                    return false;
                }
                else
                {
                    int gapTime1 = start - nowTime; //음수이어야함
                    int gapTime2 = end - nowTime; //양수이어야함

                    if (gapTime1 < 0 && gapTime2 > 0)
                        return true;
                    return false;
                }
            }
        }


        private int[] Get_Angle(double[] WMA, int control)
        {
            int[] angle = new int[RowNum];

            int day = Convert.ToInt32(WMA_input_1.Text);
            int time = Convert.ToInt32(TimeSelected[control - 1]);
            int where = Convert.ToInt32(Distance_input_1.Text);

            double radians;

            for (int k = 1; k <= RowNum - 1; k++)
            {
                if (WMA[k] == 0 || WMA[k - 1] == 0)
                {
                    FCGrid_sample[control - 1].Rows[k - 1].Cells[6].Value = 0;
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

        private double[] Get_EndPrice(int control)
        {
            double[] EndPrice = new double[RowNum];
            for (int i = 0; i < RowNum; i++)
            {
                EndPrice[i] = Convert.ToDouble(FCGrid_sample[control - 1].Rows[i].Cells[5].Value);
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

        private void set_Condition(double[] WMA, int index, int control)
        {
            double[] aa = new double[WMA.Length], bb = new double[WMA.Length], cc = new double[WMA.Length];

            Array.Copy(WMA, aa,WMA.Length);
            Array.Sort(WMA);
            Array.Copy(WMA, bb, WMA.Length);

            Array.Reverse(WMA);
            Array.Copy(WMA, cc, WMA.Length);

            if(checkSameArray(aa,bb) == true)
            {
                FCGrid_sample[control - 1].Rows[index].Cells[8].Value = "역배열";
            }
            else if(checkSameArray(aa,cc) == true)
            {
                FCGrid_sample[control - 1].Rows[index].Cells[8].Value = "정배열";
            }
            else
            {
                FCGrid_sample[control - 1].Rows[index].Cells[8].Value = "혼조세";
            }
        }

        private void Mecro(double[] WMA, int index, int control)
        {
            double[] aa = new double[WMA.Length], bb = new double[WMA.Length], cc = new double[WMA.Length];
            int angle = Convert.ToInt32(Angle_input_1.Text);

            Array.Copy(WMA, aa, WMA.Length); // WMA 복사
            //bb  // WMA sort
            //cc  // WMA reverse sort
            Array.Sort(WMA);
            Array.Copy(WMA, bb, WMA.Length);

            Array.Reverse(WMA);
            Array.Copy(WMA, cc, WMA.Length);

            //control_buy_sell // 0: 일반 1:역배 2: 정배 3:역배인데 기울기 x 4: 정배인데 기울기 x
            //control_Enable_Angle // 0:아무것도 없는 상태 1:매도 2:매수
            

            if (checkSameArray(aa, bb) == true && control_buy_sell[control - 1] != 1 && control_Enable_Angle[control - 1] != 2) //역배 && 전 상태 != 역배 && !매수
            {
                if (Math.Abs(Convert.ToInt32(FCGrid_sample[control - 1].Rows[index].Cells[7].Value)) > angle)
                {
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Value = "매도";
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Style.BackColor = Color.Tomato;
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Style.ForeColor = Color.White;
                    control_buy_sell[control - 1] = 1;
                    control_Enable_Angle[control - 1] = 1;
                    //getDeal("01","01");
                }
                else
                {
                    control_buy_sell[control - 1] = 3;
                }
            }
            if (checkSameArray(aa, bb) == true && control_buy_sell[control - 1] != 1 && control_Enable_Angle[control - 1] == 2) //역배 && 전 상태 != 역배 && 매수
            {
                 FCGrid_sample[control - 1].Rows[index].Cells[9].Value = "매도(청산)";
                 FCGrid_sample[control - 1].Rows[index].Cells[9].Style.BackColor = Color.Tomato;
                 FCGrid_sample[control - 1].Rows[index].Cells[9].Style.ForeColor = Color.White;
                 control_buy_sell[control - 1] = 1;
                 control_Enable_Angle[control - 1] = 0;
                 //getDeal("01", "01");
            }
            else if (checkSameArray(aa, bb) == true && control_buy_sell[control - 1] == 1)
            {
                control_buy_sell[control - 1] = 1;
            }
            else if (checkSameArray(aa, cc) == true && control_buy_sell[control - 1] != 2 && control_Enable_Angle[control - 1] !=1)//정배 && 전 상태 != 정배 && !매도
            {
                if (Math.Abs(Convert.ToInt32(FCGrid_sample[control - 1].Rows[index].Cells[7].Value)) > angle)
                {
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Value = "매수";
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Style.BackColor = SystemColors.Highlight;
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Style.ForeColor = Color.White;
                    control_buy_sell[control - 1] = 2;
                    control_Enable_Angle[control - 1] = 2;
                    //getDeal("01", "02");
                }
                else
                {
                    control_buy_sell[control - 1] = 4;
                }
            }
            else if (checkSameArray(aa, cc) == true && control_buy_sell[control - 1] != 2 && control_Enable_Angle[control - 1] == 1)//정배 && 전 상태 !=정배 && 매도
            {
                FCGrid_sample[control - 1].Rows[index].Cells[9].Value = "매수(청산)";
                FCGrid_sample[control - 1].Rows[index].Cells[9].Style.BackColor = SystemColors.Highlight;
                FCGrid_sample[control - 1].Rows[index].Cells[9].Style.ForeColor = Color.White;
                control_buy_sell[control - 1] = 2;
                control_Enable_Angle[control - 1] = 0;
                //getDeal("01", "02");
            }
            else if (checkSameArray(aa, cc) == true && control_buy_sell[control - 1] == 2)
            {
                control_buy_sell[control - 1] = 2;
            }
            else
            {
                control_buy_sell[control - 1] = 0;                
            }
        }

        private void Mecro_Deal(int control)
        {
            //control_Mecro 0:매도 1:매수 2:매도(청산) 3:매수(청산)
            if((string)FCGrid_sample[control - 1].Rows[0].Cells[9].Value == "매도")
            {
                if(control_Mecro[control-1] != 1)
                {
                    getDeal("01", "01");
                    control_Mecro[control-1] = 1;
                }
            }
            else if ((string)FCGrid_sample[control - 1].Rows[0].Cells[9].Value == "매수")
            {
                if (control_Mecro[control-1] != 2)
                {
                    getDeal("01", "01");
                    control_Mecro[control-1] = 2;
                }
            }
            else if ((string)FCGrid_sample[control - 1].Rows[0].Cells[9].Value == "매도(청산)")
            {
                if (control_Mecro[control-1] != 3)
                {
                    getDeal("01", "01");
                    control_Mecro[control-1] = 3;
                }
            }
            else if((string)FCGrid_sample[control - 1].Rows[0].Cells[9].Value == "매수(청산)")
            {
                if (control_Mecro[control-1] != 4)
                {
                    getDeal("01", "01");
                    control_Mecro[control-1] = 4;
                }
            }
            else
            {
                control_Mecro[control-1] = 0;
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

        private void TimeDistance_Changed(string distance, int control)
        {
            
            string Day_btn_name = "Day_btn_" + (control).ToString();
            string Min_btn_name = "Min_btn_" + (control).ToString();
            string Tick_btn_name = "Tick_btn_" + (control).ToString();

            var Day_btn = this.Controls.Find(Day_btn_name, true).FirstOrDefault();
            var Min_btn = this.Controls.Find(Min_btn_name, true).FirstOrDefault();
            var Tick_btn = this.Controls.Find(Tick_btn_name, true).FirstOrDefault();

            TimeDistance[control - 1] = distance;
            string set_time = "1";
            
            Day_btn.BackColor = SystemColors.Control;
            Day_btn.ForeColor = SystemColors.ControlText;
            Min_btn.BackColor = SystemColors.Control;
            Min_btn.ForeColor = SystemColors.ControlText;
            Tick_btn.BackColor = SystemColors.Control;
            Tick_btn.ForeColor = SystemColors.ControlText;

            switch (TimeDistance[control - 1])
            {
                case "Day":
                    Day_btn.BackColor = SystemColors.Highlight;
                    Day_btn.ForeColor = SystemColors.Control;
                    set_time = "1";
                    Time_btn_Disabled(control);
                    break;
                case "Min":
                    Min_btn.BackColor = SystemColors.Highlight;
                    Min_btn.ForeColor = SystemColors.Control;
                    set_time = TimeSelected[control - 1];
                    Time_btn_Enabled(control);
                    Time_Changed(TimeSelected[control - 1], control);
                    break;
                case "Tick":
                    Tick_btn.BackColor = SystemColors.Highlight;
                    Tick_btn.ForeColor = SystemColors.Control;
                    set_time = TimeSelected[control - 1];
                    Time_btn_Enabled(control);
                    Time_Changed(TimeSelected[control - 1], control);
                    break;
            }

            Load_Data(gFCode[control - 1], set_time, TimeDistance[control - 1], control);

        }

        private void Day_btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Day_btn_1")
            {
                control = 1;
            }
            else if (btn.Name == "Day_btn_2")
            {
                control = 2;
            }
            else if (btn.Name == "Day_btn_3")
            {
                control = 3;
            }
            else if (btn.Name == "Day_btn_4")
            {
                control = 4;
            }
            else if (btn.Name == "Day_btn_5")
            {
                control = 5;
            }
            else if (btn.Name == "Day_btn_6")
            {
                control = 6;
            }
            TimeDistance_Changed("Day", control);
        }

        private void Min_btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Min_btn_1")
            {
                control = 1;
            }
            else if (btn.Name == "Min_btn_2")
            {
                control = 2;
            }
            else if (btn.Name == "Min_btn_3")
            {
                control = 3;
            }
            else if (btn.Name == "Min_btn_4")
            {
                control = 4;
            }
            else if (btn.Name == "Min_btn_5")
            {
                control = 5;
            }
            else if (btn.Name == "Min_btn_6")
            {
                control = 6;
            }
            TimeDistance_Changed("Min", control);
        }

        private void Tick_btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Tick_btn_1")
            {
                control = 1;
            }
            else if (btn.Name == "Tick_btn_2")
            {
                control = 2;
            }
            else if (btn.Name == "Tick_btn_3")
            {
                control = 3;
            }
            else if (btn.Name == "Tick_btn_4")
            {
                control = 4;
            }
            else if (btn.Name == "Tick_btn_5")
            {
                control = 5;
            }
            else if (btn.Name == "Tick_btn_6")
            {
                control = 6;
            }
            TimeDistance_Changed("Tick", control);
        }

        //시간 단위

        private void Time_Changed(string time, int control)
        {
            string Time_btn_1_name = "Time_btn_1_" + (control).ToString();
            string Time_btn_3_name = "Time_btn_3_" + (control).ToString();
            string Time_btn_5_name = "Time_btn_5_" + (control).ToString();
            string Time_btn_10_name = "Time_btn_10_" + (control).ToString();
            string Time_btn_15_name = "Time_btn_15_" + (control).ToString();
            string Time_btn_30_name = "Time_btn_30_" + (control).ToString();
            string Time_btn_45_name = "Time_btn_45_" + (control).ToString();
            string Time_btn_60_name = "Time_btn_60_" + (control).ToString();
            string Time_ComboBox_name = "Time_ComboBox_" + (control).ToString();

            var Time_btn_1 = this.Controls.Find(Time_btn_1_name, true).FirstOrDefault();
            var Time_btn_3 = this.Controls.Find(Time_btn_3_name, true).FirstOrDefault();
            var Time_btn_5 = this.Controls.Find(Time_btn_5_name, true).FirstOrDefault();
            var Time_btn_10 = this.Controls.Find(Time_btn_10_name, true).FirstOrDefault();
            var Time_btn_15 = this.Controls.Find(Time_btn_15_name, true).FirstOrDefault();
            var Time_btn_30 = this.Controls.Find(Time_btn_30_name, true).FirstOrDefault();
            var Time_btn_45 = this.Controls.Find(Time_btn_45_name, true).FirstOrDefault();
            var Time_btn_60 = this.Controls.Find(Time_btn_60_name, true).FirstOrDefault();
            var Time_ComboBox = this.Controls.Find(Time_ComboBox_name, true).FirstOrDefault();


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

            TimeSelected[control - 1] = time;
            Time_ComboBox.Text = TimeSelected[control - 1];

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

            Load_Data(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
        }

        private void Time_btn_1_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_1_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_1_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_1_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_1_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_1_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_1_6")
            {
                control = 6;
            }
            Time_Changed("1", control);
        }

        private void Time_btn_3_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_3_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_3_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_3_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_3_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_3_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_3_6")
            {
                control = 6;
            }
            Time_Changed("3", control);
        }

        private void Time_btn_5_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_5_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_5_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_5_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_5_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_5_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_5_6")
            {
                control = 6;
            }
            Time_Changed("5", control);
        }

        private void Time_btn_10_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_10_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_10_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_10_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_10_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_10_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_10_6")
            {
                control = 6;
            }
            Time_Changed("10", control);
        }

        private void Time_btn_15_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_15_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_15_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_15_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_15_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_15_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_15_6")
            {
                control = 6;
            }
            Time_Changed("15", control);
        }

        private void Time_btn_30_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_30_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_30_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_30_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_30_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_30_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_30_6")
            {
                control = 6;
            }
            Time_Changed("30", control);
        }

        private void Time_btn_45_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_45_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_45_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_45_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_45_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_45_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_45_6")
            {
                control = 6;
            }
            Time_Changed("45", control);
        }

        private void Time_btn_60_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            if (btn.Name == "Time_btn_60_1")
            {
                control = 1;
            }
            else if (btn.Name == "Time_btn_60_2")
            {
                control = 2;
            }
            else if (btn.Name == "Time_btn_60_3")
            {
                control = 3;
            }
            else if (btn.Name == "Time_btn_60_4")
            {
                control = 4;
            }
            else if (btn.Name == "Time_btn_60_5")
            {
                control = 5;
            }
            else if (btn.Name == "Time_btn_60_6")
            {
                control = 6;
            }
            Time_Changed("60", control);
        }
        private void Time_btn_Disabled(int control)
        {
            string Time_btn_1_name = "Time_btn_1_" + (control).ToString();
            string Time_btn_3_name = "Time_btn_3_" + (control).ToString();
            string Time_btn_5_name = "Time_btn_5_" + (control).ToString();
            string Time_btn_10_name = "Time_btn_10_" + (control).ToString();
            string Time_btn_15_name = "Time_btn_15_" + (control).ToString();
            string Time_btn_30_name = "Time_btn_30_" + (control).ToString();
            string Time_btn_45_name = "Time_btn_45_" + (control).ToString();
            string Time_btn_60_name = "Time_btn_60_" + (control).ToString();
            string Time_ComboBox_name = "Time_ComboBox_" + (control).ToString();

            var Time_btn_1 = this.Controls.Find(Time_btn_1_name, true).FirstOrDefault();
            var Time_btn_3 = this.Controls.Find(Time_btn_3_name, true).FirstOrDefault();
            var Time_btn_5 = this.Controls.Find(Time_btn_5_name, true).FirstOrDefault();
            var Time_btn_10 = this.Controls.Find(Time_btn_10_name, true).FirstOrDefault();
            var Time_btn_15 = this.Controls.Find(Time_btn_15_name, true).FirstOrDefault();
            var Time_btn_30 = this.Controls.Find(Time_btn_30_name, true).FirstOrDefault();
            var Time_btn_45 = this.Controls.Find(Time_btn_45_name, true).FirstOrDefault();
            var Time_btn_60 = this.Controls.Find(Time_btn_60_name, true).FirstOrDefault();
            var Time_ComboBox = this.Controls.Find(Time_ComboBox_name, true).FirstOrDefault();

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

        private void Time_btn_Enabled(int control)
        {
            string Time_btn_1_name = "Time_btn_1_" + (control).ToString();
            string Time_btn_3_name = "Time_btn_3_" + (control).ToString();
            string Time_btn_5_name = "Time_btn_5_" + (control).ToString();
            string Time_btn_10_name = "Time_btn_10_" + (control).ToString();
            string Time_btn_15_name = "Time_btn_15_" + (control).ToString();
            string Time_btn_30_name = "Time_btn_30_" + (control).ToString();
            string Time_btn_45_name = "Time_btn_45_" + (control).ToString();
            string Time_btn_60_name = "Time_btn_60_" + (control).ToString();
            string Time_ComboBox_name = "Time_ComboBox_" + (control).ToString();

            var Time_btn_1 = this.Controls.Find(Time_btn_1_name, true).FirstOrDefault();
            var Time_btn_3 = this.Controls.Find(Time_btn_3_name, true).FirstOrDefault();
            var Time_btn_5 = this.Controls.Find(Time_btn_5_name, true).FirstOrDefault();
            var Time_btn_10 = this.Controls.Find(Time_btn_10_name, true).FirstOrDefault();
            var Time_btn_15 = this.Controls.Find(Time_btn_15_name, true).FirstOrDefault();
            var Time_btn_30 = this.Controls.Find(Time_btn_30_name, true).FirstOrDefault();
            var Time_btn_45 = this.Controls.Find(Time_btn_45_name, true).FirstOrDefault();
            var Time_btn_60 = this.Controls.Find(Time_btn_60_name, true).FirstOrDefault();
            var Time_ComboBox = this.Controls.Find(Time_ComboBox_name, true).FirstOrDefault();

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
            ComboBox combo = sender as ComboBox;
            int control = 0;
            if (combo.Name == "Time_ComboBox_1")
            {
                control = 1;
            }
            else if (combo.Name == "Time_ComboBox_2")
            {
                control = 2;
            }
            else if (combo.Name == "Time_ComboBox_3")
            {
                control = 3;
            }
            else if (combo.Name == "Time_ComboBox_4")
            {
                control = 4;
            }
            else if (combo.Name == "Time_ComboBox_5")
            {
                control = 5;
            }
            else if (combo.Name == "Time_ComboBox_6")
            {
                control = 6;
            }

            if (combo.SelectedIndex >= 0)
            {
                this.TimeSelected[control - 1] = combo.SelectedItem as string;               
            }
        }

        private void Time_ComboBox_TextChanged(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            int control = 0;
            if (combo.Name == "Time_ComboBox_1")
            {
                control = 1;
            }
            else if (combo.Name == "Time_ComboBox_2")
            {
                control = 2;
            }
            else if (combo.Name == "Time_ComboBox_3")
            {
                control = 3;
            }
            else if (combo.Name == "Time_ComboBox_4")
            {
                control = 4;
            }
            else if (combo.Name == "Time_ComboBox_5")
            {
                control = 5;
            }
            else if (combo.Name == "Time_ComboBox_6")
            {
                control = 6;
            }
            TimeSelected[control - 1] = combo.Text;
            Time_Changed(TimeSelected[control - 1], control);
        }

        private void Refresh_Data(object sender, EventArgs e)
        {
            for(int i=0; i<6; i++)
            {
                Load_Data(gFCode[i], TimeSelected[i], TimeDistance[i], i+1);
            }
            Delay(100);
            for(int j=1; j<7; j++)
            {
                Get_GridData(j);
            }
            setGridView();
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



        private void getAccount() //오른쪽 아래 계좌 조회
        {
            axGiExpertControl1.SetQueryName("AccountList");
            axGiExpertControl1.RequestData();
        }

        public void AccountInfo() //오른쪽 아래 계좌정보 조회
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

        private void Proc_SABA655Q1() //오른쪽 아래 계좌 정보 조회
        {
            Account_GridView.Rows[0].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(0));
            Account_GridView.Rows[1].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(1));
            Account_GridView.Rows[2].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(5));
            Account_GridView.Rows[3].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(18));
            Account_GridView.Rows[4].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(19));
            Account_GridView.Rows[5].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(20));
        }

        private void Comm_Obj_Account_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABA655Q1(); //총자산계좌잔고조회
        }
        private void Comm_Obj_AccountList_ReceivedData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_AccountList(); //계좌 목록 조회
        }

        private void Lookup_btn_Click(object sender, EventArgs e)
        {
            AccountInfo(); //오른쪽 아래 조회
        }

        private void getPrice()
        {
            Comm_Obj_Price.SetQueryName("SABC820Q1");
            Comm_Obj_Price.SetSingleData(0, "20200228");
            Comm_Obj_Price.SetSingleData(1, Account_Num2.Text); //00311155910
            Comm_Obj_Price.SetSingleData(2, "0000");
            Comm_Obj_Price.RequestData();
        }

        private void Proc_SABC820Q1() //가지고 있는 주식 정보
        {
            Price_GridView.Rows[0].Cells[0].Value = (string)Comm_Obj_Price.GetMultiData(0, 3);
            Price_GridView.Rows[0].Cells[1].Value = (string)Comm_Obj_Price.GetMultiData(0, 4);
            Price_GridView.Rows[0].Cells[2].Value = (string)Comm_Obj_Price.GetMultiData(0, 6);
            Price_GridView.Rows[0].Cells[3].Value = (string)Comm_Obj_Price.GetMultiData(0, 11);
            Price_GridView.Rows[0].Cells[4].Value = (string)Comm_Obj_Price.GetMultiData(0, 12);
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
            AutoClosingMessageBox(aa, "", 100);
            AutoClosingMessageBox(bb, "", 100);
            //MessageBox.Show(aa);
            //MessageBox.Show(bb);
            //MessageBox.Show((string)axGiExpertControl2.GetErrorMessage());
            //MessageBox.Show((string)axGiExpertControl2.GetErrorCode());
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void TimerSet_CheckedChanged(object sender, EventArgs e)
        {
            if (TimerSet.Checked)
                timer1.Start();
            else
                timer1.Stop();
        }
    }


}
