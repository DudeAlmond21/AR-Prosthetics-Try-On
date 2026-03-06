using UnityEngine;
using UnityEngine.SceneManagement;

public class ARTryOnModeHandler : MonoBehaviour
{
    public void SetFollowMode()
    {
        if (TryOnModeManager.Instance != null)
        {
            TryOnModeManager.Instance.SetMode(TryOnMode.Follow);
        }
    }

    public void SetGestureMode()
    {
        if (TryOnModeManager.Instance != null)
        {
            TryOnModeManager.Instance.SetMode(TryOnMode.Gesture);
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("CalibrationScene");
    }
}