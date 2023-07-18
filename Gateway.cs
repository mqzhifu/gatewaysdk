using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using LitJson;
using Pb;
using UnityEngine;


//长连接收到消息后，回调
public delegate void ReceiveMsgCallback(byte[] readBuff);
//长连接建立连接后，回调
public delegate void ConnectCallback(int status,string msg);
//长连接建立连接后，回调
public delegate void FDExceptionCallback( string msg);
//把从服务端接收到的消息，回调给：调用者
public delegate void BackMsg(GatewayMsg msg);



//网关类，主要是长连接收发消息
public class Gateway
{
    public Log log;
    public GatewayConfig    gatewayConfig;//协议定义映射表
    public ProtocolAction   protocolAction;//协议函数定义 - 控制类
    private GatewayUtil gatewayUtil;//工具类，拆包/解包
    
    public Websocket        websocket;
    public Tcp              tcp;

    public int contentType;//传输内容体类型
    public int protocolType;//传输协议类型
    public string userToken;//用户登陆成功后的 token

    public int                  logLevel;
    public GatewayHook          gatewayHook;
    public int                  loginStatus;//登陆状态：长连接建立成功后，需要登陆验证成功后，才可以有后续操作    
    public int                  connectTimeout;     //创建连接时：超时时间(秒)
    public bool                 stopHeartbeat;
    public List<Pb.Heartbeat>   HeartbeatQueue;

    public FDExceptionCallback  fdExceptionCallback;
    public ConnectCallback      connectCallback;    //长连接，成功回调
    public BackMsg              backMsg;//消息回调
    

    public enum CONN_STATE
    {
        INIT        = 1,//初始化
        ING         = 2,//连接中
        FAILED      = 3,//连接失败
        SUCCESS     = 4,//连接成功
        DISCONNECT  = 5,//连接已断开
    }

    //传输内容的格式体
    public enum CONTENT_TYPE
    {
        JSON = 1,
        PROTOBUF = 2,
    }
    //使用的网络协议
    public enum PROTOCOL_TYPE
    {
        TCP = 1,
        WS  = 2,
    }
    //登陆状态
    public enum LOGIN_STATUS
    {
        INIT    = 1,
        ING     = 2,
        FAILED  = 3,
        SUCCESS = 4,
    }
    //构造函数
    public Gateway(int logLevel)
    {
        this.logLevel = logLevel;
        this.log = new Log(logLevel, "Gateway ");
        this.backMsg = null;
        this.gatewayUtil = new GatewayUtil();
        this.gatewayHook = new GatewayHook();
        this.loginStatus = (int)Gateway.LOGIN_STATUS.INIT;
        this.connectTimeout = 2;


    }
    //初始化，开始连接后端服务器
    public void Init(int contentType, int protocolType, GatewayConfig gatewayConfig, ProtocolAction protocolAction, string userToken, BackMsg backMsg, ConnectCallback connectCallback, FDExceptionCallback fdExceptionCallback)
    {
        //this.gatewayUtil
        this.gatewayUtil.CheckConstructor(contentType, protocolType, gatewayConfig, protocolAction, userToken);

        this.contentType = contentType;
        this.protocolType = protocolType;
        this.gatewayConfig = gatewayConfig;
        this.protocolAction = protocolAction;
        this.backMsg = backMsg;
        this.userToken = userToken;
        this.fdExceptionCallback = fdExceptionCallback;
        this.connectCallback = connectCallback;
        this.HeartbeatQueue = new List<Pb.Heartbeat>();//保留五次心跳的数据

        if (this.protocolType == (int)Gateway.PROTOCOL_TYPE.WS)
        {
            this.log.Info(" init ws connect...");
            //建立长连接
            this.websocket = new Websocket(this.gatewayConfig, this.ReceiveMsg, this.ConnectCallback, this.logLevel, (int)Websocket.WS_PROTOCOL.WS,this.connectTimeout,this.FDException);
            this.websocket.Init();
        }
        else
        {
            this.tcp = new Tcp(this.gatewayConfig, this.ReceiveMsg, this.ConnectCallback, this.logLevel, (int)Websocket.WS_PROTOCOL.WS, this.connectTimeout, this.FDException);
            this.tcp.Init();
        }
    }
    //========================回调函数//========================
    //fd 连接出现了异常
    public void FDException(string msg )
    {
        this.stopHeartbeat = true;
        if (this.fdExceptionCallback != null)
        {
            this.fdExceptionCallback(msg);
        }
    }
    //创建连接：成功/失败，后，回调此函数（即使失败，也会回调，因为其内部有定时器）
    public void ConnectCallback(int status, string msg)
    {
        if (status == (int)Gateway.CONN_STATE.SUCCESS)
        {
            if (this.connectCallback != null)
            {
                this.connectCallback(status, msg);
            }
            this.CS_Login();
        }
        else
        {
            this.throwExpception("connect failed , msgL:" + msg);
        }
    }

