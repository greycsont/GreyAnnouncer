using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GreyAnnouncer;

public static class ObjectTreePrinter
{
    private static HashSet<object> _visitedObjects = new HashSet<object>();

    /// <summary>
    /// Convert C# object to tree-shaped string
    /// </summary>
    public static string GetTreeString(object obj)
    {
        if (obj == null) return null;
        
        _visitedObjects.Clear();
        var sb = new StringBuilder();
        
        sb.AppendLine($"\n--- Object Tree Struct ({obj.GetType().Name}) ---");
        
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

        // --- 1. basic datatype & string ---
        if (type.IsPrimitive || type == typeof(string) || type.IsValueType && !IsEnumerable(type))
        {
            sb.AppendLine($"{indent}└─ {name} ({type.Name}): {obj}");
            return;
        }

        // --- 2. prevent infinite loop ---
        if (!type.IsValueType && _visitedObjects.Contains(obj))
        {
            sb.AppendLine($"{indent}└─ {name} ({type.Name}): {{... Loop Detected ...}}");
            return;
        }
        _visitedObjects.Add(obj);

        // --- 3. dictionary ---
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

        // --- 3. collection datatype (list, array, dictionary) ---
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

        // --- 4. custom class & complex object ---
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

    private static string GetIndent(int depth)
        => new string(' ', depth * 4);
    
    private static bool IsEnumerable(Type type)
        => type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
}