using UnityEngine;

/// <summary>
/// Interface for properties that must be saved.
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// Get saved properties serialized as JSON string
    /// </summary>
    /// <returns></returns>
    string GetJsonProperties();
    
    /// <summary>
    /// Load properties from JSON
    /// </summary>
    /// <param name="json"></param>

    void LoadProperties(string json);
}
