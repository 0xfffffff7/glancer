using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;

namespace Glancer
{

    public class ServerCertificate{
        public X509Certificate certificate = null;
        public X509Chain chain = null;
        public SslPolicyErrors sslPolicyErrors = SslPolicyErrors.None;
        public string exceptionError = null;
    }
    
    class SslTcpClient
    {
        private ServerCertificate serverCertificate = new ServerCertificate();
        public ServerCertificate ServerCertificate { get { return serverCertificate; } }
        SslStream sslStream;
        TcpClient _tcpClient = null;

        public SslTcpClient(TcpClient client)
        {
            _tcpClient = client;
        }

        public SslStream Connect(string host, int port)
        {
            try
            {
                _tcpClient = new TcpClient(host, port);
                sslStream = new SslStream(
                    _tcpClient.GetStream(),
                    false,
                    new RemoteCertificateValidationCallback(ValidateCertificate),
                    null);
            }
            catch (AuthenticationException e)
            {
                throw e;
            }catch(Exception e){
                throw e;
            }


            try
            {
                sslStream.AuthenticateAsClient(host);
            }
            catch (AuthenticationException e)
            {
                serverCertificate.exceptionError = e.Message + HttpStream.STREAM_TERMINATE;
                if (e.InnerException != null)
                {
                    serverCertificate.exceptionError += e.InnerException.Message;
                }
                _tcpClient.Close();
                return sslStream;
            }catch(Exception e){
                throw e;
            }

            return sslStream;
        }

        private bool ValidateCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {

            serverCertificate.certificate = certificate;
            serverCertificate.chain = chain;
            serverCertificate.sslPolicyErrors = sslPolicyErrors;

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }
            return false;
        }

        public string Read() { return Encoding.UTF8.GetString(HttpStream.Read(sslStream)); }

        public void Write(string message) { HttpStream.Write(sslStream, Encoding.UTF8.GetBytes(message)); }

        public void Close() { if (sslStream != null) sslStream.Close(); }

    }
}
