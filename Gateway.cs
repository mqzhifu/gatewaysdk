using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Google.Protobuf;
using LitJson;
using Pb;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEditor.VersionControl;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using static UnityEditor.MaterialProperty;
using static UnityEditor.Progress;
using static UnityEditor.VersionControl.Asset;


public delegate void GatewayReceiveMsg(byte[] readBuff);
public delegate void ConnSuccess();

public class Gateway
{
    //public const int CONTENT_TYPE_JSON = 1;
    //public const int CONTENT_TYPE_PROTOBUF = 2;

    //public const int LOGIN_STATUS_INIT = 1;
    //public const int LOGIN_STATUS_ING = 2;
    //public const int LOGIN_STATUS_FAILED = 3;
    //public const int LOGIN_STATUS_OK = 4;

    //public const int PROTOCOL_TYPE_WS = 2;
    //public const int PROTOCOL_TYPE_TCP = 1;


    public GatewayConfig gatewayConfig;
    public ProtocolAction protocolAction;//协议函数定义 - 控制类
    public int contentType;//传输内容体类型
    public int protocolType;//传输协议类型
    public string userToken;//用户登陆成功后的 token

    private GatewayUtil gatewayUtil;//

    public Websocket websocket;
    public CancellationToken ct;
    public Log log;
    public GatewayHook gatewayHook;
    public int loginStatus;//登陆状态。长连接建立成功后，需要登陆验证成功后，才可以有后续操作

    public enum CONTENT_TYPE
    {
        JSON = 1,
        PROTOBUF = 2,
    }

    public enum PROTOCOL_TYPE
    {
        TCP = 1,
        WS = 2,
    }

    public enum LOGIN_STATUS
    {
        INIT = 1,
        ING = 2,
        FAILED = 3,
        SUCCESS = 4,

    }

    public Gateway(int contentType, int protocolType, GatewayConfig gatewayConfig, ProtocolAction protocolAction, string userToken)
    {
        this.log = new Log(1, "Gateway ");
        this.CheckConstructor(contentType, protocolType, gatewayConfig, protocolAction, userToken);
        this.gatewayUtil = new GatewayUtil();

        this.gatewayHook = new GatewayHook();
        this.loginStatus = (int)Gateway.LOGIN_STATUS.INIT;

        this.contentType = contentType;
        this.protocolType = protocolType;

        this.gatewayConfig = gatewayConfig;
        this.protocolAction = protocolAction;

        this.userToken = userToken;
    }
    //初始化，连接后端服务器
    public void Init()
    {

        if (this.protocolType == (int)Gateway.PROTOCOL_TYPE.WS)
        {
            this.log.Info(" init ws connet...");
            this.websocket = new Websocket(this.gatewayConfig , this.ReceiveMsg,this.ConnSuccessBack);
            this.websocket.Init();
        }
        else
        {

        }

        
        //this.ReceiveMsg();

    }
    public void ConnSuccessBack()
    {
        this.CS_Login();
    }
    //接收后端发送的消息
    public void ReceiveMsg(byte[] readBuff)
    {
        var msg = this.gatewayUtil.UnpackMsg(readBuff);
        this.ProcessContent(msg);


    }

