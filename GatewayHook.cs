using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEngine;


//网关类如果进入稳定期后，基本不会改了，而需要发动的部分就在这里了
public class GatewayHook
{

    //public object ParserContent(int contentType, string content, int serviceId, int funcId)
    //{
    //    string serviceIdFuncId = Convert.ToString(serviceId) + Convert.ToString(funcId);
    //    if (contentType == 1)
    //    {
    //        return this.ParserContentJson(content, serviceIdFuncId);
    //    }
    //    else
    //    {
    //        return this.ParserContentProtobuf(content, serviceIdFuncId);
    //    }


    //}

    //public object ParserContentJson(string content,string serviceIdFuncId)
    //{
    //    object ob = null;
    //    switch (serviceIdFuncId)
    //    {
    //        case "90112":
    //            Pb.LoginRes loginRes = JsonParser.Default.Parse<Pb.LoginRes>(content);
    //            ob = loginRes;
    //            break;
    //    }
    //    return ob;
    //}

    //public object ParserContentProtobuf(string content,string serviceIdFuncId)
    //{
    //    object ob = null;
    //    switch (serviceIdFuncId)
    //    {
    //        case "90112":
    //            //Pb.LoginRes loginRes = new Pb.LoginRes();、
    //            var b =  System.Text.Encoding.UTF8.GetBytes(content);
    //            Pb.LoginRes lr = Pb.LoginRes.Parser.ParseFrom(b);
    //            ob = lr;
    //            break;
    //    }
    //    return ob;
    //}

    public Pb.LoginRes ParserSC_Login(int contentType, byte[] content)
    {
        //Pb.LoginRes loginRes = JsonMapper.ToObject<Pb.LoginRes>(@content);
        //Pb.LoginRes loginRes = JsonParser.Default.Parse<Pb.LoginRes>(content);

        Pb.LoginRes ob = null;
        if (contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            string contentStr = System.Text.Encoding.Default.GetString(content);
            Pb.LoginRes loginRes = JsonParser.Default.Parse<Pb.LoginRes>(contentStr);
            ob = loginRes;
        }
        else
        {
            //new MemoryStream();
            //var b = System.Text.Encoding.UTF8.GetBytes(content);
            Pb.LoginRes lr = Pb.LoginRes.Parser.ParseFrom(content);
            ob = lr;
        }
        return ob;
    }

    public Pb.PingReq ParserSC_Ping(int contentType, byte[] content)
    {
        Pb.PingReq ob = null;
        if (contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            string contentStr = System.Text.Encoding.Default.GetString(content);
            ob = JsonParser.Default.Parse<Pb.PingReq>(contentStr);
        }
        else
        {
            ob = Pb.PingReq.Parser.ParseFrom(content);
        }

        return ob;
    }

    public Pb.PongRes ParserSC_Pong(int contentType, byte[] content)
    {
        Pb.PongRes ob = null;
        if (contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            string contentStr = System.Text.Encoding.Default.GetString(content);
            var  obj = JsonParser.Default.Parse<Pb.PongRes>(contentStr);
            ob = obj;
        }
        else
        {
            ob = Pb.PongRes.Parser.ParseFrom(content);
        }
        return ob;
    }
    //SC_KickOff
    public Pb.KickOff ParserSC_KickOff(int contentType, byte[] content)
    {
        Pb.KickOff ob = null;
        if (contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            string contentStr = System.Text.Encoding.Default.GetString(content);
            var obj = JsonParser.Default.Parse<Pb.KickOff>(contentStr);
            ob = obj;
        }
        else
        {
            ob = Pb.KickOff.Parser.ParseFrom(content);
        }
        return ob;
    }

    public Pb.ProjectPushMsg ParserSC_ProjectPushMsg(int contentType, byte[] content)
    {
        Pb.ProjectPushMsg ob = null;
        if (contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            string contentStr = System.Text.Encoding.Default.GetString(content);
            var obj = JsonParser.Default.Parse<Pb.ProjectPushMsg>(contentStr);
            ob = obj;
        }
        else
        {
            ob = Pb.ProjectPushMsg.Parser.ParseFrom(content);
        }
        return ob;
    }

    public Pb.Msg ParserSC_SendMsg(int contentType, byte[] content)
    {
        Pb.Msg ob = null;
        if (contentType == (int)Gateway.CONTENT_TYPE.JSON)
        {
            string contentStr = System.Text.Encoding.Default.GetString(content);
            var obj = JsonParser.Default.Parse<Pb.Msg>(contentStr);
            ob = obj;
        }
        else
        {
            ob = Pb.Msg.Parser.ParseFrom(content);
        }
        return ob;
    }



    //public Pb.XXXX Parser_xxxxx(int contentType, byte[] content)
    //{
    //    Pb.XXXX ob = null;
    //    if (contentType == Websocket.CONTENT_TYPE_JSON)
    //    {
    //        string contentStr = System.Text.Encoding.Default.GetString(content);
    //        ob = JsonParser.Default.Parse<Pb.XXXX>(contentStr);
    //    }
    //    else
    //    {
    //        ob = Pb.XXXX.Parser.ParseFrom(content);
    //    }
    //    return ob;
    //}
}