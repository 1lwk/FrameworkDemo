public static class IDGenerator
{
    // ���ڴ洢��ǰʵ��ID
    private static long currentInstanceID;

    // ���ص�ǰ��ʵ��ID
    public static long CurrentInstanceID() { return currentInstanceID; }

    // ���õ�ǰ��ʵ��IDΪ0
    public static void ResetInstanceID()
    {
        currentInstanceID = 0;
    }

    // ����һ���µ�ʵ��ID������
    public static long NewInstanceID()
    {
        return ++currentInstanceID;
    }

    // ���ڴ洢��ǰID
    private static long currentID;

    // ���ص�ǰ��ID
    public static long CurrentID() { return currentID; }

    // ���õ�ǰ��IDΪ0
    public static void ResetID()
    {
        currentID = 0;
    }

    // ���õ�ǰ��IDΪָ��ֵ
    public static void SetID(long currentID)
    {
        IDGenerator.currentID = currentID;
    }

    // ����һ���µ�ID������
    public static long NewID()
    {
        return ++currentID;
    }
}
