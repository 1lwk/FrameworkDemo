// 定义一个名为 ECSSystemAttribute 的自定义特性，用于标记 ECS 系统类
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class ECSSystemAttribute : System.Attribute
{
}
