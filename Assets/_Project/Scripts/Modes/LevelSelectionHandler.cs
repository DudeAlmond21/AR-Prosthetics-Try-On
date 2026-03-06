using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectionHandler : MonoBehaviour
{
    public void SelectAboveElbow()
    {
        AppState.Instance.CurrentLevel = AmputationLevel.AboveElbow;
        SceneManager.LoadScene("ProstheticTypeScene");
    }

    public void SelectBelowElbow()
    {
        AppState.Instance.CurrentLevel = AmputationLevel.BelowElbow;
        SceneManager.LoadScene("ProstheticTypeScene");
    }

    public void SelectFullArm()
    {
        AppState.Instance.CurrentLevel = AmputationLevel.FullArm;
        SceneManager.LoadScene("ProstheticTypeScene");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("ModesScene");
    }
}