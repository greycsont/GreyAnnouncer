using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Text; // 引入 StringBuilder
using System.IO;
using GreyAnnouncer;

public static class ObjectTreePrinter
{
    // 用于存储已经打印过的对象引用，避免无限循环
    private static HashSet<object> _visitedObjects = new HashSet<object>();

    /// <summary>
    /// 将任何C#对象结构转化为树状字符串表示。
    /// </summary>
    /// <param name="obj">要分析的对象。</param>
    /// <returns>包含树状结构的字符串。</returns>
    public static string GetTreeString(object obj)
    {
        if (obj == null) return null;
        
        _visitedObjects.Clear();
        var sb = new StringBuilder();
        
        sb.AppendLine($"\n--- Object Tree Struct ({obj.GetType().Name}) ---");
        
        // 调用递归方法，将 StringBuilder 传入
        AppendNode(sb, obj, 0, "ROOT");

        return sb.ToString();
    }

    private static void AppendNode(StringBuilder sb, object obj, int depth, string name)
    {
        if (obj == null)
        {
            sb.AppendLine($"{GetIndent(depth)}└─ {name}: null");
            return;
        }

        Type type = obj.GetType();
        string indent = GetIndent(depth);

        // --- 1. 处理基本类型和字符串 ---
        if (type.IsPrimitive || type == typeof(string) || type.IsValueType && !IsEnumerable(type))
        {
            sb.AppendLine($"{indent}└─ {name} ({type.Name}): {obj}");
            return;
        }

        // --- 2. 处理循环引用 ---
        if (!type.IsValueType && _visitedObjects.Contains(obj))
        {
            sb.AppendLine($"{indent}└─ {name} ({type.Name}): {{... Loop Detected ...}}");
            return;
        }
        _visitedObjects.Add(obj);

        // ⭐️ 新增/修改：处理字典类型 (IDictionary)
        if (obj is IDictionary dictionary)
        {
            sb.AppendLine($"{indent}└─ {name} ({type.Name}[{dictionary.Count}])");
            
            // 遍历字典中的每一个键值对
            foreach (DictionaryEntry entry in dictionary)
            {
                // 字典的键作为当前节点的名称
                string entryName = $"Key: {entry.Key}";
                // 递归处理字典的值
                AppendNode(sb, entry.Value, depth + 1, entryName);
            }
            return;
        }

        // --- 3. 处理集合类型 (List, Array, Dictionary) ---
        if (obj is IEnumerable enumerable)
        {
            int count = (obj is ICollection collection) ? collection.Count : 0;
            sb.AppendLine($"{indent}└─ {name} ({type.Name}[{count}])");

            int index = 0;
            foreach (var item in enumerable)
            {
                AppendNode(sb, item, depth + 1, $"[{index++}]");
            }
            return;
        }

        // --- 4. 处理自定义类或复杂对象 (使用反射) ---
        sb.AppendLine($"{indent}└─ {name} ({type.Name})");

        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            try
            {
                object propValue = prop.GetValue(obj);
                AppendNode(sb, propValue, depth + 1, prop.Name);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"{indent}│   └─ {prop.Name}: <Error: {ex.Message}>");
            }
        }
    }

    // 辅助函数：生成缩进字符串
    private static string GetIndent(int depth)
        => new string(' ', depth * 4);
    
    // 辅助函数：判断是否为可枚举类型（排除字符串）
    private static bool IsEnumerable(Type type)
        => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
}