    //处理消息(拆包后，具体的消息内容)
    //public void ProcessContent(int contentType, byte[] content, int serviceId, int funcId)
    public void ProcessContent(Pb.Msg msg)
    {
        string serviceIdFuncId = Convert.ToString(msg.ServiceId) + Convert.ToString(msg.FuncId);
        var item = this.protocolAction.GetServerOneById(msg.ServiceId, msg.FuncId);
        if (item == null)
        {

        }
        var content = System.Text.Encoding.Default.GetBytes(msg.Content);
        //var content = msg.Content.ToCharArray();
        //msg.Content
        //var content = Encoding.UTF8.GetBytes(msg.Content);
        //Google.Protobuf.ByteString.
        switch (serviceIdFuncId)
        {
            case "90112":
                var loginRes = this.gatewayHook.ParserSC_Login(contentType, content);
                if (loginRes.Code != 200)
                {
                    this.log.Err("code: " + loginRes.Code + " , ErrMsg: " + loginRes.ErrMsg);
                    this.loginStatus = (int)Gateway.LOGIN_STATUS.FAILED;
                }
                else
                {
                    this.log.Info("login success.");
                    this.loginStatus = (int)Gateway.LOGIN_STATUS.SUCCESS;
                }
                break;
            case "90114"://SC_Ping
                var pingReq = this.gatewayHook.ParserSC_Ping(contentType, content);
                var pongRes = new Pb.PongRes();
                pongRes.ClientReqTime = pingReq.ClientReqTime;
                pongRes.ClientReqTime = pingReq.ClientReceiveTime;
                pongRes.ServerReceiveTime = Util.GetTimestamp();
                pongRes.ServerResponseTime = Util.GetTimestamp();
                var sendContent = this.CompressionContent(this.protocolType, pongRes);

                this.SendMsgById(90, 108, sendContent);

                break;
            case "90116"://SC_Pong
                var contentObj = this.gatewayHook.ParserSC_Pong(contentType, content);
                break;
            case "90120"://SC_KickOff
                var kickOff = this.gatewayHook.ParserSC_KickOff(contentType, content);
                this.log.Info("ws conn has kickOff");
                break;
            case "90122"://SC_ProjectPushMsg
                var projectPushMsg = this.gatewayHook.ParserSC_ProjectPushMsg(contentType, content);
                break;
            case "90124"://SC_SendMsg
                var pb_msg = this.gatewayHook.ParserSC_SendMsg(contentType, content);
                break;
            default:
                Debug.Log("no hit.");
                break;
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
    public void SendMsg(ActionMapItem item, byte[] content)
    {
        var msg = new Pb.Msg();
        msg.Content = System.Text.Encoding.Default.GetString(content);
        //msg.Content = System.Text.Encoding.ASCII.GetString(content);
        msg.ServiceId = item.service_id;
        msg.FuncId = item.func_id;
        msg.ContentType = this.contentType;
        msg.ProtocolType = this.protocolType;

        var packContent = this.gatewayUtil.PackMsg(msg,content);
        //return;
        if (this.protocolType == (int)Gateway.PROTOCOL_TYPE.WS)
        {
            this.websocket.SendMsg(packContent);
        }
        else
        {

        }

    }
    //public async void SendMsg(ActionMapItem item, byte[] content)
    //{

    //var session = "1234567890";

    //Debug.Log("sendMsg len: " + content.Length + ",contentType:" + this.contentType + ",protocolType:" + this.protocolType + " ,  serviceId:" + item.service_id + " , funcId:" + item.func_id + ",content:" + content);


    //var contentLenByte = Util.IntToBytes(content.Length);
    //var contentTypeByte = Util.Byte4Tobyte1(Util.IntToBytes(this.contentType));
    //var protocolTypeByte = Util.Byte4Tobyte1(Util.IntToBytes(this.protocolType));
    //var serviceIdByte = Util.Byte4Tobyte1(Util.IntToBytes(item.service_id));
    //var funcIdByte = Util.Byte4Tobyte2(Util.IntToBytes(item.func_id));

    //var endChat = System.Text.Encoding.Default.GetBytes("\f");
    //byte[] sessionByteArray = System.Text.Encoding.Default.GetBytes(session);

    //var byteArr = Util.Copybyte(contentLenByte, contentTypeByte);
    //byteArr = Util.Copybyte(byteArr, protocolTypeByte);
    //byteArr = Util.Copybyte(byteArr, serviceIdByte);
    //byteArr = Util.Copybyte(byteArr, funcIdByte);
    //byteArr = Util.Copybyte(byteArr, sessionByteArray);
    ////byteArr = this.copybyte(byteArr, contentByteArray);
    //byteArr = Util.Copybyte(byteArr, content);
    //byteArr = Util.Copybyte(byteArr, endChat);



    //await clientWebSocket.SendAsync(byteArr, WebSocketMessageType.Binary, true, this.ct); //发送数据

    //}

    public void CS_Login()
    {
        this.loginStatus = (int)Gateway.LOGIN_STATUS.ING;
        var pbLogin = new Pb.Login();
        //pbLogin.Token = "aaaa";
        pbLogin.Token = this.userToken;
        var compressionContent = this.CompressionContent(this.contentType, pbLogin);
        this.SendMsgById(90, 104, compressionContent);
    }

    public void CS_Ping()
    {
        var pingReq = new Pb.PingReq();
        pingReq.ClientReqTime = Util.GetTimestamp();

        var compressionContent = this.CompressionContent(this.contentType, pingReq);
        this.SendMsgById(90, 106, compressionContent);

    }

    //=============================偏简单功能性的函数=============================

    //发送消息时，要将obj 转换成 byte[]
    public byte[] CompressionContent(int contentType, Google.Protobuf.IMessage c)
    {
        //Debug.Log("CompressionContent contentType:" + contentType);
        if (this.contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            var str = c.ToString();
            //Debug.Log(str);
            return System.Text.Encoding.GetEncoding("UTF8").GetBytes(str);
        }
        else
        {
            return c.ToByteArray();
        }
    }

    private bool CheckContentType(int flag)
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

    private bool CheckProtoclType(int flag)
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

    private bool CheckConstructor(int contentType, int protocolType, GatewayConfig gatewayConfig, ProtocolAction protocolAction, string userToken)
    {
        this.log.Info("construction , contentType:" + contentType + " , protocolType:" + protocolType + " , userToken:" + userToken);
        if (!this.CheckProtoclType(protocolType))
        {
            this.log.Err("contentType value err . ");
        }

        if (!this.CheckProtoclType(contentType))
        {
            this.log.Err("contentType value err . ");
        }

        if (userToken == "")
        {
            this.log.Err("userToken empty . ");
        }

        if (protocolAction == null || protocolAction.map.client == null || protocolAction.map.server == null)
        {
            this.log.Err("actionMap null . ");
        }

        if (protocolAction.map.client.Count == 0 || protocolAction.map.server.Count == 0)
        {
            this.log.Err("actionMap count == 0 ");
        }

        if (gatewayConfig.outIp == "" || gatewayConfig.wsPort == "" || gatewayConfig.wsUri == "")
        {
            this.log.Err("check gatewayConfig , outIp || wsPort || wsUri is empty~ ");
        }

        return true;
    }
}


class GatewayUtil
{
    public Log log;
    //解析C端发送的数据，这一层，对于用户层的content数据不做处理
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
    //拆包(后端发送的消息)
    public Pb.Msg UnpackMsg(byte[] readBuff)
    {
        //Debug.Log("ParserOneMsg");
        //byte[] strByte = System.Text.Encoding.Default.GetBytes(str);
        var strByte = readBuff;


        var contentLengthByte = Util.ByteSubstr(readBuff, 0, 4);
        //this.PrintBytesArray(contentLengthByte);
        int contentLen = BitConverter.ToInt32(Util.IntByteReversal(contentLengthByte));
        //Debug.Log("contentLen:"+ contentLen);
        int contentType = strByte[4];
        int protocolType = strByte[5];
        int serviceId = strByte[6];
        var funcIdByte = Util.ByteSubstr(readBuff, 7, 2);
        int funcId = BitConverter.ToInt16(Util.Int16ByteReversal(funcIdByte));
        byte[] sessionBytes = Util.ByteSubstr(strByte, 9, 10);
        string session = System.Text.Encoding.Default.GetString(sessionBytes);
        byte[] contentByte = Util.ByteSubstr(strByte, 19, contentLen);
        //string content = System.Text.Encoding.Default.GetString(contentByte);

        var oneMsg = new Msg();
        oneMsg.Content = System.Text.Encoding.Default.GetString(contentByte);
        oneMsg.ServiceId = serviceId;
        oneMsg.FuncId = funcId;
        oneMsg.ContentType = contentType;
        oneMsg.ProtocolType = protocolType;
        //oneMsg.ContentBytes = Google.Protobuf.ByteString.CopyFrom(contentByte, 0, contentByte.Length - 1);
        //this.log.Info("contentLen:" + contentLen + " , contentType:" + contentType + " , protocolType:" + protocolType + " , serviceId:" + serviceId + " , funcId:" + funcId + " , session:" + session);

        //this.ProcessContent(contentType, contentByte, serviceId, funcId);
        return oneMsg;
    }
    public byte[] PackMsg(Pb.Msg msg, byte[] content)
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
        //byte[] contentByteArray = System.Text.Encoding.Default.GetBytes(content);

        //var byteArrLength = 4 + 1 + 1 + 1 + 2 + 10 + content.Length + 1;
        //var byteArr = new byte[byteArrLength];
        var byteArr = Util.Copybyte(contentLenByte, contentTypeByte);
        byteArr = Util.Copybyte(byteArr, protocolTypeByte);
        byteArr = Util.Copybyte(byteArr, serviceIdByte);
        byteArr = Util.Copybyte(byteArr, funcIdByte);
        byteArr = Util.Copybyte(byteArr, sessionByteArray);
        //byteArr = this.copybyte(byteArr, contentByteArray);
        //byteArr = Util.Copybyte(byteArr, System.Text.Encoding.Default.GetBytes(msg.Content));
        //byteArr = Util.Copybyte(byteArr, System.Text.Encoding.ASCII.GetBytes(msg.Content));
        byteArr = Util.Copybyte(byteArr,content);
        byteArr = Util.Copybyte(byteArr, endChat);

        return byteArr;
    }
}