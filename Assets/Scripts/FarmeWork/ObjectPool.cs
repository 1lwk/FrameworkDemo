using System;
using System.Collections.Generic;

// 通用对象池类，适用于任何具有无参构造函数的类型 T。
public class ObjectPool<T> : IDisposable where T : new()
{
    // 最大缓存对象数量，默认值为32。
    public int MaxCacheCount = 32;

    // 缓存列表，存储可重用的对象实例。
    private static LinkedList<T> cache;

    // 在释放对象时调用的回调方法。
    private Action<T> onRelease;

    // 构造函数，初始化缓存列表和释放回调。
    public ObjectPool(Action<T> onRelease)
    {
        cache = new LinkedList<T>();
        this.onRelease = onRelease;
    }

    // 获取对象实例。如果缓存中有对象，则返回第一个对象；否则，创建一个新的对象。
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

    // 释放对象实例，将其放回缓存列表中。如果缓存已满，则不进行任何操作。
    public void Release(T value)
    {
        if (cache.Count >= MaxCacheCount)
            return;

        // 调用释放回调方法。
        onRelease?.Invoke(value);

        // 将对象添加到缓存列表的末尾。
        cache.AddLast(value);
    }

    // 清空缓存列表。
    public void Clear()
    {
        cache.Clear();
    }

    // 释放资源，将缓存列表和回调方法设置为null。
    public void Dispose()
    {
        cache = null;
        onRelease = null;
    }
}

// 专用于Queue对象的池类。
public class QueuePool<T>
{
    // 创建Queue对象池，释放时清空队列内容。
    private static ObjectPool<Queue<T>> pool = new ObjectPool<Queue<T>>((value) => value.Clear());

    // 获取Queue对象实例。
    public static Queue<T> Obtain() => pool.Obtain();

    // 释放Queue对象实例。
    public static void Release(Queue<T> value) => pool.Release(value);

    // 清空Queue对象池。
    public static void Clear() => pool.Clear();
}

// 专用于List对象的池类。
public class ListPool<T>
{
    // 创建List对象池，释放时清空列表内容。
    private static ObjectPool<List<T>> pool = new ObjectPool<List<T>>((value) => value.Clear());

    // 获取List对象实例。
    public static List<T> Obtain() => pool.Obtain();

    // 释放List对象实例。
    public static void Release(List<T> value) => pool.Release(value);

    // 清空List对象池。
    public static void Clear() => pool.Clear();
}

// 专用于HashSet对象的池类。
public class HashSetPool<T>
{
    // 创建HashSet对象池，释放时清空集合内容。
    private static ObjectPool<HashSet<T>> pool = new ObjectPool<HashSet<T>>((value) => value.Clear());

    // 获取HashSet对象实例。
    public static HashSet<T> Obtain() => pool.Obtain();

    // 释放HashSet对象实例。
    public static void Release(HashSet<T> value) => pool.Release(value);

    // 清空HashSet对象池。
    public static void Clear() => pool.Clear();
}

// 专用于Dictionary对象的池类。
public class DictionaryPool<K, V>
{
    // 创建Dictionary对象池，释放时清空字典内容。
    private static ObjectPool<Dictionary<K, V>> pool = new ObjectPool<Dictionary<K, V>>((value) => value.Clear());

    // 获取Dictionary对象实例。
    public static Dictionary<K, V> Obtain() => pool.Obtain();

    // 释放Dictionary对象实例。
    public static void Release(Dictionary<K, V> value) => pool.Release(value);

    // 清空Dictionary对象池。
    public static void Clear() => pool.Clear();
}
