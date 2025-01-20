using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveDebugger : MonoBehaviour
{
    [SerializeField] private string _saveName = "TestSave";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveManager.CurrentSaveFile.saveName = _saveName;

            SaveManager.SaveSceneToSaveFile(SceneManager.GetActiveScene(), SaveManager.CurrentSaveFile);
            SaveManager.SaveFileToDisk(SaveManager.CurrentSaveFile);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SaveManager.LoadSaveFile(SaveManager.CurrentSaveFile);
        }
    }
}
