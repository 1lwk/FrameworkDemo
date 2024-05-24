using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;

/// <summary>
/// �������࣬���ڹ���ȴ���������ý��
/// </summary>
public class Monitor
{
    // �ֵ䣬���ڴ洢�ȴ����󣬼�Ϊ���ͣ�ֵΪ����
    private readonly Dictionary<Type, object> waitObjects = new Dictionary<Type, object>();

    /// <summary>
    /// ����һ���µĵȴ�������ӵ��ֵ���
    /// </summary>
    /// <typeparam name="T">�ȴ���������ͣ������ǽṹ��</typeparam>
    /// <returns>�µĵȴ�����</returns>
    public WaitObject<T> Wait<T>() where T : struct
    {
        WaitObject<T> o = new WaitObject<T>();
        waitObjects.Add(typeof(T), o);
        return o;
    }

    /// <summary>
    /// ���õȴ�����Ľ�������ֵ����Ƴ�
    /// </summary>
    /// <typeparam name="T">��������ͣ������ǽṹ��</typeparam>
    /// <param name="result">���ֵ</param>
    public void SetResult<T>(T result) where T : struct
    {
        Type type = typeof(T);
        if (!waitObjects.TryGetValue(type, out object o))
            return;

        waitObjects.Remove(type);
        ((WaitObject<T>)o).SetResult(result);
    }

    /// <summary>
    /// �ȴ������࣬���ڱ�ʾ�첽�����Ľ��
    /// </summary>
    /// <typeparam name="T">��������ͣ������ǽṹ��</typeparam>
    public class WaitObject<T> : INotifyCompletion where T : struct
    {
        /// <summary>
        /// ��ȡ�������Ƿ������
        /// </summary>
        public bool IsCompleted { get; private set; }

        /// <summary>
        /// ��ȡ���ֵ
        /// </summary>
        public T Result { get; private set; }

        private Action callback;

        /// <summary>
        /// ���ý�������Ϊ����ɣ����ûص�
        /// </summary>
        /// <param name="result">���ֵ</param>
        public void SetResult(T result)
        {
            Result = result;
            IsCompleted = true;

            Action c = callback;
            callback = null;
            c?.Invoke();
        }

        /// <summary>
        /// ��ȡ��ǰ�ĵȴ����󣨼�����
        /// </summary>
        /// <returns>��ǰ�ĵȴ�����</returns>
        public WaitObject<T> GetAwaiter()
        {
            return this;
        }

        /// <summary>
        /// ע��ص��������ڲ������ʱ����
        /// </summary>
        /// <param name="callback">�ص�����</param>
        public void OnCompleted(Action callback)
        {
            this.callback = callback;
        }

        /// <summary>
        /// ��ȡ�������
        /// </summary>
        /// <returns>�������ֵ</returns>
        public T GetResult()
        {
            return Result;
        }
    }
}