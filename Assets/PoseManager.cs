using UnityEngine;

public class PoseManager : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string poseQ = "wall4_mcp";
    [SerializeField] private string poseW = "wall7_mcp";
    [SerializeField] private string poseE = "wall6_mcp";

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) Pose(poseQ);
        if (Input.GetKeyDown(KeyCode.W)) Pose(poseW);
        if (Input.GetKeyDown(KeyCode.E)) Pose(poseE);
    }

    void Pose(string estado)
    {
        if (playerAnimator == null)
        {
            Debug.LogError("[Pose] falta arrastar o Animator do ViconActor para o PoseManager", this);
            return;
        }
        if (!playerAnimator.HasState(0, Animator.StringToHash(estado)))
        {
            Debug.LogError($"[Pose] o Animator não tem o estado '{estado}'", this);
            return;
        }

        Debug.Log("[Pose] " + estado, this);
        playerAnimator.CrossFadeInFixedTime(estado, 0.25f, 0, 0f);
    }
}
