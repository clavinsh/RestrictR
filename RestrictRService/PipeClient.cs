using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictRService
{
    public class PipeClient
    {
        private static string pipeName = "testPipe";
        private static string writePipeName = "wPipe";

        public PipeClient()
        {
            Thread clientWriteThread = new(ClientThread);

            clientWriteThread.Start();
        }

        private void ClientThread()
        {
            NamedPipeClientStream namedPipeClientStream = new(".", pipeName, PipeDirection.Out);

            namedPipeClientStream.Connect();

            var hmm = namedPipeClientStream.IsConnected;

            Debug.WriteLine("Connected to server!");

            StreamString ss = new StreamString(namedPipeClientStream);

            ss.WriteString("hello from windows service worker!");

            namedPipeClientStream.Close();
        }


        private void ClientReadThread()
        {
            NamedPipeClientStream namedPipeClientStream = new(".", pipeName, PipeDirection.Out);
        }

        private static string ReceiveConfigurationOverNamedPipe()
        {
            using NamedPipeClientStream pipeClient = new(".", writePipeName, PipeDirection.In);

            while(!pipeClient.IsConnected)
            {
                try
                {
                    pipeClient.Connect();
                }
                catch
                { 

                }
                Thread.Sleep(1000);
            }



            pipeClient.Connect();

            byte[] buffer = new byte[4096];

            int bytesRead = pipeClient.Read(buffer, 0, buffer.Length);

            string config = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            return config;
        }


        public class StreamString
        {
            private Stream ioStream;
            private UnicodeEncoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UnicodeEncoding();
            }

            public string ReadString()
            {
                int len = 0;

                len = ioStream.ReadByte() * 256;
                len += ioStream.ReadByte();
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);

                return streamEncoding.GetString(inBuffer);
            }

            public int WriteString(string outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }

    }
}
