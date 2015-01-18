using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Security;
using System.Net.Sockets;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Net;

namespace Glancer
{
    public class SslStreamSet
    {
        public SslStream _serverStream { get; set; }
        public SslStream _clientStream { get; set; }
    }

    class SslHandshake
    {
        public static bool IsSslConnection(HttpRequestObject request){
            return (request._header._method == HTTP_METHOD.CONNECT);
        }

        private static ConnectionInfo ParseReceiveConnect(HttpRequestObject request)
        {
            ConnectionInfo info = new ConnectionInfo();

            string[] s = request._header._path.Split(':');

            info.host = s[0];
            info.port = s[1];
            info.protocol = request._header._httpVersion;

            SslTcpServer._traceLogger.OutputLog(request._header._source.Replace(System.Environment.NewLine, "\t"));

            return info;
        }

        private static void ConnectionEstablished(Stream stream, string method)
        {
            string sslConnectResponse = string.Format(SERVER_REPONSE.CONNECTION_ESTABLISHED,
                method, System.Environment.NewLine + System.Environment.NewLine);
            HttpStream.Write(stream, Encoding.UTF8.GetBytes(sslConnectResponse));

            SslTcpServer._traceLogger.OutputLog(sslConnectResponse.Replace(System.Environment.NewLine, "\t"));
        }

        public static SslStreamSet Handshake(TcpClient client, HttpRequestObject request, Stream serverStream)
        {

            ConnectionInfo connectionInfo = ParseReceiveConnect(request);

            SslStreamSet _sslStream = new SslStreamSet();

            // Connection Remote Server.
            SslTcpServer._traceLogger.OutputLog(string.Format("Connection Remote Server {0}:{1}.", connectionInfo.host, Convert.ToInt32(connectionInfo.port)));
            SslTcpClient clientSocket = new SslTcpClient(client);

            SslStream sslClientStream = clientSocket.Connect(connectionInfo.host, Convert.ToInt32(connectionInfo.port));

            // Get Remote PublicKey.
            ServerCertificate hostCertificate = clientSocket.ServerCertificate;

            // Connection Established.
            ConnectionEstablished(serverStream, connectionInfo.protocol);


            // Authenticate to Client by Self PublicKey.
            SslTcpServer._traceLogger.OutputLog("Authenticate with Client by Self PublicKey.");
            SslStream sslServerStream = AuthenticateAsServer(serverStream);

            // Output log.
            OutputSslInfo(sslServerStream, sslClientStream, SslTcpServer._traceLogger);

            _sslStream._serverStream = sslServerStream;
            _sslStream._clientStream = sslClientStream;
            return _sslStream;
        }

        private static SslStream AuthenticateAsServer(Stream serverSocket)
        {
            SslStream sslStream = new SslStream(serverSocket, false);
            sslStream.ReadTimeout = 5000;
            try
            {
                sslStream.AuthenticateAsServer(SslTcpServer._serverCertificate,
                    false, SslProtocols.Tls, true);
            }
            catch (AuthenticationException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw e;
            }

            return sslStream;
        }

        private static void OutputSslInfo(SslStream server, SslStream client, TraceLogger traceLogger)
        {

            traceLogger._bIndent = false;

            traceLogger.InputLog("\r\n\tServer SslStream Information.");
            traceLogger.InputLog(string.Format("\r\n\tCipher: {0}", server.CipherAlgorithm));
            traceLogger.InputLog(string.Format("\r\n\tCipher strength: {0}", server.CipherStrength));
            traceLogger.InputLog(string.Format("\r\n\tHash: {0}", server.HashAlgorithm));
            traceLogger.InputLog(string.Format("\r\n\tHash strength: {0}", server.HashStrength));
            traceLogger.InputLog(string.Format("\r\n\tKey exchange: {0}", server.KeyExchangeAlgorithm));
            traceLogger.InputLog(string.Format("\r\n\tKey exchange strength: {0}", server.KeyExchangeStrength));
            traceLogger.InputLog(string.Format("\r\n\tProtocol: {0}", server.SslProtocol));
            traceLogger.InputLog(string.Format("\r\n\tIs authenticated: {0} as server? {1}", server.IsAuthenticated, server.IsServer));
            traceLogger.InputLog(string.Format("\r\n\tIsSigned: {0}", server.IsSigned));
            traceLogger.InputLog(string.Format("\r\n\tIs Encrypted: {0}", server.IsEncrypted));
            traceLogger.InputLog(string.Format("\r\n\tCertificate revocation list checked: {0}", server.CheckCertRevocationStatus));

            X509Certificate localCertificate = server.LocalCertificate;
            if (localCertificate != null)
            {
                traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}",
                    localCertificate.Subject, localCertificate.GetEffectiveDateString(), localCertificate.GetExpirationDateString()));
            }
            else
            {
                traceLogger.InputLog(string.Format("\r\n\tlocalCertificate is null."));
            }

            X509Certificate remoteCertificate = server.RemoteCertificate;
            if (remoteCertificate != null)
            {
                traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}",
                    remoteCertificate.Subject, remoteCertificate.GetEffectiveDateString(), remoteCertificate.GetExpirationDateString()));
            }
            else
            {
                traceLogger.InputLog(string.Format("\r\n\tremoteCertificate is null."));
            }


            traceLogger.InputLog("\r\n");
            traceLogger.InputLog("\r\n\tClient SslStream Information.");
            traceLogger.InputLog(string.Format("\r\n\tCipher: {0}", client.CipherAlgorithm));
            traceLogger.InputLog(string.Format("\r\n\tCipher strength: {0}", client.CipherStrength));
            traceLogger.InputLog(string.Format("\r\n\tHash: {0}", client.HashAlgorithm));
            traceLogger.InputLog(string.Format("\r\n\tHash strength: {0}", client.HashStrength));
            traceLogger.InputLog(string.Format("\r\n\tKey exchange: {0}", client.KeyExchangeAlgorithm));
            traceLogger.InputLog(string.Format("\r\n\tKey exchange strength: {0}", client.KeyExchangeStrength));
            traceLogger.InputLog(string.Format("\r\n\tProtocol: {0}", client.SslProtocol));
            traceLogger.InputLog(string.Format("\r\n\tIs authenticated: {0} as server? {1}", client.IsAuthenticated, server.IsServer));
            traceLogger.InputLog(string.Format("\r\n\tIsSigned: {0}", client.IsSigned));
            traceLogger.InputLog(string.Format("\r\n\tIs Encrypted: {0}", client.IsEncrypted));
            traceLogger.InputLog(string.Format("\r\n\tCertificate revocation list checked: {0}", client.CheckCertRevocationStatus));

            localCertificate = client.LocalCertificate;
            if (localCertificate != null)
            {
                traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}",
                    localCertificate.Subject, localCertificate.GetEffectiveDateString(), localCertificate.GetExpirationDateString()));
            }
            else
            {
                traceLogger.InputLog(string.Format("\r\n\tlocalCertificate is null."));
            }

            remoteCertificate = server.RemoteCertificate;
            if (remoteCertificate != null)
            {
                traceLogger.InputLog(string.Format("\r\n\tlocalCertificate Subject: {0}  EffectiveDate: {1}  Expiration: {2}",
                    remoteCertificate.Subject, remoteCertificate.GetEffectiveDateString(), remoteCertificate.GetExpirationDateString()));
            }
            else
            {
                traceLogger.InputLog(string.Format("\r\n\tremoteCertificate is null."));
            }

            traceLogger.OutputLog();

            traceLogger._bIndent = true;
        }
    }
}
