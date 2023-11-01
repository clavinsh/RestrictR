using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestrictRService
{
    public class PipeCommunication
    {
        private static string pipeName = "testPipe";

        public PipeCommunication()
        {
            Thread serverReadThread = new(ServerReadThread);
            serverReadThread.Start();
        }

        // this pipe connection on the worker service is used
        // for receiving configuration information from the GUI directly
        private void ServerReadThread()
        {
            // listen continuously
            while(true)
            {
                using NamedPipeServerStream namedPipeServerStream = new(pipeName, PipeDirection.In);
                namedPipeServerStream.WaitForConnection();

                byte[] buffer = new byte[1024];

                int bytesRead = namedPipeServerStream.Read(buffer);

                string config = Encoding.UTF8.GetString(buffer);

                // process new config data
                Debug.WriteLine("new config received: " + config);
                Thread.Sleep(1000);
            }
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
