using UnityEngine;

public class WallMover : MonoBehaviour
{
    public bool jaBateu;

    WallGame jogo;
    Rigidbody rb;
    float velocidade;
    float zFinal;
    bool contou;

    public void Iniciar(WallGame jogo, float velocidade, float zFinal)
    {
        this.jogo = jogo;
        this.velocidade = velocidade;
        this.zFinal = zFinal;

        rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        if (jogo == null || jogo.perdeu) return;

        Vector3 p = rb.position + Vector3.back * (velocidade * Time.fixedDeltaTime);
        rb.MovePosition(p);

        if (!contou && p.z < -2f)
        {
            contou = true;
            jogo.Passou();
        }
        if (p.z < zFinal) Destroy(gameObject);
    }
}
