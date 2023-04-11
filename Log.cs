using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Log
{
    public string prefix;
    public int level;

    public Log(int level,string prefix)
    {
        this.level = level;
        this.prefix = prefix;
    }

    public void Info(object message)
    {
        Debug.Log(this.prefix + message);
    }

    public void Err(object message)
    {
        Debug.Log(this.prefix + " err " + message);
    }
}
