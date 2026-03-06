using UnityEngine;
using UnityEngine.SceneManagement;

public class CalibrationHandler : MonoBehaviour
{
    public void Calibrate()
    {
        // Later this will call IMU calibration method
        Debug.Log("Calibration set.");
    }

    public void ContinueToAR()
    {
        SceneManager.LoadScene("ARTryOnScene");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("IMUConnectionScene");
    }
}