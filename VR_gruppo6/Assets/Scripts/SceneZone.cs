using UnityEngine;

public class SceneZone : MonoBehaviour
{
    [Header("Scene Zone Settings")]
    [SerializeField] private string zoneName = "Scena";
    // [SerializeField] private GameObject doorReference;

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

    // private bool isActive = false;

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         isActive = true;
    //         Debug.Log("Entrato nella zona: " + zoneName);

    //         if (doorReference != null)
    //         {
    //             doorReference.SetActive(true);
    //         }

    //         // HideOtherDoors();
    //     }
    // }

    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         isActive = false;
    //         Debug.Log("Uscito dalla zona: " + zoneName);
    //     }
    // }

    // private void HideOtherDoors()
    // {
    //     GameObject[] allDoors = GameObject.FindGameObjectsWithTag("SharedDoor");
        
    //     foreach (GameObject door in allDoors)
    //     {
    //         if (door != doorReference && door.scene != gameObject.scene)
    //         {
    //             door.SetActive(false);
    //             Debug.Log($"Nascosta porta di: {door.scene.name}");
    //         }
    //     }
    // }
}
