using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveManager
{
    public static SaveFile CurrentSaveFile { get; private set; } = new();

    public static string SavePath => Application.dataPath + "/saves/";
    public static string FileExtension => ".json";

    /// <summary>
    /// Add all <see cref="Saveable" to save file.
    /// </summary>
    /// <param name="file"></param>
    public static void SaveSceneToSaveFile(Scene scene, SaveFile file)
    {
        foreach (var saveable in GetSaveablesInScene(scene))
        {
            file.AddSaveComponent(new SaveFileComponent
            {
                objectId = saveable.GetObjectId(),
                targetSceneName = scene.name,
                properties = saveable.GetProperties(),
            });
        }
    }

    public static void LoadSaveFile(SaveFile file)
    {
        var activeScene = SceneManager.GetActiveScene();
        var saveables = GetSaveablesInScene(activeScene);
        foreach (var saveComponent in file.components)
        {
            if (saveComponent.targetSceneName != activeScene.name)
                continue;

            foreach (var saveable in saveables)
            {
                if (saveable.GetObjectId() == saveComponent.objectId)
                {
                    saveable.LoadProperties(saveComponent.properties);
                }
            }
        }
    }

    private static Saveable[] GetSaveablesInScene(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        List<Saveable> saveables = new();
        foreach (var root in roots)
        {
            foreach (var saveable in root.GetComponentsInChildren<Saveable>(true))
            {
                saveables.Add(saveable);
            }
        }

        return saveables.ToArray();
    }

    #region IO
    public static void SaveFileToDisk(SaveFile file)
    {
        if (!SaveDirectoryExists())
        {
            CreateSaveDirectory();
        }

        string path;
        if (string.IsNullOrEmpty(file.saveName))
        {
            path = SavePath + "save_" + (GetNumSaves() + 1).ToString();
        }
        else
        {
            path = SavePath + file.saveName;
        }

        path += FileExtension;

        var json = JsonUtility.ToJson(file);

        File.WriteAllText(path, json);
    }

    private static int GetNumSaves()
    {
        if (!SaveDirectoryExists())
            return 0;

        return GetAllSaveFiles().Length;
    }

    private static bool SaveDirectoryExists()
    {
        return Directory.Exists(SavePath);
    }

    private static void CreateSaveDirectory()
    {
        Directory.CreateDirectory(SavePath);
    }

    private static SaveFile[] GetAllSaveFiles()
    {
        if (!SaveDirectoryExists())
            return Array.Empty<SaveFile>();

        var dirInfo = OpenSaveDirectory();

        var saveFiles = new List<SaveFile>();
        foreach (var file in dirInfo.GetFiles($"*{FileExtension}"))
        {
            string json = File.ReadAllText(file.FullName);
            SaveFile saveFile = JsonUtility.FromJson<SaveFile>(json);
            saveFiles.Add(saveFile);
        }

        return saveFiles.ToArray();
    }

    private static DirectoryInfo OpenSaveDirectory()
    {
        if (!SaveDirectoryExists())
        {
            throw new DirectoryNotFoundException("Save directory does not exist.");
        }

        return new DirectoryInfo(SavePath);
    }
    #endregion
}