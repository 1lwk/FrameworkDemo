public static class IDGenerator
{
    // 用于存储当前实例ID
    private static long currentInstanceID;

    // 返回当前的实例ID
    public static long CurrentInstanceID() { return currentInstanceID; }

    // 重置当前的实例ID为0
    public static void ResetInstanceID()
    {
        currentInstanceID = 0;
    }

    // 生成一个新的实例ID并返回
    public static long NewInstanceID()
    {
        return ++currentInstanceID;
    }

    // 用于存储当前ID
    private static long currentID;

    // 返回当前的ID
    public static long CurrentID() { return currentID; }

    // 重置当前的ID为0
    public static void ResetID()
    {
        currentID = 0;
    }

    // 设置当前的ID为指定值
    public static void SetID(long currentID)
    {
        IDGenerator.currentID = currentID;
    }

    // 生成一个新的ID并返回
    public static long NewID()
    {
        return ++currentID;
    }
}
