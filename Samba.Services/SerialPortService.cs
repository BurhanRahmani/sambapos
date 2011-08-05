﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;

namespace Samba.Services
{
    public static class SerialPortService
    {
        private static readonly Dictionary<string, SerialPort> Ports = new Dictionary<string, SerialPort>();

        public static void WritePort(string portName, byte[] data)
        {
            if (!Ports.ContainsKey(portName))
            {
                Ports.Add(portName, new SerialPort(portName));
            }
            var port = Ports[portName];

            try
            {
                if (!port.IsOpen) port.Open();
                if (port.IsOpen) port.Write(data, 0, data.Length);
            }
            catch (IOException)
            {

            }
        }

        public static void WritePort(string portName, string data)
        {
            WritePort(portName, Encoding.ASCII.GetBytes(data));
        }

        public static void ResetCache()
        {
            foreach (var key in Ports.Keys)
                Ports[key].Close();
            Ports.Clear();
        }
    }
}
