using System; 
using System.Collections.Generic; 
using TGame.Common; 
using UnityEditor; 
using UnityEngine; 

namespace TGame.Editor.Inspector
{
    // 自定义编辑器，指定用于 ProcedureModule 类型
    [CustomEditor(typeof(ProcedureModule))]
    public class ProcedureModuleInspector : BaseInspector
    {
        // 序列化属性，用于访问和修改 ProcedureModule 的字段
        private SerializedProperty proceduresProperty; // 提供了一种方法来访问和修改序列化的对象数据，包括数组和列表
        private SerializedProperty defaultProcedureProperty;
        private SerializedProperty ageproceduresProperty;

        // 存储所有过程类型的列表
        private List<string> allProcedureTypes;

        // 在启用检查器时调用
        protected override void OnInspectorEnable()
        {
            base.OnInspectorEnable(); // 调用基类方法
            // 查找并缓存序列化属性
            proceduresProperty = serializedObject.FindProperty("proceduresNames");//通过序列化的名称查找序列化对象中的对应属性
            defaultProcedureProperty = serializedObject.FindProperty("defaultProcedureName");
            ageproceduresProperty= serializedObject.FindProperty("defaultProcedureName");
            // 更新过程列表
            UpdateProcedures();
        }

        // 在编译完成时调用
        protected override void OnCompileComplete()
        {
            base.OnCompileComplete(); // 调用基类方法
            // 更新过程列表
            UpdateProcedures();
        }

        // 更新所有过程类型的列表，并移除不存在的过程
        private void UpdateProcedures()
        {
            // 获取所有 BaseProcedure 的子类类型
            allProcedureTypes = Utility.Types.GetAllSubclasses(typeof(BaseProcedure), false, Utility.Types.GAME_CSHARP_ASSEMBLY).ConvertAll((Type t) => { return t.FullName; });

            // 移除不存在的过程
            for (int i = proceduresProperty.arraySize - 1; i >= 0; i--)
            {
                string procedureTypeName = proceduresProperty.GetArrayElementAtIndex(i).stringValue;
                if (!allProcedureTypes.Contains(procedureTypeName))
                {
                    proceduresProperty.DeleteArrayElementAtIndex(i);
                }
            }
            serializedObject.ApplyModifiedProperties(); // 应用修改
            serializedObject.ApplyModifiedProperties(); // 应用修改
        }

        // 绘制检查器 GUI
        public override void OnInspectorGUI()
        {
            // 禁用游戏运行时的编辑功能 防止在运行时进行编辑
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                if (allProcedureTypes.Count > 0)
                {
                    // 创建垂直布局
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    {
                        for (int i = 0; i < allProcedureTypes.Count; i++)
                        {
                            GUI.changed = false; // 重置GUI.changed标志
                            int? index = FindProcedureTypeIndex(allProcedureTypes[i]); // 查找过程类型的索引 int?可空类型
                            bool selected = EditorGUILayout.ToggleLeft(allProcedureTypes[i], index.HasValue); // 创建带标签的复选框 第一个参数表示显示标签的名字 第二个参数是一个布尔值表示是否被选中
                            if (GUI.changed) // 检查GUI.changed标志
                            {
                                if (selected)
                                {
                                    AddProcedure(allProcedureTypes[i]); // 添加过程类型
                                }
                                else
                                {
                                    RemoveProcedure(index.Value); // 移除过程类型
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndVertical(); // 结束垂直布局
                }
            }
            EditorGUI.EndDisabledGroup(); // 结束禁用组

            //arraySize它用于获取或设置数组或列表的大小
            if (proceduresProperty.arraySize == 0)
            {
                if (allProcedureTypes.Count == 0)
                {
                    // 显示信息框，当找不到任何过程类型时
                    EditorGUILayout.HelpBox("Can't find any procedure", UnityEditor.MessageType.Info);
                }
                else
                {
                    // 显示信息框，当未选择任何过程类型时
                    EditorGUILayout.HelpBox("Please select a procedure at least", UnityEditor.MessageType.Info);
                }
            }
            else
            {
                if (Application.isPlaying)
                {
                    // 显示当前状态
                    EditorGUILayout.LabelField("Current Procedure", TGameFramework.Instance.GetModule<ProcedureModule>().CurrentProcedure?.GetType().FullName);
                }
                else
                {
                    // 显示默认状态
                    List<string> selectedProcedures = new List<string>();
                    //遍历这个数组
                    for (int i = 0; i < proceduresProperty.arraySize; i++)
                    {
                        //添加到selectedProcedures里面
                        selectedProcedures.Add(proceduresProperty.GetArrayElementAtIndex(i).stringValue);
                    }
                    selectedProcedures.Sort(); // 排序选择的过程类型
                    int defaultProcedureIndex = selectedProcedures.IndexOf(defaultProcedureProperty.stringValue); // 查找默认过程的索引
                    defaultProcedureIndex = EditorGUILayout.Popup("Default Procedure", defaultProcedureIndex, selectedProcedures.ToArray()); // 创建下拉菜单
                    if (defaultProcedureIndex >= 0)
                    {
                        defaultProcedureProperty.stringValue = selectedProcedures[defaultProcedureIndex]; // 更新默认过程
                    }
                }
            }

            serializedObject.ApplyModifiedProperties(); // 应用修改
        }

        // 添加过程类型到列表
        private void AddProcedure(string procedureType)
        {
            proceduresProperty.InsertArrayElementAtIndex(0); // 在索引0处插入新元素
            proceduresProperty.GetArrayElementAtIndex(0).stringValue = procedureType; // 设置新元素的值
        }

        // 从列表中移除过程类型
        private void RemoveProcedure(int index)
        {
            string procedureType = proceduresProperty.GetArrayElementAtIndex(index).stringValue; // 获取过程类型的值
            if (procedureType == defaultProcedureProperty.stringValue)
            {
                Debug.LogWarning("Can't remove default procedure"); // 打印警告信息，无法移除默认过程
                return;
            }
            proceduresProperty.DeleteArrayElementAtIndex(index); // 删除指定索引的元素
        }

        // 查找过程类型在列表中的索引
        private int? FindProcedureTypeIndex(string procedureType)
        {
            for (int i = 0; i < proceduresProperty.arraySize; i++)
            {
                SerializedProperty p = proceduresProperty.GetArrayElementAtIndex(i); // 获取指定索引的元素
                if (p.stringValue == procedureType)
                {
                    return i; // 返回找到的索引
                }
            }
            return null; // 未找到时返回null
        }
    }
}
