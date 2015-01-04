glancer (Version 0.0.1.0)
=======

# glancer  

glancer is HTTPS decode proxy server.  
It can SSL decoding and filtering.  
In addition, It can rewrite the packet on the proxy.  

# License  
Apache License, Version 2.0  
  
# How to  
  
1. Install secret.pfx in local certificate store.  
2. Resolution machine DNS at the hostname of the certificate.  
3. Please hide the browser certificate warning. 
  

`using System;`
`using System.Security.Cryptography.X509Certificates;`
``
`namespace Glancer`
`{`
`    class Program`
`    {`
`        public static int Main(string[] args)`
`        {`
`            string certificate = @"secret.pfx";`
`            string password = @"";`
`            X509Certificate serverCertificate = new X509Certificate(certificate, password);`
`
            SslTcpServer server = new SslTcpServer(serverCertificate, 8080, 5000, 5000);
            server._protocolLogDir = @"C:\tmp";
            server._traceLogDir = @"C:\tmp";
            server.RunServer();

            return 0;
        }
    }
}`
  

# Rewrite the packet  
  
Try implement IHttpEventListener  

    `interface IHttpEventListener
    {
        HttpRequestObject OnHttpRequestClient(HttpRequestObject request, Stream serverStream, Stream clientStream);
        HttpRequestObject OnHttpRequestServer(HttpRequestObject request, Stream serverStream, Stream clientStream);
        HttpResponseObject OnHttpResponseClient(HttpResponseObject response, Stream serverStream, Stream clientStream);
        HttpResponseObject OnHttpResponseServer(HttpResponseObject response, Stream serverStream, Stream clientStream);
    }`
  
  
  

#  Attention  

glancer is not complete the certificate verification(domain check) of the remote server.  
this version is still unstable.


