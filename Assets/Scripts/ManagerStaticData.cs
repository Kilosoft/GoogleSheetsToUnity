using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ManagerStaticData
{
    private static Dictionary<string, string> Jsons;
    private static Dictionary<Type, object> StaticDatas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Load()
    {
        Jsons = new Dictionary<string, string>();
        StaticDatas = new Dictionary<Type, object>();

        var files = Resources.LoadAll<TextAsset>($"StaticData");
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            Jsons[file.name] = file.text;
        }
    }

    public static T GetStaticClassInstance<T>() where T : class
    {
        var jsonName = typeof(T).Name.Replace("StaticData", "").Replace("StaticModel", "");
        var json = Jsons[jsonName];
        var data = default(T);

        if (StaticDatas.ContainsKey(typeof(T)))
        {
            data = StaticDatas[typeof(T)] as T;
        }
        else
        {
            data = JsonUtility.FromJson<T>(json);
            StaticDatas[typeof(T)] = data;
        }

        return data;
    }

    public static object GetStaticClassInstance(Type type)
    {
        var jsonName = type.Name.Replace("StaticData", "").Replace("StaticModel", "") + "s";
        var json = Jsons[jsonName];
        var data = default(object);

        if (StaticDatas.ContainsKey(type))
        {
            data = StaticDatas[type];
        }
        else
        {
            data = JsonUtility.FromJson(json, Type.GetType(jsonName + "StaticModel"));
            StaticDatas[type] = data;
        }

        return data;
    }

    public static RuntimeType CreateRuntimeInstance<RuntimeType, StaticData>() where RuntimeType : class where StaticData : class
    {
        var data = GetStaticClassInstance<StaticData>();
        var instance = Activator.CreateInstance(typeof(RuntimeType)) as RuntimeType;
        var listToInstance = typeof(RuntimeType).GetFields().FirstOrDefault(x => x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(List<>));
        var listData = typeof(StaticData).GetFields().FirstOrDefault(x => x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(List<>));

        var typeInstanceList = listToInstance.FieldType.GetGenericArguments()[0];
        var listInstance = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeInstanceList));
        foreach (var f in (IList)listData.GetValue(data))
        {
            var item = Activator.CreateInstance(typeInstanceList);
            ((IList)listInstance).Add(item);

            var fieldsData = f.GetType().GetFields();
            var fieldsInstance = typeInstanceList.GetFields();

            for (int i = 0; i < fieldsData.Length; i++)
            {
                var fieldData = fieldsData[i];
                if (fieldData.FieldType != typeof(StaticRef))
                {
                    var nameData = fieldData.Name;
                    var fieldInstance = fieldsInstance.FirstOrDefault(x => x.Name == nameData);
                    if (fieldInstance != null)
                    {
                        fieldInstance.SetValue(item, fieldData.GetValue(f));
                    }
                }
                else if (fieldData.FieldType == typeof(StaticRef))
                {
                    var nameData = fieldData.Name;
                    var fieldInstance = fieldsInstance.FirstOrDefault(x => x.Name == nameData);
                    var staticRef = fieldData.GetValue(f) as StaticRef;
                    if (string.IsNullOrEmpty(staticRef.Type) || staticRef.Type == "null") continue;
                    var typeNestedData = Type.GetType(staticRef.Type);
                    var instanceType = Type.GetType(typeNestedData.Name.Replace("StaticData", "").Replace("StaticModel", ""));
                    var nestedData = GetStaticClassInstance(Type.GetType(staticRef.Type));
                    var stringType = typeNestedData.Name.Replace("StaticData", "").Replace("StaticModel", "") + "sStaticModel";
                    var listNestedData = Type.GetType(stringType).GetFields().FirstOrDefault(x => x.FieldType.IsGenericType && x.FieldType.GetGenericTypeDefinition() == typeof(List<>));
                    var listTo = Activator.CreateInstance(typeof(List<>).MakeGenericType(instanceType));
                    foreach (var staticRefItemIndex in staticRef.Values)
                    {
                        var nestedItem = ((IList)listNestedData.GetValue(nestedData))[staticRefItemIndex];
                        var itemNestedInstance = Activator.CreateInstance(instanceType);
                        var nestedItemFieldDatas = nestedItem.GetType().GetFields();
                        for (int j = 0; j < nestedItemFieldDatas.Length; j++)
                        {
                            var nestedItemFieldData = nestedItemFieldDatas[j];
                            if (nestedItemFieldData.FieldType != typeof(StaticRef))
                            {
                                var nameNestedData = nestedItemFieldData.Name;
                                var field = itemNestedInstance.GetType().GetFields().FirstOrDefault(x => x.Name == nameNestedData);
                                if (field != null)
                                {
                                    field.SetValue(itemNestedInstance, nestedItemFieldData.GetValue(nestedItem));
                                }
                            }
                        }
                        ((IList)listTo).Add(itemNestedInstance);
                    }
                    if (staticRef.IsArray)
                    {
                        fieldInstance.SetValue(item, listTo);
                    }
                    else
                    {
                        fieldInstance.SetValue(item, ((IList)listTo).Count > 0 ? ((IList)listTo)[0] : null);
                    }
                }
            }
        }
        listToInstance.SetValue(instance, listInstance);
        return instance;
    }
}
