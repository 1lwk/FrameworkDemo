using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 发送消息的消息号和内容
/// </summary>
public class MessageType 
{
    public struct TestUIMessage
    {
        public string name;
        public int count;
    }

    public struct TestUICallMessage
    {
        public string name;
        public int callcount;
    }
}
