using UnityEngine;

public class CoinUIManager : MonoBehaviour
{
    public static CoinUIManager Instance;
    public RectTransform icon;
    private Camera cam;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    public Vector3 GetWorldTarget()
    {
        Vector3 screenPos = icon.position;
        return cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f)); // UI → world space
    }
}