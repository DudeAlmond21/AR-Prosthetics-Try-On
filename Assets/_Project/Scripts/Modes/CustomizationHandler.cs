using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomizationHandler : MonoBehaviour
{
    public void ConfirmSelection()
    {
        if (AppState.Instance.CurrentMode == UserMode.Simulation)
        {
            SceneManager.LoadScene("SimulationScene");
        }
        else
        {
            SceneManager.LoadScene("IMUConnectionScene");
        }
    }

    public void GoBack()
    {
        SceneManager.LoadScene("ProstheticTypeScene");
    }
}