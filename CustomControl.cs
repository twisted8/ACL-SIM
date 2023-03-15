﻿using EasyModbus;
using System;
using System.IO.Ports;

namespace ACLSim
{
    internal class CustomControl
    {
        ModbusClient mbc;
        ErrorHandler errorLog = new ErrorHandler();
        public event ErrorHandler.OnError onError;
        public bool enabled = true;

        public CustomControl(string port, byte driverID, bool enabled) : this(port, driverID)
        {
            this.enabled = enabled;
        }
        public CustomControl(string port, byte driverID)
        {
            errorLog.onError += (message) => onError(message);

            mbc = new ModbusClient(port)
            {
                Baudrate = 115200,
                UnitIdentifier = driverID,
                StopBits = StopBits.One,
                Parity = Parity.None
            };
        }

        // 51 - Motor running top speed
        public void SetSpeed(int value)
        {
            SetValue(51, value, 1);
        }

        public int GetSpeed()
        {
            return GetValue(51);
        }

        // 192 - Proportinal Gain
        public void SetBounceGain(int value)
        {
            SetValue(192, value, 5);
        }

        public int GetBounceGain()
        {
            return GetValue(192);
        }

        public int GetValue(int pn)
        {
            if (enabled)
            {
                try
                {
                    mbc.Connect();
                    int[] values = mbc.ReadHoldingRegisters(pn, 1);
                    return values[0];
                }
                catch (Exception ex)
                {
                    errorLog.DisplayError("failed get value (Servo " + mbc.UnitIdentifier + ") : " + ex.Message);
                }
                finally
                {
                    mbc.Disconnect();
                }
            }

            return 0;
        }

        public void SetValue(int pn, int value, int min)
        {
            if (enabled)
            { 
                if (value < min)
                {
                    errorLog.DisplayError("failed set value (Servo " + mbc.UnitIdentifier + ") " + pn + " to: " + value + " | Minimun is " + min);
                    return;
                }
                try
                {
                    mbc.Connect();
                    mbc.WriteSingleRegister(pn, value);
                }
                catch (Exception ex)
                {
                    mbc.Disconnect();
                    errorLog.DisplayError("failed set value (Servo " + mbc.UnitIdentifier + ") " + pn + " to: " + value  + " | " + ex.Message);
                }
                finally
                {
                    mbc.Disconnect();
                }

            }
        }

    }


}