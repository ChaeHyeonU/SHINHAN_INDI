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
        private int RowNum = 200;
        private string[] gFCode = new string[6] { "101Q3", "101Q3", "201Q3267", "101Q3", "101Q3", "101Q3" };
        private string[] TimeSelected = new string[6] { "3", "3", "3", "3", "3", "3" };
        private string[] TimeDistance = new string[6] { "Min", "Min", "Min", "Min", "Min", "Min" };

        string[] tmp_history_1 = File.ReadAllLines(@"..\..\history1.txt");
        string[] tmp_history_2 = File.ReadAllLines(@"..\..\history2.txt");
        string[] tmp_history_3 = File.ReadAllLines(@"..\..\history3.txt");
        string[] tmp_history_4 = File.ReadAllLines(@"..\..\history4.txt");
        string[] tmp_history_5 = File.ReadAllLines(@"..\..\history5.txt");
        string[] tmp_history_6 = File.ReadAllLines(@"..\..\history6.txt");

        private int[] control_buy_sell = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Enable_Angle = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Mecro = new int[6] { 0, 0, 0, 0, 0, 0 }; //1:매도 2:매수 3:매도(청산) 4:매수(청산) 현재 상태
        private int[] control_Mecro_Deal = new int[6] { 0, 0, 0, 0, 0, 0 }; //매크로 사용 여부
        private int[] control_Time_Set = new int[6] { 0, 0, 0, 0, 0, 0 }; // 시간설정 사용하는지
        private int[] control_Delay_Set = new int[6] { 0, 0, 0, 0, 0, 0 }; //딜레이를 사용하는지
        private int[] condition_Delay = new int[6] { 0, 0, 0, 0, 0, 0 }; // 딜레이 몇초 동안
        private int[] buy_sell_Count = new int[6] { 0, 0, 0, 0, 0, 0 }; // 매수 매도가 몇개인지 매수 + 매도 -

        private double[] start_price = new double[6] { 0, 0, 0, 0, 0, 0 };              //주문가격
        private bool[] TS_on = new bool[6] { false, false, false, false, false, false };
        private double[] tick = new double[6] { 0, 0, 0, 0, 0, 0 };                     //틱단위
        private double[] sell_first_price = new double[6] { 0, 0, 0, 0, 0, 0 };         //매도1호가
        private double[] buy_first_price = new double[6] { 0, 0, 0, 0, 0, 0 };          //매수1호가
        private int tick_control_num = 0;                                               //틱단위 컨트롤
        private double[] current_price = new double[6] { 0, 0, 0, 0, 0, 0 };                //현재가

        private string[] order_Num = new string[6] { "0", "0", "0", "0", "0", "0" };
        private int order_How = 0;

        private List<string> code_list;

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

        //청산 먼저 못하게
        //계좌 count textbox
        //청산 계좌 갯수

        public Form2()
        {
            InitializeComponent();
            this.code_list = new List<string>();
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
            }

            //Login();
            getAccount();
            Init_Orderlist();

            Comm_Obj_RTCount.RequestRTReg("AE", "*");
            Comm_Obj_RTPrice.RequestRTReg("FC", "101Q3");

            setGridView();


            // 선물 코드 목록 조회

            Comm_Obj_Code_List.SetQueryName("fut_mst");
            Comm_Obj_Code_List.RequestData();

            // 옵션 코드 목록 조회
            Comm_Obj_Code_List.SetQueryName("opt_mst");
            Comm_Obj_Code_List.RequestData();
            

            Get_RemainData_1();
            Get_RemainData_2();
            Get_RemainData_3();
            Get_RemainData_4();
            Get_RemainData_5();
            Get_RemainData_6();
            
        }

        public void Load_Data(string Fcode, string time, string distance, int control)
        {
            gFCode[control-1] = Fcode;

            if (gFCode[control - 1][0] == '1')
            {
                Comm_Obj_DATA_1.SetQueryName("TR_FCHART");
            }
            else if (gFCode[control - 1][0] == '2' || gFCode[control - 1][0] == '3')
            {
                Comm_Obj_DATA_1.SetQueryName("TR_OCHART");
            }
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
        public void Load_Data_2(string Fcode, string time, string distance, int control)
        {
            gFCode[control-1] = Fcode;

            if (gFCode[control - 1][0] == '1')
            {
                Comm_Obj_DATA_2.SetQueryName("TR_FCHART");
            }
            else if (gFCode[control - 1][0] == '2' || gFCode[control - 1][0] == '3')
            {
                Comm_Obj_DATA_2.SetQueryName("TR_OCHART");
            }

            Comm_Obj_DATA_2.SetSingleData(0, gFCode[control-1]);

            switch (distance)
            {
                case "Day":
                    Comm_Obj_DATA_2.SetSingleData(1, "D");
                    Comm_Obj_DATA_2.SetSingleData(2, time);
                    break;
                case "Min":
                    Comm_Obj_DATA_2.SetSingleData(1, "1");
                    Comm_Obj_DATA_2.SetSingleData(2, time);
                    break;
                case "Tick":
                    Comm_Obj_DATA_2.SetSingleData(1, "T");
                    Comm_Obj_DATA_2.SetSingleData(2, time);
                    break;


            }
            Comm_Obj_DATA_2.SetSingleData(3, "00000000");
            Comm_Obj_DATA_2.SetSingleData(4, "99999999");
            Comm_Obj_DATA_2.SetSingleData(5, RowNum.ToString());
            Comm_Obj_DATA_2.RequestData();

            Delay(100);
        }
        public void Load_Data_3(string Fcode, string time, string distance, int control)
        {
            gFCode[control-1] = Fcode;

            if (gFCode[control - 1][0] == '1')
            {
                Comm_Obj_DATA_3.SetQueryName("TR_FCHART");
            }
            else if (gFCode[control - 1][0] == '2' || gFCode[control - 1][0] == '3')
            {
                Comm_Obj_DATA_3.SetQueryName("TR_OCHART");
            }

            Comm_Obj_DATA_3.SetSingleData(0, gFCode[control-1]);

            switch (distance)
            {
                case "Day":
                    Comm_Obj_DATA_3.SetSingleData(1, "D");
                    Comm_Obj_DATA_3.SetSingleData(2, time);
                    break;
                case "Min":
                    Comm_Obj_DATA_3.SetSingleData(1, "1");
                    Comm_Obj_DATA_3.SetSingleData(2, time);
                    break;
                case "Tick":
                    Comm_Obj_DATA_3.SetSingleData(1, "T");
                    Comm_Obj_DATA_3.SetSingleData(2, time);
                    break;

            }
            Comm_Obj_DATA_3.SetSingleData(3, "00000000");
            Comm_Obj_DATA_3.SetSingleData(4, "99999999");
            Comm_Obj_DATA_3.SetSingleData(5, RowNum.ToString());
            Comm_Obj_DATA_3.RequestData();

            Delay(100);
        }
        public void Load_Data_4(string Fcode, string time, string distance, int control)
        {
            gFCode[control-1] = Fcode;

            if (gFCode[control - 1][0] == '1')
            {
                Comm_Obj_DATA_4.SetQueryName("TR_FCHART");
            }
            else if (gFCode[control - 1][0] == '2' || gFCode[control - 1][0] == '3')
            {
                Comm_Obj_DATA_4.SetQueryName("TR_OCHART");
            }
            Comm_Obj_DATA_4.SetSingleData(0, gFCode[control-1]);

            switch (distance)
            {
                case "Day":
                    Comm_Obj_DATA_4.SetSingleData(1, "D");
                    Comm_Obj_DATA_4.SetSingleData(2, time);
                    break;
                case "Min":
                    Comm_Obj_DATA_4.SetSingleData(1, "1");
                    Comm_Obj_DATA_4.SetSingleData(2, time);
                    break;
                case "Tick":
                    Comm_Obj_DATA_4.SetSingleData(1, "T");
                    Comm_Obj_DATA_4.SetSingleData(2, time);
                    break;
            }
            Comm_Obj_DATA_4.SetSingleData(3, "00000000");
            Comm_Obj_DATA_4.SetSingleData(4, "99999999");
            Comm_Obj_DATA_4.SetSingleData(5, RowNum.ToString());
            Comm_Obj_DATA_4.RequestData();

            Delay(100);
        }
        public void Load_Data_5(string Fcode, string time, string distance, int control)
        {
            gFCode[control-1] = Fcode;

            if (gFCode[control - 1][0] == '1')
            {
                Comm_Obj_DATA_5.SetQueryName("TR_FCHART");
            }
            else if (gFCode[control - 1][0] == '2' || gFCode[control - 1][0] == '3')
            {
                Comm_Obj_DATA_5.SetQueryName("TR_OCHART");
            }

            Comm_Obj_DATA_5.SetSingleData(0, gFCode[control-1]);

            switch (distance)
            {
                case "Day":
                    Comm_Obj_DATA_5.SetSingleData(1, "D");
                    Comm_Obj_DATA_5.SetSingleData(2, time);
                    break;
                case "Min":
                    Comm_Obj_DATA_5.SetSingleData(1, "1");
                    Comm_Obj_DATA_5.SetSingleData(2, time);
                    break;
                case "Tick":
                    Comm_Obj_DATA_5.SetSingleData(1, "T");
                    Comm_Obj_DATA_5.SetSingleData(2, time);
                    break;
            }
            Comm_Obj_DATA_5.SetSingleData(3, "00000000");
            Comm_Obj_DATA_5.SetSingleData(4, "99999999");
            Comm_Obj_DATA_5.SetSingleData(5, RowNum.ToString());
            Comm_Obj_DATA_5.RequestData();

            Delay(100);
        }
        public void Load_Data_6(string Fcode, string time, string distance, int control)
        {
            gFCode[control - 1] = Fcode;

            if (gFCode[control - 1][0] == '1')
            {
                Comm_Obj_DATA_6.SetQueryName("TR_FCHART");
            }
            else if (gFCode[control - 1][0] == '2' || gFCode[control - 1][0] == '3')
            {
                Comm_Obj_DATA_6.SetQueryName("TR_OCHART");
            }

            Comm_Obj_DATA_6.SetSingleData(0, gFCode[control - 1]);

            switch (distance)
            {
                case "Day":
                    Comm_Obj_DATA_6.SetSingleData(1, "D");
                    Comm_Obj_DATA_6.SetSingleData(2, time);
                    break;
                case "Min":
                    Comm_Obj_DATA_6.SetSingleData(1, "1");
                    Comm_Obj_DATA_6.SetSingleData(2, time);
                    break;
                case "Tick":
                    Comm_Obj_DATA_6.SetSingleData(1, "T");
                    Comm_Obj_DATA_6.SetSingleData(2, time);
                    break;
            }
            Comm_Obj_DATA_6.SetSingleData(3, "00000000");
            Comm_Obj_DATA_6.SetSingleData(4, "99999999");
            Comm_Obj_DATA_6.SetSingleData(5, RowNum.ToString());
            Comm_Obj_DATA_6.RequestData();

            Delay(100);
        }
        
        private void Proc_TR_msc()
        {
            // fst_msc, opt_msc
            //string[] output;
            int nRowSize = Comm_Obj_Code_List.GetMultiRowCount();
            for(int i=0;i<nRowSize;i++)
            {
                this.code_list.Add((string)Comm_Obj_Code_List.GetMultiData(short.Parse(i.ToString()), 1) + "(" + (string)Comm_Obj_Code_List.GetMultiData(short.Parse(i.ToString()), 2) + ")");
            }
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
        private DataTable Proc_TR_FCHART_2()
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

            short nRowSize = Convert.ToInt16(Comm_Obj_DATA_2.GetMultiRowCount());
            for (short j = 0; j < nRowSize; j++)
            {
                DataRow dr = dt.NewRow();

                for (short k = 0; k < 6; k++)
                {
                    dr[k] = (string)Comm_Obj_DATA_2.GetMultiData(j, k);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
        private DataTable Proc_TR_FCHART_3()
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

            short nRowSize = Convert.ToInt16(Comm_Obj_DATA_3.GetMultiRowCount());
            for (short j = 0; j < nRowSize; j++)
            {
                DataRow dr = dt.NewRow();

                for (short k = 0; k < 6; k++)
                {
                    dr[k] = (string)Comm_Obj_DATA_3.GetMultiData(j, k);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
        private DataTable Proc_TR_FCHART_4()
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

            short nRowSize = Convert.ToInt16(Comm_Obj_DATA_4.GetMultiRowCount());
            for (short j = 0; j < nRowSize; j++)
            {
                DataRow dr = dt.NewRow();

                for (short k = 0; k < 6; k++)
                {
                    dr[k] = (string)Comm_Obj_DATA_4.GetMultiData(j, k);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
        private DataTable Proc_TR_FCHART_5()
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

            short nRowSize = Convert.ToInt16(Comm_Obj_DATA_5.GetMultiRowCount());
            for (short j = 0; j < nRowSize; j++)
            {
                DataRow dr = dt.NewRow();

                for (short k = 0; k < 6; k++)
                {
                    dr[k] = (string)Comm_Obj_DATA_5.GetMultiData(j, k);
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }
        private DataTable Proc_TR_FCHART_6()
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

            short nRowSize = Convert.ToInt16(Comm_Obj_DATA_6.GetMultiRowCount());
            for (short j = 0; j < nRowSize; j++)
            {
                DataRow dr = dt.NewRow();

                for (short k = 0; k < 6; k++)
                {
                    dr[k] = (string)Comm_Obj_DATA_6.GetMultiData(j, k);
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

            if (fcode.Length == 5 || fcode.Length == 8)
            {
                gFCode[control - 1] = fcode;

                if (control == 1)
                {
                    Load_Data(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
                }
                else if (control == 2)
                {
                    Load_Data_2(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
                }
                else if (control == 3)
                {
                    Load_Data_3(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
                }
                else if (control == 4)
                {
                    Load_Data_4(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
                }
                else if (control == 5)
                {
                    Load_Data_5(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
                }
                else if (control == 6)
                {
                    Load_Data_6(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
                }
            }
        }

        private void Gi_FC_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_FC();
        }

        private void Gi_FC_ReceiveData_2(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_FC_2();
        }

        private void Gi_FC_ReceiveData_3(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_FC_3();
        }

        private void Gi_FC_ReceiveData_4(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_FC_4();
        }

        private void Gi_FC_ReceiveData_5(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_FC_5();
        }

        private void Gi_FC_ReceiveData_6(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_FC_6();
        }

        private void Proc_FC()
        {
            FCGrid_sample[0].DataSource = Proc_TR_FCHART();
        }
        private void Proc_FC_2()
        {
            FCGrid_sample[1].DataSource = Proc_TR_FCHART_2();
        }
        private void Proc_FC_3()
        {
                FCGrid_sample[2].DataSource = Proc_TR_FCHART_3();
        }
        private void Proc_FC_4()
        {
            FCGrid_sample[3].DataSource = Proc_TR_FCHART_4();
        }
        private void Proc_FC_5()
        {
            FCGrid_sample[4].DataSource = Proc_TR_FCHART_5();
        }
        private void Proc_FC_6()
        {
            FCGrid_sample[5].DataSource = Proc_TR_FCHART_6();
        }

        private void setGridView()
        {
            FCGrid_1 = FCGrid_sample[0];
            FCGrid_2 = FCGrid_sample[1];
            FCGrid_3 = FCGrid_sample[2];
            FCGrid_4 = FCGrid_sample[3];
            FCGrid_5 = FCGrid_sample[4];
            FCGrid_6 = FCGrid_sample[5];

            Get_RemainData_1();
            Get_RemainData_2();
            Get_RemainData_3();
            Get_RemainData_4();
            Get_RemainData_5();
            Get_RemainData_6();
        }

        private void WMA_input_btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int control = 0;
            string code = "";
            if (btn.Name == "WMA_input_btn_1")
            {
                control = 1;
                code = FCode_1.Text;
            }
            else if (btn.Name == "WMA_input_btn_2")
            {
                control = 2;
                code = FCode_2.Text;
            }
            else if (btn.Name == "WMA_input_btn_3")
            {
                control = 3;
                code = FCode_3.Text;
            }
            else if (btn.Name == "WMA_input_btn_4")
            {
                control = 4;
                code = FCode_4.Text;
            }
            else if (btn.Name == "WMA_input_btn_5")
            {
                control = 5;
                code = FCode_5.Text;
            }
            else if (btn.Name == "WMA_input_btn_6")
            {
                control = 6;
                code = FCode_6.Text;
            }

            if (code[0] == '1')
                Comm_Obj_RTPrice.RequestRTReg("FC", code);
            else if (code[0] == '2' || code[0] == '3')
                Comm_Obj_RTPrice.RequestRTReg("QC", code);

            Get_GridData(control);
        }
        
        private void Get_RemainData_1()
        {
            string[] tmp_get_price;

            //Remain_datagrid_1.Rows.Clear();
            if (Remain_datagrid_1.Rows.Count < tmp_history_1.Length)
            {
                Remain_datagrid_1.Rows.Add();
                Get_RemainData_1();
            }
            else
            {
                for (int i = 0; i < tmp_history_1.Length; i++)
                {
                    tmp_get_price = tmp_history_1[i].Split(new string[] { "\x020" }, StringSplitOptions.None);
                    for (int j = 0; j < 5; j++)
                    {
                        Remain_datagrid_1.Rows[i].Cells[j].Value = tmp_get_price[j];
                    }
                }
            }
        }
        
        private void Get_RemainData_2()
        {
            string[] tmp_get_price;
            //Remain_datagrid_2.Rows.Clear();
            if (Remain_datagrid_2.Rows.Count < tmp_history_2.Length)
            {
                Remain_datagrid_2.Rows.Add();
                Get_RemainData_2();
            }
            else
            {
                for (int i = 0; i < tmp_history_2.Length; i++)
                {
                    tmp_get_price = tmp_history_2[i].Split(new string[] { "\x020" }, StringSplitOptions.None);
                    for (int j = 0; j < 5; j++)
                    {
                        Remain_datagrid_2.Rows[i].Cells[j].Value = tmp_get_price[j];
                    }
                }
            }
        }
        private void Get_RemainData_3()
        {
            string[] tmp_get_price;
            //Remain_datagrid_3.Rows.Clear();
            if (Remain_datagrid_3.Rows.Count < tmp_history_3.Length)
            {
                Remain_datagrid_3.Rows.Add();
                Get_RemainData_3();
            }
            else
            {
                for (int i = 0; i < tmp_history_3.Length; i++)
                {
                    tmp_get_price = tmp_history_3[i].Split(new string[] { "\x020" }, StringSplitOptions.None);
                    for (int j = 0; j < 5; j++)
                    {
                        Remain_datagrid_3.Rows[i].Cells[j].Value = tmp_get_price[j];
                    }
                }
            }
        }
        private void Get_RemainData_4()
        {
            string[] tmp_get_price;
            //Remain_datagrid_4.Rows.Clear();
            if (Remain_datagrid_4.Rows.Count < tmp_history_4.Length)
            {
                Remain_datagrid_4.Rows.Add();
                Get_RemainData_4();
            }
            else
            {
                for (int i = 0; i < tmp_history_4.Length; i++)
                {
                    tmp_get_price = tmp_history_4[i].Split(new string[] { "\x020" }, StringSplitOptions.None);
                    for (int j = 0; j < 5; j++)
                    {
                        Remain_datagrid_4.Rows[i].Cells[j].Value = tmp_get_price[j];
                    }
                }
            }
        }
        private void Get_RemainData_5()
        {
            string[] tmp_get_price;
            //Remain_datagrid_5.Rows.Clear();
            if (Remain_datagrid_5.Rows.Count < tmp_history_5.Length)
            {
                Remain_datagrid_5.Rows.Add();
                Get_RemainData_5();
            }
            else
            {
                for (int i = 0; i < tmp_history_5.Length; i++)
                {
                    tmp_get_price = tmp_history_5[i].Split(new string[] { "\x020" }, StringSplitOptions.None);
                    for (int j = 0; j < 5; j++)
                    {
                        Remain_datagrid_5.Rows[i].Cells[j].Value = tmp_get_price[j];
                    }
                }
            }
        }
        private void Get_RemainData_6()
        {
            string[] tmp_get_price;
            //Remain_datagrid_6.Rows.Clear();
            if (Remain_datagrid_6.Rows.Count < tmp_history_6.Length)
            {
                Remain_datagrid_6.Rows.Add();
                Get_RemainData_6();
            }
            else
            {
                for (int i = 0; i < tmp_history_6.Length; i++)
                {
                    tmp_get_price = tmp_history_6[i].Split(new string[] { "\x020" }, StringSplitOptions.None);
                    for (int j = 0; j < 5; j++)
                    {
                        Remain_datagrid_6.Rows[i].Cells[j].Value = tmp_get_price[j];
                    }
                }
            }
        }    

        private void Get_GridData(int control)
        {
            string startWma_name = "startWma_" + (control).ToString();
            string endWma_name = "endWma_" + (control).ToString();
            string intervalWma_name = "intervalWma_" + (control).ToString();
            string Wma_name = "WMA_input_" + (control).ToString();
            string Angle_name = "Angle_input_" + (control).ToString();
            string Distance_name = "Distance_input_" + (control).ToString();
            string tmp_fcode = "FCode_" + (control).ToString();

            string FCode = this.Controls.Find(tmp_fcode, true).FirstOrDefault().Text;
            var tmpText_start = this.Controls.Find(startWma_name, true).FirstOrDefault();
            var tmpText_end = this.Controls.Find(endWma_name, true).FirstOrDefault();
            var tmpText_interval = this.Controls.Find(intervalWma_name, true).FirstOrDefault();
            var tmpText_wma = this.Controls.Find(Wma_name, true).FirstOrDefault();
            var tmpText_angle = this.Controls.Find(Angle_name, true).FirstOrDefault();
            var tmpText_distance = this.Controls.Find(Distance_name, true).FirstOrDefault();


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
                //control_Mecro_Deal[control - 1] = 1;

                Set_Control_Mecro(control);
                Mecro_Set_Checked(control);
                Time_Set_Checked(control);
                Delay_Set_Checked(control);

                if (control_Time_Set[control-1] == 1)
                {
                    if (setTimeDeal(control) == true)
                    {
                        Mecro_Deal(control);
                    }
                }
                else
                {
                    Mecro_Deal(control);
                }
            }
        }
        private void Mecro_Set_Checked(int control) // 0: 매크로사용 x   1: 매크로사용
        {
            string Mecro_Checked = "MecroSet_" + (control).ToString();
            CheckBox MecroSet = (CheckBox)this.Controls.Find(Mecro_Checked, true).FirstOrDefault();

            if (MecroSet.Checked)
                control_Mecro_Deal[control - 1] = 1;
            else
                control_Mecro_Deal[control - 1] = 0;
        }
        private void Time_Set_Checked(int control) // 0: 시간설정사용 x   1: 시간설정사용
        {
            string Time_Checked = "TimeSetCheck_" + (control).ToString();
            string tmp_StartTime = "startTime_" + (control).ToString();
            string tmp_EndTime = "endTime_" + (control).ToString();

            var startTime = this.Controls.Find(tmp_StartTime, true).FirstOrDefault();
            var endTime = this.Controls.Find(tmp_EndTime, true).FirstOrDefault();

            CheckBox Time_Set_Checked = (CheckBox)this.Controls.Find(Time_Checked, true).FirstOrDefault();

            if (Time_Set_Checked.Checked)
            {
                if (startTime.Text.Length != 4 || endTime.Text.Length != 4)
                {
                    MessageBox.Show("시간입력 다시 4자리로 입력");
                }
                else if (Convert.ToInt32(startTime.Text) < 0900 || Convert.ToInt32(endTime.Text) > 1545)
                {
                    MessageBox.Show("장마감시간");
                }
                else
                {
                    //MessageBox.Show("시간 설정되었습니다");
                    control_Time_Set[control - 1] = 1;
                }
            }
            else
                control_Time_Set[control - 1] = 0;
        }
        private void Delay_Set_Checked(int control) // 0: 딜레이설정사용 x   1 >: 딜레이설정사용중
        {
            string Time_Checked = "SetDelay_" + (control).ToString();
            CheckBox Time_Set_Checked = (CheckBox)this.Controls.Find(Time_Checked, true).FirstOrDefault();

            if (Time_Set_Checked.Checked)
                control_Delay_Set[control - 1] = 1;
            else
                control_Delay_Set[control - 1] = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (condition_Delay[0] > 0)
            {
                condition_Delay[0] = condition_Delay[0] - 1;
                //Visible_Delay_1.Text = Convert.ToString(condition_Delay[0]);
            }
            else
            {
                timer1.Stop();
                //MessageBox.Show("1번 타이머 종료");
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (condition_Delay[1] > 0)
            {
                condition_Delay[1] = condition_Delay[1] - 1;
                //Visible_Delay_2.Text = Convert.ToString(condition_Delay[1]);
            }
                
            else
            {
                timer2.Stop();
                //MessageBox.Show("2번 타이머 종료");
            }
                
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (condition_Delay[2] > 0)
            {
                condition_Delay[2] = condition_Delay[2] - 1;
                //Visible_Delay_3.Text = Convert.ToString(condition_Delay[2]);
            }
                
            else
            {
                //MessageBox.Show("3번 타이머 종료");
                timer3.Stop();
            }
        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            if (condition_Delay[3] > 0)
            {
                condition_Delay[3] = condition_Delay[3] - 1;
                //Visible_Delay_4.Text = Convert.ToString(condition_Delay[3]);
            }
                
            else
            {
                //MessageBox.Show("4번 타이머 종료");
                timer4.Stop();
            }
                
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            if (condition_Delay[4] > 0)
            {
                condition_Delay[4] = condition_Delay[4] - 1;
                //Visible_Delay_5.Text = Convert.ToString(condition_Delay[4]);
            }
            else
            {
                //MessageBox.Show("5번 타이머 종료");
                timer5.Stop();
            }
               
        }

        private void timer6_Tick(object sender, EventArgs e)
        {
            if (condition_Delay[5] > 0)
            {
                condition_Delay[5] = condition_Delay[5] - 1;
                //Visible_Delay_6.Text = Convert.ToString(condition_Delay[5]);
            }
                
            else
            {
                //MessageBox.Show("6번 타이머 종료");
                timer6.Stop();
            }
        }


        private void Set_Control_Mecro(int control)
        {
            //control_Mecro 1:매도 2:매수 3:매도(청산) 4:매수(청산)

            string tmp_fcode = "FCode_" + (control).ToString();
            string  FCode= this.Controls.Find(tmp_fcode, true).FirstOrDefault().Text;
            buy_sell_Count[control - 1] = 0;

            for (int i = 0; i < Price_GridView.Rows.Count; i++)
            {
                if (FCode.Equals((string)Price_GridView.Rows[i].Cells[0].Value))
                {
                    if (Convert.ToString(Price_GridView.Rows[i].Cells[1]) == "02" || Convert.ToString(Price_GridView.Rows[i].Cells[1]) == "2")
                    {
                        control_Mecro[control - 1] = 2; //매수
                        buy_sell_Count[control - 1] = Convert.ToInt32(Price_GridView.Rows[i].Cells[2]);
                    }
                    else if (Convert.ToString(Price_GridView.Rows[i].Cells[1]) == "01" || Convert.ToString(Price_GridView.Rows[i].Cells[1]) == "1")
                    {
                        control_Mecro[control - 1] = 1; //매도
                        buy_sell_Count[control - 1] = Convert.ToInt32(Price_GridView.Rows[i].Cells[2]);
                    }
                    else if(Convert.ToString(Price_GridView.Rows[i].Cells[1]) == "0")
                    {
                        control_Mecro[control - 1] = 3; //청산
                        buy_sell_Count[control - 1] = 0;
                    }
                    else if (Price_GridView.Rows[i].Cells[1] == null)
                    {
                        control_Mecro[control - 1] = 0; //일반
                        buy_sell_Count[control - 1] = 0;
                    }
                }
            }
        }


        private void Mecro_Deal(int control)
        {
            //control_Mecro 1:매도 2:매수 3:청산

            string Acc_num = Account_Num_1.Text;
            string Acc_pw = Acc_PW_1.Text;
            string code = gFCode[control - 1];
            string cont;              // 01: 매도   02: 매수
            string price = "0";
            string type = "M";        //호가유형 L:지정가 M:시장가 C:조건부 B:최유리
            string tmp_Count = "CountText_" + (control).ToString();
            string count = (this.Controls.Find(tmp_Count, true).FirstOrDefault()).Text;

            if (control_Mecro_Deal[control - 1] == 1) //매크로 사용 여부
            {
                if (control_Delay_Set[control - 1] == 1) //딜레이 사용 여부 // 딜레이 사용
                {
                    if (condition_Delay[control - 1] == 0) //현재 딜레이 상태 여부 
                    {
                        string tmp_Delay = "SetDelayText_" + (control).ToString();
                        int Delay = Convert.ToInt32(this.Controls.Find(tmp_Delay, true).FirstOrDefault().Text);

                        if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매도")
                        {
                            if (control_Mecro[control - 1] != 1)
                            {
                                cont = "01";
                                getDeal(Acc_num, Acc_pw, code, count, price, cont, type, "1", "","0");
                                //control_Mecro[control - 1] = 1;
                                //buy_sell_Count[control - 1] -= Convert.ToInt32(count);

                                Get_RealTimeData(Acc_num, Acc_pw);

                                condition_Delay[control - 1] = Delay;

                                if (control == 1)
                                {
                                    timer1.Tick += new EventHandler(timer1_Tick);
                                    timer1.Start();
                                    //MessageBox.Show("1번 타이머 시작");
                                }
                                else if (control == 2)
                                {
                                    timer2.Tick += new EventHandler(timer2_Tick);
                                    timer2.Start();
                                    //MessageBox.Show("2번 타이머 시작");
                                }
                                else if (control == 3)
                                {
                                    timer3.Tick += new EventHandler(timer3_Tick);
                                    timer3.Start();
                                    //MessageBox.Show("3번 타이머 시작");
                                }
                                else if (control == 4)
                                {
                                    timer4.Tick += new EventHandler(timer4_Tick);
                                    timer4.Start();
                                    //MessageBox.Show("4번 타이머 시작");
                                }
                                else if (control == 5)
                                {
                                    timer5.Tick += new EventHandler(timer5_Tick);
                                    timer5.Start();
                                    //MessageBox.Show("5번 타이머 시작");
                                }
                                else if (control == 6)
                                {
                                    timer6.Tick += new EventHandler(timer6_Tick);
                                    timer6.Start();
                                    //MessageBox.Show("6번 타이머 시작");
                                }
                            }
                        }
                        else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매수")
                        {
                            if (control_Mecro[control - 1] != 2)
                            {
                                cont = "02";
                                getDeal(Acc_num, Acc_pw, code, count, price, cont, type, "1", "", "0");
                                //control_Mecro[control - 1] = 2;
                                //buy_sell_Count[control - 1] += Convert.ToInt32(count);
                                
                                Get_RealTimeData(Acc_num, Acc_pw);

                                condition_Delay[control - 1] = Delay;

                                if (control == 1)
                                {
                                    timer1.Tick += new EventHandler(timer1_Tick);
                                    timer1.Start();
                                    //MessageBox.Show("1번 타이머 시작");
                                }
                                else if (control == 2)
                                {
                                    timer2.Tick += new EventHandler(timer2_Tick);
                                    timer2.Start();
                                    //MessageBox.Show("2번 타이머 시작");
                                }
                                else if (control == 3)
                                {
                                    timer3.Tick += new EventHandler(timer3_Tick);
                                    timer3.Start();
                                    //MessageBox.Show("3번 타이머 시작");
                                }
                                else if (control == 4)
                                {
                                    timer4.Tick += new EventHandler(timer4_Tick);
                                    timer4.Start();
                                    //MessageBox.Show("4번 타이머 시작");
                                }
                                else if (control == 5)
                                {
                                    timer5.Tick += new EventHandler(timer5_Tick);
                                    timer5.Start();
                                    //MessageBox.Show("5번 타이머 시작");
                                }
                                else if (control == 6)
                                {
                                    timer6.Tick += new EventHandler(timer6_Tick);
                                    timer6.Start();
                                    //MessageBox.Show("6번 타이머 시작");
                                }
                            }
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매도(청산)")
                    {
                        if (control_Mecro[control - 1] != 3 && buy_sell_Count[control - 1] > 0)
                        {
                            cont = "01";
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type, "1", "", "0");
                            //buy_sell_Count[control - 1] = 0;
                            //control_Mecro[control - 1] = 3;

                            Get_RealTimeData(Acc_num, Acc_pw);
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매수(청산)")
                    {
                        if (control_Mecro[control - 1] != 3 && buy_sell_Count[control - 1] < 0)
                        {
                            cont = "02";
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type, "1", "", "0");
                            //buy_sell_Count[control - 1] = 0;
                            //control_Mecro[control - 1] = 3;

                            Get_RealTimeData(Acc_num, Acc_pw);
                        }
                    }
                    else
                    {
                        //control_Mecro[control - 1] = 0;
                    }

                }
                else //딜레이 사용 x
                {
                    if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매도")
                    {
                        if (control_Mecro[control - 1] != 1)
                        {
                            cont = "01";
                            getDeal(Acc_num, Acc_pw, code, count, price, cont, type,"1","","0");
                            //buy_sell_Count[control - 1] -= Convert.ToInt32(count);
                            //control_Mecro[control - 1] = 1;

                            Get_RealTimeData(Acc_num, Acc_pw);
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매수")
                    {
                        if (control_Mecro[control - 1] != 2)
                        {
                            cont = "02";
                            getDeal(Acc_num, Acc_pw, code, count, price, cont, type, "1", "", "0");
                            //buy_sell_Count[control - 1] += Convert.ToInt32(count);
                            //control_Mecro[control - 1] = 2;

                            Get_RealTimeData(Acc_num, Acc_pw);
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매도(청산)")
                    {
                        if (control_Mecro[control - 1] != 3 && buy_sell_Count[control - 1] > 0)
                        {
                            cont = "01";
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type, "1", "", "0");
                            //buy_sell_Count[control - 1] = 0;
                            //control_Mecro[control - 1] = 3;

                            Get_RealTimeData(Acc_num, Acc_pw);
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매수(청산)")
                    {
                        if (control_Mecro[control - 1] != 3 && buy_sell_Count[control - 1] < 0)
                        {
                            cont = "02";
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type, "1", "", "0");
                            //buy_sell_Count[control - 1] = 0;
                            //control_Mecro[control - 1] = 3;

                            Get_RealTimeData(Acc_num, Acc_pw);
                        }
                    }
                    else
                    {
                        //control_Mecro[control - 1] = 0;
                    }
                }
            }
        }

        private bool setTimeDeal(int control) //startTime endTime
        {
            string tmp_Start = "startTime_" + (control).ToString();
            string tmp_End = "endTime_" + (control).ToString();
            int start = Convert.ToInt32((this.Controls.Find(tmp_Start, true).FirstOrDefault()).Text);
            int end = Convert.ToInt32((this.Controls.Find(tmp_End, true).FirstOrDefault()).Text);

            int nowTime = Convert.ToInt32(DateTime.Now.ToString("HHmm"));
            int gapTime1 = start - nowTime; //음수이어야함
            int gapTime2 = end - nowTime; //양수이어야함

            if (start == 0000 && end == 0000)
            {
                return true;
            }
            else if (gapTime1 < 0 && gapTime2 > 0)
                return true;
            return false;
        }


        private int[] Get_Angle(double[] WMA, int control)
        {
            int[] angle = new int[RowNum];

            string tmp_Day = "WMA_input_" + (control).ToString();
            string tmp_Where = "Distance_input_" + (control).ToString();

            int day = Convert.ToInt32((this.Controls.Find(tmp_Day, true).FirstOrDefault()).Text);
            int where = Convert.ToInt32((this.Controls.Find(tmp_Where, true).FirstOrDefault()).Text);
            int time = Convert.ToInt32(TimeSelected[control - 1]);

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

            Array.Copy(WMA, aa, WMA.Length);
            Array.Sort(WMA);
            Array.Copy(WMA, bb, WMA.Length);

            Array.Reverse(WMA);
            Array.Copy(WMA, cc, WMA.Length);

            if (checkSameArray(aa, bb) == true)
            {
                FCGrid_sample[control - 1].Rows[index].Cells[8].Value = "역배열";
            }
            else if (checkSameArray(aa, cc) == true)
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
            string tmpAngle = "Angle_input_" + (control).ToString();
            int angle = Convert.ToInt32((this.Controls.Find(tmpAngle, true).FirstOrDefault()).Text);

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
            }
            else if (checkSameArray(aa, bb) == true && control_buy_sell[control - 1] == 1)
            {
                control_buy_sell[control - 1] = 1;
            }
            else if (checkSameArray(aa, cc) == true && control_buy_sell[control - 1] != 2 && control_Enable_Angle[control - 1] != 1)//정배 && 전 상태 != 정배 && !매도
            {
                if (Math.Abs(Convert.ToInt32(FCGrid_sample[control - 1].Rows[index].Cells[7].Value)) > angle)
                {
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Value = "매수";
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Style.BackColor = SystemColors.Highlight;
                    FCGrid_sample[control - 1].Rows[index].Cells[9].Style.ForeColor = Color.White;
                    control_buy_sell[control - 1] = 2;
                    control_Enable_Angle[control - 1] = 2;
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

            if (control == 1)
            {
                Load_Data(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 2)
            {
                Load_Data_2(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 3)
            {
                Load_Data_3(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 4)
            {
                Load_Data_4(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 5)
            {
                Load_Data_5(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 6)
            {
                Load_Data_6(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }

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

            if (control == 1)
            {
                Load_Data(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 2)
            {
                Load_Data_2(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 3)
            {
                Load_Data_3(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 4)
            {
                Load_Data_4(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 5)
            {
                Load_Data_5(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
            else if (control == 6)
            {
                Load_Data_6(gFCode[control - 1], TimeSelected[control - 1], TimeDistance[control - 1], control);
            }
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
            //getPrice();
            Load_Data(gFCode[0], TimeSelected[0], TimeDistance[0],1);
            Load_Data_2(gFCode[1], TimeSelected[1], TimeDistance[1],2);
            Load_Data_3(gFCode[2], TimeSelected[2], TimeDistance[2],3);
            Load_Data_4(gFCode[3], TimeSelected[3], TimeDistance[3],4);
            Load_Data_5(gFCode[4], TimeSelected[4], TimeDistance[4],5);
            Load_Data_6(gFCode[5], TimeSelected[5], TimeDistance[5],6);

            Delay(100);

            for (int j = 1; j < 7; j++)
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

        private void getAccount() //계좌 조회
        {
            Comm_Obj_Accountinfo.SetQueryName("AccountList");
            Comm_Obj_Accountinfo.RequestData();
        }

        private void Comm_Obj_AccountList_ReceivedData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_AccountList(); //계좌 목록 조회
        }

        private void Proc_AccountList()
        {
            short nRowSize = Comm_Obj_Accountinfo.GetMultiRowCount();
            for (short i = 0; i < nRowSize; i++)
            {
                Account_Num_1.Items.Add((string)Comm_Obj_Accountinfo.GetMultiData(i, 0));
            }

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

        private void Lookup_btn_Click(object sender, EventArgs e)
        {
            AccountInfo(); //오른쪽 아래 조회
        }

        public void AccountInfo() //오른쪽 아래 계좌정보 조회
        {
            if (Acc_PW_1.Text != "0000")
            {
                MessageBox.Show("비밀번호 확인");
            }
            else
            {
                Comm_Obj_Account.SetQueryName("SABA655Q1");
                Comm_Obj_Account.SetSingleData(0, Account_Num_1.Text); //00311155910
                Comm_Obj_Account.SetSingleData(1, "01");
                Comm_Obj_Account.SetSingleData(2, Acc_PW_1.Text);
                Comm_Obj_Account.RequestData();
            }
        }
        private void Comm_Obj_Account_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABA655Q1(); //총자산계좌잔고조회
        }

        private void Proc_SABA655Q1() //오른쪽 아래 계좌 정보
        {
            Account_GridView.Rows[0].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(0));
            Account_GridView.Rows[1].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(1));
            Account_GridView.Rows[2].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(5));
            Account_GridView.Rows[3].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(18));
            Account_GridView.Rows[4].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(19));
            Account_GridView.Rows[5].Cells[0].Value = Convert.ToInt64(Comm_Obj_Account.GetSingleData(20));
        }    

        private void Price_Lookup_btn_Click(object sender, EventArgs e)
        {
            getPrice();
        }
        
        private void getPrice()
        {
            string nowdate = DateTime.Now.ToString("yyyyMMdd");
            Comm_Obj_Price.SetQueryName("SABC820Q1");
            Comm_Obj_Price.SetSingleData(0, nowdate);
            Comm_Obj_Price.SetSingleData(1, Account_Num_1.Text); //00311155910
            Comm_Obj_Price.SetSingleData(2, Acc_PW_1.Text);
            Comm_Obj_Price.RequestData();
        }
        
        private void Comm_Obj_Price_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABC820Q1();
        }

        private void Proc_SABC820Q1() //가지고 있는 주식 정보
        {
            int nRowSize = Comm_Obj_Price.GetMultiRowCount();
            DataTable dt = new DataTable();
            dt.Columns.Add("종목코드");
            dt.Columns.Add("매수매도구분");  //1 매도 2 매수
            dt.Columns.Add("잔고");
            dt.Columns.Add("평균가(단)");
            dt.Columns.Add("평가손익");
            dt.Columns.Add("매매손익");
            dt.Columns.Add("평가금액");

            for (int i = 0; i < nRowSize; i++)
            {
                DataRow dr = dt.NewRow();
                dr[0] = (string)Comm_Obj_Price.GetMultiData(Convert.ToInt16(i), 0);
                dr[1] = (string)Comm_Obj_Price.GetMultiData(Convert.ToInt16(i), 2);
                dr[2] = (string)Comm_Obj_Price.GetMultiData(Convert.ToInt16(i), 3);
                dr[3] = (string)Comm_Obj_Price.GetMultiData(Convert.ToInt16(i), 4);
                dr[4] = (string)Comm_Obj_Price.GetMultiData(Convert.ToInt16(i), 6);
                dr[5] = (string)Comm_Obj_Price.GetMultiData(Convert.ToInt16(i), 11);
                dr[6] = (string)Comm_Obj_Price.GetMultiData(Convert.ToInt16(i), 12);
                dt.Rows.Add(dr);
            }
            Price_GridView.DataSource = dt;
        }
        

        private void Get_RealTimeData(string Acc_num, string Acc_pw)
        {
            string nowdate = DateTime.Now.ToString("yyyyMMdd");
            Comm_Obj_RealTimeData.SetQueryName("SABC258Q1");
            Comm_Obj_RealTimeData.SetSingleData(0, Acc_num);
            Comm_Obj_RealTimeData.SetSingleData(1, Acc_pw);
            Comm_Obj_RealTimeData.SetSingleData(2, "0"); // 상품구분 0:전체 1:선물 2:옵션(지수옵션+주식옵션) 3:주식옵션만
            Comm_Obj_RealTimeData.SetSingleData(3, "000"); //시장ID코드 생략 또는 000
            Comm_Obj_RealTimeData.SetSingleData(4, nowdate); //매매일자
            Comm_Obj_RealTimeData.SetSingleData(5, "1"); //조회구분 0:전체 1:체결 2:미체결
            Comm_Obj_RealTimeData.SetSingleData(6, "1"); //합산구분 0:합산 1:건별
            Comm_Obj_RealTimeData.SetSingleData(7, "0"); //Sort구분 0:주문번호순 1:주문번호 역순
            Comm_Obj_RealTimeData.SetSingleData(8, "0"); //종목별합산구분 0:일반 조회 1:종목별합산조회
            Comm_Obj_RealTimeData.RequestData();
        }

        private void Comm_Obj_RealTime_Lookup(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABC258Q1();
        }

        private void Proc_SABC258Q1()
        {
            // 체결시간 매매구분 종목코드 체결단가 체결수량
            int nRowSize = Comm_Obj_RealTimeData.GetMultiRowCount();
            string temp ="";
            string[][] value = new string[nRowSize][];
            for(short i=0; i<nRowSize; i++)
            {
                for(short j=0; j<=27; j++)
                {
                    value[i][j] = (string)Comm_Obj_RealTimeData.GetMultiData(i, j);
                }
            }

            for(int k=0; k<nRowSize; k++)
            {
                if(value[k][1] == FCode_1.Text)
                {
                    temp = value[k][18] + " " + value[k][3] + " " + value[k][1] + " " + value[k][8] + " " + value[k][7];
                    for(int a=0; a<tmp_history_1.Length; a++)
                    {
                        if(temp.Equals(tmp_history_1[a]) == false)
                        {
                            if (control_Mecro[0] == 1 || control_Mecro[0] == 2)
                            {
                                Array.Resize(ref tmp_history_1, tmp_history_1.Length + 1);
                                tmp_history_1[tmp_history_1.Length - 1] = temp;
                            }
                            else if (control_Mecro[0] == 3)
                            {
                                Array.Resize(ref tmp_history_1, 1);
                                tmp_history_1[0] = temp;
                            }
                        }
                    }
                }
                else if (value[k][1] == FCode_2.Text)
                {
                    temp = value[k][18] + " " + value[k][3] + " " + value[k][1] + " " + value[k][8] + " " + value[k][7];
                    for (int a = 0; a < tmp_history_2.Length; a++)
                    {
                        if (temp.Equals(tmp_history_2[a]) == false)
                        {
                            if (control_Mecro[1] == 1 || control_Mecro[1] == 2)
                            {
                                Array.Resize(ref tmp_history_2, tmp_history_2.Length + 1);
                                tmp_history_2[tmp_history_2.Length - 1] = temp;
                            }
                            else if (control_Mecro[1] == 3)
                            {
                                Array.Resize(ref tmp_history_2, 1);
                                tmp_history_2[0] = temp;
                            }
                        }
                    }
                }
                else if (value[k][1] == FCode_3.Text)
                {
                    temp = value[k][18] + " " + value[k][3] + " " + value[k][1] + " " + value[k][8] + " " + value[k][7];
                    for (int a = 0; a < tmp_history_3.Length; a++)
                    {
                        if (temp.Equals(tmp_history_3[a]) == false)
                        {
                            if (control_Mecro[2] == 1 || control_Mecro[2] == 2)
                            {
                                Array.Resize(ref tmp_history_3, tmp_history_3.Length + 1);
                                tmp_history_3[tmp_history_3.Length - 1] = temp;
                            }
                            else if (control_Mecro[2] == 3)
                            {
                                Array.Resize(ref tmp_history_3, 1);
                                tmp_history_3[0] = temp;
                            }
                        }
                    }
                }
                else if (value[k][1] == FCode_4.Text)
                {
                    temp = value[k][18] + " " + value[k][3] + " " + value[k][1] + " " + value[k][8] + " " + value[k][7];
                    for (int a = 0; a < tmp_history_4.Length; a++)
                    {
                        if (temp.Equals(tmp_history_4[a]) == false)
                        {
                            if (control_Mecro[3] == 1 || control_Mecro[3] == 2)
                            {
                                Array.Resize(ref tmp_history_4, tmp_history_4.Length + 1);
                                tmp_history_4[tmp_history_4.Length - 1] = temp;
                            }
                            else if (control_Mecro[3] == 3)
                            {
                                Array.Resize(ref tmp_history_4, 1);
                                tmp_history_4[0] = temp;
                            }
                        }
                    }
                }
                else if (value[k][1] == FCode_5.Text)
                {
                    temp = value[k][18] + " " + value[k][3] + " " + value[k][1] + " " + value[k][8] + " " + value[k][7];
                    for (int a = 0; a < tmp_history_5.Length; a++)
                    {
                        if (temp.Equals(tmp_history_5[a]) == false)
                        {
                            if (control_Mecro[4] == 1 || control_Mecro[4] == 2)
                            {
                                Array.Resize(ref tmp_history_5, tmp_history_5.Length + 1);
                                tmp_history_5[tmp_history_5.Length - 1] = temp;
                            }
                            else if (control_Mecro[4] == 3)
                            {
                                Array.Resize(ref tmp_history_5, 1);
                                tmp_history_5[0] = temp;
                            }
                        }
                    }
                }
                else if (value[k][1] == FCode_6.Text)
                {
                    temp = value[k][18] + " " + value[k][3] + " " + value[k][1] + " " + value[k][8] + " " + value[k][7];
                    for (int a = 0; a < tmp_history_6.Length; a++)
                    {
                        if (temp.Equals(tmp_history_6[a]) == false)
                        {
                            if (control_Mecro[5] == 1 || control_Mecro[5] == 2)
                            {
                                Array.Resize(ref tmp_history_6, tmp_history_6.Length + 1);
                                tmp_history_6[tmp_history_6.Length - 1] = temp;
                            }
                            else if (control_Mecro[5] == 3)
                            {
                                Array.Resize(ref tmp_history_6, 1);
                                tmp_history_6[0] = temp;
                            }
                        }
                    }
                }
            }
        }

        private void getDeal(string Acc_num, string Acc_pw, string code, string count, string price, string buyorsell, string type, string newordelete, string ordernumber, string changeCount)
        {
            Comm_Obj_Deal.SetQueryName("SABC100U1");
            Comm_Obj_Deal.SetSingleData(0, Acc_num); // 계좌번호
            Comm_Obj_Deal.SetSingleData(1, Acc_pw); //비밀번호
            Comm_Obj_Deal.SetSingleData(2, code); //종목코드
            Comm_Obj_Deal.SetSingleData(3, count); // 주문수량 
            Comm_Obj_Deal.SetSingleData(4, price); //주문단가 -999.99 ~ 999.99
            Comm_Obj_Deal.SetSingleData(5, "0"); // 주문조건 0:일반(FAS) 3:IOC(FAK) 4:FOK
            Comm_Obj_Deal.SetSingleData(6, buyorsell); // 매매구분 01:매도 02:매수
            Comm_Obj_Deal.SetSingleData(7, type); //호가유형 L:지정가 M:시장가 C:조건부 B:최유리
            Comm_Obj_Deal.SetSingleData(8, "1"); //차익거래구분 1:차익 2:헷지 3:기타
            Comm_Obj_Deal.SetSingleData(9, newordelete); //처리구분 1:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(10, changeCount); //정정취소수량구분 0:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(11, ordernumber); //원주문번호 (신규매도/매수시 생략)
            Comm_Obj_Deal.SetSingleData(12, ""); //예약주문여부 1:예약 (예약주문 어닌경우생략)
            Comm_Obj_Deal.RequestData();
        }

        private void Comm_Obj_Deal_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABC100U1();
        }

        private void Proc_SABC100U1()
        {
            string aa = (string)Comm_Obj_Deal.GetSingleData(0); //0.주문번호
            //string bb = (string)Comm_Obj_Deal.GetSingleData(1); //1.ORC주문번호
            if (order_How != 0)
            {
                order_Num[order_How - 1] = aa;
            }
            MessageBox.Show(aa);
        }

        private void Init_Orderlist()  //주문내역 날짜 초기화
        {
            this.Start_date_picker.CustomFormat = "yyyyMMdd";
            this.End_date_picker.CustomFormat = "yyyyMMdd";

            string start_date = DateTime.Now.AddDays(-7).ToString("yyyyMMdd");
            string end_date = DateTime.Now.ToString("yyyyMMdd");

            this.Start_date_picker.Value = DateTime.ParseExact(start_date, "yyyyMMdd", null);
            this.End_date_picker.Value = DateTime.ParseExact(end_date, "yyyyMMdd", null);
        }

        private void Orderlist_date_Changed(object sender, EventArgs e)
        {
            get_Orderlist(Account_Num_1.Text, Acc_PW_1.Text);
        }

        private void get_Orderlist(string Acc_num, string Acc_pw)  //주문내역 가져오기
        {
            Comm_Obj_Orderlist.SetQueryName("SABC203Q2");
            Comm_Obj_Orderlist.SetSingleData(0, Acc_num); // 계좌번호
            Comm_Obj_Orderlist.SetSingleData(1, Acc_pw); //비밀번호
            Comm_Obj_Orderlist.SetSingleData(2, "%"); //매매구분 %:전체
            Comm_Obj_Orderlist.SetSingleData(3, "%"); //종목코드 %:전체
            Comm_Obj_Orderlist.SetSingleData(4, "000"); //시장ID코드
            Comm_Obj_Orderlist.SetSingleData(5, "%"); // 옵션구분코드
            Comm_Obj_Orderlist.SetSingleData(6, this.Start_date_picker.Text); //조회시작일
            Comm_Obj_Orderlist.SetSingleData(7, this.End_date_picker.Text); //조회종료일
            Comm_Obj_Orderlist.RequestData();
        }

        private void Comm_obj_Orderlist_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_SABC203Q2();
        }

        private void Proc_SABC203Q2()
        {
            int nRowSize = Comm_Obj_Orderlist.GetMultiRowCount();

            DataTable dt = new DataTable();
            dt.Columns.Add("주문일자");
            dt.Columns.Add("종목코드");
            dt.Columns.Add("매매구분");
            dt.Columns.Add("체결수량");
            dt.Columns.Add("체결단가");
            dt.Columns.Add("체결금액");
            dt.Columns.Add("손익금액");
            dt.Columns.Add("수수료");

            for (int i = 0; i < nRowSize; i++)
            {
                DataRow dr = dt.NewRow();
                dr[0] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 0);
                dr[1] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 1);
                dr[2] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 2);
                dr[3] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 3);
                dr[4] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 4);
                dr[5] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 5);
                dr[6] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 6);
                dr[7] = (string)Comm_Obj_Orderlist.GetMultiData(Convert.ToInt16(i), 7);
                dt.Rows.Add(dr);
            }
            Order_list.DataSource = dt;
        }

        private void Comm_obj_Code_List_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_TR_msc();
        }

        private void AccountComboChange2(object sender, EventArgs e)
        {
            short index = Convert.ToInt16(Account_Num_1.SelectedIndex);
            Account_Name_1.Text = Convert.ToString(Comm_Obj_Accountinfo.GetMultiData(index, 1));
        }

        private void txtInterval_KeyPress(object sender, KeyPressEventArgs e)
        {
            //숫자만 입력되도록 필터링
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))    //숫자와 백스페이스를 제외한 나머지를 바로 처리
            {
                e.Handled = true;
            }
        }

        private void FCode_1_Click(object sender, EventArgs e)
        {
            TextBox Tmp = sender as TextBox;
            select_code_form sc_form = new select_code_form(Tmp, this.code_list);
            sc_form.Show();
        }

        private void Stop_Loss(double price, int k)
        {
            
                string SL_Control_Name = "TS_Control_" + (k).ToString();
                CheckBox SL_Control = (CheckBox)this.Controls.Find(SL_Control_Name, true).FirstOrDefault();
                if(SL_Control.Checked == true)
                {
                    string tmp_Profit = "SL_HighTick_" + (k).ToString();
                    string tmp_Loss = "SL_LowTick_" + (k).ToString();
                    string tmp_Combo = "SL_OrderHow_" + (k).ToString();
                    string tmp_fcode = "FCode_" + (k).ToString();

                    int Profit = Convert.ToInt32((this.Controls.Find(tmp_Profit, true).FirstOrDefault()).Text);
                    int Loss = Convert.ToInt32(this.Controls.Find(tmp_Loss, true).FirstOrDefault().Text);
                    ComboBox Combo = (ComboBox)this.Controls.Find(tmp_Combo, true).FirstOrDefault();
                    string code = this.Controls.Find(tmp_fcode, true).FirstOrDefault().Text;
                    int index = Combo.SelectedIndex;

                    get_Tick(k);

                    string cont = "";
                    string count = "";

                    for (int i = 0; i < Price_GridView.Rows.Count; i++)
                    {
                        if (code.Equals((string)Price_GridView.Rows[i].Cells[0].Value))
                        {
                            start_price[k - 1] = Convert.ToDouble(Price_GridView.Rows[i].Cells[4].Value); //평균가
                            cont = Convert.ToString(Price_GridView.Rows[i].Cells[2].Value); //매수 매도 구분
                            count = Convert.ToString(Price_GridView.Rows[i].Cells[3].Value);
                        }
                        else
                        {
                            start_price[k - 1] = 0;
                        }
                    }

                    if (price > start_price[k - 1] + (Profit * tick[k - 1]) && start_price[k - 1] != 0)
                    {
                        //익절
                        if (cont == "2") //매수한 계약 일떄
                        {
                            if (index == 0) //시장가
                            {
                                getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, "0", "01", "M", "1", "", "0");   //01 : 매도 02: 매수
                                Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                            }
                            else if (index >= 1 || index <= 10) //상대 호가
                            {
                                if (order_Num[k - 1].Equals("0"))  //처음 주문일때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(sell_first_price[k - 1] + (index * tick[k - 1])), "01", "L", "1", "", "0");   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }
                                else //이전에 주문을 넣었을때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(sell_first_price[k - 1] + (index * tick[k - 1])), "01", "L", "2", order_Num[k - 1], count);   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }
                            }
                            else if (index >= 11 || index <= 20) //우선 호가
                            {
                                if (order_Num[k - 1].Equals("0"))  //처음 주문일때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(buy_first_price[k - 1] + ((index - 10) * tick[k - 1])), "01", "L", "1", "", "0");   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }
                                else //이전에 주문을 넣었을때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(buy_first_price[k - 1] + ((index - 10) * tick[k - 1])), "01", "L", "2", order_Num[k - 1], count);   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }
                            }
                        }
                    }
                    else if (price < start_price[k - 1] - (Loss * tick[k - 1]) && start_price[k - 1] != 0)
                    {
                        //손절
                        if (cont == "1") //매도한 계약일때
                        {
                            if (index == 0) //시장가
                            {
                                getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, "0", "02", "M", "1", "", "0");   //01 : 매도 02: 매수
                                Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                            }
                            else if (index >= 1 || index <= 10) //상대 호가
                            {
                                if (order_Num[k - 1].Equals("0"))  //처음 주문일때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(buy_first_price[k - 1] - (index * tick[k - 1])), "02", "L", "1", "", "0");   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }
                                else //이전에 주문을 넣었을때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(buy_first_price[k - 1] - (index * tick[k - 1])), "02", "L", "2", order_Num[k - 1], count);   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }

                            }
                            else if (index >= 11 || index <= 20) //우선 호가
                            {
                                if (order_Num[k - 1].Equals("0"))  //처음 주문일때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(sell_first_price[k - 1] - ((index - 10) * tick[k - 1])), "02", "L", "1", "", "0");   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }
                                else //이전에 주문을 넣었을때
                                {
                                    order_How = k;
                                    getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, count, Convert.ToString(sell_first_price[k - 1] - ((index - 10) * tick[k - 1])), "02", "L", "2", order_Num[k - 1], count);   //01 : 매도 02: 매수

                                    Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                                    order_How = 0;
                                }
                            }
                        }
                    }
                }
            
        }


        private void SL_Button_Click(object sender, EventArgs e)
        {
            Button tmp = sender as Button;
            if (tmp.Name == "SL_Button_1")
            {
                if (SL_Control_1.Checked == true)
                {
                    if (string.IsNullOrEmpty(SL_OrderHow_1.Text) || string.IsNullOrEmpty(SL_HighTick_1.Text) || string.IsNullOrEmpty(SL_LowTick_1.Text))
                    {
                        SL_Control_1.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(1번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "SL_Button_1")
            {
                if (SL_Control_2.Checked == true)
                {
                    if (string.IsNullOrEmpty(SL_OrderHow_2.Text) || string.IsNullOrEmpty(SL_HighTick_2.Text) || string.IsNullOrEmpty(SL_LowTick_2.Text))
                    {
                        SL_Control_2.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(2번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "SL_Button_3")
            {
                if (SL_Control_3.Checked == true)
                {
                    if (string.IsNullOrEmpty(SL_OrderHow_3.Text) || string.IsNullOrEmpty(SL_HighTick_3.Text) || string.IsNullOrEmpty(SL_LowTick_3.Text))
                    {
                        SL_Control_3.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(3번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "SL_Button_4")
            {
                if (SL_Control_4.Checked == true)
                {
                    if (string.IsNullOrEmpty(SL_OrderHow_4.Text) || string.IsNullOrEmpty(SL_HighTick_4.Text) || string.IsNullOrEmpty(SL_LowTick_4.Text))
                    {
                        SL_Control_4.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(4번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "SL_Button_5")
            {
                if (SL_Control_5.Checked == true)
                {
                    if (string.IsNullOrEmpty(SL_OrderHow_5.Text) || string.IsNullOrEmpty(SL_HighTick_5.Text) || string.IsNullOrEmpty(SL_LowTick_5.Text))
                    {
                        SL_Control_5.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(5번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "SL_Button_6")
            {
                if (SL_Control_6.Checked == true)
                {
                    if (string.IsNullOrEmpty(SL_OrderHow_6.Text) || string.IsNullOrEmpty(SL_HighTick_6.Text) || string.IsNullOrEmpty(SL_LowTick_6.Text))
                    {
                        SL_Control_6.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(6번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
            }
        }


        private void SL_pickone(object sender, EventArgs e)
        {
            CheckBox SL_checkbox = sender as CheckBox;

            if (SL_checkbox.Checked == true)
            {
                if (SL_checkbox.Name == "SL_Control_1")
                {
                    TS_Control_1.Checked = false;
                }
                else if (SL_checkbox.Name == "SL_Control_2")
                {
                    TS_Control_2.Checked = false;
                }
                else if (SL_checkbox.Name == "SL_Control_3")
                {
                    TS_Control_3.Checked = false;
                }
                else if (SL_checkbox.Name == "SL_Control_4")
                {
                    TS_Control_4.Checked = false;
                }
                else if (SL_checkbox.Name == "SL_Control_5")
                {
                    TS_Control_5.Checked = false;
                }
                else if (SL_checkbox.Name == "SL_Control_6")
                {
                    TS_Control_6.Checked = false;
                }
            }
        }


        private void TrailingStop_start(int control)
        {
            string TS_StartTick_name = "TS_StartTick_" + (control).ToString();
            string TS_EndTick_name = "TS_EndTick_" + (control).ToString();
            string TS_Final_High_name = "TS_Final_High_" + (control).ToString();
            string TS_Final_Low_name = "TS_Final_Low_" + (control).ToString();
            string FCode_name = "FCode_" + (control).ToString();
            string CountText_name = "CountText_" + (control).ToString();

            var TS_StartTick = this.Controls.Find(TS_StartTick_name, true).FirstOrDefault();
            var TS_EndTick = this.Controls.Find(TS_EndTick_name, true).FirstOrDefault();
            var TS_Final_High = this.Controls.Find(TS_Final_High_name, true).FirstOrDefault();
            var TS_Final_Low = this.Controls.Find(TS_Final_Low_name, true).FirstOrDefault();
            var FCode = this.Controls.Find(FCode_name, true).FirstOrDefault();
            var CountText = this.Controls.Find(CountText_name, true).FirstOrDefault();

            double starttick = Convert.ToDouble(TS_StartTick.Text);       //ts시점
            double endtick = Convert.ToDouble(TS_EndTick.Text);           //ts지점
            double final_high = Convert.ToDouble(TS_Final_High.Text);     //익절지점
            double final_low = Convert.ToDouble(TS_Final_Low.Text);       //손절지

            get_Tick(control);
            TrailingStop_function(control, FCode.Text, start_price[control - 1], starttick, endtick, final_high, final_low);

        }

        private void TrailingStop_function(int control, string code, double price, double starttick, double endtick, double final_high, double final_low)
        {
            string TS_OrderHow_name = "TS_OrderHow_" + (control).ToString();
            ComboBox TS_OrderHow = (ComboBox)this.Controls.Find(TS_OrderHow_name, true).FirstOrDefault();

            double TS_now_endprice = 0;
            string buyorsell = "";
            int isremain = 0;
            int row_number = Price_GridView.Rows.Count;

            for (int i = 0; i < row_number; i++)
            {
                if (code.Equals((string)Price_GridView.Rows[i].Cells[0].Value))
                {
                    buyorsell = (string)Price_GridView.Rows[i].Cells[1].Value;
                    isremain = Convert.ToInt32(Price_GridView.Rows[i].Cells[2].Value);
                    start_price[control - 1] = Convert.ToDouble(Price_GridView.Rows[i].Cells[3].Value);         //평균가
                    TS_now_endprice = Convert.ToDouble(Price_GridView.Rows[i].Cells[4].Value);      //현재가
                }
                else
                {
                    isremain = 0;
                    start_price[control - 1] = 0;
                }
            }
            if (isremain != 0)               //주문량이 있을경우
            {
                double orderprice = 0;
                if (TS_now_endprice >= start_price[control - 1] + tick[control - 1] * final_high || TS_now_endprice <= start_price[control - 1] - tick[control - 1] * final_low)
                {
                    //익절 or 손절 주문

                    string strTarget = TS_OrderHow.Text;
                    string ordertype = "L";
                    string changebuysell = "";

                    if (buyorsell.Equals("1"))          //매도주문이후 매수청산
                    {
                        changebuysell = "02";                   //매수
                    }
                    else if (buyorsell.Equals("2"))      //매수이후 매도청산
                    {
                        changebuysell = "01";                   //매도
                    }

                    if (strTarget == "시장가")
                    {
                        ordertype = "M";    //시장가
                        orderprice = 0;
                    }
                    else
                    {
                        string strTmp = Regex.Replace(strTarget, @"\D", "");
                        int ntmp = int.Parse(strTmp);
                        ordertype = "L";        //지정가

                        if (strTarget[0].Equals('상'))           //상대호가
                        {
                            if (buyorsell == "1")            //매도이후
                            {
                                orderprice = buy_first_price[control - 1] + (tick[control - 1] * (ntmp - 1));       //매수n호가
                            }
                            else if (buyorsell == "2")       //매수이후
                            {
                                orderprice = sell_first_price[control - 1] - (tick[control - 1] * (ntmp - 1));       //매도n호가
                            }
                        }
                        else if (strTarget[0].Equals('우'))      //우선호가
                        {
                            if (buyorsell == "1")            //매도이후
                            {
                                orderprice = sell_first_price[control - 1] - (tick[control - 1] * (ntmp - 1));         //매도n호가
                            }
                            else if (buyorsell == "2")       //매수이후
                            {
                                orderprice = buy_first_price[control - 1] + (tick[control - 1] * (ntmp - 1));           //매수n호가
                            }
                        }
                    }
                    if (order_Num[control - 1].Equals("0"))  //처음 주문일때
                    {
                        order_How = control;
                        getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, isremain.ToString(), Convert.ToString(orderprice), changebuysell, ordertype, "1", "", "0");   //01 : 매도 02: 매수

                        Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                        order_How = 0;
                    }
                    else //이전에 주문을 넣었을때
                    {
                        order_How = control;
                        getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, isremain.ToString(), Convert.ToString(orderprice), changebuysell, ordertype, "2", order_Num[control - 1], "0");   //01 : 매도 02: 매수

                        Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                        order_How = 0;
                    }

                    TS_on[control - 1] = false;

                }
                else
                {
                    double TS_ing = price + tick[control - 1] * starttick;

                    if (TS_now_endprice >= TS_ing)
                    {
                        TS_on[control - 1] = true;
                        TrailingStop_function(control, code, TS_ing, starttick, endtick, final_high, final_low);     //트레일링 상승
                    }
                    else if (TS_on[control - 1] == true && TS_now_endprice <= price + tick[control - 1] * endtick)
                    {
                        //스탑지점 주문
                        string strTarget = TS_OrderHow.Text;
                        string ordertype = "L";
                        string changebuysell = "";
                        if (buyorsell.Equals("1"))          //매도주문이후 매수청산
                        {
                            changebuysell = "02";                   //매수
                        }
                        else if (buyorsell.Equals("2"))      //매수이후 매도청산
                        {
                            changebuysell = "01";                   //매도
                        }

                        if (strTarget == "시장가")
                        {
                            ordertype = "M";    //시장가
                            orderprice = 0;
                        }
                        else
                        {
                            string strTmp = Regex.Replace(strTarget, @"\D", "");
                            int ntmp = int.Parse(strTmp);
                            ordertype = "L";        //지정가

                            if (strTarget[0].Equals('상'))           //상대호가
                            {
                                if (buyorsell == "1")            //매도이후
                                {
                                    orderprice = buy_first_price[control - 1] + (tick[control - 1] * (ntmp - 1));       //매수n호가 
                                }
                                else if (buyorsell == "2")       //매수이후
                                {
                                    orderprice = sell_first_price[control - 1] - (tick[control - 1] * (ntmp - 1));       //매도n호가
                                }
                            }
                            else if (strTarget[0].Equals('우'))      //우선호가
                            {
                                if (buyorsell == "1")            //매도이후
                                {
                                    orderprice = sell_first_price[control - 1] - (tick[control - 1] * (ntmp - 1));         //매도n호가 
                                }
                                else if (buyorsell == "2")       //매수이후
                                {
                                    orderprice = buy_first_price[control - 1] + (tick[control - 1] * (ntmp - 1));           //매수n호가
                                }
                            }
                        }
                        if (order_Num[control - 1].Equals("0"))  //처음 주문일때
                        {
                            order_How = control;
                            getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, isremain.ToString(), Convert.ToString(orderprice), changebuysell, ordertype, "1", "", "0");   //01 : 매도 02: 매수

                            Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                            order_How = 0;
                        }
                        else //이전에 주문을 넣었을때
                        {
                            order_How = control;
                            getDeal(Account_Num_1.Text, Acc_PW_1.Text, code, isremain.ToString(), Convert.ToString(orderprice), changebuysell, ordertype, "2", order_Num[control - 1], "0");   //01 : 매도 02: 매수

                            Get_RealTimeData(Account_Num_1.Text, Acc_PW_1.Text);
                            order_How = 0;
                        }
                        TS_on[control - 1] = false;
                    }
                }
            }
        }


        private void TS_Button_Click(object sender, EventArgs e)
        {
            Button tmp = sender as Button;
            if (tmp.Name == "TS_Button_1")
            {
                if (TS_Control_1.Checked == true)
                {
                    if (string.IsNullOrEmpty(TS_OrderHow_1.Text) ||
                        string.IsNullOrEmpty(TS_EndTick_1.Text) ||
                        string.IsNullOrEmpty(TS_StartTick_1.Text) ||
                        string.IsNullOrEmpty(TS_Final_High_1.Text) ||
                        string.IsNullOrEmpty(TS_Final_Low_1.Text))
                    {
                        TS_Control_1.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(1번 종목)\n트레일링스탑 설정되었습니다.", "", 1);
                }
            }
            else if (tmp.Name == "TS_Button_2")
            {
                if (TS_Control_2.Checked == true)
                {
                    if (string.IsNullOrEmpty(TS_OrderHow_2.Text) ||
                        string.IsNullOrEmpty(TS_EndTick_2.Text) ||
                        string.IsNullOrEmpty(TS_StartTick_2.Text) ||
                        string.IsNullOrEmpty(TS_Final_High_2.Text) ||
                        string.IsNullOrEmpty(TS_Final_Low_2.Text))
                    {
                        TS_Control_2.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(2번 종목)\n트레일링스탑 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "TS_Button_3")
            {
                if (TS_Control_3.Checked == true)
                {
                    if (string.IsNullOrEmpty(TS_OrderHow_3.Text) ||
                        string.IsNullOrEmpty(TS_EndTick_3.Text) ||
                        string.IsNullOrEmpty(TS_StartTick_3.Text) ||
                        string.IsNullOrEmpty(TS_Final_High_3.Text) ||
                        string.IsNullOrEmpty(TS_Final_Low_3.Text))
                    {
                        TS_Control_3.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(3번 종목)\n트레일링스탑 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "TS_Button_4")
            {
                if (TS_Control_4.Checked == true)
                {
                    if (string.IsNullOrEmpty(TS_OrderHow_4.Text) ||
                        string.IsNullOrEmpty(TS_EndTick_4.Text) ||
                        string.IsNullOrEmpty(TS_StartTick_4.Text) ||
                        string.IsNullOrEmpty(TS_Final_High_4.Text) ||
                        string.IsNullOrEmpty(TS_Final_Low_4.Text))
                    {
                        TS_Control_4.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(4번 종목)\n트레일링스탑 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "TS_Button_5")
            {
                if (TS_Control_5.Checked == true)
                {
                    if (string.IsNullOrEmpty(TS_OrderHow_5.Text) ||
                        string.IsNullOrEmpty(TS_EndTick_5.Text) ||
                        string.IsNullOrEmpty(TS_StartTick_5.Text) ||
                        string.IsNullOrEmpty(TS_Final_High_5.Text) ||
                        string.IsNullOrEmpty(TS_Final_Low_5.Text))
                    {
                        TS_Control_5.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(5번 종목)\n트레일링스탑 설정되었습니다.", "", 10);
                }
            }
            else if (tmp.Name == "TS_Button_6")
            {
                if (TS_Control_6.Checked == true)
                {
                    if (string.IsNullOrEmpty(TS_OrderHow_6.Text) ||
                        string.IsNullOrEmpty(TS_EndTick_6.Text) ||
                        string.IsNullOrEmpty(TS_StartTick_6.Text) ||
                        string.IsNullOrEmpty(TS_Final_High_6.Text) ||
                        string.IsNullOrEmpty(TS_Final_Low_6.Text))
                    {
                        TS_Control_6.Checked = false;
                        MessageBox.Show("입력값 오류입니다.");
                    }
                    else
                        AutoClosingMessageBox("(6번 종목)\n트레일링스탑 설정되었습니다.", "", 10);
                }
            }

        }

        private void TS_pickone(object sender, EventArgs e)
        {
            CheckBox TS_checkbox = sender as CheckBox;

            if (TS_checkbox.Checked == true)
            {
                if (TS_checkbox.Name == "TS_Control_1")
                {
                    SL_Control_1.Checked = false;
                }
                else if (TS_checkbox.Name == "TS_Control_2")
                {
                    SL_Control_2.Checked = false;
                }
                else if (TS_checkbox.Name == "TS_Control_3")
                {
                    SL_Control_3.Checked = false;
                }
                else if (TS_checkbox.Name == "TS_Control_4")
                {
                    SL_Control_4.Checked = false;
                }
                else if (TS_checkbox.Name == "TS_Control_5")
                {
                    SL_Control_5.Checked = false;
                }
                else if (TS_checkbox.Name == "TS_Control_6")
                {
                    SL_Control_6.Checked = false;
                }
            }
        }

        private void get_Tick(int control)
        {
            string FCode_name = "FCode_" + (control).ToString();
            var FCode = this.Controls.Find(FCode_name, true).FirstOrDefault();
            string code = FCode.Text;
            if (code[0] == '1')
            {
                Comm_Obj_Tick.SetQueryName("FH");
            }
            else if (code[0] == '2' || code[0] == '3')
            {
                Comm_Obj_Tick.SetQueryName("QH");
            }

            Comm_Obj_Tick.SetSingleData(0, code);  //코드
            tick_control_num = control;
            Comm_Obj_Tick.RequestData();
        }

        private void Comm_Obj_Tick_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            double sell_first = Convert.ToDouble(Comm_Obj_Tick.GetSingleData(3));    //매도1호가
            double second = Convert.ToDouble(Comm_Obj_Tick.GetSingleData(9));        //매도2호가  
            double buy_first = Convert.ToDouble(Comm_Obj_Tick.GetSingleData(4));     //매수1호가
            tick[tick_control_num - 1] = sell_first - second;
            sell_first_price[tick_control_num - 1] = sell_first;
            buy_first_price[tick_control_num - 1] = buy_first;
        }

        private void Interval_Set_Click(object sender, EventArgs e)
        {
            timer.Interval = Convert.ToInt32(Mecro_Inerval.Text);
            timer.Start();
        }

        private void Interval_Stop_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void Comm_Obj_Price_ReceiveRTData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveRTDataEvent e)
        {
            string code = (string)Comm_Obj_RTPrice.GetSingleData(1);
            if (code.Equals(gFCode[0]))
            {
                current_price[0] = Convert.ToDouble(Comm_Obj_RTPrice.GetSingleData(4));
                if (TS_Control_1.Checked == true)
                    TrailingStop_start(1);
                else if (SL_Control_1.Checked == true)
                    Stop_Loss(current_price[0], 1);
                if (gFCode[1][0] == '1')
                    Comm_Obj_RTPrice.RequestRTReg("FC", gFCode[1]);
                else if (gFCode[1][0] == '2' || gFCode[1][0] == '3')
                    Comm_Obj_RTPrice.RequestRTReg("QC", gFCode[1]);

            }
            else if (code.Equals(gFCode[1]))
            {
                current_price[1] = Convert.ToDouble(Comm_Obj_RTPrice.GetSingleData(4));
                if (TS_Control_1.Checked == true)
                    if (TS_Control_2.Checked == true)
                        TrailingStop_start(2);
                    else if (SL_Control_1.Checked == true)
                        Stop_Loss(current_price[1], 2);
                if (gFCode[2][0] == '1')
                    Comm_Obj_RTPrice.RequestRTReg("FC", gFCode[2]);
                else if (gFCode[2][0] == '2' || gFCode[2][0] == '3')
                    Comm_Obj_RTPrice.RequestRTReg("QC", gFCode[2]);

            }
            else if (code.Equals(gFCode[2]))
            {
                current_price[2] = Convert.ToDouble(Comm_Obj_RTPrice.GetSingleData(4));
                if (TS_Control_1.Checked == true)
                    if (TS_Control_1.Checked == true)
                        TrailingStop_start(3);
                    else if (SL_Control_1.Checked == true)
                        Stop_Loss(current_price[2], 3);
                if (gFCode[3][0] == '1')
                    Comm_Obj_RTPrice.RequestRTReg("FC", gFCode[3]);
                else if (gFCode[3][0] == '2' || gFCode[3][0] == '3')
                    Comm_Obj_RTPrice.RequestRTReg("QC", gFCode[3]);

            }
            else if (code.Equals(gFCode[3]))
            {
                current_price[3] = Convert.ToDouble(Comm_Obj_RTPrice.GetSingleData(4));
                if (TS_Control_1.Checked == true)
                    if (TS_Control_1.Checked == true)
                        TrailingStop_start(4);
                    else if (SL_Control_1.Checked == true)
                        Stop_Loss(current_price[3], 4);
                if (gFCode[4][0] == '1')
                    Comm_Obj_RTPrice.RequestRTReg("FC", gFCode[4]);
                else if (gFCode[4][0] == '2' || gFCode[4][0] == '3')
                    Comm_Obj_RTPrice.RequestRTReg("QC", gFCode[4]);

            }
            else if (code.Equals(gFCode[4]))
            {
                current_price[4] = Convert.ToDouble(Comm_Obj_RTPrice.GetSingleData(4));
                if (TS_Control_1.Checked == true)
                    if (TS_Control_1.Checked == true)
                        TrailingStop_start(5);
                    else if (SL_Control_1.Checked == true)
                        Stop_Loss(current_price[4], 5);
                if (gFCode[5][0] == '1')
                    Comm_Obj_RTPrice.RequestRTReg("FC", gFCode[5]);
                else if (gFCode[5][0] == '2' || gFCode[5][0] == '3')
                    Comm_Obj_RTPrice.RequestRTReg("QC", gFCode[5]);

            }
            else if (code.Equals(gFCode[5]))
            {
                current_price[5] = Convert.ToDouble(Comm_Obj_RTPrice.GetSingleData(4));
                if (TS_Control_1.Checked == true)
                    if (TS_Control_1.Checked == true)
                        TrailingStop_start(6);
                    else if (SL_Control_1.Checked == true)
                        Stop_Loss(current_price[5], 6);
                if (gFCode[0][0] == '1')
                    Comm_Obj_RTPrice.RequestRTReg("FC", gFCode[0]);
                else if (gFCode[0][0] == '2' || gFCode[0][0] == '3')
                    Comm_Obj_RTPrice.RequestRTReg("QC", gFCode[0]);
            }
            /*
            DataTable dt = new DataTable();
            dt.Columns.Add("종목코드");
            dt.Columns.Add("매수매도구분");  //1 매도 2 매수
            dt.Columns.Add("잔고");
            dt.Columns.Add("평균가(단)");
            dt.Columns.Add("평가금액");
            dt.Columns.Add("평가손익");

            DataRow dr = dt.NewRow();

            dr[0] = (string)Comm_Obj_RTCount.GetSingleData(2); //종목코드
            dr[1] = (string)Comm_Obj_RTCount.GetSingleData(5); //매도매수 구분
            dr[2] = (string)Comm_Obj_RTCount.GetSingleData(9); //청산 가능수량
            dr[3] = (string)Comm_Obj_RTCount.GetSingleData(7); //평균단가
            dr[4] = (string)Comm_Obj_RTCount.GetSingleData(11); //평가 금액
            dr[5] = (string)Comm_Obj_RTCount.GetSingleData(12); // 평가 손익
            dt.Rows.Add(dr);

            Price_GridView.DataSource = dt;*/

        }

        private void Comm_Obj_ReceiveRTCount(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveRTDataEvent e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("종목코드");
            dt.Columns.Add("매수매도구분");  //1 매도 2 매수
            dt.Columns.Add("잔고");
            dt.Columns.Add("평균가(단)");
            dt.Columns.Add("평가금액");
            dt.Columns.Add("평가손익");

            DataRow dr = dt.NewRow();

            dr[0] = (string)Comm_Obj_RTCount.GetSingleData(2); //종목코드
            dr[1] = (string)Comm_Obj_RTCount.GetSingleData(5); //매도매수 구분
            dr[2] = (string)Comm_Obj_RTCount.GetSingleData(9); //청산 가능수량
            dr[3] = (string)Comm_Obj_RTCount.GetSingleData(7); //평균단가
            dr[4] = (string)Comm_Obj_RTCount.GetSingleData(11); //평가 금액
            dr[5] = (string)Comm_Obj_RTCount.GetSingleData(12); // 평가 손익
            dt.Rows.Add(dr);

            Price_GridView.DataSource = dt;
        }

        private void Close_Form(object sender, FormClosingEventArgs e)
        {
            File.WriteAllLines(@"..\..\history1.txt", tmp_history_1);
            File.WriteAllLines(@"..\..\history2.txt", tmp_history_2);
            File.WriteAllLines(@"..\..\history3.txt", tmp_history_3);
            File.WriteAllLines(@"..\..\history4.txt", tmp_history_4);
            File.WriteAllLines(@"..\..\history5.txt", tmp_history_5);
            File.WriteAllLines(@"..\..\history6.txt", tmp_history_6);
        }

        private void sell_button_Click(object sender, EventArgs e)
        {
            Comm_Obj_Deal.SetQueryName("SABC100U1");
            Comm_Obj_Deal.SetSingleData(0, "00311155910"); // 계좌번호
            Comm_Obj_Deal.SetSingleData(1, "0000"); //비밀번호
            Comm_Obj_Deal.SetSingleData(2, "101Q3"); //종목코드
            Comm_Obj_Deal.SetSingleData(3, "1"); // 주문수량 
            Comm_Obj_Deal.SetSingleData(4, "0"); //주문단가 -999.99 ~ 999.99
            Comm_Obj_Deal.SetSingleData(5, "0"); // 주문조건 0:일반(FAS) 3:IOC(FAK) 4:FOK
            Comm_Obj_Deal.SetSingleData(6, "01"); // 매매구분 01:매도 02:매수
            Comm_Obj_Deal.SetSingleData(7, "M"); //호가유형 L:지정가 M:시장가 C:조건부 B:최유리
            Comm_Obj_Deal.SetSingleData(8, "1"); //차익거래구분 1:차익 2:헷지 3:기타
            Comm_Obj_Deal.SetSingleData(9, "1"); //처리구분 1:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(10, "0"); //정정취소수량구분 0:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(11, ""); //원주문번호 (신규매도/매수시 생략)
            Comm_Obj_Deal.SetSingleData(12, ""); //예약주문여부 1:예약 (예약주문 어닌경우생략)
            Comm_Obj_Deal.RequestData();
        }

        private void buy_button_Click(object sender, EventArgs e)
        {
            Comm_Obj_Deal.SetQueryName("SABC100U1");
            Comm_Obj_Deal.SetSingleData(0, "00311155910"); // 계좌번호
            Comm_Obj_Deal.SetSingleData(1, "0000"); //비밀번호
            Comm_Obj_Deal.SetSingleData(2, "101Q3"); //종목코드
            Comm_Obj_Deal.SetSingleData(3, "1"); // 주문수량 
            Comm_Obj_Deal.SetSingleData(4, "0"); //주문단가 -999.99 ~ 999.99
            Comm_Obj_Deal.SetSingleData(5, "0"); // 주문조건 0:일반(FAS) 3:IOC(FAK) 4:FOK
            Comm_Obj_Deal.SetSingleData(6, "02"); // 매매구분 01:매도 02:매수
            Comm_Obj_Deal.SetSingleData(7, "M"); //호가유형 L:지정가 M:시장가 C:조건부 B:최유리
            Comm_Obj_Deal.SetSingleData(8, "1"); //차익거래구분 1:차익 2:헷지 3:기타
            Comm_Obj_Deal.SetSingleData(9, "1"); //처리구분 1:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(10, "0"); //정정취소수량구분 0:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(11, ""); //원주문번호 (신규매도/매수시 생략)
            Comm_Obj_Deal.SetSingleData(12, ""); //예약주문여부 1:예약 (예약주문 어닌경우생략)
            Comm_Obj_Deal.RequestData();
        }
    }
}
