using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

namespace RestrictR
{
    public class PipeCommunication
    {
        private static string pipeName = "testPipe";

        public PipeCommunication()
        {
            //Thread serverReadThread = new(ClientWriteThread);
            //serverReadThread.Start();
        }

        // this pipe connection on the GUI is used to send configuration information
        // to the worker service directly
        // method is meant to be called right when the configuration changes
        public static async Task SendConfig(string config)
        {
            using NamedPipeClientStream namedPipeClientStream = new(".", pipeName, PipeDirection.Out);
            await namedPipeClientStream.ConnectAsync();
            Debug.WriteLine("Connected to server!");

            byte[] configBytes = Encoding.UTF8.GetBytes(config);

            await namedPipeClientStream.WriteAsync(configBytes);
        }

        private void ClientWriteThread()
        {
            NamedPipeClientStream namedPipeClientStream = new(".", pipeName, PipeDirection.Out);

            namedPipeClientStream.Connect();

            var hmm = namedPipeClientStream.IsConnected;

            Debug.WriteLine("Connected to server!");

            StreamString ss = new StreamString(namedPipeClientStream);

            ss.WriteString(@"[{applicationInstallLocation: ""C:\somedirectory\bannable_app_folder""}]");

            Debug.WriteLine("Closing connection to the server");

            namedPipeClientStream.Close();
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
