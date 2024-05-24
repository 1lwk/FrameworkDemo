using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TestUIView :MonoBehaviour
{
    public Button testbtn;

    private void Start()
    {
        GameManager.Message.Subscribe<MessageType.TestUICallMessage>(TestUICallMessage);
        testbtn.onClick.AddListener(async () =>
        {
             await GameManager.Message.Post<MessageType.TestUIMessage>(new MessageType.TestUIMessage { name="�����Ҳ��Ե�����",count=10});
        });
    }

    private async Task TestUICallMessage(MessageType.TestUICallMessage arg)
    {
        
        Debug.Log("�����ҵĻص���Ϣ"+arg.name+"�Ͳ���������"+arg.callcount);
        await Task.Yield();
    }

    private void Update()
    {
        
    }
}
