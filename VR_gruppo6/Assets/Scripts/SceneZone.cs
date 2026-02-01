using UnityEngine;

public class SceneZone : MonoBehaviour
{
    [Header("Scene Zone Settings")]
    [SerializeField] private string zoneName = "Scena";
    [SerializeField] private bool isActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isActive = true;
            Debug.Log("Entrato nella zona: " + zoneName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isActive = false;
            Debug.Log("Uscito dalla zona: " + zoneName);
        }
    }
}
