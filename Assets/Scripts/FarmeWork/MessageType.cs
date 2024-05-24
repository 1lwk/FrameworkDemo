using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
