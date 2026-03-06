using UnityEngine;

public class ProstheticAnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayGesture(GestureType gesture)
    {
        switch (gesture)
        {
            case GestureType.Forward:
                animator.SetTrigger("LiftUp");
                break;

            case GestureType.Side:
                animator.SetTrigger("LiftSide");
                break;

            case GestureType.None:
                animator.SetTrigger("Idle");
                break;
        }
    }
}