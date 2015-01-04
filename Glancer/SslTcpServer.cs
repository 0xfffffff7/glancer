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
        public static string CONNECTION_ESTABLISHED = "{0} 200 Connection established{1}Proxy-connection: Keep-alive{2}";
    }
    
    public class ConnectionInfo{
        public string host = string.Empty;
        public string port = string.Empty;
        public string protocol = string.Empty;
    }

    public sealed class SslTcpServer
    {
        static X509Certificate _serverCertificate = null;
        int _readTimeout;
        int _writeTimeout;
        int _lisstenPort;
        private TraceLogger _traceLogger = new TraceLogger();
        public string _protocolLogDir { set; get; }
        public string _traceLogDir { set; get; }

        public SslTcpServer(X509Certificate secret, int lisstenPort, int readTimeout, int writeTimeout)
        {
            _readTimeout = readTimeout;
            _writeTimeout = writeTimeout;
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
            SslTcpClient clientSocket = null;
            try
            {
                NetworkStream stream = serverSocket.GetStream();
                ConnectionInfo connectionInfo = SslReceiveConnect(stream);

                // Connection Remote Server.
                _traceLogger.OutputLog( string.Format("Connection Remote Server {0}:{1}.", connectionInfo.host, Convert.ToInt32(connectionInfo.port)));
                clientSocket = new SslTcpClient(_readTimeout, _writeTimeout);
                SslStream sslClientStream = clientSocket.Connect(connectionInfo.host, Convert.ToInt32(connectionInfo.port));

                // Get PublicKey.
                ServerCertificate hostCertificate = clientSocket.ServerCertificate;

                // Authenticate with Client by Self PublicKey.
                _traceLogger.OutputLog("Authenticate with Client by Self PublicKey.");
                SslStream sslServerStream = AuthenticateAsServer(stream);

                // Output log.
                OutputSslInfo(sslServerStream, sslClientStream);

                // Create Event Listener.
                TraceLogger protocolLogger = new TraceLogger();
                protocolLogger.InitLogger(_protocolLogDir, false, false, session + ".log");
                HttpEventListener listner = new HttpEventListener(protocolLogger);

                // Proxy work.
                HttpStreamProxy.Proxy(sslClientStream, _writeTimeout, sslServerStream, _readTimeout, listner);

            }
            catch(Exception e)
            {
                _traceLogger.InputLog("Exception");
                _traceLogger.OutputLog(e.Message);
            }
            finally
            {
                serverSocket.Close();
                if (clientSocket != null)
                {
                    clientSocket.Close();
                }
            }
        }

        private ConnectionInfo SslReceiveConnect(NetworkStream stream)
        {
            string sslConnectRequest = Encoding.UTF8.GetString(HttpStream.Read(stream, _readTimeout));
            string[] s = sslConnectRequest.Split(' ');
            string[] uri = s[1].Split(':');

            _traceLogger.OutputLog(sslConnectRequest.Replace(System.Environment.NewLine, "\t"));

            ConnectionInfo info = new ConnectionInfo();
            info.host = uri[0];
            info.port = uri[1];
            info.protocol = s[2].Substring(0, 8);

            // Connection Established.
            ConnectionEstablished(stream, info.protocol);

            return info;
        }

        private SslStream AuthenticateAsServer(NetworkStream serverSocket)
        {
            SslStream sslStream = new SslStream(serverSocket, false);
            sslStream.ReadTimeout = 5000;
            try
            {
                sslStream.AuthenticateAsServer(_serverCertificate,
                    false, SslProtocols.Tls, true);
            }
            catch (AuthenticationException e)
            {
                sslStream.Close();
                serverSocket.Close();
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

            return sslStream;
        }

        void ConnectionEstablished(NetworkStream stream, string method)
        {
            string sslConnectResponse = string.Format(SERVER_REPONSE.CONNECTION_ESTABLISHED,
                method, System.Environment.NewLine, System.Environment.NewLine + System.Environment.NewLine);
            HttpStream.Write(stream, Encoding.UTF8.GetBytes(sslConnectResponse), _writeTimeout);

            _traceLogger.OutputLog(sslConnectResponse.Replace(System.Environment.NewLine, "\t"));
        }

        private void OutputSslInfo(SslStream server, SslStream client){

            _traceLogger._bIndent = false;

            _traceLogger.InputLog("\r\n\tServer SslStream Information.");
            _traceLogger.InputLog(string.Format("\r\n\tCipher: {0}", server.CipherAlgorithm));
            _traceLogger.InputLog(string.Format("\r\n\tCipher strength: {0}", server.CipherStrength));
            _traceLogger.InputLog(string.Format("\r\n\tHash: {0}", server.HashAlgorithm));
            _traceLogger.InputLog(string.Format("\r\n\tHash strength: {0}", server.HashStrength));
            _traceLogger.InputLog(string.Format("\r\n\tKey exchange: {0}", server.KeyExchangeAlgorithm));
            _traceLogger.InputLog(string.Format("\r\n\tKey exchange strength: {0}", server.KeyExchangeStrength));
            _traceLogger.InputLog(string.Format("\r\n\tProtocol: {0}", server.SslProtocol));
            _traceLogger.InputLog(string.Format("\r\n\tIs authenticated: {0} as server? {1}", server.IsAuthenticated, server.IsServer));
            _traceLogger.InputLog(string.Format("\r\n\tIsSigned: {0}", server.IsSigned));
            _traceLogger.InputLog(string.Format("\r\n\tIs Encrypted: {0}", server.IsEncrypted));
            _traceLogger.InputLog(string.Format("\r\n\tCertificate revocation list checked: {0}", server.CheckCertRevocationStatus));

            X509Certificate localCertificate = server.LocalCertificate;
            if (localCertificate != null){
                _traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}", 
                    localCertificate.Subject, localCertificate.GetEffectiveDateString(), localCertificate.GetExpirationDateString()));
            }else{
                _traceLogger.InputLog(string.Format("\r\n\tlocalCertificate is null."));
            }

            X509Certificate remoteCertificate = server.RemoteCertificate;
            if (remoteCertificate != null){
                _traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}", 
                    remoteCertificate.Subject, remoteCertificate.GetEffectiveDateString(), remoteCertificate.GetExpirationDateString()));
            }else{
                _traceLogger.InputLog(string.Format("\r\n\tremoteCertificate is null."));
            }


            _traceLogger.InputLog("\r\n");
            _traceLogger.InputLog("\r\n\tClient SslStream Information.");
            _traceLogger.InputLog(string.Format("\r\n\tCipher: {0}", client.CipherAlgorithm));
            _traceLogger.InputLog(string.Format("\r\n\tCipher strength: {0}", client.CipherStrength));
            _traceLogger.InputLog(string.Format("\r\n\tHash: {0}", client.HashAlgorithm));
            _traceLogger.InputLog(string.Format("\r\n\tHash strength: {0}", client.HashStrength));
            _traceLogger.InputLog(string.Format("\r\n\tKey exchange: {0}", client.KeyExchangeAlgorithm));
            _traceLogger.InputLog(string.Format("\r\n\tKey exchange strength: {0}", client.KeyExchangeStrength));
            _traceLogger.InputLog(string.Format("\r\n\tProtocol: {0}", client.SslProtocol));
            _traceLogger.InputLog(string.Format("\r\n\tIs authenticated: {0} as server? {1}", client.IsAuthenticated, server.IsServer));
            _traceLogger.InputLog(string.Format("\r\n\tIsSigned: {0}", client.IsSigned));
            _traceLogger.InputLog(string.Format("\r\n\tIs Encrypted: {0}", client.IsEncrypted));
            _traceLogger.InputLog(string.Format("\r\n\tCertificate revocation list checked: {0}", client.CheckCertRevocationStatus));

            localCertificate = client.LocalCertificate;
            if (localCertificate != null)
            {
                _traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}",
                    localCertificate.Subject, localCertificate.GetEffectiveDateString(), localCertificate.GetExpirationDateString()));
            }
            else
            {
                _traceLogger.InputLog(string.Format("\r\n\tlocalCertificate is null."));
            }

            remoteCertificate = server.RemoteCertificate;
            if (remoteCertificate != null)
            {
                _traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}",
                    remoteCertificate.Subject, remoteCertificate.GetEffectiveDateString(), remoteCertificate.GetExpirationDateString()));
            }
            else
            {
                _traceLogger.InputLog(string.Format("\r\n\tremoteCertificate is null."));
            }

            _traceLogger.OutputLog();

            _traceLogger._bIndent = true;
        }
    }
}