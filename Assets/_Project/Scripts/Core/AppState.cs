using UnityEngine;

public enum UserMode
{
    Simulation,
    Amputee,
    AbleBodied
}

public enum AmputationLevel
{
    None,
    AboveElbow,
    BelowElbow,
    FullArm
}

public enum ProstheticType
{
    None,
    Mechanical,
    Bionic,
    Cosmetic
}

public enum ArmSide
{
    Right,
    Left
}

public class AppState : MonoBehaviour
{
    public static AppState Instance;

    public UserMode CurrentMode = UserMode.Simulation;
    public AmputationLevel CurrentLevel = AmputationLevel.None;
    public ProstheticType CurrentProsthetic = ProstheticType.None;
    public ArmSide SelectedArmSide = ArmSide.Right;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}