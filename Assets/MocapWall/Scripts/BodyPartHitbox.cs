using UnityEngine;

public class BodyPartHitbox : MonoBehaviour
{
    void OnTriggerEnter(Collider outro)
    {
        WallMover parede = outro.GetComponentInParent<WallMover>();
        if (parede != null && WallGame.Instancia != null)
            WallGame.Instancia.Bateu(parede, name);
    }
}
