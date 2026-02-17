using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_BasePanel : MonoBehaviour
{
    // singleton
    private static UI_BasePanel _instance;
    public static UI_BasePanel instance => _instance;

    [Header("Quick slots UI")]
    public Image[] quickSlotImages;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void UpdateQuickSlots()
    {
        var quickItems = Inventory.instance.GetQuickSlotItems();
        for (int i = 0; i < quickSlotImages.Length; i++)
        {
            if (i < quickItems.Count && quickItems[i] != null)
            {
                quickSlotImages[i].sprite = quickItems[i].GetItemIcon();
                quickSlotImages[i].color = Color.white;
            }
            else
            {
                quickSlotImages[i].sprite = null;
                quickSlotImages[i].color = new Color(1,1,1,0);
            }
        }
    }
}