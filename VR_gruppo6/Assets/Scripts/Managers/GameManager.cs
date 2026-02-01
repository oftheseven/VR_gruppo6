using UnityEngine;

public class GameManager : MonoBehaviour
{
    // singleton
    private static GameManager _instance;
    public static GameManager instance => _instance;
}