    //接收后端发送的消息
    public void ReceiveMsg(byte[] readBuff)
    {
        var msg = this.gatewayUtil.UnpackMsg(readBuff);
        this.ProcessContent(msg);
    }
    //========================回调函数//========================

    //处理消息(拆包后，具体的消息内容)
    public void ProcessContent(GatewayMsg msg)
    {
        string serviceIdFuncId = Convert.ToString(msg.ServiceId) + Convert.ToString(msg.FuncId);
        var item = this.protocolAction.GetServerOneById(msg.ServiceId, msg.FuncId);
        if (item == null)
        {

        }
        Debug.Log("ReceiveMsg ProcessContent serviceIdFuncId:"+ serviceIdFuncId);
        switch (serviceIdFuncId)
        {
            case "90112"://SC_Login  ,登陆结果
                var loginRes = this.gatewayHook.ParserSC_Login(contentType, msg.Content);
                if (loginRes.Code != 200)
                {
                    this.log.Err("code: " + loginRes.Code + " , ErrMsg: " + loginRes.ErrMsg);
                    this.loginStatus = (int)Gateway.LOGIN_STATUS.FAILED;
                }
                else
                {
                    this.log.Info("login success.");                                  
                    this.loginStatus = (int)Gateway.LOGIN_STATUS.SUCCESS;
                    //连接成功后，试着PING一下，没问题后，再开启心跳
                    this.CS_Ping();
                }
                break;
            case "90114"://SC_Ping，服务端PING客户端
                var pingReq = this.gatewayHook.ParserSC_Ping(contentType, msg.Content);
                var pongRes = new Pb.PongRes();
                pongRes.ClientReqTime = pingReq.ClientReqTime;
                pongRes.ClientReqTime = pingReq.ClientReceiveTime;
                pongRes.ServerReceiveTime = Util.GetTimestamp();
                pongRes.ServerResponseTime = Util.GetTimestamp();
                var sendContent = this.CompressionContent(this.protocolType, pongRes);

                this.SendMsgById(90, 108, sendContent);

                break;
            case "90116"://SC_Pong，服务端响应客户端PING
                var contentObj = this.gatewayHook.ParserSC_Pong(contentType, msg.Content);
                this.log.debug("SC_Pong:"+ contentObj.ServerReceiveTime);
                //this.CS_ProjectPushMsg("test unity send msg from c# script");
                var rtt = contentObj.ServerReceiveTime - contentObj.ClientReqTime;
                this.log.debug("SC_Pong ClientReqTime:" + contentObj.ClientReqTime + " ,ServerReceiveTime:"+ contentObj.ServerReceiveTime + ",ServerResponseTime:" + contentObj.ServerResponseTime+ ",rtt:"+ rtt);
                if (this.gatewayConfig.client_heartbeat_time > 0)
                {
                    this.Heartbeat();
                }

                break;
            case "90120"://SC_KickOff，被其它连接给踢掉了
                var kickOff = this.gatewayHook.ParserSC_KickOff(contentType, msg.Content);
                this.log.Info("ws conn has kickOff");
                break;
            case "90124"://SC_ProjectPushMsg，服务端推送消息
                var projectPushMsg = this.gatewayHook.ParserSC_ProjectPushMsg(contentType, msg.Content);
                this.log.debug("projectPushMsg content:"+ projectPushMsg.Content);
                if (this.backMsg != null)
                {
                    this.backMsg(msg);
                }
                break;
            case "90126"://SC_SendMsg
                var pb_msg = this.gatewayHook.ParserSC_SendMsg(contentType, msg.Content);
                break;
            case "90118"://SC_Heartbeat，服务端响应心跳
                var pbHeartbeat = this.gatewayHook.ParserSC_Heartbeat(contentType, msg.Content);
                break;
            default:
                Debug.Log("no hit.");
                break;
        }
    }
    //关闭连接
    public void Close()
    {
        this.stopHeartbeat = true;
        if (this.protocolType == (int)Gateway.PROTOCOL_TYPE.WS)
        {
            this.websocket.Close();
        }
        else
        {
            this.tcp.Close();
        }
    }
    //定时心跳
    public async void Heartbeat()
    {
        this.stopHeartbeat = false;
        while (true)
        {
            this.log.debug("Heartbeat once:");
            if (this.GetConnectStatus() != (int)Gateway.CONN_STATE.SUCCESS || this.stopHeartbeat)
            {
                this.log.Info("stop Heartbeat");
                break;
            }

            this.CS_Heartbeat();
            await Task.Delay(this.gatewayConfig.client_heartbeat_time * 1000);
        }
    }
    //发送一条消息给后端(通过ID)
    public void SendMsgById(int serviceId, int funcId, byte[] content)
    {
        this.log.Info("sendMsgById id:" + serviceId + funcId + " , content:" + content);
        var item = this.protocolAction.GetClientOneById(serviceId, funcId);
        if (item == null)
        {
            this.log.Err("id not in map");
        }
        this.SendMsg(item, content);
    }
    //发送一条消息给后端(通过名称)
    public void SendMsgByName(string serviceName, string funcName, byte[] content)
    {
        this.log.Info("sendMsgByName serviceName:" + serviceName + funcName + " , content:" + content);
        var item = this.protocolAction.GetClientOneByName(serviceName, funcName);
        if (item == null)
        {
            this.log.Err("id not in map");
        }
        this.SendMsg(item, content);
    }
    //发送消息
    public void SendMsg(ActionMapItem item, byte[] content)
    {
        if (content.Length <= 0 )
        {
            this.log.Err(" SendMsg content.Length <=0");
            return;
        }

        var msg             = new GatewayMsg();
        msg.ServiceId       = item.service_id;
        msg.FuncId          = item.func_id;
        msg.ContentType     = this.contentType;
        msg.ProtocolType    = this.protocolType;
        msg.Content         = content;

        var packContent = this.gatewayUtil.PackMsg(msg);
        //return;
        if (this.protocolType == (int)Gateway.PROTOCOL_TYPE.WS)
        {
            this.websocket.SendMsg(packContent);
        }
        else
        {
            this.tcp.SendMsg(packContent);            
        }

    }
    //推送消息
    public void CS_ProjectPushMsg(string content)
    {
        var projectPushMsg = new Pb.ProjectPushMsg();
        projectPushMsg.SourceProjectId = 6;
        projectPushMsg.TargetProjectId = 6;
        projectPushMsg.TargetUids = "1,2";
        projectPushMsg.Content = content;
        var str = this.CompressionContent(this.contentType, projectPushMsg);
        this.SendMsgById(90, 122, str);
    }
    //登陆
    public void CS_Login()
    {
        this.loginStatus = (int)Gateway.LOGIN_STATUS.ING;
        var pbLogin = new Pb.Login();
        //pbLogin.Token = "aaaa";//测试登陆失败
        pbLogin.Token = this.userToken;
        var compressionContent = this.CompressionContent(this.contentType, pbLogin);
        this.SendMsgById(90, 104, compressionContent);
    }
    //发送ping消息
    public void CS_Ping()
    {
        var pingReq = new Pb.PingReq();
        pingReq.ClientReqTime = Util.GetTimestamp();
        //Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        //pingReq.ClientReqTime = unixTimestamp;

        var compressionContent = this.CompressionContent(this.contentType, pingReq);
        this.SendMsgById(90, 106, compressionContent);
    }
    //发送心跳消息
    public void CS_Heartbeat()
    {
        this.log.debug("CS_Heartbeat once");
        var heartbeat = new Pb.Heartbeat();
        heartbeat.ClientReceiveTime = Util.GetTimestamp();
        heartbeat.RequestId = heartbeat.ClientReceiveTime + "";
        var compressionContent = this.CompressionContent(this.contentType, heartbeat);

        this.SendMsgById(90, 110, compressionContent);
        //this.PushHeartbeat(heartbeat);
    }
    //存储每次心跳记录
    public void PushHeartbeat(Pb.Heartbeat heartbeat )
    {
        var len = this.HeartbeatQueue.Count;
        this.log.debug("PushHeartbeat len:" + len);
        if (len < 5)
        {
            this.HeartbeatQueue.Add(heartbeat);
            return;
        }

        for (var i = 0; i < len; i++)
        {
            if (this.HeartbeatQueue[i].ServerReceiveTime > 0 || this.HeartbeatQueue[i].ServerResponseTime > 0 )
            {
                this.HeartbeatQueue.RemoveAt(i);
            }
        }

        len = this.HeartbeatQueue.Count;
        if (len >=5 )
        {
            this.log.Info("HeartbeatQueue.Count > 5 ,del first element");
            this.HeartbeatQueue.RemoveAt(0);
        }
        this.HeartbeatQueue.Add(heartbeat);

    }
    //接收到服务端的心跳响应后，做更新
    public void UpHeartbeat(Pb.Heartbeat heartbeat)
    {
        for (var i = 0; i < this.HeartbeatQueue.Count; i++)
        {
            if (this.HeartbeatQueue[i].RequestId == heartbeat.RequestId)
            {
                this.HeartbeatQueue[i].ServerReceiveTime = heartbeat.ServerReceiveTime;
                this.HeartbeatQueue[i].ServerReceiveTime = heartbeat.ServerResponseTime;
            }
        }
        this.log.debug("UpHeartbeat not found.");
    }
    //获取最近一次 RTT 值
    public Int64 GetRTT()
    {
        if (this.HeartbeatQueue.Count <=0)
        {
            this.log.debug("GetRTT this.HeartbeatQueue.Count <=0");
            return 0;
        }

        for (var i = this.HeartbeatQueue.Count - 1; i >=0; i--)
        {
            if (this.HeartbeatQueue[i].ServerReceiveTime > 0 || this.HeartbeatQueue[i].ServerResponseTime > 0)
            {
                var rtt = this.HeartbeatQueue[i].ServerResponseTime - this.HeartbeatQueue[i].ClientReqTime;
                return rtt;
            }
        }
        this.log.debug("HeartbeatQueue all element no  ServerResponseTime.");
        return 0;

    }
    //获取当前长连接的状态
    public int GetConnectStatus()
    {
        if (this.protocolType == (int)Gateway.PROTOCOL_TYPE.WS)
        {
            return this.websocket.state;
        }
        else
        {
            return this.tcp.state;
        }
    }


