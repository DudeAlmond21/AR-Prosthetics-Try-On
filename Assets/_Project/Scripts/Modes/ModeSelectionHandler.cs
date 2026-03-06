using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeSelectionHandler : MonoBehaviour
{
    public void SelectSimulation()
    {
        AppState.Instance.CurrentMode = UserMode.Simulation;
        SceneManager.LoadScene("LevelSelectionScene");
    }

    public void SelectAmputee()
    {
        AppState.Instance.CurrentMode = UserMode.Amputee;
        SceneManager.LoadScene("LevelSelectionScene");
    }

    public void SelectAbleBodied()
    {
        AppState.Instance.CurrentMode = UserMode.AbleBodied;
        SceneManager.LoadScene("LevelSelectionScene");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}