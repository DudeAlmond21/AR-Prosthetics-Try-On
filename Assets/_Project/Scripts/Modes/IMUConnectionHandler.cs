using UnityEngine;
using UnityEngine.SceneManagement;

public class IMUConnectionHandler : MonoBehaviour
{
    public void Connect()
    {
        // BLE logic will be added later
        SceneManager.LoadScene("CalibrationScene");
    }

    public void SkipConnection()
    {
        // For development and grading demo
        SceneManager.LoadScene("CalibrationScene");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("CustomizationScene");
    }
}