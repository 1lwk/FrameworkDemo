using UnityEngine;
using UnityEngine.AddressableAssets;

// ����һ����Ϊ AssetModule ���࣬�̳��� BaseGameModule
public partial class AssetModule : BaseGameModule
{
#if UNITY_EDITOR
    // �� Unity �༭���У�����һ�������ַ��� BUNDLE_LOAD_NAME
    //[XLua.BlackList]
    public const string BUNDLE_LOAD_NAME = "Tools/Build/Bundle Load";
#endif

    // �������� Transform ���͵Ĺ��б��������ڴ������ʹ�ú��ͷŵĶ���
    public Transform usingObjectRoot;
    public Transform releaseObjectRoot;

    // ��д���� BaseGameModule �� OnModuleUpdate ����
    protected internal override void OnModuleUpdate(float deltaTime)
    {
        base.OnModuleUpdate(deltaTime); // ���û���� OnModuleUpdate ����
        UpdateGameObjectRequests(); // ������Ϸ��������
    }

    // ����һ�����ͷ��� LoadAsset������ͬ��������Դ
    public T LoadAsset<T>(string path) where T : Object
    {
        // ʹ�� Addressables ������Դ���ȴ���ɣ�Ȼ�󷵻ؼ��ص���Դ
        return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
    }

    // ����һ�����ͷ��� LoadObjectAsync�������첽������Դ
    public void LoadObjectAsync<T>(string path, AssetLoadTask.OnLoadFinishEventHandler onLoadFinish) where T : UnityEngine.Object
    {
        // ʹ�� Addressables �첽������Դ�����ڼ�����ɺ���ûص�����
        Addressables.LoadAssetAsync<T>(path).Completed += (obj) =>
        {
            onLoadFinish?.Invoke(obj.Result); // ���ûص����������ݼ��ص���Դ
        };
    }
}
