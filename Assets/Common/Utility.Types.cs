using System;
using System.Collections.Generic;
using System.Reflection;

namespace TGame.Common
{
	public static partial class Utility
    {
        public static class Types
        {
            public readonly static Assembly GAME_CSHARP_ASSEMBLY = Assembly.Load("Assembly-CSharp");
            public readonly static Assembly GAME_EDITOR_ASSEMBLY = Assembly.Load("Assembly-CSharp-Editor");

            /// <summary>
            /// 获取所有能从某个类型分配的属性列表
            /// </summary>
            public static List<PropertyInfo> GetAllAssignablePropertiesFromType(Type basePropertyType, Type objType, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
            {
                List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
                PropertyInfo[] properties = objType.GetProperties(bindingFlags);
                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo propertyInfo = properties[i];
                    if (basePropertyType.IsAssignableFrom(propertyInfo.PropertyType))
                    {
                        propertyInfos.Add(propertyInfo);
                    }
                }
                return propertyInfos;
            }

            /// <summary>
            /// 获取某个类型的所有子类型
            /// </summary>
            /// <param name="baseClass">父类</param>
            /// <param name="assemblies">程序集,如果为null则查找当前程序集</param>
            /// <returns></returns>
            public static List<Type> GetAllSubclasses(Type baseClass, bool allowAbstractClass, params Assembly[] assemblies)
            {
                // 创建一个用于存储子类型的列表
                List<Type> subclasses = new List<Type>();

                // 如果 assemblies 参数为 null，则使用调用该方法的程序集
                if (assemblies == null)
                {
                    assemblies = new Assembly[] { Assembly.GetCallingAssembly() };
                }

                // 遍历提供的每个程序集
                foreach (var assembly in assemblies)
                {
                    // 遍历程序集中的每个类型
                    foreach (var type in assembly.GetTypes())
                    {
                        // 检查当前类型是否是 baseClass 的子类型或实现
                        if (!baseClass.IsAssignableFrom(type))
                            continue; // 如果不是，跳过此类型

                        // 如果不允许抽象类，并且当前类型是抽象类，则跳过此类型
                        if (!allowAbstractClass && type.IsAbstract)
                            continue; // 如果是抽象类，跳过此类型

                        // 将符合条件的类型添加到子类型列表中
                        subclasses.Add(type);
                    }
                }

                // 返回所有符合条件的子类型
                return subclasses;
            }

        }
    }
}