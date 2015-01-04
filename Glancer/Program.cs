using System;
using System.Security.Cryptography.X509Certificates;

namespace Glancer
{
    class Program
    {
        public static int Main(string[] args)
        {
            string certificate = @"secret.pfx";
            string password = @"";
            X509Certificate serverCertificate = new X509Certificate(certificate, password);

            SslTcpServer server = new SslTcpServer(serverCertificate, 8080, 5000, 5000);
            server._protocolLogDir = @"C:\tmp";
            server._traceLogDir = @"C:\tmp";
            server.RunServer();

            return 0;
        }
    }
}
