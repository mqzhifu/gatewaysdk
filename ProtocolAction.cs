using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class ProtocolActionMap
{
    public Dictionary<string, ActionMapItem> client;
    public Dictionary<string, ActionMapItem> server;
}

public class ActionMapItem
{
    public string   service_name;
    public int      service_id;
    public int      id;
    public int      func_id;
    public string   func_name;
    public string   request;
    public string   response;
    public string   desc;

}

public class ProtocolAction
{
    public ProtocolActionMap map;
    public ProtocolAction()
    {

    }

    public void SetMap(ProtocolActionMap protocolActionMap)
    {
        this.map = protocolActionMap;
    }

    public ActionMapItem GetClientOneById(int serviceId, int funcId)
    {
        foreach ((string key, ActionMapItem value) in this.map.client)
        {
            if (value.service_id == serviceId && value.func_id == funcId)
            {
                return value;
            }
        }
        Debug.Log("GetClientGetOneById empty id:" + serviceId + funcId);
        return null;
    }

    public ActionMapItem GetClientOneByName(string serviceName, string funcName)
    {
        foreach ((string key, ActionMapItem value) in this.map.client)
        {
            if (value.service_name == serviceName && value.func_name == funcName)
            {
                return value;
            }
        }
        Debug.Log("GetClientOneByName empty id:" + serviceName + funcName);
        return null;
    }

    public ActionMapItem GetServerOneById(int serviceId, int funcId)
    {
        foreach ((string key, ActionMapItem value) in this.map.server)
        {
            if (value.service_id == serviceId && value.func_id == funcId)
            {
                return value;
            }
        }
        Debug.Log("GetClientGetOneById empty id:" + serviceId + funcId);
        return null;
    }
}
