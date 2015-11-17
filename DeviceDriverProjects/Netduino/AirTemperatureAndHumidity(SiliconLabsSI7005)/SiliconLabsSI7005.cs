//---------------------------------------------------------------------------------
// Copyright (c) 2015, devMobile Software
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//---------------------------------------------------------------------------------
using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;


namespace devMobile.NetMF.Sensor
{
   public class SiliconLabsSI7005
   {
      private byte deviceId;
      private const byte DeviceIdDefault = 0x40;
      private int clockRateKHz; 
      private const int ClockRateKHzDefault = 400;
      private int transactionTimeoutmSec ;
      private const int TransactionTimeoutmSecDefault = 1000;

      private const int RegisterIdConfiguration = 0x03;
      private const int RegisterIdStatus = 0x00;
      private const byte RegisterIdDeviceId = 0x11; 
      private const byte StatusReadyMask = 0x01;
      private const byte CommandMeasureTemperature = 0x11;  
      private const byte CommandMeasureHumidity = 0x01;
      private const byte REG_DATA_H = 0x01;


      public SiliconLabsSI7005(byte deviceId = DeviceIdDefault, int clockRateKHz = ClockRateKHzDefault, int transactionTimeoutmSec = TransactionTimeoutmSecDefault)
      {
         this.deviceId = deviceId;
         this.clockRateKHz = clockRateKHz;
         this.transactionTimeoutmSec = transactionTimeoutmSec;

         using (OutputPort i2cPort = new OutputPort(Pins.GPIO_PIN_SDA, true))
         {
            i2cPort.Write(false);
            Thread.Sleep(250);
         }

         using (I2CDevice device = new I2CDevice(new I2CDevice.Configuration(deviceId, clockRateKHz)))
         {
            byte[] writeBuffer = { RegisterIdDeviceId };
            byte[] readBuffer = new byte[1];

            // The first request always fails
            I2CDevice.I2CTransaction[] action = new I2CDevice.I2CTransaction[] 
            { 
               I2CDevice.CreateWriteTransaction(writeBuffer),
               I2CDevice.CreateReadTransaction(readBuffer)
            };

            if( device.Execute(action, transactionTimeoutmSec) == 0 )
            {
            //   throw new ApplicationException("Unable to send get device id command");
            }
         }
      }



      public double Temperature()
      {
         using (I2CDevice device = new I2CDevice(new I2CDevice.Configuration(deviceId, clockRateKHz)))
         {
            //Debug.Print("Temperature Measurement start");

            byte[] CmdBuffer = { RegisterIdConfiguration, CommandMeasureTemperature };

            I2CDevice.I2CTransaction[] CmdAction = new I2CDevice.I2CTransaction[] 
            { 
               I2CDevice.CreateWriteTransaction(CmdBuffer),
            };

            if (device.Execute(CmdAction, transactionTimeoutmSec) == 0)
            {
               throw new ApplicationException("Unable to send measure temperature command");
            }
            

            //Debug.Print("Measurement wait");
            bool conversionInProgress = true;

            // Wait for measurement
            do
            {
               byte[] WaitWriteBuffer = { RegisterIdStatus };
               byte[] WaitReadBuffer = new byte[1];

               I2CDevice.I2CTransaction[] waitAction = new I2CDevice.I2CTransaction[] 
               { 
                  I2CDevice.CreateWriteTransaction(WaitWriteBuffer),
                  I2CDevice.CreateReadTransaction(WaitReadBuffer)
               };

               if (device.Execute(waitAction, transactionTimeoutmSec) == 0)
               {
                  throw new ApplicationException("Unable to read status register");
               }

               if ((WaitReadBuffer[RegisterIdStatus] & StatusReadyMask) != StatusReadyMask)
               {
                  conversionInProgress = false;
               }
            } while (conversionInProgress);


            //Debug.Print("Measurement read");
            // Read temperature value
            byte[] valueWriteBuffer = { REG_DATA_H };
            byte[] valueReadBuffer = new byte[2];

            I2CDevice.I2CTransaction[] valueAction = new I2CDevice.I2CTransaction[] 
            { 
               I2CDevice.CreateWriteTransaction(valueWriteBuffer),
               I2CDevice.CreateReadTransaction(valueReadBuffer)
            };

            if (device.Execute(valueAction, transactionTimeoutmSec) == 0)
            {
               throw new ApplicationException("Unable to read data register");
            }

            // Convert bye to centigrade
            int temp = valueReadBuffer[0];

            temp = temp << 8;
            temp = temp + valueReadBuffer[1];
            temp = temp >> 2;
            /* 
              Formula: Temperature(C) = (Value/32) - 50	  
            */
            double temperature = (temp / 32.0) - 50.0;

            //Debug.Print(" Temp " + temperature.ToString("F1"));

            return temperature;
         }
      }



      public double Humidity()
      {
         using (I2CDevice device = new I2CDevice(new I2CDevice.Configuration(deviceId, clockRateKHz)))
         {
            //Debug.Print("Humidity Measurement start");

            byte[] CmdBuffer = { RegisterIdConfiguration, CommandMeasureHumidity };

            I2CDevice.I2CTransaction[] CmdAction = new I2CDevice.I2CTransaction[] 
            { 
               I2CDevice.CreateWriteTransaction(CmdBuffer),
            };

            if (device.Execute(CmdAction, transactionTimeoutmSec) == 0)
            {
               throw new ApplicationException("Unable to send measure humidity command");
            }

            //Debug.Print("Measurement wait");
            bool humidityConversionInProgress = true;

            // Wait for measurement
            do
            {
               byte[] WaitWriteBuffer = { RegisterIdStatus };
               byte[] ValueReadBuffer = new byte[1];

               I2CDevice.I2CTransaction[] waitAction = new I2CDevice.I2CTransaction[] 
               { 
                  I2CDevice.CreateWriteTransaction(WaitWriteBuffer),
                  I2CDevice.CreateReadTransaction(ValueReadBuffer)
               };

               if (device.Execute(waitAction, transactionTimeoutmSec) == 0)
               {
                  throw new ApplicationException("Unable to read status register");
               }

               if ((ValueReadBuffer[RegisterIdStatus] & StatusReadyMask) != StatusReadyMask)
               {
                  humidityConversionInProgress = false;
               }
            } while (humidityConversionInProgress);


            //Debug.Print("Measurement read");
            byte[] valueWriteBuffer = { REG_DATA_H };
            byte[] valueRreadBuffer = new byte[2];

            I2CDevice.I2CTransaction[] valueAction = new I2CDevice.I2CTransaction[] 
            { 
               I2CDevice.CreateWriteTransaction(valueWriteBuffer),
               I2CDevice.CreateReadTransaction(valueRreadBuffer)
            };

            if (device.Execute(valueAction, transactionTimeoutmSec) == 0)
            {
               throw new ApplicationException("Unable to read data register");
            }

            int hum = valueRreadBuffer[0];

            hum = hum << 8;
            hum = hum + valueRreadBuffer[1];
            hum = hum >> 4;
            /* 
            Formula:
            Humidity(%) = (Value/16) - 24	  
            */
            double humidity = (hum / 16.0) - 24.0;

            //Debug.Print(" Humidity " + humidity.ToString("F1"));
            return humidity;
         }
      }
   }
}
