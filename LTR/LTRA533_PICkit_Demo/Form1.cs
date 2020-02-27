using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
//using System.Management;
using ZedGraph;


namespace LTR2594_PICkitS_Demo
{
    public partial class LTR2594_PICkitS_Demo : Form
    {
        private byte SLAVE_ADDR = 0x46;
        private byte CS_CONTR = 0x80;
        private byte PS_CONTR = 0x81;
        private byte PS_LED = 0x82;
        private byte PS_N_PULSES = 0x83;
        private byte PS_MEAS_RATE = 0x84;
        private byte CS_TIME_SCALE = 0X85;
        private byte CS_INT_TIME_STEPS = 0x86;
        private byte CS_MRR_STEPS = 0x87;
        private byte CS_STATUS = 0x88;
        private byte CS_GREEN_DATA_LSB = 0x8B;
        private byte CS_GREEN_DATA_MSB = 0x8C;
        private byte CS_IR_DATA_LSB = 0x8D;
        private byte CS_IR_DATA_MSB = 0x8E;
        private byte CS_CLEAR_DATA_LSB = 0x8F;
        private byte CS_CLEAR_DATA_MSB = 0x90;
        private byte CS_RED_DATA_LSB = 0x91;
        private byte CS_RED_DATA_MSB = 0x92;
        private byte CS_BLUE_DATA_LSB = 0x93;
        private byte CS_BLUE_DATA_MSB = 0x94;
        private byte CS_SELECT_PS_GAIN = 0x95;
        private byte PS_LED_INV = 0x96;
        private byte PS_N_PULSES_INV = 0x97;
        private byte PS_STATUS = 0x99;
        private byte PS_DATA_LSB = 0x9A;
        private byte PS_DATA_MSB = 0x9B;
        private byte PS_SAR = 0x9C;
        private byte PS_SAR_THRES = 0x9D;
        private byte INTERRUPT = 0xA0;
        private byte INTERRUPT_PERSIST = 0xA1;
        private byte PS_THRES_HIGH_LSB = 0xA4;
        private byte PS_THRES_HIGH_MSB = 0xA5;
        private byte PS_THRES_LOW_LSB = 0xA6;
        private byte PS_THRES_LOW_MSB = 0xA7;
        private byte PXTALK_LSB = 0xA8;
        private byte PXTALK_MSB = 0xA9;
        private byte ALS_THRES_HIGH_LSB = 0xAA;
        private byte ALS_THRES_HIGH_MSB = 0xAB;
        private byte ALS_THRES_LOW_LSB = 0xAC;
        private byte ALS_THRES_LOW_MSB = 0xAD;
        private byte PART_ID = 0xAE;
        private byte MANUFAC_ID = 0xAF;


        /* ================================================= */
        /* ==================
         *  Global Variables
         * ==================
	     */
        private int gain = 1;
        double Int_time = 0, Meas_rate = 0;
        private double als_res = 1;
        private double int_fac = 1.0;
        private int prev_ps = 0;
        private int PartID = 0;
        int PS_data;
        int PS_SAR_data;
        int ProximityVal = 0;
        double PS_START_FLAG = 0; //0:1st data, 1: non 1st data
        double PS_K_FAC = 7;
        double PS_OLD_DATA = 0;
        double PS_DELTA = 0;
        double PS_DELTA_FAC = 0;
        double PS_AVG1 = 0;
        double PS_AVG2 = 0;

        public int[] psdata = new int[30];
        int[] mov_data = new int[30];
        int ps_delta = 0;
        int ps_stable_data = 0;
        int ps_initial = 0;
        int OLD_DATA = 0;
        int START_FLAG = 0;
        int ptr = 0;
        int buffer_full = 0;
        int mov_ptr = 0;
        int mov_old_data = 0;
        int stable_ctr = 0;
        //int PS_persist = 0;

        /*Get stable PS*/
        int PS_min = 0;
        int PS_old = 0;
        int PS_new = 0;
        int PS_raw = 0;
        int PS_raw_old = 0;
        int PS_delta = 0;
        int PS_persist = 0;
        int DIR_o = 0;
        int DIR_n = 0;
        /**************/

        int ALS_START_FLAG = 0;//0:1st data, 1:non 1st data
        int maxDELTA = 500;
        int ALS_K_FAC = 7;
        int ALS_OLD_DATA = 0;
        int GREEN_OLD_DATA = 0;
        int RED_OLD_DATA = 0;
        int BLUE_OLD_DATA = 0;
        int ALS_DELTA = 0;
        int ALS_DELTA_FAC = 0;
        int IR_START_FLAG = 0;
        int IR_OLD_DATA = 0;
        int IR_DELTA = 0;
        int IR_DELTA_FAC = 0;

        int ALS_data = 0,R_data=0,B_data=0, IR_data = 0,SAR_data=0;
        int Green_dark = 150;//66 for LTR533, 150 for LTR311
        int Red_dark = 190;//185
        int Blue_dark = 0;//0
        int Clear_dark = 0;//0
        int IR_dark = 70;//61
        int CLEAR_CH = 0;
        int RED_CH = 0;
        int GREEN_CH = 0;
        int BLUE_CH = 0;
        double Xt = 0, Yt = 0, Zt = 0, xcord = 0, ycord = 0; //These are the X,Y,Z tristimuls values
        double CCT = 0.0;
        double n = 0.0;
        double CCT_XYZ = 0.0;
        double lux_data=0, ratio=0;
        int Status_reg = 0;

        int[] PS_array = new int[0xFFF];
        int PS_sum = 0;
        int sample_count = 0;
        int counter = 0;
        int size = 1;
        string video_state = "stop";
        string previous_state = "NEAR";
        string logic_state = "NEAR";

        /* used in Run and Stop */
        private bool run = true; 

	    /* used for activating Debug Mode */
        private bool debugmode = false;

	    /* used in Save and Run */
        private int readFuncSel = -1;
        // private int readINTModeSel = -1;

        /* used for turning on/off the sunflower image */
        private bool sunflower_effect = true;

        /* used for datalogging to text file */
        private bool datalog_to_file = false;

        // Datalog
        private string newfolder = "";
        private string newfile = "";
        private const string DATALOGDIR = @"C:\tmp\LiteON_Datalog";
        private string TEXTFILENAME = "LTR-CSVS052-01_Datalog_{0:yyyy-MM-dd_hh-mm-ss-tt}.txt";
        private int unitcount = 0;
        string currtime, currdate;
        
        /* ZedGraph */
        //private bool plot_als_data = false;
        private PointPairList m_pointsList1_als;
        private PointPairList m_pointsList2_als;
        //private PointPairList m_pointsList3_als;
        double _x_als = 0.0;
        double _y1_als = 0.0, _y2_als = 0.0, _y3_als = 0.0;
        private string plotTitle1 = "", plotTitle2 = "", plotTitle3 = "";
        double minX = 0.0, maxX = 9.0;
        private bool plot_ps_data = false;
        private PointPairList m_pointsList1_ps;
        double _x_ps = 0.0;
        double _y1_ps = 0.0;
        private string plotTitle_ps = "";
        private string plotTitle_als = "";
        Stopwatch sw = new Stopwatch();

        /* Auto Gain Control */
        private bool agc = false;

        /* Extra for RGB sensor */
        public List<double> YData_List = new List<double>(); // this is the yreading
        public List<double> XData_List = new List<double>(); // this is xreading

        string PicPath =  Path.GetDirectoryName(Application.ExecutablePath)+ "/Sunflower.JPG";
        string PicPath1 = Path.GetDirectoryName(Application.ExecutablePath) + "/pic1.jpg";       //This is for the CIE1931
        string PicPath2 = Path.GetDirectoryName(Application.ExecutablePath) + "/rsz_cctx.jpg";       //This is for CCT
        string PicPath4 = Path.GetDirectoryName(Application.ExecutablePath) + "/rsz_tabletphoto.jpg";    //This is for the smaller tablet picture  
       
        
        
        //This is for cursor movement
        private List<PointF> myPts = new List<PointF>();
        Pen pen_din2 = new Pen(Color.Black, 4);

        int ycct = 49;
        int xcct = 50;

        int ycie = 268;
        int xcie = 18;

        double xcorr = 1.0;
        double ycorr = 1.0;
        double zcorr = 1.0;

        //double Xt = 0, Yt = 0, Zt = 0, xcord = 0, ycord = 0; //These are the X,Y,Z tristimuls values
        //double CCT = 0.0;
        //double CCT_XYZ = 0.0;

        double R_factor = 1.0;
        double G_factor = 1.0;
        double B_factor = 1.0;

        

        /* Initialise all components in GUI */
        public LTR2594_PICkitS_Demo()
        {
            InitializeComponent();
             try{
                Image src1 = new Bitmap(PicPath1);
                pic_CIE1931.Image = src1;
                Image src2 = new Bitmap(PicPath2);
                pic_CCT.Image = src2;
                //Image scr3 = new Bitmap(PicPath);
                //pictureBox_Main.Image = scr3;
                //Image scr4 = new Bitmap(PicPath4);
                //pictureBox_HPScreen.Image = scr4;
                //pictureBox_HPScreen.Visible = false;
            }
            catch( Exception e)
            {
                Console.WriteLine("{0} exception caught.",e);
                Console.WriteLine(PicPath1);
                Console.WriteLine(PicPath2);
              //  Console.WriteLine("Bitmap allocated address: {0}",src1);
                System.Windows.Forms.Application.Exit();
            }
            
            

        }



