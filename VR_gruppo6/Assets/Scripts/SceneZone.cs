using UnityEngine;

public class SceneZone : MonoBehaviour
{
    [Header("Scene Zone Settings")]
    [SerializeField] private string zoneName = "Scena";

    private static string activeZoneName = "";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activeZoneName = zoneName;
            Debug.Log("Entrato nella zona: " + zoneName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Uscito dalla zona: " + zoneName);
        }
    }

    public static bool IsPlayerInScene(string sceneName)
    {
        return activeZoneName == sceneName;
    }

    public static string GetCurrentPlayerScene()
    {
        return activeZoneName;
    }
}
