glancer (Version 0.2.1.0)
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
3. Please hide the browser certificate warning. (Off Domain check. etc..)
4. Setting ConfigFile.  

```
  <appSettings>
    <add key="CERTIFICATE" value="secret.pfx"/>
    <add key="CERTIFICATE_PASSWORD" value=""/>
    <add key="PROTOCOLLOG_DIR" value=""/>
    <add key="TRACELOG_DIR" value=""/>
    <add key="PROTOCOLLOG" value="off" />
    <add key="TRACELOG" value="off" />
    <add key="LISTEN_PORT" value="8080"/>
    <add key="READ_TIMEOUT" value="5000"/>
    <add key="WRITE_TIMEOUT" value="5000"/>
  </appSettings>
```

# Rewrite the packet  
  
Try implement IHttpEventListener  

```
    interface IHttpEventListener
    {
        HttpRequestObject OnHttpRequestClient(HttpRequestObject request, Stream serverStream, Stream clientStream);
        HttpRequestObject OnHttpRequestServer(HttpRequestObject request, Stream serverStream, Stream clientStream);
        HttpResponseObject OnHttpResponseClient(HttpResponseObject response, Stream serverStream, Stream clientStream);
        HttpResponseObject OnHttpResponseServer(HttpResponseObject response, Stream serverStream, Stream clientStream);
    }
```
  
  
  

#  Attention  

glancer is not complete the certificate verification(domain check) of the remote server.  
this version is still unstable.


