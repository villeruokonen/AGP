using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;

/// <summary>
/// Serializable property
/// </summary>
[Serializable]
public struct SaveProperty
{
    public UnityEngine.Object target; // Inspector only

    [HideInInspector] public MemberProperty[] memberProperties;

    public Dictionary<string, object> GetSerializedMembers()
    {
        var data = new Dictionary<string, object>();
        var type = target.GetType();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            try
            {
                data[field.Name] = field.GetValue(target);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (property.CanRead)
            {
                try
                {
                    data[property.Name] = property.GetValue(target);
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }

        return data;
    }

    public void SetMembers(MemberProperty[] memberProperties)
    {
        var type = target.GetType();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var memberField = memberProperties.FirstOrDefault(mp => mp.memberName == field.Name);
            if (memberField.memberName != null)
            {
                field.SetValue(target, memberField.memberValue);
            }
        }

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var memberProperty = memberProperties.FirstOrDefault(mp => mp.memberName == property.Name);
            if (memberProperty.memberName != null)
            {
                property.SetValue(target, memberProperty.memberValue);
            }
        }
    }
}

[System.Serializable]
public struct MemberProperty
{
    public string memberName;
    public object memberValue;
}
