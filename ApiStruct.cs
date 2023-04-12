using System.Collections.Generic;

public class UserLoginReq 
{
    public string username;
    public string password;
}

public class HttpCommonRes
{
    public int code;
    public string msg;
    public object data;
}

public class UserLoginRes
{
    public User user;
    public string token;
    public int expires_at;
    public bool is_new_token;
    public bool is_new_reg;
}

public class User
{
    public int id;
    public int created_at;
    public int project_id;
    public int status;
}

public class GatewayConfig
{
    public string listenIp;
    public string outIp;
    public string outDomain;
    public string wsPort;
    public string tcpPort;
    public string udpPort;
    public string wsUri;
    public int default_protocol_type;
    public int default_content_type;
    public string loginAuthType;
    public string login_auth_secret_key;
    public int maxClientConnMum;
    public int msg_content_max;
    public int io_timeout;
    public int connTimeout;
    public int client_heartbeat_time;
    public int server_heartbeat_time;
}

public class GatewayMsg
{
    public int ContentType;
    public int ProtocolType;
    public byte[] Content;
    public int ServiceId;
    public int FuncId;
}