        /* Load Demo Program */
        private void LTR2594_PICkitS_Demo_Load(object sender, EventArgs e)
        {
            

                textBox_Cal_R_factor.Text = R_factor.ToString("F");
                textBox_Cal_G_factor.Text = G_factor.ToString("F");
                textBox_Cal_B_factor.Text = B_factor.ToString("F");

                /* Supply Voltage dropdown box content
                 * PICkit's limitation is 2v8 minimum to device.
		         */
                /*
               comboBox_SupplyVoltage.Items.Add("3.0 (Default)");
               comboBox_SupplyVoltage.Items.Add("3.1");
               comboBox_SupplyVoltage.Items.Add("3.2");
               comboBox_SupplyVoltage.Items.Add("3.3");
               comboBox_SupplyVoltage.Items.Add("3.4");
               comboBox_SupplyVoltage.Items.Add("3.5");
               comboBox_SupplyVoltage.Items.Add("3.6");
               comboBox_SupplyVoltage.SelectedIndex = 0; // default selection

               /* I2C Bitrate dropdown box content */
                /*
                comboBox_I2CBitRate.Items.Add("400 (Default)");
                comboBox_I2CBitRate.Items.Add("100");
                comboBox_I2CBitRate.SelectedIndex = 0; // default selection
                
                /* ======================== */
                /* =======================
                *  ALS Config //for 533
                * =======================  */

                comboBox_CS_MRR_Scale.Items.Add("0.2ms/step");
                comboBox_CS_MRR_Scale.Items.Add("0.39ms/step");
                comboBox_CS_MRR_Scale.Items.Add("0.78ms/step(default)");
                comboBox_CS_MRR_Scale.Items.Add("1.56ms/step");
                comboBox_CS_MRR_Scale.SelectedIndex = 2;

                comboBox_CS_Int_Time_Scale.Items.Add("0.78ms/step(default)");
                comboBox_CS_Int_Time_Scale.Items.Add("0.39ms/step");
                comboBox_CS_Int_Time_Scale.Items.Add("0.2ms/step");
                comboBox_CS_Int_Time_Scale.Items.Add("0.2ms/step");
                comboBox_CS_Int_Time_Scale.SelectedIndex = 0;

                numericUpDown_CS_INT_TIME_step.Value = 64;
                numericUpDown_CS_MR_steps.Value = 128;
                
                comboBox_CS_Gain.Items.Add("1X(default)");
                comboBox_CS_Gain.Items.Add("2X");
                comboBox_CS_Gain.Items.Add("4X");
                comboBox_CS_Gain.Items.Add("8X");
                comboBox_CS_Gain.Items.Add("16X");
                comboBox_CS_Gain.Items.Add("32X");
                comboBox_CS_Gain.Items.Add("64X");
                comboBox_CS_Gain.Items.Add("128X");
                comboBox_CS_Gain.Items.Add("256X");
                comboBox_CS_Gain.Items.Add("512X");
                
                comboBox_CS_Gain.SelectedIndex = 5;

                comboBox_CS_Mode.Items.Add("Active Continuous");
                comboBox_CS_Mode.Items.Add("Active Single");
                comboBox_CS_Mode.SelectedIndex = 0;
                comboBox_CS_Mode.Enabled=false;
                //label36.Hide();

               
                numericUpDown_ALS_Thres_High.Value = 65535;
                numericUpDown_ALS_Thres_Low.Value = 0;
                //numericUpDown_ALS_Thres_High.Hide();
                // label21.Hide();
                comboBox_CS_interrupt_enable.Items.Add("Disable");
                comboBox_CS_interrupt_enable.Items.Add("Enable");
                comboBox_CS_interrupt_enable.SelectedIndex = 0;

                numericUpDown_ALS_persist.Value = 1; 

                /* =======================
                *  PS Config //for 533
                * =======================
                */
                /* PS Resolution Selection dropdown box content */
                comboBox_PS_Resolution.Items.Add("11 bit (Default)");
                comboBox_PS_Resolution.Items.Add("16 bit");
                comboBox_PS_Resolution.SelectedIndex = 0; // default selection

                /* PS Threshold default values */
                numericUpDown_PS_Thres_High.Value = 200;
                numericUpDown_PS_Thres_Low.Value = 100;
                

                /* PS Measurement Time Selection dropdown box content */
                comboBox_PSMeasurementTime.Items.Add("0.39");
                comboBox_PSMeasurementTime.Items.Add("0.78ms");
                comboBox_PSMeasurementTime.Items.Add("1.56ms");
                comboBox_PSMeasurementTime.Items.Add("3.125ms");
                comboBox_PSMeasurementTime.Items.Add("6.25ms");
                comboBox_PSMeasurementTime.Items.Add("12.5ms");
                comboBox_PSMeasurementTime.Items.Add("25ms");
                comboBox_PSMeasurementTime.Items.Add("50ms");
                comboBox_PSMeasurementTime.Items.Add("100ms(default)");
                comboBox_PSMeasurementTime.Items.Add("125ms");
                comboBox_PSMeasurementTime.Items.Add("150ms");
                comboBox_PSMeasurementTime.Items.Add("175ms");
                comboBox_PSMeasurementTime.Items.Add("200ms");
                comboBox_PSMeasurementTime.Items.Add("200ms");
                comboBox_PSMeasurementTime.Items.Add("200ms");
                
                comboBox_PSMeasurementTime.SelectedIndex = 8; // default selection


            /* LED Peak Current dropdown box content */
            /*LED driver*/
                comboBox_PSDrivePkCurr.Items.Add("0mA");
                comboBox_PSDrivePkCurr.Items.Add("8mA");
                comboBox_PSDrivePkCurr.Items.Add("16mA(default)");
                comboBox_PSDrivePkCurr.Items.Add("24mA");
                comboBox_PSDrivePkCurr.Items.Add("32mA");
                comboBox_PSDrivePkCurr.Items.Add("40mA");
                comboBox_PSDrivePkCurr.Items.Add("48mA");
                comboBox_PSDrivePkCurr.Items.Add("56mA");
                comboBox_PSDrivePkCurr.Items.Add("64mA");
                comboBox_PSDrivePkCurr.Items.Add("72mA");
                comboBox_PSDrivePkCurr.Items.Add("80mA");
                comboBox_PSDrivePkCurr.Items.Add("88mA");
                comboBox_PSDrivePkCurr.Items.Add("96mA");
                comboBox_PSDrivePkCurr.Items.Add("104mA");
                comboBox_PSDrivePkCurr.Items.Add("112mA");
                comboBox_PSDrivePkCurr.Items.Add("120mA");
                comboBox_PSDrivePkCurr.SelectedIndex = 2;
            
            /*VSCEL driver
                comboBox_PSDrivePkCurr.Items.Add("0mA");
                comboBox_PSDrivePkCurr.Items.Add("1mA");
                comboBox_PSDrivePkCurr.Items.Add("2mA(default)");
                comboBox_PSDrivePkCurr.Items.Add("3mA");
                comboBox_PSDrivePkCurr.Items.Add("4mA");
                comboBox_PSDrivePkCurr.Items.Add("5mA");
                comboBox_PSDrivePkCurr.Items.Add("6mA");
                comboBox_PSDrivePkCurr.Items.Add("7mA");
                comboBox_PSDrivePkCurr.Items.Add("8mA");
                comboBox_PSDrivePkCurr.Items.Add("9mA");
                comboBox_PSDrivePkCurr.Items.Add("10mA");
                comboBox_PSDrivePkCurr.Items.Add("11mA");
                comboBox_PSDrivePkCurr.Items.Add("12mA");
                comboBox_PSDrivePkCurr.Items.Add("13mA");
                comboBox_PSDrivePkCurr.Items.Add("14mA");
                comboBox_PSDrivePkCurr.Items.Add("15mA");

                comboBox_PSDrivePkCurr.SelectedIndex = 9;
                //comboBox_PSDrivePkCurr.Enabled = false;

                /*NTF FTN enable*/
                comboBox_PS_FTN_NTF_en.Items.Add("Disable FTN/NTF!");
                comboBox_PS_FTN_NTF_en.Items.Add("Enable FTN/NTF!");
                comboBox_PS_FTN_NTF_en.SelectedIndex = 1;

                comboBox_PS_averaging.Items.Add("No averaging(default)");
                comboBox_PS_averaging.Items.Add("2x averaging");
                comboBox_PS_averaging.Items.Add("4x averaging");
                comboBox_PS_averaging.Items.Add("8x averaging");
                comboBox_PS_averaging.SelectedIndex = 0;

                comboBox_PS_Efuse_Enable.Items.Add("Efuse Disabled");
                comboBox_PS_Efuse_Enable.Items.Add("Efuse Enabled");
                comboBox_PS_Efuse_Enable.SelectedIndex = 0;

                comboBox_PS_Offset_enable.Items.Add("Offset disabled");
                comboBox_PS_Offset_enable.Items.Add("Offset enabled");
                comboBox_PS_Offset_enable.SelectedIndex = 0;

                comboBox_Int_Pin_polarity.Items.Add("Active low");
                comboBox_Int_Pin_polarity.Items.Add("Active high");
                comboBox_Int_Pin_polarity.SelectedIndex = 0;

                numericUpDown_PS_Persist.Value=3;
                numericUpDown_PS_LED_Pulse_Count.Value=16;

                comboBox_Pulse_Duty.Items.Add("12.5%");
                comboBox_Pulse_Duty.Items.Add("25%");
                comboBox_Pulse_Duty.Items.Add("50%");
                comboBox_Pulse_Duty.Items.Add("100%(default)");
                comboBox_Pulse_Duty.SelectedIndex = 3;
                //comboBox_Pulse_Duty.Enabled = false;

                comboBox_PS_LEDPulseWidth.Items.Add("8us");
                comboBox_PS_LEDPulseWidth.Items.Add("16us");
                comboBox_PS_LEDPulseWidth.Items.Add("32us(default)");
                comboBox_PS_LEDPulseWidth.Items.Add("64us");
                comboBox_PS_LEDPulseWidth.SelectedIndex = 2; // default selection

                comboBox_PS_Gain.Items.Add("1x(default)");
                comboBox_PS_Gain.Items.Add("2x");
                comboBox_PS_Gain.Items.Add("4x");
                comboBox_PS_Gain.SelectedIndex = 0;
                
                /* ======================= */
                //checkBox_Mov_avg.Checked = true;
                //checkBox_delta_check.Checked = true;
                checkBox_fuse_current.Hide();
                checkBox_CLEAR_Enable.Checked = true;
                checkBox_IR_Enable.Checked = true;
                checkBox_RB_Enable.Checked = true;
                groupBox_ALS_function.Visible = false;
                groupBox_PS_function.Visible = false;
                
            //jy 28 Oct
                tabControl1.TabPages.Remove(DEBUG);

            /* Try to establish communication with PICkit Serial
             * device and initialize communication threads used by
             * class library.
	         */
			try{
            if (PICkitS.Basic.Initialize_PICkitSerial())
            {
                /* Configure PICkit Serial control block for I2C_M
                 * (I2C Master) communication and tells class library
                 * to interpret incoming data as same.
		         */
                if (PICkitS.I2CM.Configure_PICkitSerial_For_I2CMaster() != true)
                {
                    MessageBox.Show("Configure_PICkitSerial_For_I2CMaster failed!","Error" ,MessageBoxButtons.OK,MessageBoxIcon.Error);
                }

                if (PICkitS.I2CM.Set_Source_Voltage(3.0) != true)
                {
                    MessageBox.Show("Set_Source_Voltage failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                /* Update status textbox */
                textBox_Execute_Status.Text = "Liteon Demo Board Found !!" + "\r\n" +
                    "Change settings and then RUN to proceed...";
                this.toolStripStatusLabel1.Text = "USB Device is attached";
                /* ======================
             * Setting Supply Voltage
             * ======================
             * Readback from combobox and apply voltage
	         */
               // Set_DeviceSupplyVoltage(1);//3.1V - not used for new pickit hardware


                /* ===================
                 * Setting I2C Bitrate
                 * ===================
                 * Readback from combobox and apply i2c freq
                 */
                Set_DeviceI2CFreq(0);//400Khz


                /*===========================
                 * Verify PartID 
                 * ===========================
                 */
                // PartID = Func_I2C_ReadReg(PART_ID);
                //MessageBox.Show("Part ID = 0x" + Convert.ToString(i2c_data_array_PartID, 16) +
                //    "; Manufac ID = 0x" + Convert.ToString(i2c_data_array_ManufacID, 16) + "\r\n");

                tabControl1.SelectedIndex = 0;
                tabControl1.SelectedTab = CCT_DEMO;
                /* Disable Run and Stop buttons on next tab */
                Execute_Run.Enabled = true;
                Execute_Stop.Enabled = false;
                Debug_Mode.Visible=false;
                
                Exit.Enabled = true;
            }
            /* PICkitS board not detected */
            else
            {
                /* Update status textbox */
                textBox_Execute_Status.Text = "Liteon Demo Board NOT Found !!" + "\r\n" +
                    "Check connection or change a new board to proceed...";
                this.toolStripStatusLabel1.Text = "USB Device is detached";

               
                Execute_Run.Enabled = false;
                Execute_Stop.Enabled = false;
                Exit.Enabled = true;

                /* Turn off interrupt indicators */
                
                pictureBox_PSInterrupt.BackColor = Color.LightGray;
            }
			}
			catch(Exception ex)
			{

					Console.WriteLine(ex);
			}
        }

        private void video_function(string func)
        {
            if (func == "NEAR")
            {
                if (previous_state == "FAR")
                {
                    //MediaPlayer.Ctlcontrols.play();
                    video_state = "play";
                    previous_state = "NEAR";
                }
            }
            else
            {
                if (previous_state == "NEAR")
                {
                    previous_state = "FAR";
                    //MediaPlayer.Ctlcontrols.pause();
                }
            }
        }


        /* ############################
         * ##   RUN button clicked   ##
         * ############################ 
         */
        private int Func_ALS_Avg(int ALS_data)
        {
            int final_data=0;

           // if(ALS_START_FLAG = 0)
            {
                
            }

            return final_data;
        }

        private void get_stable_PS()
        {
            int k = 7;
            PS_raw = ProximityVal;
            PS_delta = PS_raw - PS_old;
            if (PS_START_FLAG == 0)
            {
                PS_new = PS_raw;
                PS_START_FLAG = 1;
            }
            else
            {
                if (PS_delta >= 0)
                {
                    DIR_n = 1;

                }
                else
                {
                    DIR_n = -1;

                }
                if (DIR_n == DIR_o)
                {
                    if (PS_persist == 3)
                    {
                        PS_new = PS_old + PS_delta;
                        PS_persist = 0;
                    }
                    //else
                        //PS_new = PS_old + PS_delta * (k * PS_delta / (1000 + k * PS_delta));
                    PS_persist++;
                }
                else
                {
                    PS_persist = 0;
                    PS_new = PS_old + PS_delta * (k * PS_delta / (1000 + k * PS_delta));
                }


                DIR_o = DIR_n;
            }
            PS_old = PS_new;
            //PS_raw_old = PS_raw;
            ProximityVal = PS_new;
            textBox_Execute_Status.AppendText(",stable, DIR," + DIR_n+", PS_new,"+PS_new + ", delta," + PS_delta);
        }

        private void get_mov_avg_data()
        {
           
            int sample = 0;
            int sum = 0;
            int avg = 0;
            int noise = ProximityVal - avg;
            //int maxDELTA = 200;
            int maxDELTA = Convert.ToInt16(textBox_MaxDelta.Text);

            if (Math.Abs(noise) < maxDELTA )
            {
                buffer_full = 0;
                mov_ptr = 0;
                //for (int i = 0; i < 4; i++)
                //{
                  //  sum += mov_data[i];
                   // if (mov_data[i] != 0) sample++;
                //}
                //avg = sum / sample;
               
            }
            if(mov_ptr>=4)
            {
                buffer_full = 1;
                mov_ptr=0;
                //avg = ProximityVal;
                //mov_ptr++;
            }
            mov_data[mov_ptr] = ProximityVal;
            mov_ptr++;
            if(buffer_full==1)
            {
                sample = 4;
            }
            else
            {
                sample = mov_ptr;
            }

            for (int i = 0; i < sample; i++)
            {
              sum += mov_data[i];
            }
            avg = sum / sample;
            
            ProximityVal = avg;
            textBox_Execute_Status.AppendText("\r\nsum=" + sum.ToString() + ",S =" + sample.ToString() + ", PS=" + ProximityVal.ToString());
        }

        private void get_avg_data()
        {
            int READ_DATA = ProximityVal;
            int K_FAC = 7;
            int DELTA = 0;
            int DELTA2 = 0;
            int NOISE = 0;
            //int maxDELTA = 200;
            //int maxDELTA2 = 200;
            int maxDELTA = Convert.ToInt16(textBox_MaxDelta.Text);
            int maxDELTA2 = Convert.ToInt16(textBox_MaxDelta.Text);

            switch (START_FLAG)
            {
                case 0:
                    ps_stable_data = READ_DATA;
                    ps_initial = READ_DATA;
                    START_FLAG = 1;
                    break;
                case 1:
                    DELTA = READ_DATA - ps_initial;
                    NOISE = READ_DATA - OLD_DATA;

                    if (Math.Abs(DELTA) > maxDELTA)
                    {
                        //stable_ctr = 0;
                        //if (stable_ctr!=0)
                        //{
                        DELTA2 = READ_DATA - ps_stable_data;
                        if (Math.Abs(DELTA2) > maxDELTA2)
                        {
                            //stable_ctr = 0;
                            ps_stable_data = READ_DATA;
                            ProximityVal = ps_stable_data;
                        }
                        else
                        {
                            if (Math.Abs(NOISE) < maxDELTA)
                                ProximityVal = ps_stable_data + NOISE * K_FAC / 20;
                            else
                            {
                                ps_stable_data = READ_DATA;
                                ProximityVal = READ_DATA;
                                //ProximityVal = ps_stable_data + NOISE ;
                                //stable_ctr=1;
                            }
                        }
                        // }
                        // else
                        // {
                        // ps_stable_data = READ_DATA;
                        //  ProximityVal = ps_stable_data;
                        //stable_ctr = 0;
                        // stable_ctr=1;
                        //}
                        if (ps_initial > READ_DATA) ps_initial = READ_DATA;
                    }
                    else
                    {
                        //if (READ_DATA < ps_initial + maxDELTA) ps_stable_data = ps_initial;
                        if (Math.Abs(NOISE) < maxDELTA)
                        {
                            ProximityVal = ps_initial + NOISE * K_FAC / 20;
                            stable_ctr = 0;
                        }
                        else
                        {
                            ps_stable_data = READ_DATA;
                            ProximityVal = READ_DATA;
                        }

                    }
                    break;
            }

            textBox_Execute_Status.AppendText("\r\nps_stable=" + ps_stable_data.ToString() + ", Raw=" + READ_DATA.ToString() + ", PS=" + ProximityVal.ToString() + ",ps initial=" + ps_initial.ToString() + ",DELTA=" + DELTA.ToString() + ",Noise=" + NOISE.ToString());
            OLD_DATA = READ_DATA;
        }

        private void Execute_Run_Click(object sender, EventArgs e)
        {
            
            unitcount++;
            currtime = DateTime.Now.ToLongTimeString();

            Func_I2C_WriteReg(PS_CONTR, 0x01);//reset sensor

            buffer_full = 0;
            mov_ptr = 0;
            /* Allow run */
            run = true;
            textBox_Execute_Status.AppendText(" Program Running...");
            textBox_Execute_Status.AppendText("\r\n=============" + "\r\nIternation: " + Convert.ToString(unitcount));
            textBox_Execute_Status.AppendText("\r\n=============");

            /* Disable Run button and enable Stop button */
            Execute_Run.Enabled = false;
            Execute_Run.Text = "PROGRAM RUNNING";
            Execute_Stop.Enabled = true;
            Execute_Stop.Text = "STOP";
            Exit.Enabled = false;

            //string PicPath = Directory.GetCurrentDirectory() + "/Desktop/LTR/Sunflower.JPG";
            Bitmap Sunflower;

            int Scale, Zoom, x, y;
            //int i;
            int PS_status, ALS_status;
            int MaxPS = 0;
            int MinPS = 65535;

            double R_new = 0.0;
            double G_new = 0.0;
            double B_new = 0.0;

            //tabControl1.SelectedIndex = 1;
            for (int i = 0; i < 30; i++)
            {
                psdata[i] = 0;
                mov_data[i] = 0;
            }
            counter = 1;
            ALS_START_FLAG = 0;
            IR_START_FLAG = 0;
            PS_START_FLAG = 0;

            /* Trap start */
            try
            {
                /* Load sunflower picture and read all pixel data */
                Sunflower = new Bitmap(PicPath, true);

                int[,] Red_orig = new int[Sunflower.Width, Sunflower.Height], 
                    Green_orig = new int[Sunflower.Width, Sunflower.Height], 
                    Blue_orig = new int[Sunflower.Width, Sunflower.Height];

                int[,] Red_newColour  = new int[Sunflower.Width, Sunflower.Height], 
                    Green_newColour  = new int[Sunflower.Width, Sunflower.Height], 
                    Blue_newColour  = new int[Sunflower.Width, Sunflower.Height];

                /* Read sunflower bitmap and store pixel value into array */
                Red_orig = Func_ReadPixel(Sunflower, "Red");
                Green_orig = Func_ReadPixel(Sunflower, "Green");
                Blue_orig = Func_ReadPixel(Sunflower, "Blue");

                
                int PSLowThreshold = (int)numericUpDown_PS_Thres_Low.Value;
                int PSHighThreshold = (int)numericUpDown_PS_Thres_High.Value;
                //MessageBox.Show(textBox_PSLowThreshold.Text + " : " + textBox_PSHighThreshold.Text);

                //xi2c_data_array_PSThreshHighStr_Lo_tmp1 = i2c_data_array_PSThreshHighStr_Lo + (i2c_data_array_PSThreshHighStr_Hi << 8); 
                //string currtime;

                /* ZedGraph */
                byte numData_ = 0; //(Nelson-test)
                m_pointsList1_als = new PointPairList();
                m_pointsList2_als = new PointPairList();
                m_pointsList1_ps = new PointPairList(); 
                this.button_clearplots_Click(null, null);
                
                PartID = Func_I2C_ReadReg(PART_ID);
                textBox_Execute_Status.AppendText("\r\nPart ID is 0x" + Convert.ToString(PartID, 16));
                //tabControl1.SelectedIndex = 1;
                tabControl1.SelectedTab = CCT_DEMO;

                sw.Start();
                Init_DeviceALS();
                Init_DevicePS();
                Func_I2C_WriteReg(0xB8, 0xC0);      //Enable Efuse calibration step to 11
                Func_I2C_WriteReg(0xB4, 0xE0);      //Enable IR Efuse selection

                if (checkBox_ALS.Checked)
                {
                    Set_ALSControl(1);
                }
                if (checkBox_PS.Checked)
                {
                    Set_PSControl(1);
                }

                START_FLAG = 0;
                size =Convert.ToUInt16(numericUpDown_size.Value);
                /* Loop start */
                while (run == true)
                {
                    currtime = DateTime.Now.ToLongTimeString();
					pic_CIE1931.Invalidate();
                    if (checkBox_PS_Interrupt.Checked)
                    {
                        PS_status = Func_Read_PSStatus();
                        if ((PS_status & 0x02) == 0x02)
                        {
                            //pictureBox_PS_SAT.BackColor = Color.Red;
                        }
                        else
                        {
                            //pictureBox_PS_SAT.BackColor = Color.Green;
                        }

                            //textBox_Execute_Status.AppendText("\r\n[" + currtime + "] PS Status: 0x" + Convert.ToString(PS_status, 16)+", ");

                        if ((PS_status & 0x03) == 0x03)
                        {
                            textBox_Execute_Status.AppendText("\r\n[" + currtime + "] PS Status: 0x" + Convert.ToString(PS_status, 16) + ", ");
                            pictureBox_PSInterrupt.BackColor = Color.Red;
                            if ((PS_status & 0x20) == 0x20)
                            {
                                textBox_PSlogic.Text = "NEAR!!!";
                                ProximityVal = Func_Read_PSdata();
                                logic_state = "NEAR";

                            }
                            else if ((PS_status & 0x10) == 0x10)
                            {
                                textBox_PSlogic.Text = "FAR!!!";
                                ProximityVal = Func_Read_PSdata();
                                //Func_Display_ProximityVal(ProximityVal, Status_reg);
                                logic_state = "FAR";
                            }
                            else
                            {
                                ProximityVal = Func_Read_PSdata();
                                if ((ProximityVal > PSHighThreshold))
                                {
                                    textBox_PSlogic.Text = "NEAR!!!";
                                    //textBox_plot_PSLogic.Text = "NEAR";
                                    logic_state = "NEAR";
                                    //Console.Beep(2000,300);
                                }
                                else if ((ProximityVal < PSLowThreshold))
                                {
                                    textBox_PSlogic.Text = "FAR!!!";
                                    //textBox_plot_PSLogic.Text = "FAR";
                                    logic_state = "FAR";
                                    //pictureBox_PSInterrupt.BackColor = Color.Red;
                                }
                            }
                            Func_Display_ProximityVal(ProximityVal, Status_reg);
                           
                        }
                        else
                        {
                            pictureBox_PSInterrupt.BackColor = Color.Green;
                        }

                        ALS_status = Func_Read_ALSStatus();
                        if ((ALS_status & 0x01)== 0x01)
                        {
                            textBox_Execute_Status.AppendText("\r\n[" + currtime + "] ALS Status: 0x" + Convert.ToString(ALS_status, 16));
                            Func_Read_ALSdata(ALS_status);
                            //textBox_Execute_Status.AppendText(", ALS , IR, Lux = " + Convert.ToString(ALS_data) + ", " + Convert.ToString(IR_data) + ", " + Convert.ToString(lux_data)+", "+sw.ElapsedMilliseconds+", ");
                        }

                    }
                    else/*polling mode*/
                    {
                        if (checkBox_PS.Checked)
                        {
                            PS_status = Func_Read_PSStatus();
                            if ((PS_status & 0x04) == 0x04)
                            {
                               // pictureBox_PS_SAT.BackColor = Color.Red;
                            }
                            else
                            {
                                //pictureBox_PS_SAT.BackColor = Color.Green;
                            }

                            //textBox_Execute_Status.AppendText("\r\n[" + currtime + "] 1. PS Status: 0x" + Convert.ToString(PS_status, 16) + ", ");
                            //PS_status = 1;
                            if ((PS_status & 1) == 1) //Poll for PS data
                            {
                                textBox_Execute_Status.AppendText("\r\n[" + currtime + "] 1. PS Status: 0x" + Convert.ToString(PS_status, 16) );
                                ProximityVal = Func_Read_PSdata();
                                textBox_Execute_Status.AppendText(", " + sw.ElapsedMilliseconds + "ms");
                                textBox_Timelapsed.Text = sw.ElapsedMilliseconds.ToString();
                                //ProximityVal=Func_PS_Avg(PS_array[sample_count]);
                                    //get_stable_PS();
                               
                                  if(checkBox_Mov_avg.Checked)
                                    get_mov_avg_data();
                                  if(checkBox_delta_check.Checked)
                                    get_avg_data();

                                    if ((ProximityVal > PSHighThreshold))
                                    {
                                        textBox_PSlogic.Text = "NEAR!!!";
                                        //textBox_plot_PSLogic.Text = "NEAR";
                                        logic_state = "NEAR";
                                        //Console.Beep(2000,300);
                                        //pictureBox_PSInterrupt.BackColor = Color.Red;
                                        //Thread.Sleep(100);
                                        
                                    }
                                    else if ((ProximityVal < PSLowThreshold))
                                    {
                                        textBox_PSlogic.Text = "FAR!!!";
                                        //textBox_plot_PSLogic.Text = "FAR";
                                        logic_state = "FAR";
                                        //pictureBox_PSInterrupt.BackColor = Color.Green;
                                        //Thread.Sleep(100);
                                    }
                                    //pictureBox_PSInterrupt.BackColor = Color.Green;
                                    //MessageBox.Show(Convert.ToString(ProximityVal));
                                    Func_Display_ProximityVal(ProximityVal, Status_reg);
                                if (ProximityVal > MaxPS) MaxPS = ProximityVal;
                                if (ProximityVal < MinPS) MinPS = ProximityVal;
                                textBox_MaxPS.Text = MaxPS.ToString();
                                textBox_MinPS.Text = MinPS.ToString();
                            }
                        }

                        if (checkBox_ALS.Checked)
                        {

                            ALS_status = Func_Read_ALSStatus();
                            
                            if ((ALS_status & 0x40) == 0x40)
                            {
                                pictureBox_Data_Valid.BackColor = Color.Red;
                            }
                            else
                            {
                                pictureBox_Data_Valid.BackColor = Color.Green;
                            }
                            //textBox_Execute_Status.AppendText("\r\n[" + currtime + "] 2. ALS Status: 0x" + Convert.ToString(ALS_status, 16));
                            //ALS_status = 1;
                            if ((ALS_status & 0x01) == 0x01)
                            {
                                textBox_Execute_Status.AppendText("\r\n[" + currtime + "] 2. ALS Status: 0x" + Convert.ToString(ALS_status, 16));
                                Func_Read_ALSdata(ALS_status);
                                textBox_Timelapsed.Text = sw.ElapsedMilliseconds.ToString();
                                //textBox_Execute_Status.AppendText(", ALS , IR, Lux = " + Convert.ToString(ALS_data) + ", " + Convert.ToString(IR_data) + ", " + Convert.ToString(lux_data));
                            }
                        }
                    }

                    /****************Display Graph******************/
                    /* Graph */
                    if (checkBox_plot.Checked)
                    {
                        _x_ps += 0.1;
                        plotTitle_ps = "PS";
                        plotTitle_als = "ALS";
                        _y1_ps = ProximityVal;
                        _y1_als = lux_data;
                        m_pointsList1_ps.Add(_x_ps, _y1_ps);
                        m_pointsList1_als.Add(_x_ps, _y1_als);
                        CreateGraph_ps(zedgraph_ps);
                        if (_x_ps > maxX)
                        {
                            minX += 1;
                            maxX += 1;
                        }
                    }
                    /*************Display Graph******************/    

                    /* Sunflower Effects */
                   // if (checkBox_sunflower.Checked)
                   /*
                    {

                        //Scale = Func_ScaleCalc(AmbientLuxVal);
                        Scale = 1;

                        Red_newColour = Func_ScalePixel(Red_orig, Sunflower.Width, Sunflower.Height, Scale);
                        Green_newColour = Func_ScalePixel(Green_orig, Sunflower.Width, Sunflower.Height, Scale);
                        Blue_newColour = Func_ScalePixel(Blue_orig, Sunflower.Width, Sunflower.Height, Scale);

                        /*
                        for (x = 0; x < Sunflower.Width; x++)
                        {
                            for (y = 0; y < Sunflower.Height; y++)
                            {
                                Color newColor = Color.FromArgb(Red_newColour[x, y], Green_newColour[x, y], Blue_newColour[x, y]);
                                Sunflower.SetPixel(x, y, newColor);
                            }
                        }
                        */
                        /* ====================
                         * PS Sunflower Effects
                         * ====================
                         */
                         /*
                        if (!checkBox_PS.Checked)
                            Zoom = 1;
                        else
                        {
                                Zoom = Convert.ToInt32(0.5 * (0.002632 * ProximityVal) + 0.7368);
                        }

                        Sunflower = Func_ZoomImage(Sunflower, Red_newColour, Green_newColour, Blue_newColour, Zoom);

                        /* Outputs image into picturebox */
                        //pictureBox_Main.Image = Sunflower;
                    
                    //} /* End of Sunflower Effects */


                    R_new = Convert.ToDouble(RED_CH) * Math.Round(R_factor, 2);
                    G_new = Convert.ToDouble(GREEN_CH) * Math.Round(G_factor, 2);
                    B_new = Convert.ToDouble(BLUE_CH) * Math.Round(B_factor, 2);

                    if (R_new > 255 || G_new > 255 || B_new > 255)
                    {
                        double MaxRGB = FindMaxRGB(R_new, G_new, B_new);

                        R_new = (R_new / MaxRGB) * 255;
                        G_new = (G_new / MaxRGB) * 255;
                        B_new = (B_new / MaxRGB) * 255;
                    }

                    panel_Colour.BackColor = Color.FromArgb(Convert.ToInt32(R_new), Convert.ToInt32(G_new), Convert.ToInt32(B_new));
                    //CCT_XYZ = CCT;
                    //this.Update();
                    //Thread.Sleep((int)numericUpDown_poll_time.Value);
                    //Thread.Sleep(100);
                    this.Refresh();
                    Application.DoEvents();
                }
            }

            /* Trap recover */
            catch (System.ArgumentException)
            {
                string ErrorStr_InvalidPath = "Invalid Picture Path. \r\n" +
                    "Check that the file \"Sunflower.JPG\" is in \r\n" + 
                    Convert.ToString(Directory.GetCurrentDirectory());
                MessageBox.Show(PicPath);
            }
        }


        /* #############################
         * ##   STOP button clicked   ##
         * ############################# 
         */
        private void Execute_Stop_Click(object sender, EventArgs e)
        {
            currtime = DateTime.Now.ToLongTimeString();

            /* Stops the execution */
            run = false;
            Set_ALSControl(0);
            Set_PSControl(0);
            /* Updates status textbox */
            textBox_Execute_Status.AppendText ("\r\n\r\n[" + currtime + "] " +
                "Program Stopped !!\r\n\r\n");
            //MediaPlayer.Ctlcontrols.pause();
            sw.Stop();
            sw.Reset();

            if (datalog_to_file == true)
            {
                string filepathstr, datastr;

                textBox_Execute_Status.SelectAll();
                textBox_Execute_Status.Copy();

                filepathstr = newfolder + "\\" + newfile;
                // MessageBox.Show(filepathstr);
                datastr = textBox_Execute_Status.Text;
                // MessageBox.Show(datastr);
                WriteFile(filepathstr, datastr);
            }

            textBox_Execute_Status.AppendText("Program Stopped !!");

            /* Turns off interrupt indicators */
            
            pictureBox_PSInterrupt.BackColor = Color.LightGray;

            /* Enable Run button and update status */
            Execute_Run.Enabled = true;
            Execute_Run.Text = "RUN";
            Execute_Stop.Enabled = false;
            Execute_Stop.Text = "PROGRAM STOPPED";
            //Set_Save.Enabled = true;
            //Set_Save.Text = "SAVE";
            //Cancel.Enabled = false;
            Exit.Enabled = true;
            //ObjectFound.Text = "";

            //Set_DeviceFunction(0);
        }



       
        /* #############################
         * ##   EXIT button clicked   ##
         * ############################# 
         */
        private void Execute_Exit_Click(object sender, EventArgs e)
        {
            int dummyresult = 0;
            
            
            if (datalog_to_file == true)
            {
                currtime = DateTime.Now.ToLongTimeString();
                string filepathstr, datastr;

                filepathstr = newfolder + "\\" + newfile;
                // MessageBox.Show(filepathstr);
                datastr = "[--- Datalog Ended at " + currtime + " ---]";
                // MessageBox.Show(datastr);
                WriteFile(filepathstr, datastr);

                MessageBox.Show("PLEASE TAKE NOTE OF BELOW.... \r\n\r\nDatalog Directory : " + newfolder + "\r\n" +
                        "Datalog Filename : " + newfile);
            }


            dummyresult = Func_I2C_WriteReg(PS_CONTR, 0);
            

            PICkitS.I2CM.Set_Source_Voltage(0.0);
            Environment.Exit(0);
        }

        /* ###################
         * ##   ALS Set Up   ##
         * ###################
         */
        private void Init_DeviceALS()
        {
            Func_DebugMsg("<<<<<< RGB >>>>>>");
            int buf = 0;
            if (checkBox_CLEAR_Enable.Checked) buf = buf + 32;
            if (checkBox_RB_Enable.Checked) buf = buf + 16;
            if (checkBox_IR_Enable.Checked) buf = buf + 8;
            if (checkBox_PS_Gain_Enable.Checked) buf = buf + 4;
            buf = buf + comboBox_PS_Gain.SelectedIndex;
            Func_I2C_WriteReg(CS_SELECT_PS_GAIN, buf);//0x38

            buf = comboBox_CS_Int_Time_Scale.SelectedIndex * 4 + comboBox_CS_MRR_Scale.SelectedIndex;
            Func_I2C_WriteReg(CS_TIME_SCALE, buf);//0x02, 0.78ms per step & 0.78ms per step

            buf = (int)numericUpDown_CS_INT_TIME_step.Value;
            Func_I2C_WriteReg(CS_INT_TIME_STEPS, buf);//0x40, 50ms integration

            buf = (int)numericUpDown_CS_MR_steps.Value;
            Func_I2C_WriteReg(CS_MRR_STEPS, buf);//0xFF, 200ms measurement 

            //Func_I2C_WriteReg(CS_CONTR, 0x01);
            
        }


        /* ###################
         * ##   PS Set Up   ##
         * ###################
         */
        private void Init_DevicePS()
        {
            Func_DebugMsg("<<<<<< PS >>>>>>");
            int buf = 0;
            int buf_inv = 0xFF;
            /* ##  PS configuration  ## */
            //buf = 0xA9;
            buf = comboBox_Pulse_Duty.SelectedIndex * 64 + comboBox_PS_LEDPulseWidth.SelectedIndex * 16 + comboBox_PSDrivePkCurr.SelectedIndex;
            Func_I2C_WriteReg(PS_LED,buf);//PS Led
            Func_I2C_WriteReg(PS_LED_INV, 255-buf);//PS Led

            buf = (int)numericUpDown_PS_LED_Pulse_Count.Value;
            Func_I2C_WriteReg(PS_N_PULSES, buf);//0x10,pulse number
            Func_I2C_WriteReg(PS_N_PULSES_INV, 255-buf);//pulse number

            buf = 0x80+comboBox_PS_averaging.SelectedIndex*16+ comboBox_PSMeasurementTime.SelectedIndex;
            Func_I2C_WriteReg(PS_MEAS_RATE, buf);//0x89,Meas rate

            buf = (int)numericUpDown_PS_SAR_Thres.Value;
            Func_I2C_WriteReg(PS_SAR_THRES, buf);//0x7F, PS SAR

            buf = (int)numericUpDown_PS_Offset.Value;
            Func_I2C_WriteReg(PXTALK_LSB, buf & 0xFF);
            Func_I2C_WriteReg(PXTALK_MSB, buf >> 8);

            Func_I2C_WriteReg(0x65, 0x79);//Password
            Func_I2C_WriteReg(0x03, 0x82);//Password
            Func_I2C_WriteReg(0xB0, 0x00);// 0x00->Select LED, 0x04->Select VSCEL

            if (checkBox_PS_Interrupt.Checked)
            {
                Func_I2C_WriteReg(INTERRUPT, 0x02);
            }
            else
            {
                Func_I2C_WriteReg(INTERRUPT, 0x00);
            }

            int PS_Th_low,PS_Th_hi;
            PS_Th_hi = (int)numericUpDown_PS_Thres_High.Value;
            PS_Th_low = (int)numericUpDown_PS_Thres_Low.Value;
            Func_I2C_WriteReg(PS_THRES_HIGH_LSB, PS_Th_hi & 0xFF);
            Func_I2C_WriteReg(PS_THRES_HIGH_MSB, PS_Th_hi >> 8);
            Func_I2C_WriteReg(PS_THRES_LOW_LSB, PS_Th_low & 0xFF);
            Func_I2C_WriteReg(PS_THRES_LOW_MSB, PS_Th_low >> 8);



        }/* End of PS Setup */

        

        /* ################
         * ## Debug Mode ##
         * ################
         */
         
        private void Func_DebugMsg(string message)
        {
            if (debugmode == true)
            {
                DialogResult dresult = MessageBox.Show(message + "\r\n\r\n\r\n" + 
                    "Continue in Debug mode?", "Debug", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                if (dresult == DialogResult.No)
                {
                    debugmode = false;
                    Debug_Mode.Checked = false;
                }
            }
            else
            { 
                /* NOP */
            }
        }


        private void Debug_Mode_CheckedChanged(object sender, EventArgs e)
        {
            /* If checkbox for "Debug Mode" is checked */
            if (Debug_Mode.Checked)
            {
                debugmode = true;
            }
            else
            {
                debugmode = false;
            }
        }

        

/* ################################################################### */
/* ################################################################### */
        private int Func_I2C_ReadReg(byte addr)
        {
            bool result = false;
            byte[] i2c_data_array = new byte[0xFF];
            int status;
            string tempStr = "";

            result = PICkitS.Basic.Send_I2CRead_Cmd(SLAVE_ADDR, addr,1, ref i2c_data_array, ref tempStr);

            if (result == false)
            {
                status = -1;
                DialogResult dresult = MessageBox.Show("Error Reading from Register 0x" + Convert.ToString(addr,16) + "\r\n\r\n" + 
                    "Quit demo program?", "I2C Read Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (dresult == DialogResult.Yes)
                {
                    PICkitS.I2CM.Set_Source_Voltage(0.0);
                    Environment.Exit(0);
                }
                else PICkitS.Device.Clear_Comm_Errors();
            }
            else
                status = i2c_data_array[0];

            Func_DebugMsg("Reading register 0x" + Convert.ToString(addr, 16) + "\r\n" + 
                "I2C Read result: " + Convert.ToString(result) +
                "\r\n\r\n" + "tempStr: " + tempStr);

            return status;
        }

        private int Func_I2C_WriteReg(byte i2c_reg_addr, int data)
        {
            bool result = false;
            byte[] i2c_data_array = new byte[0xFF];
            int status;
            string tempStr = "";

            i2c_data_array[0] = Convert.ToByte(data);

            result = PICkitS.Basic.Send_I2CWrite_Cmd(SLAVE_ADDR, i2c_reg_addr,1, ref i2c_data_array, ref tempStr);

            if (result == false)
            {
                status = -1;
                //MessageBox.Show("Error Writing to Register " + regname);
                DialogResult dresult = MessageBox.Show("Error Writing to Register 0x" + Convert.ToString(i2c_reg_addr,16) + "\r\n\r\n" +
                    "Quit demo program?", "I2C Write Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (dresult == DialogResult.Yes)
                {
                    PICkitS.I2CM.Set_Source_Voltage(0.0);
                    Environment.Exit(0);
                }
				else PICkitS.Device.Clear_Comm_Errors();
            }
            else
                status = 0;

            Func_DebugMsg("Writting register 0x" + Convert.ToString(i2c_reg_addr, 16) + "\r\n" + 
                "I2C Write result: " + Convert.ToString(result) +
                "\r\n\r\n" + "tempStr: " + tempStr);

            return status;
        }
        
        private int Func_CheckStatusReg()
        {
            /* Checks for new interrupt and new data and readback respective ALS/PS data reg.
	         * Returns as an array to function call.
	         */
            int read_data;

            read_data = Func_I2C_ReadReg(PS_STATUS);

            /* KIV: Need to create some sort of error handling */
            if (read_data < 0)
                read_data= 0;

            int status = read_data & 0x12;
            //MessageBox.Show("Status Reg: " + Convert.ToString(status));

            int TriggerMode = 0;

            switch (status)
            {
                case 0:
                    /* NOP */
                    TriggerMode = 0;
                    break;
                case 2:
                    /* PS to trigger INT */
                    TriggerMode = 1;
                    break;

                case 16:
                    /* ALS to trigger INT */
                    TriggerMode = 2;
                    break;
                
                case 18:
                    /* Both ALS and PS trigger INT */
                    TriggerMode = 3;
                    break;
                
                default:
                    goto case 0;
            }

            //MessageBox.Show("TrigMode: " + Convert.ToString(TriggerMode)
            //    + " | readFuncSel: " + Convert.ToString(readFuncSel)
            //    + " | INTMode: " + Convert.ToString(INTMode));

            //Func_InterruptBoxControl(TriggerMode, readFuncSel, INTMode);

            return (read_data & 0x12);
        }

        private int Func_Read_PSStatus()
        {
            int statusdata = 0;

            statusdata = Func_I2C_ReadReg(PS_STATUS);
            
            Func_DebugMsg("PS Status: 0x" + Convert.ToString(statusdata));
            if (statusdata < 0)
            { statusdata = -1; return statusdata; }
            
            return statusdata;
        }

        private int Func_Read_ALSStatus()
        {
            int statusdata = 0;

            statusdata = Func_I2C_ReadReg(CS_STATUS);

            Func_DebugMsg("ALS Status: 0x" + Convert.ToString(statusdata));
            if (statusdata < 0)
            { statusdata = -1; return statusdata; }

            return statusdata;
        }

        private int Func_Read_ALSdata(int status)
        {
            int result = 0;
            int gain_change = 0;
            int average_status = 0;
            byte[] i2c_data_array = new byte[0xFF];
            string tempStr = "";

            int ALS_DARK_DATA = 0;
            int GREEN_CH2 = 0;
            int GREEN_RAW_DATA = 0;
            int RED_RAW_DATA = 0;
            int BLUE_RAW_DATA = 0;
            int Max_rawdata = 0;
            int dynamic_delta = 0;

            int IR_READ_DATA = 0;

           
            PICkitS.Basic.Send_I2CRead_Cmd(SLAVE_ADDR, CS_GREEN_DATA_LSB , 0x0A, ref i2c_data_array, ref tempStr);
            GREEN_CH = (i2c_data_array[1]) * 256 + i2c_data_array[0];
            IR_data = (i2c_data_array[3]) * 256 + i2c_data_array[2];
            CLEAR_CH =(i2c_data_array[5]) * 256 + i2c_data_array[4];
            RED_CH = (i2c_data_array[7]) * 256 + i2c_data_array[6];
            BLUE_CH = (i2c_data_array[9]) * 256 + i2c_data_array[8];

            //ALS_data = GREEN_CH;
            if (CLEAR_CH == 0)
            {
                IR_data = 0;
                RED_CH = 0;
                GREEN_CH = 0;
                BLUE_CH = 0;
            }
            if (GREEN_CH <5)
            {
                IR_data = 0;
                RED_CH = 0;
                GREEN_CH = 0;
                BLUE_CH = 0;
            }
            
            /*averaging function*/
            if (checkBox_ALSdelta_avg.Checked)
            {
                GREEN_RAW_DATA = GREEN_CH;
                RED_RAW_DATA = RED_CH;
                BLUE_RAW_DATA = BLUE_CH;
                ALS_K_FAC = Convert.ToInt16(numericUpDown_ALS_K_fac.Value);

                if (ALS_START_FLAG == 0)
                {
                    GREEN_CH=GREEN_RAW_DATA;
                    RED_CH = RED_RAW_DATA;
                    BLUE_CH = BLUE_RAW_DATA;
                    ALS_START_FLAG = 1;
                }
                else
                {
                    /*Averaging function*/
                    if (GREEN_CH > Max_rawdata) Max_rawdata = GREEN_CH;
                    if (RED_CH > Max_rawdata) Max_rawdata = RED_CH;
                    if (BLUE_CH > Max_rawdata) Max_rawdata = BLUE_CH;
                    if (IR_data > Max_rawdata) Max_rawdata = IR_data;

                    dynamic_delta = 5 * Max_rawdata / 100;
                    maxDELTA = 500;
                    if (dynamic_delta > maxDELTA) maxDELTA = dynamic_delta;

                    GREEN_CH =Func_averaging(GREEN_RAW_DATA, GREEN_OLD_DATA);
                    RED_CH = Func_averaging(RED_RAW_DATA, RED_OLD_DATA);
                    BLUE_CH = Func_averaging(BLUE_RAW_DATA, BLUE_OLD_DATA);
                    /*
                    ALS_DELTA = ALS_READ_DATA - ALS_OLD_DATA;
                    if (Math.Abs(ALS_DELTA) > maxDELTA)
                        ALS_data = ALS_READ_DATA;
                    else
                    {
                        ALS_DELTA_FAC = (int)Math.Round(ALS_DELTA * ALS_K_FAC * Math.Abs(ALS_DELTA) / (1000.0 + Math.Abs(ALS_DELTA) * ALS_K_FAC), 0);
                        if (Math.Abs(ALS_DELTA_FAC) > 0)
                            ALS_data = ALS_OLD_DATA + ALS_DELTA_FAC;
                        else
                        {
                            if (Math.Abs(ALS_DELTA) > 2)
                            {
                                if (ALS_DELTA > 0)
                                    ALS_data = ALS_OLD_DATA + 2;
                                else
                                    ALS_data = ALS_OLD_DATA - 2;
                            }
                            else
                            {
                                ALS_data = ALS_OLD_DATA + ALS_DELTA;
                            }
                        }
                    }
                    */
                }
                GREEN_OLD_DATA = GREEN_CH;
                RED_OLD_DATA = RED_CH;
                BLUE_OLD_DATA = BLUE_CH;

                /*IR data*/
                IR_READ_DATA = IR_data;
                ALS_K_FAC = Convert.ToInt16(numericUpDown_ALS_K_fac.Value);
                if (IR_START_FLAG == 0)
                {
                    IR_data = IR_READ_DATA;
                    IR_START_FLAG = 1;
                }
                else
                {
                    IR_DELTA = IR_READ_DATA - IR_OLD_DATA;
                    if (Math.Abs(IR_DELTA) > maxDELTA)
                        IR_data = IR_READ_DATA;
                    else
                    {
                        IR_DELTA_FAC = (int)Math.Round(IR_DELTA * ALS_K_FAC * Math.Abs(IR_DELTA) / (1000.0 + Math.Abs(IR_DELTA) * ALS_K_FAC), 0);
                        if (Math.Abs(IR_DELTA_FAC) > 0)
                            IR_data = IR_OLD_DATA + IR_DELTA_FAC;
                        else
                        {
                            if (Math.Abs(IR_DELTA) > 2)
                            {
                                if (IR_DELTA > 0)
                                    IR_data = IR_OLD_DATA + 2;
                                else
                                    IR_data = IR_OLD_DATA - 2;
                            }
                            else
                            {
                                IR_data = IR_OLD_DATA + IR_DELTA;
                            }
                        }
                    }
                }
                IR_OLD_DATA = IR_data;
            }
            /*averaging function*/

            if (checkBox_auto_averaging.Checked)
            {/*
                if (ALS_data < 50)//auto averaging control
                {
                    Func_I2C_WriteReg(ALS_AVE_FAC, 0x00);
                    average_status = 0;
                }
                else
                {
                    Func_I2C_WriteReg(ALS_AVE_FAC, 0x00);
                    average_status = 1;
                }
                */
            }
            if (checkBox_AGC.Checked)
            {
                if (GREEN_CH > 50000 || IR_data > 50000)
                {
                    if (comboBox_CS_Gain.SelectedIndex > 0)
                    {
                        comboBox_CS_Gain.SelectedIndex--;
                        Set_ALSControl(1);
                        gain_change = 1;
                    }
                }
                if(GREEN_CH < 200)
                {
                    if (comboBox_CS_Gain.SelectedIndex < 9)
                    {
                        comboBox_CS_Gain.SelectedIndex++;
                        Set_ALSControl(1);
                        gain_change = 1;
                    }
                }
            }

            if (gain_change == 0)
            {
                
                if (checkBox_dark_comp.Checked)
                {
                    
                    if(GREEN_CH < Green_dark+20) GREEN_CH=0;
                    if(RED_CH< Red_dark+20) RED_CH=0;
                    if (BLUE_CH < Blue_dark + 20) BLUE_CH = 0;
                    if(IR_data < IR_dark+20) IR_data=0;
                    if (CLEAR_CH < Clear_dark + 20) CLEAR_CH = 0;
                }

               

                //lux_data = Math.Round(GREEN_CH / gain /int_fac/als_res*0.69, 1);
                lux_data = Math.Round(GREEN_CH / gain / int_fac / als_res * 0.7, 1);

                if(lux_data < 2)
                    //lux_data = Math.Round(GREEN_CH / gain / int_fac / als_res * 0.69, 5);
                    lux_data = Math.Round(GREEN_CH / gain / int_fac / als_res * 0.7, 5);
               
                ratio = Math.Round(IR_data * 1.000 / GREEN_CH, 3);


                //CCT = Math.Round((Convert.ToDouble(BLUE_CH) / Convert.ToDouble(RED_CH)) * 4484.0 + 1638.0,0); // using simple formula from Jingyuan
                //CCT = Math.Round((Convert.ToDouble(BLUE_CH) / Convert.ToDouble(RED_CH)) * 2093.0 + 2682.0, 0); // using simple formula from Jingyuan
                //CCT = Math.Round((Convert.ToDouble(BLUE_CH) / Convert.ToDouble(RED_CH)) * 4752.0 + 1578.0, 0); // new coating
                CCT = Math.Round((Convert.ToDouble(BLUE_CH) / Convert.ToDouble(RED_CH)) * 5311.0 + 964.0, 0); // new coating July19

                CCT_XYZ = CCT;
                //Xt = 4.4875 * RED_CH - 0.5965 * GREEN_CH + 3.3890 * BLUE_CH;
                //Yt = 2.6528 * RED_CH + 0.7226 * GREEN_CH + 3.2925 * BLUE_CH;
                //Zt = 0.1316 * RED_CH - 0.9430 * GREEN_CH + 9.0755 * BLUE_CH;

                //Old LTR533 transformation matrix
                //Xt = 4.3891 * RED_CH - 1.117 * GREEN_CH + 2.9564 * BLUE_CH;
                //Yt = 2.9592 * RED_CH + 0.1697 * GREEN_CH + 2.8004 * BLUE_CH;
                //Zt = -1.3923 * RED_CH + 0.6979 * GREEN_CH + 6.4792 * BLUE_CH;

                // JY on 29Oct19
                Xt = 3.2676 * RED_CH + 1.0031 * GREEN_CH + 1.022 * BLUE_CH;
                Yt = 2.1325 * RED_CH + 1.9051 * GREEN_CH + 0.9801 * BLUE_CH;
                Zt = -1.1757 * RED_CH - 1.0319 * GREEN_CH + 8.9734 * BLUE_CH;

                xcord = Math.Round(Xt / (Xt + Yt + Zt),3);
                ycord = Math.Round(Yt / (Xt + Yt + Zt),3);

                n = (xcord - 0.332) / (ycord - 0.1858);

                CCT_XYZ = -449 * Math.Pow(n, 3) + 3525 * Math.Pow(n, 2) - 6823.3 * Math.Pow(n, 1) + 5520.33;

                textBox_ALS_data.Text = Convert.ToString(GREEN_CH);
                textBox_RED_CH.Text = Convert.ToString(RED_CH);
                textBox_BLUE_CH.Text = Convert.ToString(BLUE_CH);
                textBox_CCT_data.Text = Convert.ToString(CCT);
                textBox_CCT_x.Text = Convert.ToString(xcord);
                textBox_CCT_y.Text = Convert.ToString(ycord);
                textBox_IR_data.Text = Convert.ToString(IR_data);
                textBox_CLEAR_CH.Text = CLEAR_CH.ToString();
                textBox_LUX_data.Text = Convert.ToString(lux_data);
                textBox_Ratio.Text = Convert.ToString(ratio);
                textBox_Gain.Text = comboBox_CS_Gain.SelectedItem.ToString();
                //textBox_Gain.Text = "test";


                textBox_CCT_RED.Text = Convert.ToString(RED_CH);
                textBox_CCT_GREEN.Text = Convert.ToString(GREEN_CH);
                textBox_CCT_BLUE.Text = Convert.ToString(BLUE_CH);
                textBox_CCT_IR.Text = Convert.ToString(IR_data);
                textBox_CCT_CLEAR.Text = Convert.ToString(CLEAR_CH);
                textBox_CCT_LUX.Text=Convert.ToString(lux_data);

                textBox_CCT_CCT.Text = CCT_XYZ.ToString("#");
                textBox_CCT_CIEx.Text = xcord.ToString("0.000");
                textBox_CCT_CIEy.Text = ycord.ToString("0.000"); ;
                textBox_CCT_CIE_X.Text = Convert.ToString(Xt);
                textBox_CCT_CIE_Y.Text = Convert.ToString(Yt);
                textBox_CCT_CIE_Z.Text = Convert.ToString(Zt);
                //Normalized Yt to lux
                double normtoLux = lux_data/Yt;
                textBox_CCT_CIE_X.Text = (Xt * normtoLux).ToString("#");
                textBox_CCT_CIE_Y.Text = (Yt*normtoLux).ToString("#");
                textBox_CCT_CIE_Z.Text = (Zt * normtoLux).ToString("#");

                textBox_Execute_Status.AppendText(",R,G,B,IR,C,Lux,CCT,CCT_XYZ,x,y = " + Convert.ToString(RED_CH) + ", " + Convert.ToString(GREEN_CH) + ", " + Convert.ToString(BLUE_CH) + ", "
                    + Convert.ToString(IR_data) + ", " + Convert.ToString(CLEAR_CH) + ", " + Convert.ToString(lux_data) + ", "
                    + Convert.ToString(CCT) + ", "+ Convert.ToString(xcord) + ", " + Convert.ToString(ycord) + ", "
                    + sw.ElapsedMilliseconds + " ms,"+ comboBox_CS_Gain.SelectedIndex);
                /*averaging function logging*/
                if (checkBox_ALSdelta_avg.Checked)
                {
                    textBox_Execute_Status.AppendText(",raw, " + Convert.ToString(RED_RAW_DATA) + ", " + Convert.ToString(GREEN_RAW_DATA) + ", "
                    + Convert.ToString(BLUE_RAW_DATA) + ", " + Convert.ToString(IR_READ_DATA) + ", max_V, " + Convert.ToString(dynamic_delta)
                    + ", max_delta, " + Convert.ToString(maxDELTA));
                }
            }
            return result;
        }

        private int Func_averaging(int curr_data,int old_data)
        {
            int delta=0;
            int final_data=0;
            int delta_fac=0;

            delta = curr_data - old_data;

            if (Math.Abs(delta) > maxDELTA)
                final_data = curr_data;
            else
            {
                delta_fac = (int)Math.Round(delta * ALS_K_FAC * Math.Abs(delta) / (1000.0 + Math.Abs(delta) * ALS_K_FAC), 0);
                if (Math.Abs(delta_fac) > 0)
                    final_data = old_data + delta_fac;
                else
                {
                    if (Math.Abs(delta) > 2)
                    {
                        if (delta > 0)
                            final_data = old_data + 2;
                        else
                            final_data = old_data - 2;
                    }
                    else
                    {
                        final_data = old_data + delta;
                    }
                }
            }

            //textBox_Execute_Status.AppendText(" DF is " + Convert.ToString(delta_fac)+" ");
            return final_data;
        }

        private int Func_Read_PSdata()
        {
            int PSdata = 0;
            int PS_READ_DATA = 0;
            //bool result = false;
            byte[] i2c_data_array = new byte[0xFF];
            //int status;
            string tempStr = "";

            PICkitS.Basic.Send_I2CRead_Cmd(SLAVE_ADDR, PS_DATA_LSB, 0x02, ref i2c_data_array, ref tempStr);
           
            PSdata = (i2c_data_array[1]) * 256 + i2c_data_array[0];

            PS_SAR_data = Func_I2C_ReadReg(PS_SAR);

            if (checkBox_PSdelta_avg.Checked)
            {

                /*delta averaging function*/
                PS_K_FAC = Convert.ToInt16(numericUpDown_PS_K_fac.Value);
                PS_READ_DATA = PSdata;
                if (PS_START_FLAG == 0)
                {
                    PSdata = PS_READ_DATA;
                    PS_START_FLAG = 1;
                }
                else
                {
                    PS_DELTA = PS_READ_DATA - PS_OLD_DATA;

                    if (Math.Abs(PS_DELTA) > maxDELTA)
                        PSdata = PS_READ_DATA;
                    else
                    {
                        PS_AVG1 = PS_OLD_DATA + PS_DELTA * PS_K_FAC * Math.Abs(PS_DELTA) / (1000 + Math.Abs(PS_DELTA) * PS_K_FAC)+0.1;
                        PS_AVG2 = (PS_OLD_DATA + PS_READ_DATA) / 2+0.1;

                        if (Math.Abs(PS_READ_DATA - PS_AVG1) < Math.Abs(PS_READ_DATA - PS_AVG2))
                            PSdata = (int) Math.Round(PS_AVG1,0);
                        else
                            PSdata = (int)Math.Round(PS_AVG2,0);
                    }
                }
                PS_OLD_DATA = PSdata;
                textBox_Execute_Status.AppendText(" PS raw: " + Convert.ToString(PS_READ_DATA));
            }
            textBox_Execute_Status.AppendText(", PS Count: " + Convert.ToString(PSdata));

            return PSdata;
        }

        private int[,] Func_ReadPixel(Bitmap pic, string colourchannel)
        {
            int[,] PixColourVal = new int[pic.Width, pic.Height];
            int x, y;

            for (x = 0; x < pic.Width; x++)
            {
                for (y = 0; y < pic.Height; y++)
                {
                    Color pixColour = pic.GetPixel(x, y);
                    if (colourchannel == "Red")
                    {
                        PixColourVal[x, y] = pixColour.R;
                    }
                    else if (colourchannel == "Green")
                    {
                        PixColourVal[x, y] = pixColour.G;
                    }
                    else if (colourchannel == "Blue")
                    {
                        PixColourVal[x, y] = pixColour.B;
                    }
                }
            }
            return PixColourVal;
        }

        private int Func_ScaleCalc(double lux)
        {
            double[] LuxArray = { 10.0, 30.0, 50.0, 80.0, 130.0, 250.0, 500.0, 1000.0, 3000.0, 5000.0, 10000.0, 30000.0, 50000.0, 60000.0 };
            int[] ScaleArray = { -250, -200, -128, -100, -70, -30, 1, 8, 15, 30, 50, 100, 150, 200, 250 };

            int scale = 0;
            double remainder = 0;
            int length = LuxArray.Length;

            for (int i = 0; i < length; ++i)
            {
                remainder = lux - LuxArray[i];
                //textBox_Execute_Status.AppendText("\r\nRemainder[" + Convert.ToString(i) + "] = " + Convert.ToString(remainder));

                if (remainder < 0)
                {
                    scale = ScaleArray[i];
                    break;
                }
            }

            //textBox_Execute_Status.AppendText("\r\nscale = " + Convert.ToString(scale));
            return scale;
        }

        private int[,] Func_ScalePixel(int[,] input_pix, int width, int height, int factor)
        {
            int[,] NewPixVal = new int[width, height];
            int NewVal;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if ((input_pix[x, y] + factor) > 255)
                        NewVal = 255;
                    else if ((input_pix[x, y] + factor) < 0)
                        NewVal = 0;
                    else
                        NewVal = input_pix[x, y] + factor;

                    NewPixVal[x, y] = NewVal;
                }
            }

            return NewPixVal;
        }

        private Bitmap Func_ZoomImage(Bitmap inputimage, int[,] red, int[,] green, int[,] blue, int zoomfactor)
        {
            Bitmap outputimage = inputimage;

            int HalfWidth = inputimage.Width / 2;
            int HalfHeight = inputimage.Height / 2;

            for (int x = (HalfWidth - (HalfWidth / zoomfactor)); x < (HalfWidth + (HalfWidth / zoomfactor)); x++)
            {
                for (int y = (HalfHeight - (HalfHeight / zoomfactor)); y < (HalfHeight + (HalfHeight / zoomfactor)); y++)
                {

                    Color colour = Color.FromArgb(red[x, y], green[x, y], blue[x, y]);

                    for (int i = 0; i < zoomfactor; i++)
                    {
                        for (int j = 0; j < zoomfactor; j++)
                        {
                            if (x < HalfWidth)
                            {
                                if (y < HalfHeight)
                                {
                                    outputimage.SetPixel((((x - HalfWidth) * zoomfactor) + HalfWidth + i), (((y - HalfHeight) * zoomfactor) + HalfHeight + j), colour);
                                }
                                else
                                {
                                    outputimage.SetPixel((((x - HalfWidth) * zoomfactor) + HalfWidth + i), (((y - HalfHeight) * zoomfactor) + HalfHeight - j), colour);
                                }
                            }
                            else
                            {
                                if (y < HalfHeight)
                                {
                                    outputimage.SetPixel((((x - HalfWidth) * zoomfactor) + HalfWidth - i), (((y - HalfHeight) * zoomfactor) + HalfHeight + j), colour);
                                }
                                else
                                {
                                    outputimage.SetPixel((((x - HalfWidth) * zoomfactor) + HalfWidth - i), (((y - HalfHeight) * zoomfactor) + HalfHeight - j), colour);
                                }
                            }
                        }
                    }
                }
            }

            return outputimage;
        }

        private void Func_Display_ProximityVal(double proxval,int status)
        {
            currtime = DateTime.Now.ToLongTimeString();
            //MessageBox.Show(Convert.ToString(proxval));

            if (!checkBox_PS.Checked)
            {
                textBox_PSData.Text = "";
               // textBox_plot_pscount.Text = textBox_PSData.Text;
            }
            else
            {
                textBox_PSData.Text = Convert.ToString(proxval);
               // textBox_plot_pscount.Text = textBox_PSData.Text;
                textBox_PS_SAR.Text = Convert.ToString(PS_SAR_data);
                
                
//                textBox_Execute_Status.AppendText("PS Count: " + Convert.ToString(proxval));
            }
        }

        private void Func_Display_Ambient_Light_data()
        {
            currtime = DateTime.Now.ToLongTimeString();

            if(!checkBox_ALS.Checked)
            {

            }
            else
            {
                textBox_Execute_Status.AppendText(", ALS , IR, Lux = " + Convert.ToString(ALS_data) + ", " + Convert.ToString(IR_data) + ", " + Convert.ToString(lux_data));
            }
        }
       
      
/* ################################################################### */
/* ################################################################### */

        private void Set_DeviceSupplyVoltage(int readVoltSel)
        {
            /* Variables initialisation */
            double VoltVal;

            switch (readVoltSel)
            {
                case 0:
                    VoltVal = 3.0;
                    break;
                case 1:
                    VoltVal = 3.1;
                    break;
                case 2:
                    VoltVal = 3.2;
                    break;
                case 3:
                    VoltVal = 3.3;
                    break;
                case 4:
                    VoltVal = 3.4;
                    break;
                case 5:
                    VoltVal = 3.5;
                    break;
                case 6:
                    VoltVal = 3.6;
                    break;
                default:
                    goto case 3;
            }

            /* Pass voltage value for PICkitS to set */
            PICkitS.I2CM.Set_Source_Voltage(VoltVal);
            Thread.Sleep(500);

            Func_DebugMsg("VoltVal = " + Convert.ToString(VoltVal));
        }


        private void Set_DeviceI2CFreq(int readBitSel)
        {
            /* Variables initialisation */
            double BitrateVal;

            switch (readBitSel)
            {
                case 0:
                    BitrateVal = 400.0;
                    break;
                case 1:
                    BitrateVal = 100.0;
                    break;
                default:
                    goto case 0;
            }

            /* Pass bit rate for PICkitS to set */
            PICkitS.I2CM.Set_I2C_Bit_Rate(BitrateVal);

            Func_DebugMsg("BitrateVal = " + Convert.ToString(BitrateVal));
        }

        private void compute_CS_time()
        {
            int CS_Int_time_scale = comboBox_CS_Int_Time_Scale.SelectedIndex;
            int CS_MR_time_scale = comboBox_CS_MRR_Scale.SelectedIndex;
            int CS_MRR_step = (int)numericUpDown_CS_MR_steps.Value;
            int CS_Int_time_step = (int)numericUpDown_CS_INT_TIME_step.Value;

            switch (CS_Int_time_scale)
            {
                case 0: Int_time = 0.78 * CS_Int_time_step; break;
                case 1: Int_time = 0.39 * CS_Int_time_step; break;
                case 2: Int_time = 0.2 * CS_Int_time_step; break;
                case 3: Int_time = 0.2 * CS_Int_time_step; break;

            }
            int_fac = Int_time/100;

            switch (CS_MR_time_scale)
            {
                case 0: Meas_rate = 0.2 * CS_MRR_step; break;
                case 1: Meas_rate = 0.39 * CS_MRR_step; break;
                case 2: Meas_rate = 0.78 * CS_MRR_step; break;
                case 3: Meas_rate = 1.56 * CS_MRR_step; break;

            }
            textBox_CS_Int_Time.Text = Int_time.ToString();
            textBox_CS_Meas_Rate.Text = Meas_rate.ToString();

        }

        private void Set_ALSControl(int enable)
        {
            int dummyresult = -1;
            int write_data;
            int gain_sel = comboBox_CS_Gain.SelectedIndex;
            
            

            switch(gain_sel)
            {
                case 0: gain = 1; break;
                case 1: gain = 2; break;
                case 2: gain = 4; break;
                case 3: gain = 8; break;
                case 4: gain = 16; break;
                case 5: gain = 32; break;
                case 6: gain = 64; break;
                case 7: gain = 128; break;
                case 8: gain = 256; break;
                case 9: gain = 512; break;
               
            }
            compute_CS_time();


            if (enable == 1)
            {
                write_data = comboBox_CS_Gain.SelectedIndex*4 + comboBox_CS_Mode.SelectedIndex*2+ 1;
                //write_data = 0x01;
            }
            else
            {
                write_data = comboBox_CS_Gain.SelectedIndex * 4 + 0;
                //write_data = 0x00;
            }
            dummyresult = Func_I2C_WriteReg(CS_CONTR, write_data);
        }

       
       


       
        private void Set_PSControl(int enable)
        {
            int dummyresult = -1;
            int write_data=0;
            int efuse_enable = comboBox_PS_Efuse_Enable.SelectedIndex;
            int PS_res = comboBox_PS_Resolution.SelectedIndex;
            int off_enable = comboBox_PS_Offset_enable.SelectedIndex;
            int FTN_NTF_enable = comboBox_PS_FTN_NTF_en.SelectedIndex;

            write_data = write_data + efuse_enable * 64;
            write_data = write_data + PS_res * 16;
            write_data = write_data + off_enable * 8;
            write_data = write_data + FTN_NTF_enable * 4;

            if (enable == 1)
            {
                write_data = write_data + 0x02;
                //write_data = 0x02;//0x10
            }
            else
            {
                write_data = write_data + 0x00;//0x10
            }
                

            dummyresult = Func_I2C_WriteReg(PS_CONTR, write_data);
        }

        private void Set_Engg_control()
        {
            int dummyresult = -1;
            int write_data;

            //write_data = 0x00;
            write_data = 192;
            //MessageBox.Show("i2c_data_array_PSLED: 0x" + Convert.ToString(i2c_data_array_PSLED, 16));

            dummyresult = Func_I2C_WriteReg(0xED,192);
            dummyresult = Func_I2C_WriteReg(0xEE, 128);
        }
    
        private void Set_PS_interrupt()
        {
            int dummyresult = -1;
            int write_data;

            //write_data = 0x29;
            write_data = 0x81 + (comboBox_Int_Pin_polarity.SelectedIndex << 2);
            //MessageBox.Show("i2c_data_array_PSLED: 0x" + Convert.ToString(i2c_data_array_PSLED, 16));

            dummyresult = Func_I2C_WriteReg(INTERRUPT, write_data);
        }

        private void Set_PSLED(int pulse_duty, int pulse_width, int led_current)
        {
            int dummyresult = -1;
            int write_data;

            //write_data = 0x00;
            write_data = 0x60 + (pulse_width << 3) + led_current;
            //MessageBox.Show("i2c_data_array_PSLED: 0x" + Convert.ToString(i2c_data_array_PSLED, 16));
            dummyresult = Func_I2C_WriteReg(PS_LED, write_data);
        }

        private void Set_PSLEDPulseCount(int PS_averaging, int led_pulse)
        {
            int dummyresult = -1;
            int write_data;

            //write_data = 0x00;
            write_data = (PS_averaging<<6) + led_pulse;
            //MessageBox.Show("i2c_data_array_PSLEDPulseCnt: 0x" + Convert.ToString(i2c_data_array_PSLEDPulseCnt, 16));

            dummyresult = Func_I2C_WriteReg(PS_N_PULSES, write_data);
        }

        private void Set_PSMeasRate(int readPSMeasRepRate)
        {
            int dummyresult = -1;
            int write_data;

            //write_data = 0x00;
            write_data = 0x0+readPSMeasRepRate;
            //MessageBox.Show("i2c_data_array_PSMeasRate: 0x" + Convert.ToString(i2c_data_array_PSMeasRate, 16));

            dummyresult = Func_I2C_WriteReg(PS_MEAS_RATE, write_data);
        }

        private void pic_CCT_Click(object sender, EventArgs e)
        {

        }

        private void Set_PSThreshold(int thres_low, int thres_hi)
        {
            int result = -1;
            int write_data;
            

            /* High Threshold Values
             * Write to reg and check if error
             */ 
            write_data = thres_hi & 255;
            result = Func_I2C_WriteReg(PS_THRES_HIGH_LSB, write_data);
            Func_DebugMsg("PSHighThreshold_Lo: result = " + Convert.ToString(result));
            if (result < 0)
                goto fail_msg;

            /* Write to reg and check if error */
            write_data = (thres_hi / 256) & 7;
            result = Func_I2C_WriteReg(PS_THRES_HIGH_MSB, write_data);
            Func_DebugMsg("PSHighThreshold_Hi: result = " + Convert.ToString(result));
            if (result < 0)
                goto fail_msg;


            /* Low Threshold Values
             * Write to reg and check if error
             */ 
            write_data =thres_low & 255;
            result = Func_I2C_WriteReg(PS_THRES_LOW_LSB, write_data);
            Func_DebugMsg("PSLowThreshold_Lo: result = " + Convert.ToString(result));
            if (result < 0)
                goto fail_msg;

            /* Write to reg and check if error */
            write_data = (thres_low / 256) & 7;
            result = Func_I2C_WriteReg(PS_THRES_LOW_MSB, write_data);
            Func_DebugMsg("PSLowThreshold_Hi: result = " + Convert.ToString(result));
            if (result < 0)
                goto fail_msg;


            /* KIV: come up with better failure handling */
            fail_msg:
            if (result < 0)
                MessageBox.Show("PS Threshold setup FAILED...");
        }

        private void button_calibrateRGB_Click(object sender, EventArgs e)
        {
            CalibrateRGB();
        }

        private void CalibrateRGB()
        {
            //int IR_CH = 0;
            //int RED_CH = 0;
            //int GREEN_CH = 0;
            //int BLUE_CH = 0;

            double LUX = 0.0;
            double ALS_Gain = 1, ALS_Resolution = 1;

            // Setting Supply Voltage
            //Set_DeviceSupplyVoltage(comboBox_SupplyVoltage.SelectedIndex);

            // Setting I2C Bitrate
            //Set_DeviceI2CFreq(comboBox_I2CBitRate.SelectedIndex);

            Init_DeviceALS();
            Init_DevicePS();


            //Func_I2C_WriteReg(ANALOG_TEST, 0x04);//0x00-LED, 0x04-VSCEL



            if (checkBox_ALS.Checked)
            {

                Set_ALSControl(1);
            }
            if (checkBox_PS.Checked)
            {

                Set_PSControl( 1);
            }

            Thread.Sleep(500);

           

            Func_Read_ALSdata(1);
            //Func_CalculateLUX(true, ALS_Resolution, ALS_Gain, out IR_CH, out RED_CH, out GREEN_CH, out BLUE_CH, out LUX);
            

            R_factor = (double)(Convert.ToDouble(GREEN_CH) *1.000/ Convert.ToDouble(RED_CH));
            G_factor = (double)(Convert.ToDouble(GREEN_CH) *1.000/ Convert.ToDouble(GREEN_CH));
            B_factor = (double)(Convert.ToDouble(GREEN_CH) *1.000/ Convert.ToDouble(BLUE_CH));

            //R_factor = (Convert.ToDouble(RED_CH));
            //G_factor = (Convert.ToDouble(GREEN_CH));
            //B_factor = (Convert.ToDouble(BLUE_CH));
            //R_factor = 1.222;

            textBox_Cal_R_factor.Text = R_factor.ToString("F");
            textBox_Cal_G_factor.Text = G_factor.ToString("F");
            textBox_Cal_B_factor.Text = B_factor.ToString("F");


        }

        private void button_Clear_log_Click(object sender, EventArgs e)
        {
            textBox_Execute_Status.Text = "";
        }

        private void numericUpDown_CS_INT_TIME_step_ValueChanged(object sender, EventArgs e)
        {
            compute_CS_time();

        }

        private void numericUpDown_CS_MR_steps_ValueChanged(object sender, EventArgs e)
        {
            compute_CS_time();

        }

        private void button_Calibrate_Dark_Click(object sender, EventArgs e)
        {
            int i;
            /*initialise dark count to 0*/
            Blue_dark = 0;
            Green_dark = 0;
            Red_dark = 0;
            IR_dark = 0;
            Clear_dark = 0;
            /**/

            Init_DeviceALS();
            Set_ALSControl(1);
            button_Calibrate_Dark.Enabled=false;
            checkBox_dark_comp.Checked = false;
            for (i = 0; i < 10; i++)
            {
                Thread.Sleep(500);
                Func_Read_ALSdata(1);
                if (BLUE_CH > Blue_dark) Blue_dark = BLUE_CH;
                if (GREEN_CH > Green_dark) Green_dark = GREEN_CH;
                if (RED_CH > Red_dark) Red_dark = RED_CH;
                if (IR_data > IR_dark) IR_dark = IR_data;
                if (CLEAR_CH > Clear_dark) Clear_dark = CLEAR_CH;

                textBox_Blue_dark.Text = Blue_dark.ToString();
                textBox_Green_dark.Text = Green_dark.ToString();
                textBox_Red_dark.Text = Red_dark.ToString();
                textBox_IR_dark.Text = IR_dark.ToString();
                textBox_Clear_dark.Text = Clear_dark.ToString();
            }
            Set_ALSControl(0);
            button_Calibrate_Dark.Enabled = true;
            checkBox_dark_comp.Checked = true;
        }

        private void button_unlock_Click(object sender, EventArgs e)
        {
            if(textBox_Password.Text=="apps")
            {
                groupBox_ALS_function.Visible = true;
                groupBox_PS_function.Visible = true;
            }
            else
            {
                //groupBox_ALS_function.Visible = false;
                //groupBox_PS_function.Visible = false;
                MessageBox.Show("Wrong password! Please enter again!");
            }
        }

        private void button_Lock_Click(object sender, EventArgs e)
        {
            groupBox_ALS_function.Visible = false;
            groupBox_PS_function.Visible = false;
        }

        private void button_Connect_Click(object sender, EventArgs e)
        {
            if (PICkitS.Basic.Initialize_PICkitSerial())
            {
                PICkitS.I2CM.Configure_PICkitSerial_For_I2CMaster();
                PICkitS.I2CM.Set_Source_Voltage(3.0);
                Set_DeviceI2CFreq(0);//400Khz
                this.toolStripStatusLabel1.Text = "USB Device is attached";
                tabControl1.SelectedIndex = 0;
                /* Disable Run and Stop buttons on next tab */
                Execute_Run.Enabled = true;
                Execute_Stop.Enabled = false;
                Debug_Mode.Visible = false;
                Exit.Enabled = true;
            }
            else
            {
                this.toolStripStatusLabel1.Text = "USB Device is detached";

                Execute_Run.Enabled = false;
                Execute_Stop.Enabled = false;
                Exit.Enabled = true;
            }
        }

        private void Set_PSOffset(int readPSOffset)
        {
            int result = -1;
            int write_data;

            
            
            /* PS Offset value
             * Write to reg and check if error
             */
               write_data = readPSOffset & 255;
            result = Func_I2C_WriteReg(PXTALK_LSB, write_data);
            Func_DebugMsg("PSHighThreshold_Lo: result = " + Convert.ToString(result));
            if (result < 0)
                goto fail_msg;

            /* Write to reg and check if error */
            write_data = readPSOffset / 256 ;
            result = Func_I2C_WriteReg(PXTALK_MSB, write_data);
            Func_DebugMsg("PSHighThreshold_Hi: result = " + Convert.ToString(result));
            if (result < 0)
                goto fail_msg;


            /* KIV: come up with better failure handling */
        fail_msg:
            if (result < 0)
                MessageBox.Show("PS OFfset setup FAILED...");
        }

        private void checkBox_sunflower_CheckedChanged(object sender, EventArgs e)
        {
            /*
            if (checkBox_sunflower.Checked)
            {
                sunflower_effect = true;
            }
            else
            {
                sunflower_effect = false;
            }
            */
        }

        private void checkBox_datalog_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_datalog.Checked)
            {
                datalog_to_file = true;

                newfolder = CreateFolder();
                if (newfolder != "")
                {
                    newfile = CreateFile(newfolder);
                    textBox_Execute_Status.AppendText("\r\n\r\nDatalog Directory : " + newfolder + "\r\n" +
                        "Datalog Filename : " + newfile);

                }
                else
                {
                    MessageBox.Show("<< ERROR >> : Unable to create datalog folder !!", "ERROR", MessageBoxButtons.OK);
                    Environment.Exit(0);
                }
            }
            else
            {
                datalog_to_file = false;
            }
        }

        private void button_write_addr_Click(object sender, EventArgs e)
        {
            byte waddr,wdata;

            waddr = Convert.ToByte(textBox_W_addr.Text, 16);
            wdata = Convert.ToByte(textBox_W_data.Text, 16);
            Func_I2C_WriteReg(waddr,wdata);
        }

       

        private void button_read_addr_Click(object sender, EventArgs e)
        {
            int result = 0;
            int i=0;

            byte[] i2c_data_array = new byte[0xFF];
            //int status;
            string tempStr = "";
            byte readsize;
            byte startaddr;

            startaddr = Convert.ToByte(textBox_R_addr.Text, 16);
            readsize = (byte)numericUpDown_readsize.Value;

            PICkitS.Basic.Send_I2CRead_Cmd(SLAVE_ADDR, startaddr, readsize, ref i2c_data_array, ref tempStr);
            for(i=0;i<readsize; i++)
            {
                textBox_debug_box.AppendText("\r\nAddr 0x" + Convert.ToString(startaddr + i, 16) + "= 0x" + Convert.ToString(i2c_data_array[i], 16));
            }
        }

        private string CreateFolder()
        {
            // Define where to create new folder
            string newPath = DATALOGDIR;

            try
            {
                // Create the folder
                System.IO.Directory.CreateDirectory(newPath);
                return (newPath);
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                return ("");
            }
        }

        private string CreateFile(string newPath)
        {
            if (newPath != "")
            {
                // create file with date/time as filename
                string Filename = string.Format(TEXTFILENAME, DateTime.Now);

                // Combine the new file name with the path
                string newFilePath = newPath + "\\" + Filename;

                // Create the file
                FileStream filecreate = File.Create(newFilePath);
                filecreate.Close();

                return (Filename);
            }
            else
            {
                return ("");
            }
        }

        
        private void WriteFile(string filepath, string datalogstr)
        {
            // write to file
            FileStream fileops = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.Write);
            fileops.Close();
            StreamWriter filewrite = new StreamWriter(filepath, true, Encoding.ASCII);
            filewrite.Write(datalogstr);
            filewrite.Close();
        }


