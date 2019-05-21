﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace HardwareMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string TERM_CHAR = "$";

        private string[] ports;
        private SerialPort serial;
        private string message = "";

        public MainWindow()
        {
            InitializeComponent();

            SetupUI();
        }

        public void SetupUI()
        {
            ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                portSelect.Items.Add(port);
            }

            OutputMessage("Ready.\n");
        }

        private void PortSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            serial = new SerialPort((string)portSelect.SelectedItem, 9600, Parity.None, 8, StopBits.One);
        }

        private void TryConnectToArduino()
        {
            try
            {
                serial.Open();
                serial.DataReceived += new SerialDataReceivedEventHandler(Serial_OnDataReceive);
            }
            catch (Exception ex)
            {
                OutputException(ex);
            }
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            TryConnectToArduino();
        }

        private void SendBtn_Click(object sender, RoutedEventArgs e)
        {
            message = messageTxtBox.Text;
            Thread messageSender = new Thread(new ThreadStart(SendMessage));
            messageSender.Start();
        }

        private void SendMessage()
        {
            if (serial.IsOpen)
            {
                try
                {
                    // Send the binary data out the port
                    byte[] hexstring = Encoding.ASCII.GetBytes(message + TERM_CHAR); // Append terminating character
                    foreach (byte hexval in hexstring)
                    {
                        byte[] _hexval = new byte[] { hexval };     // need to convert byte 
                                                                    // to byte[] to write
                        serial.Write(_hexval, 0, 1);
                        Thread.Sleep(1);
                    }
                }
                catch (Exception ex)
                {
                    OutputException(ex);
                }
            }
        }

        private void Serial_OnDataReceive(object sender, SerialDataReceivedEventArgs args)
        {
            SerialPort sp = (SerialPort)sender;
            string received = sp.ReadExisting();
            OutputMessage(received);
        }

        private void DisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            serial.Close();
        }

        private void OutputException(Exception ex)
        {
            TextRange output = new TextRange(outputBox.Document.ContentEnd, outputBox.Document.ContentEnd);
            output.Text = ex.Message + "\n" + ex.StackTrace;
            output.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);
        }

        private void OutputMessage(string text, bool newLine = true)
        {
            TextRange output = new TextRange(outputBox.Document.ContentEnd, outputBox.Document.ContentEnd);
            output.Text = text + (newLine ? "\r\n" : "");
            output.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }
    }
}