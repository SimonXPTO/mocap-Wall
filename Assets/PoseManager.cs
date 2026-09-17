
using UnityEngine;

public class PoseManager : MonoBehaviour
{
    [SerializeField]private Animator playerAnimator;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            playerAnimator.SetTrigger("T");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            playerAnimator.SetTrigger("I");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerAnimator.SetTrigger("superman");
        }
    }
}