        private void pic_cie1931_Paint(object sender, PaintEventArgs e)
        {
            //This is for x,y
            try
            {

               // Invalidate();
                Graphics g = e.Graphics;
                Point p = new Point();
                Point p2 = new Point();
                int i = 1;


                myPts.Clear();

                xcie = (int)(18 + (xcord * 293.33));
                ycie = (int)(268 - (ycord * 293.33));
                p.X = xcie;
                p.Y = ycie;


                g.FillEllipse(Brushes.Crimson, p.X, p.Y, 10, 10);
                //g.DrawRectangle(Pens.Black, x, y, (x + 1), y+1);
                //g.DrawLine(pen_din2, myPts[i], myPts[i - 1]);


            }
            catch (Exception ee)
            {
                //MessageBox.Show(ee.Message, "Error Msg On_paint CY5 routine");
                MessageBox.Show(ee.Source, ee.Message);
            }
        }




        private void pic_cct_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Invalidate();
                Graphics g = e.Graphics;
                Point p = new Point();
                Point p2 = new Point();
                int i = 1;


                myPts.Clear();

                ycct = 198 - (int)((CCT_XYZ - 1000) * 0.0165);
                //ycct = 198 - (int)((CCT - 1000) * 0.0165);
                p.X = xcct;
                p.Y = ycct;
                p2.X = xcct + 55;
                p2.Y = ycct;
                myPts.Add(p);
                myPts.Add(p2);

