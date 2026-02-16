using UnityEngine;

[System.Serializable]
public class GreenScreenTarget
{
    [Header("Identification")]
    public string id;
    public string displayName;
    
    [Header("Visual")]
    public Renderer targetRenderer;
    
    [Header("Quest Data (TortaInTesta)")]
    public int correctImageIndex = -1; // -1 qualsiasi immagine va bene

    [Header("State")]
    public bool isCompleted = false;
    
    public bool IsValid()
    {
        return targetRenderer != null && !string.IsNullOrEmpty(id);
    }
}