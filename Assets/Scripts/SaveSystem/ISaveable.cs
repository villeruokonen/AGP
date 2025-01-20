using UnityEngine;

/// <summary>
/// Interface for properties that must be saved.
/// </summary>
public interface ISaveable
{
    /// <summary>
    /// Get anything needing to be saved
    /// </summary>
    /// <returns></returns>
    SaveProperty[] GetProperties();
    
    /// <summary>
    /// Load properties
    /// </summary>
    /// <param name="properties"></param>

    void LoadProperties(SaveProperty[] properties);
}
