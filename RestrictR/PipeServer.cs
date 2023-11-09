using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;


namespace RestrictR
{
    public class PipeServer
    {
        private static int numThreads = 4;
        private static string pipeName = "testPipe";

        public PipeServer()
        {
            Thread serverReadThread = new(ServerThread);

            serverReadThread.Start();
        }

        private void ServerThread()
        {
            NamedPipeServerStream namedPipeServerStream = new(pipeName, PipeDirection.In);

            namedPipeServerStream.WaitForConnection();

            Debug.WriteLine("Client has connected");


            while (namedPipeServerStream.IsConnected)
            {
                Thread.Sleep(1000); // sleep for a second
            }

            try
            {
                StreamString ss = new StreamString(namedPipeServerStream);

                while (true)
                {
                    if (!namedPipeServerStream.IsConnected)
                    {
                        Debug.WriteLine("connection lost");
                        // heart beat implementation here
                    }

                    string msg = ss.ReadString();

                    Debug.WriteLine($"received msg: {msg}");

                    Thread.Sleep(1000); // sleep for a second
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("ERROR: " + e.ToString());
            }


            namedPipeServerStream.Close();
        }


        //public static void Main()
        //{
        //    int i;
        //    Thread?[] servers = new Thread[numThreads];

        //    Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
        //    Console.WriteLine("Waiting for client connect...\n");
        //    for (i = 0; i < numThreads; i++)
        //    {
        //        servers[i] = new Thread(ServerThread);
        //        servers[i]?.Start();
        //    }
        //    Thread.Sleep(250);
        //    while (i > 0)
        //    {
        //        for (int j = 0; j < numThreads; j++)
        //        {
        //            if (servers[j] != null)
        //            {
        //                if (servers[j]!.Join(250))
        //                {
        //                    Console.WriteLine("Server thread[{0}] finished.", servers[j]!.ManagedThreadId);
        //                    servers[j] = null;
        //                    i--;    // decrement the thread watch count
        //                }
        //            }
        //        }
        //    }
        //}

        //private static void ServerThread(object? data)
        //{
        //    NamedPipeServerStream pipeServer =
        //        new NamedPipeServerStream("testPipe", PipeDirection.InOut, numThreads);

        //    int threadId = Thread.CurrentThread.ManagedThreadId;

        //    pipeServer.WaitForConnection();

        //    Console.WriteLine("Client connected on thread[{0}].", threadId);

        //    try
        //    {
        //        // Read the request from the client. Once the client has
        //        // written to the pipe its security token will be available.

        //        StreamString ss = new StreamString(pipeServer);

        //        // Verify our identity to the connected client using a
        //        // string that the client anticipates.

        //        ss.WriteString("I am the one true server!!!");

        //        string fileName = ss.ReadString();

        //        // Read in the contents of the file while impersonating the client.
        //        ReadFileToStream fileReader = new ReadFileToStream(ss, fileName);

        //        // Display the name of the user we are impersonating.
        //        Console.WriteLine("Reading file: {0} on thread[{1}] as user: {2}.",
        //            fileName, threadId, pipeServer.GetImpersonationUserName());

        //        pipeServer.RunAsClient(fileReader.Start);
        //    }
        //    catch (IOException e)
        //    {
        //        Console.WriteLine("ERROR: {0}", e.Message);
        //    }
        //    pipeServer.Close();
        //}

        //private static void ServerThread(string config)
        //{
        //    using NamedPipeServerStream pipeServer = new(pipeName, PipeDirection.InOut);
        //    pipeServer.WaitForConnection();

        //    byte[] configBytes = Encoding.UTF8.GetBytes(config);

        //    pipeServer.Write(configBytes, 0, configBytes.Length);
        //}

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

    public class ReadFileToStream
    {
        private string fn;
        private StreamString ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }
    }
}
