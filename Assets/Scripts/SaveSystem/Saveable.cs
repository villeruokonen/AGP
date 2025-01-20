using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// MonoBehaviour component for GameObjects whose state
/// is saveable
/// </summary>
[ExecuteAlways]
public class Saveable : MonoBehaviour, ISaveable
{
    [SerializeField] [HideInInspector] private string _objectId;
    
    [SerializeField] private SaveProperty[] Properties;

    public string GetObjectId()
    {
        EnsureObjectId();

        return _objectId;
    }

    public void AddProperty(SaveProperty property)
    {
        Properties.Append(property);
    }

    private void EnsureObjectId()
    {
        if (string.IsNullOrEmpty(_objectId))
        {
            _objectId = System.Guid.NewGuid().ToString();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_objectId))
        {
            _objectId = System.Guid.NewGuid().ToString();

            Debug.Log($"Created new objectId {_objectId} for {this}", this);
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif

#region Serialization
    public SaveProperty[] GetProperties()
    {
        var serializedProperties = new List<SaveProperty>();

        foreach (var property in Properties)
        {
            var serializedData = property.GetSerializedMembers();
            var objectProperties = new List<MemberProperty>(serializedData.Count);

            foreach (var kvp in serializedData)
            {
                objectProperties.Add(new MemberProperty
                {
                    memberName = kvp.Key,
                    memberValue = kvp.Value
                });
            }

            // Only save serialized stuff; no need to add value target
            serializedProperties.Add(new SaveProperty
            {
                memberProperties = objectProperties.ToArray()
            });
        }

        return serializedProperties.ToArray();
    }

    public void LoadProperties(SaveProperty[] properties)
    {
        foreach (var property in properties)
        {
            property.SetMembers(property.memberProperties);
        }
    }
#endregion
}