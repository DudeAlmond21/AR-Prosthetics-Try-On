using UnityEngine;
using UnityEngine.SceneManagement;

public class ProstheticTypeHandler : MonoBehaviour
{
    public void SelectMechanical()
    {
        AppState.Instance.CurrentProsthetic = ProstheticType.Mechanical;
        SceneManager.LoadScene("CustomizationScene");
    }

    public void SelectBionic()
    {
        AppState.Instance.CurrentProsthetic = ProstheticType.Bionic;
        SceneManager.LoadScene("CustomizationScene");
    }

    public void SelectCosmetic()
    {
        AppState.Instance.CurrentProsthetic = ProstheticType.Cosmetic;
        SceneManager.LoadScene("CustomizationScene");
    }

    public void GoBack()
    {
        SceneManager.LoadScene("LevelSelectionScene");
    }
}