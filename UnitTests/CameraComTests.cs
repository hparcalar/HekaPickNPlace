using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class CameraComTests
    {
        [TestMethod]
        public void TriggerCamera()
        {
            TcpClient client = new TcpClient();
            client.Connect("192.168.0.17", 50000);

            NetworkStream stream = client.GetStream();

            string clientMessage = "101, 1, 0, 0";
            byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
            stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            stream.Flush();

            int length = 0;
            Byte[] bytes = new Byte[1024];

            if ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                // Convert byte array to string message. 						
                string serverMessage = Encoding.ASCII.GetString(incommingData);
                Debug.WriteLine("server message received as: " + serverMessage);

                string[] resultElements = serverMessage.Split(',');
                if (resultElements[1].Contains("1011"))
                {
                    // error when camera couldnt be triggered
                    Assert.IsTrue(false);
                }
                else if (resultElements[1].Contains("1102")) {
                    // camera is triggered successfully
                    Assert.IsTrue(true);
                }
            }

            client.Close();
        }

        [TestMethod]
        public void GetVisionTargets()
        {
            TcpClient client = new TcpClient();
            client.Connect("192.168.0.17", 50000);

            NetworkStream stream = client.GetStream();

            string clientMessage = "102, 1";
            byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
            stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            stream.Flush();

            int length = 0;
            Byte[] bytes = new Byte[1024];

            if ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                // Convert byte array to string message. 						
                string serverMessage = Encoding.ASCII.GetString(incommingData);
                Debug.WriteLine("server message received as: " + serverMessage);

                string[] resultElements = serverMessage.Split(',');
                if (resultElements[1].Contains("1002"))
                {
                    // error when camera couldnt found any match
                    Assert.IsTrue(false);
                }
                else if (resultElements[1].Contains("1100"))
                {
                    // there is a match
                    Assert.IsTrue(true);
                }
            }

            client.Close();
        }

        [TestMethod]
        public void GetPathPlanning()
        {
            TcpClient client = new TcpClient();
            client.Connect("192.168.0.17", 50000);

            NetworkStream stream = client.GetStream();

            string clientMessage = "105, 1, 1"; // 3rd parameter: 1=joint pos, 2=TCP pos
            byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
            stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            stream.Flush();

            int length = 0;
            Byte[] bytes = new Byte[1024];

            if ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                var incommingData = new byte[length];
                Array.Copy(bytes, 0, incommingData, 0, length);
                // Convert byte array to string message. 						
                string serverMessage = Encoding.ASCII.GetString(incommingData);
                Debug.WriteLine("server message received as: " + serverMessage);

                string[] resultElements = serverMessage.Split(',');
                if (resultElements[1].Contains("1020"))
                {
                    // error when camera couldnt found any match
                    Assert.IsTrue(false);
                }
                else if (resultElements[1].Contains("1103"))
                {
                    // there is a path plan
                    Assert.IsTrue(true);
                }
            }

            client.Close();
        }
    }
}
