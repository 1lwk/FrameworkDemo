using System;
using System.Collections.Generic;

// ͨ�ö�����࣬�������κξ����޲ι��캯�������� T��
public class ObjectPool<T> : IDisposable where T : new()
{
    // ��󻺴����������Ĭ��ֵΪ32��
    public int MaxCacheCount = 32;

    // �����б��洢�����õĶ���ʵ����
    private static LinkedList<T> cache;

    // ���ͷŶ���ʱ���õĻص�������
    private Action<T> onRelease;

    // ���캯������ʼ�������б���ͷŻص���
    public ObjectPool(Action<T> onRelease)
    {
        cache = new LinkedList<T>();
        this.onRelease = onRelease;
    }

    // ��ȡ����ʵ��������������ж����򷵻ص�һ�����󣻷��򣬴���һ���µĶ���
    public T Obtain()
    {
        T value;
        if (cache.Count == 0)
        {
            value = new T();
        }
        else
        {
            value = cache.First.Value;
            cache.RemoveFirst();
        }
        return value;
    }

    // �ͷŶ���ʵ��������Żػ����б��С���������������򲻽����κβ�����
    public void Release(T value)
    {
        if (cache.Count >= MaxCacheCount)
            return;

        // �����ͷŻص�������
        onRelease?.Invoke(value);

        // ��������ӵ������б��ĩβ��
        cache.AddLast(value);
    }

    // ��ջ����б�
    public void Clear()
    {
        cache.Clear();
    }

    // �ͷ���Դ���������б�ͻص���������Ϊnull��
    public void Dispose()
    {
        cache = null;
        onRelease = null;
    }
}

// ר����Queue����ĳ��ࡣ
public class QueuePool<T>
{
    // ����Queue����أ��ͷ�ʱ��ն������ݡ�
    private static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>((value) => value.Clear());

    // ��ȡQueue����ʵ����
    public static Queue<T> Obtain() => pool.Obtain();

    // �ͷ�Queue����ʵ����
    public static void Release(Queue<T> value) => pool.Release(value);

    // ���Queue����ء�
    public static void Clear() => pool.Clear();
}

// ר����List����ĳ��ࡣ
public class ListPool<T>
{
    // ����List����أ��ͷ�ʱ����б����ݡ�
    private static ObjectPool<List<T>> pool = new ObjectPool<List<T>>((value) => value.Clear());

    // ��ȡList����ʵ����
    public static List<T> Obtain() => pool.Obtain();

    // �ͷ�List����ʵ����
    public static void Release(List<T> value) => pool.Release(value);

    // ���List����ء�
    public static void Clear() => pool.Clear();
}

// ר����HashSet����ĳ��ࡣ
public class HashSetPool<T>
{
    // ����HashSet����أ��ͷ�ʱ��ռ������ݡ�
    private static ObjectPool<HashSet<T>> pool = new ObjectPool<HashSet<T>>((value) => value.Clear());

    // ��ȡHashSet����ʵ����
    public static HashSet<T> Obtain() => pool.Obtain();

    // �ͷ�HashSet����ʵ����
    public static void Release(HashSet<T> value) => pool.Release(value);

    // ���HashSet����ء�
    public static void Clear() => pool.Clear();
}

// ר����Dictionary����ĳ��ࡣ
public class DictionaryPool<K, V>
{
    // ����Dictionary����أ��ͷ�ʱ����ֵ����ݡ�
    private static ObjectPool<Dictionary<K, V>> pool = new ObjectPool<Dictionary<K, V>>((value) => value.Clear());

    // ��ȡDictionary����ʵ����
    public static Dictionary<K, V> Obtain() => pool.Obtain();

    // �ͷ�Dictionary����ʵ����
    public static void Release(Dictionary<K, V> value) => pool.Release(value);

    // ���Dictionary����ء�
    public static void Clear() => pool.Clear();
}
