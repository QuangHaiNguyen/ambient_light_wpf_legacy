using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Reflection;
using System.Timers;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Interop;


namespace AmbientLight
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int num_of_width_segment = 8;
        const int num_of_height_segment = 8;
        int width_segment;
        int height_segment;
        int screen_width;
        int screen_height;
        bool serial_ack = true;

        Bitmap screenBmp = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);

        System.Windows.Shapes.Rectangle[] arrayRectangle = new System.Windows.Shapes.Rectangle[28];
        List<string> portList = new List<string>();

        int[] norhtLED_Red = new int[8];
        int[] norhtLED_Green = new int[8];
        int[] norhtLED_Blue = new int[8];


        int[] westLED_Red = new int[7];
        int[] westLED_Green = new int[7];
        int[] westLED_Blue = new int[7];

    
        int[] eastLED_Red = new int[7];
        int[] eastLED_Green = new int[7];
        int[] eastLED_Blue = new int[7];

        int[] lookUpTableWest = new int[7] {15, 16, 17, 18, 19, 20, 21 };
        int[] lookUpTableEast = new int[7] {6, 5, 4, 3, 2, 1, 0 };
        int[] lookUpTableNorth = new int[8] { 14, 13, 12, 11, 10, 9, 8, 7 };

        SerialPort comPort = new SerialPort();
        bool connect = false;
        bool started = false;
        Timer timer = new Timer();
        delegate void UpdateTextHandler(string updatedText);
        delegate void UpdateColorHandler(int index, byte r, byte g, byte b);

        string selectedComport;

        public MainWindow()
        {
            InitializeComponent();

            screen_height = (int)SystemParameters.PrimaryScreenHeight;
            screen_width = (int)SystemParameters.PrimaryScreenWidth;
            InfoTextBox.AppendText("Screen Width: " + screen_width.ToString() + "\n");
            InfoTextBox.AppendText("Screen Height: " + screen_height.ToString() + "\n");

            width_segment = screen_width / num_of_width_segment;
            InfoTextBox.AppendText("Segment width: " + width_segment.ToString() + "\n");

            height_segment = screen_height / num_of_height_segment;
            InfoTextBox.AppendText("Segment height: " + height_segment.ToString() + "\n");


            ReturnPortName();

            timer.Interval = 10;
            timer.Elapsed += new ElapsedEventHandler(TimerElapsed);

            startButton.IsEnabled = false;

            MonkeyJob();
        }

        //orientation: 1 west
        //             2 east
        //             0 north
        void CalculateAverageColor(int index, int orientation, int[] arrayRed, int[] arrayGreen, int[] arrayBlue, Bitmap bitmap )
        {
            System.Drawing.Color color = new System.Drawing.Color();

            int segment_area = width_segment * height_segment;

            arrayRed[index] = 0;
            arrayGreen[index] = 0;
            arrayBlue[index] = 0;

            for (int i = 0; i < height_segment; i++)
            {
                for (int j = 0; j < width_segment; j++)
                {
                    if (orientation == 1)
                    {
                        color = bitmap.GetPixel(j, i + (index * height_segment));
                    }
                    else if (orientation == 2)
                    {
                        color = bitmap.GetPixel(screen_width - width_segment + j, i + (index * height_segment));
                    }
                    else
                    {
                        color = bitmap.GetPixel(index * width_segment + j, i);
                    }
                    arrayRed[index] += color.R;
                    arrayGreen[index] += color.G;
                    arrayBlue[index] += color.B;

                }
            }

            arrayRed[index] /= (segment_area);
            arrayGreen[index] /= (segment_area);
            arrayBlue[index] /= (segment_area);
        }



        void MonkeyJob()
        {
            arrayRectangle[0] = r0;
            arrayRectangle[1] = r1;
            arrayRectangle[2] = r2;
            arrayRectangle[3] = r3;
            arrayRectangle[4] = r4;
            arrayRectangle[5] = r5;
            arrayRectangle[6] = r6;
            arrayRectangle[7] = r7;
            arrayRectangle[8] = r8;
            arrayRectangle[9] = r9;
            arrayRectangle[10] = r10;
            arrayRectangle[11] = r11;
            arrayRectangle[12] = r12;
            arrayRectangle[13] = r13;
            arrayRectangle[14] = r14;
            arrayRectangle[15] = r15;
            arrayRectangle[16] = r16;
            arrayRectangle[17] = r17;
            arrayRectangle[18] = r18;
            arrayRectangle[19] = r19;
            arrayRectangle[20] = r20;
            arrayRectangle[21] = r21;
            arrayRectangle[22] = r22;
            arrayRectangle[23] = r23;
            arrayRectangle[24] = r24;
            arrayRectangle[25] = r25;
            arrayRectangle[26] = r26;
            arrayRectangle[27] = r27;
        }
        /*
         * 1: north
         * 2: south
         * 3: west
         * 4: east
         */
        void ColorDataSerialSend(int index, int[] R_Array, int[] G_Array, int[] B_Array, int direction)
        {
            string sentString;
            int ledPosition = index;
            switch (direction)
            {
                case 1:
                    ledPosition = lookUpTableNorth[index];
                    break;

                case 3:
                    ledPosition = lookUpTableWest[index];
                    break;

                case 4:
                    ledPosition = lookUpTableEast[index];
                    break;
            }
            sentString = "A" + ledPosition.ToString() + "r" + R_Array[index].ToString() +
                                                    "g" + G_Array[index].ToString() +
                                                    "b" + B_Array[index].ToString() + "E";

            //InfoTextBox.Dispatcher.Invoke(DispatcherPriority.Send, new UpdateTextHandler(UpdateInfoTextBox), sentString);

            try
            {
                comPort.Write(sentString);

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        void TimerElapsed(object sender, EventArgs e)
        {
            if(serial_ack)
            {
                RetrieveColorData();

            }
                
        }

        void UpdateInfoTextBox(string updatedText)
        {
            InfoTextBox.AppendText(updatedText);
            InfoTextBox.ScrollToEnd();

        }

        void UpdateColor(int index, byte r, byte g, byte b)
        {
            arrayRectangle[index].Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));

        }


        private void refreshComPort_Click(object sender, RoutedEventArgs e)
        {
            ReturnPortName();
        }

        private void ReturnPortName()
        {

            portList.Clear();
            portList = SerialPort.GetPortNames().ToList();
            comboBoxComPort.ItemsSource = portList;

        }

        private void comboBoxComPort_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBoxComPort = sender as ComboBox;
            comboBoxComPort.ItemsSource = portList;
            comboBoxComPort.SelectedIndex = 0;
        }

        private void comboBoxComPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedComport = comboBoxComPort.SelectedItem as string;
            InfoTextBox.AppendText(selectedComport + "\r\n");
            InfoTextBox.ScrollToEnd();
        }

        private void connectButton_Click(object sender, RoutedEventArgs e)
        {

            if (connect)
            {

                DisconnectToSerialPort();
            }
            else
            {
                String port = selectedComport;
                int baudrate = 115200;
                int databits = 8;
                Parity parity = (Parity)Enum.Parse(typeof(Parity), "None");
                StopBits stopbits = (StopBits)Enum.Parse(typeof(StopBits), "1");
                ConnectToSerialPort(port, baudrate, parity, databits, stopbits);

            }

        }

        private void DisconnectToSerialPort()
        {
            if (comPort.IsOpen)
            {
                timer.Stop();
                comPort.DiscardOutBuffer();
                comPort.DiscardInBuffer();
                comPort.Close();
                connectButton.Content = "Connect";
                InfoTextBox.AppendText("Disconnected\n");
                InfoTextBox.ScrollToEnd();
                connect = false;
                started = false;
                startButton.IsEnabled = false;
                comboBoxComPort.IsEnabled = true;
            }
        }

        private void ConnectToSerialPort(String port, int baudrate, Parity parity, int databits, StopBits stopbits)
        {

            comPort.PortName = port;
            comPort.BaudRate = baudrate;
            comPort.Parity = parity;
            comPort.DataBits = databits;
            comPort.StopBits = stopbits;
            try
            {
                connectButton.Content = "Disconn";
                comPort.Open();
                InfoTextBox.AppendText("Connected to " + port + "\r\n");
                InfoTextBox.ScrollToEnd();
                connect = true;
                startButton.IsEnabled = true;
                comboBoxComPort.IsEnabled = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void ComPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string result = comPort.ReadLine();
            
            if (result.Length != 0)
            {
                if (result.Equals("ack"))
                {
                    serial_ack = true;
                }

                InfoTextBox.Dispatcher.Invoke(DispatcherPriority.Send, new UpdateTextHandler(UpdateInfoTextBox), result + "\n");
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (started)
            {
                startButton.Content = "Start";
                timer.Stop();
                timer.Close();
                timer.Enabled = false;
                started = false;
                connectButton.IsEnabled = true;
            }
            else
            {
                startButton.Content = "Stop";
                timer.Enabled = true;
                comPort.DataReceived += new SerialDataReceivedEventHandler(ComPortDataReceived);
                started = true;
                connectButton.IsEnabled = false;
            }

        }

        private void RetrieveColorData()
        {
            timer.Stop();

            Graphics gr = Graphics.FromImage(screenBmp);

            gr.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
            for (int i = 0; i<8; i++)
            {
                CalculateAverageColor(i, 0, norhtLED_Red, norhtLED_Green, norhtLED_Blue, screenBmp);
                arrayRectangle[i].Dispatcher.Invoke(DispatcherPriority.Send, new UpdateColorHandler(UpdateColor),
                                                                 i,
                                                                 (byte)norhtLED_Red[i],
                                                                 (byte)norhtLED_Green[i],
                                                                 (byte)norhtLED_Blue[i]);

                ColorDataSerialSend(i, norhtLED_Red, norhtLED_Green, norhtLED_Blue, 1);

                if (i < 7)
                {
                    CalculateAverageColor(i, 1, westLED_Red, westLED_Green, westLED_Blue, screenBmp);
                    CalculateAverageColor(i, 2, eastLED_Red, eastLED_Green, eastLED_Blue, screenBmp);

                    arrayRectangle[27 - i].Dispatcher.Invoke(DispatcherPriority.Send, new UpdateColorHandler(UpdateColor),
                                                                 27 - i,
                                                                 (byte)westLED_Red[i],
                                                                 (byte)westLED_Green[i],
                                                                 (byte)westLED_Blue[i]);

                    arrayRectangle[i + 8].Dispatcher.Invoke(DispatcherPriority.Send, new UpdateColorHandler(UpdateColor),
                                                                 i + 8,
                                                                 (byte)eastLED_Red[i],
                                                                 (byte)eastLED_Green[i],
                                                                 (byte)eastLED_Blue[i]);

                    ColorDataSerialSend(i, westLED_Red, westLED_Green, westLED_Blue, 3);
                    ColorDataSerialSend(i, eastLED_Red, eastLED_Green, eastLED_Blue, 4);
                }

            }
            comPort.Write("\n");
            serial_ack = false;
            timer.Start();
        }
    }
}
