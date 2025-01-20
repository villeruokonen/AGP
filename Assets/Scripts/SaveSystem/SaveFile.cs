using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// Storage form of save file with
/// array of <see cref="SaveFileComponent"/>
/// </summary>
[Serializable]
public class SaveFile
{
    public string saveName = string.Empty;
    public SaveFileComponent[] components = new SaveFileComponent[0];

    public void AddSaveComponent(SaveFileComponent component)
    {
        Debug.Log($"Added {component} to savefile");
        components.Append(component);
    }
}