                g.DrawLine(pen_din2, myPts[i], myPts[i - 1]);


            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Source, ee.Message);
            }
        }

        //(Nelson-test)>
        private void CreateGraph_als(ZedGraphControl zgc, byte numData_)
        {
            GraphPane myPane = zgc.GraphPane;
            LineItem myCurve1, myCurve2, myCurve3;
            double minY = 0.0;

            myPane.XAxis.Scale.Max = maxX;
            myPane.XAxis.Scale.Min = minX;
            myPane.Title.Text = "ALS Count vs Time";
            myPane.YAxis.Scale.Min = minY;

            myPane.XAxis.Title.Text = "Time(s)";
            myPane.YAxis.Title.Text = "Count";

            myPane.CurveList.Clear();
            if (numData_ == 1)
            {
                myPane.YAxis.Scale.Max = maxScale_als(1);
                myCurve1 = myPane.AddCurve(plotTitle1, m_pointsList1_als, Color.Red, SymbolType.None);
            }
            else if (numData_ == 2)
            {
                myPane.YAxis.Scale.Max = maxScale_als(2);
                myCurve1 = myPane.AddCurve(plotTitle1, m_pointsList1_als, Color.Red, SymbolType.None);
                myCurve1 = myPane.AddCurve(plotTitle2, m_pointsList2_als, Color.Blue, SymbolType.None);
            }
            else if (numData_ == 3)
            {
                myPane.YAxis.Scale.Max = maxScale_als(3);
                myCurve1 = myPane.AddCurve(plotTitle1, m_pointsList1_als, Color.Red, SymbolType.None);
                myCurve2 = myPane.AddCurve(plotTitle2, m_pointsList2_als, Color.Blue, SymbolType.None);
                myCurve3 = myPane.AddCurve(plotTitle3, m_pointsList2_als, Color.Green, SymbolType.None);
            }

            myPane.Chart.Fill = new Fill(Color.Gray);
            zgc.AxisChange();
            zgc.Refresh();
        }

        private double maxScale_als(byte numOfData)
        {
            double genData = 0;

            if (numOfData == 1)
            {
                genData = _y1_als;
            }
            else if (numOfData == 2)
            {
                if (_y1_als >= _y2_als)
                {
                    genData = _y1_als;
                }
                else if (_y2_als > _y1_als)
                {
                    genData = _y2_als;
                }
            }
            else if (numOfData == 3)
            {
                if ((_y1_als >= _y2_als) && (_y1_als >= _y3_als))
                {
                    genData = _y1_als;
                }
                else if ((_y2_als >= _y1_als) && (_y2_als >= _y3_als))
                {
                    genData = _y2_als;
                }
                else if ((_y3_als >= _y1_als) && (_y3_als >= _y2_als))
                {
                    genData = _y3_als;
                }
            }
            genData += 500.0;
            return genData;
        }

        private void CreateGraph_ps(ZedGraphControl zgc)
        {
            GraphPane myPane = zgc.GraphPane;
            LineItem myCurve1,myCurve2;
            double minY = 0.0;

            myPane.XAxis.Scale.Max = maxX;
            myPane.XAxis.Scale.Min = minX;
            myPane.Title.Text = "Lux/Counts vs Time";
            myPane.YAxis.Scale.Min = minY;

            myPane.XAxis.Title.Text = "Time(s)";
            myPane.YAxis.Title.Text = "Count";

            myPane.CurveList.Clear();
            myPane.YAxis.Scale.Max = maxScale_ps(1);
            myCurve1 = myPane.AddCurve(plotTitle_ps, m_pointsList1_ps, Color.Red, SymbolType.None);
            myCurve2 = myPane.AddCurve(plotTitle_als, m_pointsList1_als, Color.Yellow, SymbolType.None);

            myPane.Chart.Fill = new Fill(Color.Gray);
            zgc.AxisChange();
            zgc.Refresh();
        }

        private double maxScale_ps(byte numOfData)
        {
            double genData = 0;

            genData = _y1_ps + 500.0;
            return genData;
        }

        private void button_clearplots_Click(object sender, EventArgs e)
        {
            /* ZedGraph */
            //ZedGraphControl zgc_als = zedgraph_als;
            ZedGraphControl zgc_ps = zedgraph_ps;

            //GraphPane alsPane = zgc_als.GraphPane;
            GraphPane psPane = zgc_ps.GraphPane;

            /* Reset X axis */
            minX = 0.0;
            maxX = 9.0;

            /* Clear ALS Curve */
            /*
            //alsPane.CurveList.Clear();
           // alsPane.Chart.Fill = new Fill(Color.White);
           // alsPane.XAxis.Scale.Min = 0.0;
            m_pointsList1_als.Clear();
            m_pointsList2_als.Clear();
            _x_als = 0.0;
            _y1_als = 0.0;
            _y2_als = 0.0;
            _y3_als = 0.0;
            //zgc_als.AxisChange();
            //zgc_als.Refresh();
            */
            /* Clear PS Curve */
            psPane.CurveList.Clear();
            psPane.Chart.Fill = new Fill(Color.White);
            psPane.XAxis.Scale.Min = 0.0;
            m_pointsList1_ps.Clear();
            _x_ps = 0.0;
            _y1_ps = 0.0;
            zgc_ps.AxisChange();
            zgc_ps.Refresh();
        }
        //(Nelson-test)<

        private double FindMaxRGB(double R, double G, double B)
        {
            double Max = 0;

            if (G >= R && G >= B)
                Max = G;
            else if (R > G && R > B)
                Max = R;
            else if (B > G && B > R)
                Max = B;

            return Max;
        }



    } /* End of LTR2594_PICkitS_Demo */
} /* End of LTR2594_PICkitS_Demo namespace */
