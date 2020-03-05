﻿using System;
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
        private int RowNum = 100;
        private string[] gFCode = new string[6] { "101Q3", "101Q3", "101Q3", "101Q3", "101Q3", "101Q3" };
        private string[] TimeSelected = new string[6] { "3", "3", "3", "3", "3", "3" };
        private string[] TimeDistance = new string[6] { "Min", "Min", "Min", "Min", "Min", "Min" };
        private int[] control_buy_sell = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Enable_Angle = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Mecro = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Mecro_Deal = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Time_Set = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] control_Delay_Set = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] condition_Delay = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int[] buy_sell_Count = new int[6] { 0, 0, 0, 0, 0, 0 };
        private int control_num = 0;


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

            //Account_Num_1.Text = (string)Account_Num_1.Items[0];
            setGridView();


            // 선물 코드 목록 조회
            Comm_Obj_Code_List.SetQueryName("fut_mst");
            Comm_Obj_Code_List.RequestData();

            // 옵션 코드 목록 조회
            Comm_Obj_Code_List.SetQueryName("opt_mst");
            Comm_Obj_Code_List.RequestData();
        }

        public void Load_Data(string Fcode, string time, string distance, int control)
        {
            control_num = 1;
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

            control_num = 2;
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

            control_num = 3;
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

            control_num = 4;
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

            control_num = 5;
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

            control_num = 6;
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

        private void Mecro_Deal(int control)
        {
            //control_Mecro 0:매도 1:매수 2:매도(청산) 3:매수(청산)

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
                                getDeal(Acc_num, Acc_pw, code, count, price, cont, type);
                                control_Mecro[control - 1] = 1;
                                buy_sell_Count[control - 1] -= Convert.ToInt32(count);

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
                                getDeal(Acc_num, Acc_pw, code, count, price, cont, type);
                                control_Mecro[control - 1] = 2;
                                buy_sell_Count[control - 1] += Convert.ToInt32(count);

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
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type);
                            buy_sell_Count[control - 1] = 0;
                            control_Mecro[control - 1] = 3;
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매수(청산)")
                    {
                        if (control_Mecro[control - 1] != 4 && buy_sell_Count[control - 1] < 0)
                        {
                            cont = "02";
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type);
                            buy_sell_Count[control - 1] = 0;
                            control_Mecro[control - 1] = 4;
                        }
                    }
                    else
                    {
                        control_Mecro[control - 1] = 0;
                    }

                }
                else //딜레이 사용 x
                {
                    if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매도")
                    {
                        if (control_Mecro[control - 1] != 1)
                        {
                            cont = "01";
                            getDeal(Acc_num, Acc_pw, code, count, price, cont, type);
                            buy_sell_Count[control - 1] -= Convert.ToInt32(count);
                            control_Mecro[control - 1] = 1;
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매수")
                    {
                        if (control_Mecro[control - 1] != 2)
                        {
                            cont = "02";
                            getDeal(Acc_num, Acc_pw, code, count, price, cont, type);
                            buy_sell_Count[control - 1] += Convert.ToInt32(count);
                            control_Mecro[control - 1] = 2;
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매도(청산)")
                    {
                        if (control_Mecro[control - 1] != 3 && buy_sell_Count[control - 1] > 0)
                        {
                            cont = "01";
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type);
                            buy_sell_Count[control - 1] = 0;
                            control_Mecro[control - 1] = 3;
                        }
                    }
                    else if ((string)FCGrid_sample[control - 1].Rows[1].Cells[9].Value == "매수(청산)")
                    {
                        if (control_Mecro[control - 1] != 4 && buy_sell_Count[control - 1] < 0)
                        {
                            cont = "02";
                            getDeal(Acc_num, Acc_pw, code, Convert.ToString(buy_sell_Count[control - 1]), price, cont, type);
                            buy_sell_Count[control - 1] = 0;
                            control_Mecro[control - 1] = 4;
                        }
                    }
                    else
                    {
                        control_Mecro[control - 1] = 0;
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
            _caption = caption; 
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed, null, timeout, System.Threading.Timeout.Infinite);
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
            dt.Columns.Add("매도매수");
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
        
        private void getDeal(string Acc_num, string Acc_pw, string code, string count, string price, string control, string type)
        {
            Comm_Obj_Deal.SetQueryName("SABC100U1");
            Comm_Obj_Deal.SetSingleData(0, Acc_num); // 계좌번호
            Comm_Obj_Deal.SetSingleData(1, Acc_pw); //비밀번호
            Comm_Obj_Deal.SetSingleData(2, code); //종목코드
            Comm_Obj_Deal.SetSingleData(3, count); // 주문수량 
            Comm_Obj_Deal.SetSingleData(4, price); //주문단가 -999.99 ~ 999.99
            Comm_Obj_Deal.SetSingleData(5, "0"); // 주문조건 0:일반(FAS) 3:IOC(FAK) 4:FOK
            Comm_Obj_Deal.SetSingleData(6, control); // 매매구분 01:매도 02:매수
            Comm_Obj_Deal.SetSingleData(7, type); //호가유형 L:지정가 M:시장가 C:조건부 B:최유리
            Comm_Obj_Deal.SetSingleData(8, "1"); //차익거래구분 1:차익 2:헷지 3:기타
            Comm_Obj_Deal.SetSingleData(9, "1"); //처리구분 1:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(10, "0"); //정정취소수량구분 0:신규 2:정정 3:취소
            Comm_Obj_Deal.SetSingleData(11, ""); //원주문번호 (신규매도/매수시 생략)
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
            string bb = (string)Comm_Obj_Deal.GetSingleData(1); //1.ORC주문번호
            AutoClosingMessageBox(aa, "", 100);
            AutoClosingMessageBox(bb, "", 100);
            //MessageBox.Show(aa);
            //MessageBox.Show(bb);
            //MessageBox.Show((string)axGiExpertControl2.GetErrorMessage());
            //MessageBox.Show((string)axGiExpertControl2.GetErrorCode());
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

        private void Comm_obj_Code_List_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {
            Proc_TR_msc();
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

        /*private void Sell_btn_Click(object sender, EventArgs e)
        {
            string Acc_num = Account_Num_1.Text;
            string Acc_pw = Acc_PW_1.Text;
            string code = "101Q3"; // 나중에 고쳐야될부분
            string count = "3"; //  Convert.ToString(Stock_Count.Value);
            string control = "01";
            string price;
            string type;

            if (Order_type.Text == "시장가")
            {
                price = "0";
                type = "M";
            }
            else
            {
                price = Order_Price.Text;
                type = "L";
            }


            getDeal(Acc_num, Acc_pw, code, count, price, control, type);
        }

        private void Buy_btn_Click(object sender, EventArgs e)
        {
            string Acc_num = Account_Num_1.Text;
            string Acc_pw = Acc_PW_1.Text;
            string code = Order_Code.Text;
            string count = Convert.ToString(Stock_Count.Value);
            string control = "02";
            string price;
            string type;

            if (Order_type.Text == "시장가")
            {
                price = "0";
                type = "M";
            }
            else
            {
                price = Order_Price.Text;
                type = "L";
            }
            getDeal(Acc_num, Acc_pw, code, count, price, control, type);
        }*/

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

        private void SL_function(int control)
        {

        }

        private void SL_Button_Click(object sender, EventArgs e)
        {
            Button tmp = sender as Button;
            int control = 0;
            if (tmp.Name == "SL_Button_1")
            {
                if (SL_Control_1.Checked == true)
                {
                    control = 1;
                    AutoClosingMessageBox("(1번 종목)\n스탑로스 설정되었습니다.", "", 1);
                }
                else
                    control = 0;
            }
            else if (tmp.Name == "SL_Button_1")
            {
                if (SL_Control_2.Checked == true)
                {
                    control = 2;
                    AutoClosingMessageBox("(2번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
                else
                    control = 0;
            }
            else if (tmp.Name == "SL_Button_3")
            {
                if (SL_Control_3.Checked == true)
                {
                    control = 3;
                    AutoClosingMessageBox("(3번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
                else
                    control = 0;
            }
            else if (tmp.Name == "SL_Button_4")
            {
                if (SL_Control_4.Checked == true)
                {
                    control = 4;
                    AutoClosingMessageBox("(4번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
                else
                    control = 0;
            }
            else if (tmp.Name == "SL_Button_5")
            {
                if(SL_Control_5.Checked == true)
                {
                    control = 5;
                    AutoClosingMessageBox("(5번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
                else
                    control = 0;
            }
            else if (tmp.Name == "SL_Button_6")
            {
                if (SL_Control_6.Checked == true)
                {
                    control = 6;
                    AutoClosingMessageBox("(6번 종목)\n스탑로스 설정되었습니다.", "", 10);
                }
                else
                    control = 0;
            }

            if(control != 0)
                SL_function(control);
        }

        double start_price = 270.00;    //주문가격
        bool TS_on = false;
        double tick = 0.05;

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
            double final_low = Convert.ToDouble(TS_Final_Low.Text);       //손절지점

            TrailingStop_function(control, FCode.Text, CountText.Text, start_price, starttick, endtick, final_high, final_low);
        }

        private void TrailingStop_function(int control,string code, string count, double price, double starttick, double endtick, double final_high, double final_low)
        {
            string TS_OrderHow_name = "TS_OrderHow_" + (control).ToString();
            ComboBox TS_OrderHow = (ComboBox)this.Controls.Find(TS_OrderHow_name, true).FirstOrDefault();


            double TS_now_endprice = Convert.ToDouble(FCGrid_sample[control - 1].Rows[1].Cells[5].Value);      //현재가

            if (TS_now_endprice >= start_price + tick * final_high || TS_now_endprice <= start_price - tick * final_low)
            {
                //익절 or 손절 주문
                /*
                getDeal(Account_Num_1.Text,Acc_PW_1.Text, FCode.Text, CountText.Text, TS_OrderHow.SelectedIndex)
                TS_on = false;
                */        
            }
            else
            {
                double TS_ing = price + tick * starttick;

                if (TS_now_endprice >= TS_ing)
                {
                    TS_on = true;
                    TrailingStop_function(control, code, count, TS_ing, starttick, endtick, final_high, final_low);
                }
                else if(TS_on == true && TS_now_endprice == price + tick * endtick)
                {
                    //스탑지점 주문
                    /*
                    getDeal(Account_Num_1.Text,Acc_PW_1.Text, FCode.Text, CountText.Text, TS_OrderHow.SelectedIndex)
                    TS_on = false;
                    */
                }

            }

        }
        
        private void Tick_Select(int control)
        {
            string FCode_name = "FCode_" + (control).ToString();
            var FCode = this.Controls.Find(FCode_name, true).FirstOrDefault();

            Comm_Obj_FH_Real.SetQueryName("FH");
            Comm_Obj_FH_Real.SetSingleData(0, FCode.Text);  //코드
            Comm_Obj_FH_Real.RequestData();
        }

        private void Comm_Obj_FH_Real_ReceiveData(object sender, AxGIEXPERTCONTROLLib._DGiExpertControlEvents_ReceiveDataEvent e)
        {

        }
    }
}
