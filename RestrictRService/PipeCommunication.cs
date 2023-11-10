using DataPacketLibrary;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace RestrictRService
{
    public class PipeCommunication
    {
        private static string pipeName = "testPipe";
        private static int bufferSize = 1024;
        private readonly ApplicationBlocker _appBlocker;
        private readonly WebsiteBlocker _webBlocker;

        public PipeCommunication(ApplicationBlocker appBlocker, WebsiteBlocker webBlocker)
        {
            _appBlocker = appBlocker;
            _webBlocker = webBlocker;
            Thread serverReadThread = new(ServerReadThread);
            serverReadThread.Start();
        }

        // this pipe connection on the worker service is used
        // for receiving configuration information from the GUI directly
        private void ServerReadThread()
        {
            // listen continuously
            while (true)
            {
                PipeSecurity ps = new();
                SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
                ps.AddAccessRule( new PipeAccessRule(
                    sid,
                    PipeAccessRights.ReadWrite,
                    AccessControlType.Allow
                    ));

                using NamedPipeServerStream namedPipeServerStream = NamedPipeServerStreamAcl.Create(pipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.None, 4096, 4096, ps);
                namedPipeServerStream.WaitForConnection();

                byte[] buffer = new byte[bufferSize];

                int bytesRead = namedPipeServerStream.Read(buffer);

                string config = Encoding.UTF8.GetString(buffer);

                // process new config data
                Debug.WriteLine("new config received: " + config);

                ConfigurationPacket receivedPacket = ConvertJsonString(config)
                    ?? throw new Exception("Received packet from pipe is null");

                // set both apps and sites
                if (receivedPacket.BlockedAppInstallLocations != null
                    && receivedPacket.BlockedSites != null)
                {

                }
                // set just the apps
                else if (receivedPacket.BlockedAppInstallLocations != null)
                {
                    _appBlocker.SetBlockedApps(receivedPacket.BlockedAppInstallLocations);
                }
                // set just the sites
                else if (receivedPacket.BlockedSites != null)
                {
                    if (receivedPacket.BlockedSites.BlockAllSites)
                    {
                        _webBlocker.AddBlockAllInternetRule();
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private static ConfigurationPacket ConvertJsonString(string jsonString)
        {
            jsonString = jsonString.TrimEnd('\0');

            try
            {
                ConfigurationPacket deserialized = JsonSerializer.Deserialize<ConfigurationPacket>(jsonString)
                    ?? throw new JsonException("Deserialization returned null.");
                return deserialized;
            }
            catch (JsonException ex)
            {
                // Handle JSON deserialization error
                Console.WriteLine("Error deserializing JSON: " + ex.Message);
                // You might want to log the error or perform other error handling tasks here
                throw; // Rethrow the exception for further handling, if necessary
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                Console.WriteLine("Error: " + ex.Message);
                // You might want to log the error or perform other error handling tasks here
                throw; // Rethrow the exception for further handling, if necessary
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
