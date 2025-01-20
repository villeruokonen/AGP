using UnityEngine;

public class Saveable : MonoBehaviour, ISaveable
{
    [SerializeField] SaveProperty[] Properties;

    public string GetJsonProperties()
    {
        return JsonUtility.ToJson(Properties);
    }

    public void LoadProperties(string json)
    {
        Properties = JsonUtility.FromJson<SaveProperty[]>(json);
    }
}