    //=============================偏简单功能性的函数=============================

    //发送消息时，要将obj 转换成 byte[]
    public byte[] CompressionContent(int contentType, Google.Protobuf.IMessage c)
    {
        //Debug.Log("CompressionContent contentType:" + contentType);
        if (this.contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            //这里JS对long int 给转换成了字符串，得给转会long int
            //var jsonS = JsonFormatter.Default.Format(c);
            //this.log.debug("========"+ jsonS);
            var str = c.ToString();
            //Debug.Log(str);
            Regex r = new Regex(@"""\d{13}""");
            Match m = r.Match(str);
            //Debug.Log("m.Success:" + m.Success);
            if (m.Success)
            {
                this.log.debug("match index:"+m.Index );
                var startStr = str.Substring(0,m.Index - 1);
                var middleStr = str.Substring(m.Index+1,13);
                var endStr = str.Substring(m.Index + 13 + 2);

                var newStr = startStr + middleStr + endStr;
                //this.log.debug("newStr:" + newStr);
                str = newStr;


            }
            //return System.Text.Encoding.GetEncoding("UTF8").GetBytes(str);
            return System.Text.Encoding.Default.GetBytes(str);
        }
        else
        {
            return c.ToByteArray();
        }
    }    

    public void throwExpception(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }
}




