using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace PickNPlace.Camera
{
    public class MechWorker
    {
        public bool TriggerCamera(string programId)
        {
            bool connResult = false;

            TcpClient client = new TcpClient();
            client.Connect("192.168.0.17", 50000);

            NetworkStream stream = client.GetStream();

            string clientMessage = "101, "+ programId +", 0, 0";
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
                //Debug.WriteLine("server message received as: " + serverMessage);

                string[] resultElements = serverMessage.Split(',');
                if (resultElements[1].Contains("1011"))
                {
                    // error when camera couldnt be triggered
                    connResult = false;
                }
                else if (resultElements[1].Contains("1102"))
                {
                    // camera is triggered successfully
                    connResult = true;
                }
            }

            client.Close();

            return connResult;
        }

        public string GetVisionTargets(string programId)
        {
            string respMessage = "";

            TcpClient client = new TcpClient();
            client.Connect("192.168.0.17", 50000);

            NetworkStream stream = client.GetStream();

            string clientMessage = "102, " + programId;
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
                //Debug.WriteLine("server message received as: " + serverMessage);
                respMessage = serverMessage;

                string[] resultElements = serverMessage.Split(',');
                if (resultElements[1].Contains("1002"))
                {
                    // error when camera couldnt found any match
                    // Assert.IsTrue(false);
                }
                else if (resultElements[1].Contains("1100"))
                {
                    // there is a match
                    // Assert.IsTrue(true);
                }
            }

            client.Close();

            return respMessage;
        }
    }
}
