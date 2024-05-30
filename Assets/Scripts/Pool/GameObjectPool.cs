using System;
using System.Collections.Generic;
using TGame.Asset;
using UnityEngine;
using UnityEngine.AddressableAssets;

// 定义一个泛型类 GameObjectPool，用于管理 GameObject 资源
public class GameObjectPool<T> where T : GameObjectPoolAsset
{
    // 用于存储所有的对象池，每个对象池由一个哈希值标识
    private readonly Dictionary<int, Queue<T>> gameObjectPool = new Dictionary<int, Queue<T>>();

    //存储所有的加载请求，等待异步加载处理
    private readonly List<GameObjectLoadRequest<T>> requests = new List<GameObjectLoadRequest<T>>();

    //存储当前正在使用的游戏对象，使用它们的实例ID作为键
    private readonly Dictionary<int, GameObject> usingObjects = new Dictionary<int, GameObject>();

    //同步加载一个游戏对象
    public T LoadGameObject(string path, Action<GameObject> createNewCallback = null)
    {
        //计算路径的哈希值，用于标识对象池
        int hash = path.GetHashCode();

        //尝试获取该哈希值对应的对象池队列，如果不存在则创建一个新的队列
        if (!gameObjectPool.TryGetValue(hash, out Queue<T> q))
        {
            q = new Queue<T>();
            gameObjectPool.Add(hash, q);
        }

        // 如果对象池中没有可用的对象，则加载新的对象
        if (q.Count == 0)
        {
            // 使用 Addressables 同步加载资源
            GameObject prefab = Addressables.LoadAssetAsync<GameObject>(path).WaitForCompletion();
            GameObject go = UnityEngine.Object.Instantiate(prefab); // 实例化游戏对象
            T asset = go.AddComponent<T>(); // 添加自定义组件
            createNewCallback?.Invoke(go); // 调用回调函数（如果存在）
            asset.ID = hash; // 设置对象的哈希值ID
            go.SetActive(false); // 初始化为不激活状态
            q.Enqueue(asset); // 将新创建的对象加入队列
        }

        // 从对象池队列中取出一个对象，并标记为已加载
        {
            T asset = q.Dequeue();
            OnGameObjectLoaded(asset); // 处理对象加载后的操作
            return asset; // 返回加载的对象
        }
    }

    /// <summary>
    /// 异步加载游戏对象
    /// </summary>
    /// <param name="path">需要加载的资源的路径</param>
    /// <param name="callback">每次调用LoadGameObjectAsync，无论是否从缓存里取出的，都会通过这个回调进行通知</param>
    /// <param name="createNewCallback">游戏对象第一次被克隆后调用，对象池取出的复用游戏对象，不会回调</param>
    public void LoadGameObjectAsync(string path, Action<T> callback, Action<GameObject> createNewCallback = null)
    {
        // 创建一个新的加载请求，并加入请求列表中
        GameObjectLoadRequest<T> request = new GameObjectLoadRequest<T>(path, callback, createNewCallback);
        requests.Add(request);
    }

    // 卸载所有游戏对象
    public void UnloadAllGameObjects()
    {
        // 先将所有请求加载完毕
        while (requests.Count > 0)
        {
            UpdateLoadRequests(); // 处理所有加载请求
        }

        // 将所有正在使用的对象卸载
        if (usingObjects.Count > 0)
        {
            List<int> list = new List<int>();
            foreach (var id in usingObjects.Keys)
            {
                list.Add(id);
            }
            foreach (var id in list)
            {
                GameObject obj = usingObjects[id];
                UnloadGameObject(obj); // 卸载具体的游戏对象
            }
        }

        // 将所有缓存的对象清理掉
        if (gameObjectPool.Count > 0)
        {
            foreach (var q in gameObjectPool.Values)
            {
                foreach (var asset in q)
                {
                    UnityEngine.Object.Destroy(asset.gameObject); // 销毁游戏对象
                }
                q.Clear(); // 清空队列
            }
            gameObjectPool.Clear(); // 清空对象池
        }
    }

    // 卸载单个游戏对象
    public void UnloadGameObject(GameObject go)
    {
        if (go == null)
            return;

        // 获取游戏对象上的自定义组件
        T asset = go.GetComponent<T>();
        if (asset == null)
        {
            Debug.LogError($"Unload GameObject失败，找不到GameObjectAsset:{go.name}");
            UnityEngine.Object.Destroy(go); // 如果找不到组件，则直接销毁游戏对象
            return;
        }

        // 将游戏对象重新加入对象池
        if (!gameObjectPool.TryGetValue(asset.ID, out Queue<T> q))
        {
            q = new Queue<T>();
            gameObjectPool.Add(asset.ID, q);
        }
        q.Enqueue(asset); // 加入对象池队列
        usingObjects.Remove(go.GetInstanceID()); // 从正在使用的对象中移除
        go.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().releaseObjectRoot); // 移动到释放对象的根节点
        go.gameObject.SetActive(false); // 设置为不激活状态
    }

    // 更新所有加载请求
    public void UpdateLoadRequests()
    {
        if (requests.Count > 0) //检查请求列表是否不为空
        {
            foreach (var request in requests) //遍历所有请求
            {  
                int hash = request.Path.GetHashCode(); //计算请求路径的哈希值
                if (!gameObjectPool.TryGetValue(hash, out Queue<T> q)) //检查对象池中是否有该哈希值对应的队列
                {
                    q = new Queue<T>(); //如果没有，创建一个新的队列
                    gameObjectPool.Add(hash, q); // 将新队列添加到对象池
                }

                if (q.Count == 0) // 如果队列为空，表示没有可用对象
                {
                    //异步加载资源，并处理加载完成事件
                    Addressables.LoadAssetAsync<GameObject>(request.Path).Completed += (obj) =>
                    {
                        GameObject go = UnityEngine.Object.Instantiate(obj.Result); // 实例化加载的游戏对象
                        T asset = go.AddComponent<T>(); // 为实例化的对象添加自定义组件
                        request.CreateNewCallback?.Invoke(go); //调用请求中的回调函数（如果存在）
                        asset.ID = hash; //设置自定义组件的ID为路径的哈希值
                        go.SetActive(false); //将实例化的对象设置为不激活状态

                        OnGameObjectLoaded(asset); //调用对象加载完成后的处理方法
                        request.LoadFinish(asset); // 标记请求已完成
                    };
                }
                else
                {
                    // 如果对象池中有可用对象，则直接取出
                    T asset = q.Dequeue(); // 从队列中取出一个对象
                    OnGameObjectLoaded(asset); // 调用对象加载完成后的处理方法
                    request.LoadFinish(asset); // 标记请求已完成
                }
            }
            requests.Clear(); // 清空请求列表
        }
    }

    // 处理游戏对象加载后的操作
    private void OnGameObjectLoaded(T asset)
    {
        asset.transform.SetParent(TGameFramework.Instance.GetModule<AssetModule>().usingObjectRoot); //移动到正在使用的根节点
        int id = asset.gameObject.GetInstanceID();
        usingObjects.Add(id, asset.gameObject); //将对象加入正在使用的字典中
    }
}
