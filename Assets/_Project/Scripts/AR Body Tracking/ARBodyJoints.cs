/// <summary>
/// ARKit body joint indices for ARFoundation 5.x.
/// Use these instead of the removed ARHumanBodyJointIndex enum.
/// These integer values match the ARKit 91-joint skeleton exactly.
/// </summary>
public static class ARBodyJoint
{
    public const int Root            = 0;
    public const int Hips            = 1;
    public const int LeftUpLeg       = 2;
    public const int LeftLeg         = 3;
    public const int LeftFoot        = 4;
    public const int RightUpLeg      = 7;
    public const int RightLeg        = 8;
    public const int RightFoot       = 9;
    public const int Spine1          = 12;
    public const int Spine2          = 13;
    public const int Spine3          = 14;
    public const int Spine4          = 15;
    public const int Spine5          = 16;
    public const int Spine6          = 17;
    public const int Spine7          = 18;
    public const int LeftShoulder1   = 19;   // ← prosthetic attaches here
    public const int LeftArm         = 20;   // left upper arm
    public const int LeftForearm     = 21;
    public const int LeftHand        = 22;
    // left hand fingers 23-56
    public const int RightShoulder1  = 57;   // ← mirror source
    public const int RightArm        = 58;   // right upper arm
    public const int RightForearm    = 59;
    public const int RightHand       = 60;
    // right hand fingers 61-91
    public const int Neck1           = 84;
    public const int Head            = 85;

    public const int TotalJoints     = 91;
}