//工具类，拆包/解包
class GatewayUtil
{
    public Log log;
    //解析C端发送的数据，这一层，对于用户层的 content 数据不做处理
    //1-4字节：当前包数据总长度，~可用于：TCP粘包的情况
    //5字节：content type
    //6字节：protocol type
    //7字节 :服务Id
    //8-9字节 :函数Id
    //10-19：预留，还没想好，可以存sessionId，也可以换成UID
    //19 以后为内容体
    //结尾会添加一个字节：\f ,可用于 TCP 粘包 分隔
    public GatewayUtil()
    {
        this.log = new Log(1, "GatewayUtil ");
    }
    //一个包，最小的长度
    public int MsgMinLength()
    {
        return 4 + 1 + 1 + 1 + 2 + 10 + 1;
    }

    //拆包(后端发送的消息)
    public GatewayMsg UnpackMsg(byte[] readBuff)
    {
        if (readBuff.Length < this.MsgMinLength())
        {
            this.throwExpception("msg length  < "+ this.MsgMinLength());
        }

        //var strByte = readBuff;
        var contentLengthByte = Util.ByteSubstr(readBuff, 0, 4);
        
        int contentLen = BitConverter.ToInt32(Util.IntByteReversal(contentLengthByte));
        int contentType = readBuff[4];
        int protocolType = readBuff[5];
        int serviceId = readBuff[6];
        var funcIdByte = Util.ByteSubstr(readBuff, 7, 2);
        int funcId = BitConverter.ToInt16(Util.Int16ByteReversal(funcIdByte));
        byte[] sessionBytes = Util.ByteSubstr(readBuff, 9, 10);
        string session = System.Text.Encoding.Default.GetString(sessionBytes);
        byte[] contentByte = Util.ByteSubstr(readBuff, 19, contentLen);
        //string content = System.Text.Encoding.Default.GetString(contentByte);
        //this.PrintBytesArray(contentLengthByte);

        if (contentLen != contentByte.Length)
        {
            this.throwExpception("contentLen != contentByte.Length:"+ contentLen +" != "+ contentByte.Length);
        }

        var oneMsg = new GatewayMsg();
        oneMsg.Content = contentByte;
        oneMsg.ServiceId = serviceId;
        oneMsg.FuncId = funcId;
        oneMsg.ContentType = contentType;
        oneMsg.ProtocolType = protocolType;
        //oneMsg.ContentBytes = Google.Protobuf.ByteString.CopyFrom(contentByte, 0, contentByte.Length - 1);
        //this.log.Info("contentLen:" + contentLen + " , contentType:" + contentType + " , protocolType:" + protocolType + " , serviceId:" + serviceId + " , funcId:" + funcId + " , session:" + session);

        //this.ProcessContent(contentType, contentByte, serviceId, funcId);
        return oneMsg;
    }
    //封闭包
    public byte[] PackMsg(GatewayMsg msg)
    {
        var session = "1234567890";
        this.log.Info("sendMsg len: " + msg.Content.Length + ",contentType:" + msg.ContentType + ",protocolType:" + msg.ProtocolType + " ,  serviceId:" + msg.ServiceId + " , funcId:" + msg.FuncId + ",content:" + msg.Content);


        var contentLenByte = Util.IntToBytes(msg.Content.Length);
        var contentTypeByte = Util.Byte4Tobyte1(Util.IntToBytes(msg.ContentType));
        var protocolTypeByte = Util.Byte4Tobyte1(Util.IntToBytes(msg.ProtocolType));
        var serviceIdByte = Util.Byte4Tobyte1(Util.IntToBytes(msg.ServiceId));
        var funcIdByte = Util.Byte4Tobyte2(Util.IntToBytes(msg.FuncId));

        //this.PrintBytesArray(contentLenByte);

        var endChat = System.Text.Encoding.Default.GetBytes("\f");
        byte[] sessionByteArray = System.Text.Encoding.Default.GetBytes(session);

        var byteArr = Util.Copybyte(contentLenByte, contentTypeByte);
        byteArr = Util.Copybyte(byteArr, protocolTypeByte);
        byteArr = Util.Copybyte(byteArr, serviceIdByte);
        byteArr = Util.Copybyte(byteArr, funcIdByte);
        byteArr = Util.Copybyte(byteArr, sessionByteArray);
        byteArr = Util.Copybyte(byteArr, msg.Content);
        byteArr = Util.Copybyte(byteArr, endChat);

        return byteArr;
    }

