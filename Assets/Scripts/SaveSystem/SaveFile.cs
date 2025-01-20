using UnityEngine;

/// <summary>
/// Component of <see cref="SaveFile"/> with 
/// an object ID and array of <see cref="SaveProperty"/>
/// </summary>
public struct SaveFileComponent
{
    /// <summary>
    /// ID of the object that the properties belong to
    /// </summary>
    public int ObjectId { get; set; }
    public SaveProperty[] Properties { get; set; }
}

/// <summary>
/// Storage form of save file with
/// array of <see cref="SaveFileComponent"/>
/// </summary>
public class SaveFile
{
    SaveFileComponent[] Components { get; set; }
}
