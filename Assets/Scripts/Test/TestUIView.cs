using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class TestUIView :MonoBehaviour
{
    public Button testbtn;
    public Text test_text;

    private void Start()
    {
        Profiler.BeginSample("GC_Test");
        GameManager.Message.Subscribe<MessageType.TestUICallMessage>(TestUICallMessage);
        testbtn.onClick.AddListener(async () =>
        {
             await GameManager.Message.Post<MessageType.TestUIMessage>(new MessageType.TestUIMessage { name="�����Ҳ��Ե�����",count=10});
        });
        Profiler.EndSample();
    }

    private async Task TestUICallMessage(MessageType.TestUICallMessage arg)
    {
        test_text.text = arg.name;
        Debug.Log("�����ҵĻص���Ϣ"+arg.name+"�Ͳ���������"+arg.callcount);
        await Task.Yield();
    }

    private void Update()
    {
        
    }
}
