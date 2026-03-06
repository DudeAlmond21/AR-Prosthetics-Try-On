using UnityEngine;

public enum TryOnMode
{
    Follow,
    Gesture
}

public class TryOnModeManager : MonoBehaviour
{
    public static TryOnModeManager Instance;

    public TryOnMode currentMode = TryOnMode.Gesture;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMode(TryOnMode mode)
    {
        currentMode = mode;
        Debug.Log("Mode changed to: " + mode);
    }
}