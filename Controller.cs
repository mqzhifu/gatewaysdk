using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

//入口文件
public class Controller 
{
    public Log log;
    public HttpUtil httpUtil;
    public Gateway gateway;
    public ApiLogic apiLogic;
    public ProtocolAction protocolAction;
    public int logLevel;
    public Controller(int logLevel)
    {
        this.log = new Log(logLevel, "Controller ");
        this.logLevel = logLevel;
    }

    public void UseHttp(string dns, string sourceType, string projectId, string access, string username, string password)
    {
        this.httpUtil = new HttpUtil(dns, sourceType, projectId, access,this.logLevel);
        this.apiLogic = new ApiLogic(this.httpUtil,this.logLevel);
        this.apiLogic.LoginBlock(username, password);
    }

    //使用网关组件，它强依赖 apiLogicAwait ，且 apiLogicAwait 要登陆成功
    public void UseGateway(int contentType, int protocolType, BackMsg backMsg, ConnectCallback connectCallback, FDExceptionCallback fdExceptionCallback)
    {
        if (this.httpUtil == null)
        {
            this.throwException("httpUtil null");
        }
        // apiLogicAwait 要登陆成功
        //if (this.apiLogicAwait.initStatus != (int)ApiLogicAwait.API_LOGIC_INIT_STATUS.SUCCESS)
        //{
        //    this.throwExpception("ApiLogicAwait.API_LOGIC_INIT_STATUS != SUCCESS");
        //}
        if (this.apiLogic.userToken == "" || this.apiLogic.userId == 0)
        {
            this.throwException("userToken or userId empty");
        }

        this.protocolAction = new ProtocolAction(this.logLevel);//创建长连接自定义协议管理类
        this.apiLogic.SetProtocolAction(this.protocolAction);
        //请求 HTTP 获取后端的配置数据，这里最好捕获一下异常。
        this.apiLogic.InitGateway();

        this.gateway = new Gateway(this.logLevel);
        //这里最好捕获一下异常，比如：创建连接失败
        this.gateway.Init(contentType, protocolType, this.apiLogic.gatewayConfig, this.protocolAction, this.apiLogic.userToken, backMsg,connectCallback,fdExceptionCallback);
    }
    //
    public void throwException(string errInfo)
    {
        this.log.Err(errInfo);
        throw new Exception(errInfo);
    }
}
