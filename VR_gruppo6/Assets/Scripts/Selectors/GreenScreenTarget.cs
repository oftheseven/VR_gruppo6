using UnityEngine;

[System.Serializable]
public class GreenScreenTarget
{
    [Header("Identification")]
    public string id;
    public string displayName;
    
    [Header("Visual")]
    public Renderer targetRenderer;
    public Renderer previewRenderer;
    public Camera previewCamera;
    public int appliedImageIndex = -1;

    [Header("Materials")]
    public Material defaultMaterial;

    [Header("Available images")]
    public Texture2D[] availableImages = new Texture2D[4];
    
    [Header("Quest Data (TortaInTesta)")]
    public int correctImageIndex = -1; // -1 qualsiasi immagine va bene

    [Header("State")]
    public bool isCompleted = false;
    
    public bool IsValid()
    {
        return targetRenderer != null && 
               !string.IsNullOrEmpty(id) && 
               availableImages != null && 
               availableImages.Length > 0;
    }
    
    public int GetValidImageCount()
    {
        if (availableImages == null) return 0;
        
        int count = 0;
        foreach (var tex in availableImages)
        {
            if (tex != null) count++;
        }
        return count;
    }
}