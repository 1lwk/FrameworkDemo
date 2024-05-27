using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Koakuma.Game;

public class TestMessageHandler : MessageHandler<MessageType.TestUIMessage>
{
    public override async Task HandlerMessage(MessageType.TestUIMessage arg)
    {
        Debug.Log(arg.name);
        Debug.Log(arg.count);
        GameManager.Message.Post<MessageType.TestUICallMessage>(new MessageType.TestUICallMessage { name="���ǲ��ԵĻص�",callcount=arg.count+10}).Coroutine();
        await Task.Yield();
    }
}
