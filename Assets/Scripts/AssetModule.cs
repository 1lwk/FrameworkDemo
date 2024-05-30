using UnityEngine;
using UnityEngine.AddressableAssets;

// 定义一个名为 AssetModule 的类，继承自 BaseGameModule
public partial class AssetModule : BaseGameModule
{
#if UNITY_EDITOR
    // 在 Unity 编辑器中，定义一个常量字符串 BUNDLE_LOAD_NAME
    //[XLua.BlackList]
    public const string BUNDLE_LOAD_NAME = "Tools/Build/Bundle Load";
#endif

    // 定义两个 Transform 类型的公有变量，用于存放正在使用和释放的对象
    public Transform usingObjectRoot;
    public Transform releaseObjectRoot;

    // 重写基类 BaseGameModule 的 OnModuleUpdate 方法
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime); // 调用基类的 OnModuleUpdate 方法
        UpdateGameObjectRequests(); // 更新游戏对象请求
    }

    // 定义一个泛型方法 LoadAsset，用于同步加载资源
    public T LoadAsset<T>(string path) where T : Object
    {
        // 使用 Addressables 加载资源并等待完成，然后返回加载的资源
        return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
    }

    // 定义一个泛型方法 LoadObjectAsync，用于异步加载资源
    public void LoadObjectAsync<T>(string path, AssetLoadTask.OnLoadFinishEventHandler onLoadFinish) where T : UnityEngine.Object
    {
        // 使用 Addressables 异步加载资源，并在加载完成后调用回调函数
        Addressables.LoadAssetAsync<T>(path).Completed += (obj) =>
        {
            onLoadFinish?.Invoke(obj.Result); // 调用回调函数，传递加载的资源
        };
    }
}
