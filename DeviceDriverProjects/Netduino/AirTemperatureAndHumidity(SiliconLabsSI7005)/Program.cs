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
using Microsoft.SPOT;
using devMobile.NetMF.Sensor;


namespace AirTemperatureAndHumidity
{
   public class Program
   {
      public static void Main()
      {
         SiliconLabsSI7005 sensor = new SiliconLabsSI7005();

         while (true)
         {
            double temperature = sensor.Temperature();

            double humidity = sensor.Humidity();

            Debug.Print("T:" + temperature.ToString("F1") + " H:" + humidity.ToString("F1"));

            Thread.Sleep(5000);
         }
      }
   }
}
