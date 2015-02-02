using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Glancer
{
    class HttpStreamProxy
    {

        static public void Proxy(TcpClient serverSocket, IHttpEventListener listner)
        {

            Stream server = serverSocket.GetStream();
            Stream client = null;
            TcpClient clinetSocket = null;
            bool bConnect = false;
            string host = string.Empty;
            bool bSSL = false;


            while (true)
            {

                //-----------------------------------------
                // Request.
                // Clinet -> Proxy
                //-----------------------------------------
                
                HttpRequestObject request = null;
                try
                {
                    request = HttpStream.ReadRequest(server);

                    if (request == null)
                    {
                        break;
                    }

                    //-----------------------------------------
                    // SSL Handshake.
                    //-----------------------------------------
                    if (SslHandshake.IsSslConnection(request))
                    {
                        SslStreamSet sslset = SslHandshake.Handshake(clinetSocket, request, server);
                        server = sslset._serverStream;
                        client = sslset._clientStream;
                        bConnect = true;
                        bSSL = true;
                        continue;
                    }
                    //-----------------------------------------
                    // HTTP Conection.
                    //-----------------------------------------
                    else if ( bSSL == false && (bConnect == false || host != request._header._host))
                    {
                        if (clinetSocket != null)
                        {
                            client.Close();
                            clinetSocket.Close();
                        }

                        clinetSocket = new TcpClient();
                        clinetSocket.Connect(request._header._host, request._header._port);
                        client = clinetSocket.GetStream();
                        bConnect = true;
                    }

                    //-----------------------------------------
                    // save host.
                    //-----------------------------------------
                    host = request._header._host;


                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        request = listner.OnHttpRequestClient(request, server, client);
                    }

                }catch(Exception ex){
                    Close(serverSocket, clinetSocket, server, client);
                    throw ex;
                }


                //-----------------------------------------
                // Modify Request.
                // Proxy -> Server
                //-----------------------------------------
                try
                {
                    HttpHeader.ModifyProxyRequest(request._header);

                }catch(Exception ex){
                    Close(serverSocket, clinetSocket, server, client);
                    throw ex;
                }



                //-----------------------------------------
                // Request.
                // Proxy -> Server
                //-----------------------------------------
                try
                {
                    HttpStream.Write(request, client);

                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        request = listner.OnHttpRequestServer(request, server, client);
                    }

                }
                catch (Exception ex)
                {
                    Close(serverSocket, clinetSocket, server, client);
                    throw ex;
                }





                //-----------------------------------------
                // Response.
                // Proxy <- Server
                //-----------------------------------------
                HttpResponseObject response = null;
                try
                {
                    response = HttpStream.ReadResponse(client);
                    if (response == null)
                    {
                        break;
                    }

                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        response = listner.OnHttpResponseClient(response, server, client);
                    }

                }
                catch (Exception ex)
                {
                    Close(serverSocket, clinetSocket, server, client);
                    throw ex;
                }




                //-----------------------------------------
                // Response.
                // Client <- Proxy
                //-----------------------------------------
                try
                {
                    HttpStream.Write(response, server);

                    //-----------------------------------------
                    // Observer.
                    //-----------------------------------------
                    if (listner != null)
                    {
                        response = listner.OnHttpResponseServer(response, server, client);
                    }

                }
                catch (Exception ex)
                {
                    Close(serverSocket, clinetSocket, server, client);
                    throw ex;
                }


                if (request._header._isKeepAllive == false || response._header._isKeepAllive == false)
                {
                    break;
                }

            }

            Close(serverSocket, clinetSocket, server, client);
        }

        static void Close(TcpClient serverSocket, TcpClient clinetSocket, Stream server, Stream client)
        {
            if (clinetSocket != null)
            {
                client.Close();
                clinetSocket.Close();
            }
            server.Close();
            serverSocket.Close();
        }
    }
}
