using UnityEngine;

public class CustomerAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private static readonly int IsWalking =
        Animator.StringToHash("IsWalking");

    private static readonly int Celebration =
        Animator.StringToHash("Celebration");

    public void SetWalking(bool walking)
    {
        animator.SetBool(IsWalking, walking);
    }

    public void PlayCelebration()
    {
        animator.SetTrigger(Celebration);
    }
}