    public void throwExpception(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }

    public bool CheckContentType(int flag)
    {
        var list = Enum.GetValues(typeof(Gateway.CONTENT_TYPE));
        foreach (var value in list)
        {
            if (flag == (int)value)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckProtoclType(int flag)
    {
        var list = Enum.GetValues(typeof(Gateway.PROTOCOL_TYPE));
        foreach (var value in list)
        {
            if (flag == (int)value)
            {
                return true;
            }
        }
        return false;
    }

    public void CheckConstructor(int contentType, int protocolType, GatewayConfig gatewayConfig, ProtocolAction protocolAction, string userToken)
    {
        this.log.Info("construction , contentType:" + contentType + " , protocolType:" + protocolType + " , userToken:" + userToken);
        if (!this.CheckProtoclType(protocolType))
        {
            this.throwExpception("protocolType value err . ");
        }

        if (!this.CheckContentType(contentType))
        {
            this.throwExpception("contentType value err . ");
        }

        if (userToken == "")
        {
            this.throwExpception("userToken empty . ");
        }

        if (protocolAction == null || protocolAction.map.client == null || protocolAction.map.server == null)
        {
            this.throwExpception("actionMap null . ");
        }

        if (protocolAction.map.client.Count == 0 || protocolAction.map.server.Count == 0)
        {
            this.throwExpception("actionMap count == 0");
        }

        if (gatewayConfig.outIp == "" || gatewayConfig.wsPort == "" || gatewayConfig.wsUri == "")
        {
            this.throwExpception("check gatewayConfig , outIp || wsPort || wsUri is empty~");
        }
    }

}