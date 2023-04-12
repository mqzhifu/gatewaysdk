using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using UnityEditor.VersionControl;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

//简易日志类，用于输出调试信息
public class Log
{
    public enum LEVEL
    {
        
        INFO    = 1,
        WARN    = 2,
        ERROR   = 4,
        DEBUG   = 8,
        ALL     = 15,
    }
    public string prefix;
    public int level;

    public Log(int level,string prefix)
    {
        this.level = level;
        this.prefix = prefix;
    }

    public void debug(object message)
    {
        this.Print((int)Log.LEVEL.DEBUG, message);
    }

    public void Info(object message)
    {
        this.Print((int)Log.LEVEL.INFO, message);
    }

    public void Err(object message)
    {
        this.Print((int)Log.LEVEL.ERROR, message);
    }

    public void Print(int level ,object messsage)
    {
        int rs = this.level & level;
        if (rs == level)
        {
            Debug.Log("[" + this.GetLevelStr(level) +"]" +  " ["+ this.prefix +"]" + messsage);
        }
    }

    public string GetLevelStr(int level)
    {
        var list = Enum.GetValues(typeof(Log.LEVEL));
        foreach (var value in list)
        {
            if (level == (int)value)
            {
                return value.ToString();
            }
        }
        return "";
    }
}
