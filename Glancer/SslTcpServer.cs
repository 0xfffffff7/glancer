using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace Glancer
{
    public static class SERVER_REPONSE{
        public static string CONNECTION_ESTABLISHED = "{0} 200 Connection established{1}";
    }
    
    public class ConnectionInfo{
        public string host = string.Empty;
        public string port = string.Empty;
        public string protocol = string.Empty;
    }

    public sealed class SslTcpServer
    {
        public static X509Certificate _serverCertificate = null;
        int _lisstenPort;
        public static TraceLogger _traceLogger = new TraceLogger();
        public string _protocolLogDir { set; get; }
        public string _traceLogDir { set; get; }

        public SslTcpServer(X509Certificate secret, int lisstenPort)
        {
            _serverCertificate = secret;
            _lisstenPort = lisstenPort;
        }

        public void RunServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _lisstenPort);
            listener.Start();

            while (true)
            {
                TcpClient serverSocket = listener.AcceptTcpClient();

                // Create TCP Session.
                string session = Guid.NewGuid().ToString();
                _traceLogger.InitLogger(_traceLogDir, true, true, session + "_tcp.log");
                _traceLogger.OutputLog("RunServer().");
                _traceLogger.OutputLog("Listen Start.");
                _traceLogger.OutputLog("AcceptTcpClient().");

                ServerProcess(serverSocket, session);
            }
        }

        void ServerProcess(TcpClient serverSocket, string session)
        {
            try
            {
                // Create Event Listener.
                TraceLogger protocolLogger = new TraceLogger();
                protocolLogger.InitLogger(_protocolLogDir, false, false, session + ".log");
                HttpEventListener listner = new HttpEventListener(protocolLogger);

                // Proxy work.
                HttpStreamProxy.Proxy(serverSocket, listner);

            }
            catch(Exception e)
            {
                _traceLogger.InputLog("Exception");
                _traceLogger.OutputLog(e.Message);
            }

        }
    }
}