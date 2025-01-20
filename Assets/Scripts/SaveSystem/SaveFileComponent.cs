
using System;
using System.Linq;

/// <summary>
/// Component of <see cref="SaveFile"/>.
/// At least one exists for each <see cref="Saveable"/>.
/// </summary>
[Serializable]
public struct SaveFileComponent
{
    /// <summary>
    /// Unique ID of the object that the properties belong to
    /// </summary>
    public string objectId;

    /// <summary>
    /// Target scene to which the properties belong to
    /// </summary>
    public string targetSceneName;

    public SaveProperty[] properties;

    public void AddSaveProperty(SaveProperty property)
    {
        properties.Append(property);
    }

    public override string ToString()
    {
        var propertiesString = string.Join(", ", properties.Select(p => p.ToString()));
        return $"{targetSceneName}/{objectId}/{propertiesString}";
    }
}
