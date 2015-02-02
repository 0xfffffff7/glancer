using System;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;

namespace Glancer
{
    class Program
    {
        public static int Main(string[] args)
        {
            string certificate = ConfigurationManager.AppSettings["CERTIFICATE"];
            string password = ConfigurationManager.AppSettings["CERTIFICATE_PASSWORD"];
            string protocollog_dir =  ConfigurationManager.AppSettings["PROTOCOLLOG_DIR"];
            string tracelog_dir = ConfigurationManager.AppSettings["TRACELOG_DIR"];
            int port = Convert.ToInt32(ConfigurationManager.AppSettings["LISTEN_PORT"]);
            int read_timeout = Convert.ToInt32(ConfigurationManager.AppSettings["READ_TIMEOUT"]);
            int write_timeout = Convert.ToInt32(ConfigurationManager.AppSettings["WRITE_TIMEOUT"]);

            X509Certificate serverCertificate = new X509Certificate(certificate, password);

            SslTcpServer server = new SslTcpServer(serverCertificate, port);
            server._protocolLogDir = protocollog_dir;
            server._traceLogDir = tracelog_dir;
            SslTcpServer._read_timeout = read_timeout;
            SslTcpServer._write_timeout = write_timeout;
            SslTcpServer._protocolLog = (ConfigurationManager.AppSettings["PROTOCOLLOG"] == "on");
            SslTcpServer._traceLog = (ConfigurationManager.AppSettings["TRACELOG"] == "on");

            server.RunServer();

            return 0;
        }
    }
}
