using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WallGame : MonoBehaviour
{
    [System.Serializable]
    public class TipoParede
    {
        public string nome = "Parede";
        public GameObject molde;
    }

    public static WallGame Instancia;

    public List<TipoParede> paredes = new List<TipoParede>();
    public float velocidade = 5f;
    public float zInicial = 30f;
    public float zFinal = -8f;
    public float intervalo = 8f;
    public bool automatico = true;
    public bool invencivel = false;

    public TMP_Text textoPontos;
    public TMP_Text textoMensagem;

    public int pontos;
    public bool perdeu;

    const string Controlos = "Q = T     W = I     E = superman     Espaço / 1 / 2 / 3 = mandar parede";

    float proxima;
    string ultimaParte;
    int pontosMostrados = -1;
    bool perdeuMostrado;

    void Awake() => Instancia = this;

    void Start()
    {
        foreach (var p in paredes)
            if (p.molde != null) p.molde.SetActive(false);
        if (textoPontos == null || textoMensagem == null) CriarUI();
        proxima = Time.time + 3f;
    }

    void Update()
    {
        if (pontos != pontosMostrados || perdeu != perdeuMostrado)
        {
            pontosMostrados = pontos;
            perdeuMostrado = perdeu;
            AtualizarUI();
        }

        if (perdeu)
        {
            if (Input.GetKeyDown(KeyCode.R)) Recomecar();
            return;
        }

        if (automatico && Time.time >= proxima) Criar(Random.Range(0, paredes.Count));
        if (Input.GetKeyDown(KeyCode.Space)) Criar(Random.Range(0, paredes.Count));
        if (Input.GetKeyDown(KeyCode.Alpha1)) Criar(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Criar(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Criar(2);
    }

    void Criar(int i)
    {
        if (i < 0 || i >= paredes.Count || paredes[i].molde == null) return;

        GameObject molde = paredes[i].molde;
        Vector3 pos = molde.transform.position;
        pos.z = zInicial;

        GameObject parede = Instantiate(molde, pos, molde.transform.rotation, transform);
        parede.name = paredes[i].nome;
        parede.AddComponent<WallMover>().Iniciar(this, velocidade, zFinal);
        parede.SetActive(true);

        proxima = Time.time + intervalo;
    }

    public void Passou()
    {
        if (!perdeu) pontos++;
    }

    public void Bateu(WallMover parede, string parte)
    {
        if (perdeu || parede.jaBateu) return;
        parede.jaBateu = true;
        if (invencivel)
        {
            Debug.Log($"[MocapWall] {parte} bateu na {parede.name}", parede);
            return;
        }

        perdeu = true;
        ultimaParte = parte;
    }

    void Recomecar()
    {
        foreach (Transform parede in transform) Destroy(parede.gameObject);
        pontos = 0;
        perdeu = false;
        proxima = Time.time + 3f;
    }

    void AtualizarUI()
    {
        if (textoPontos != null) textoPontos.text = "Pontos: " + pontos;
        if (textoMensagem == null) return;
        textoMensagem.text = perdeu ? $"PERDESTE ({ultimaParte})     R para recomeçar" : Controlos;
        textoMensagem.color = perdeu ? Color.red : Color.white;
    }

    void CriarUI()
    {
        var canvasGo = new GameObject("UI do Jogo", typeof(Canvas), typeof(CanvasScaler));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler escala = canvasGo.GetComponent<CanvasScaler>();
        escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escala.referenceResolution = new Vector2(1920, 1080);

        if (textoPontos == null)
        {
            textoPontos = CriarTexto(canvasGo.transform, "Pontos", 46, TextAlignmentOptions.TopLeft);
            RectTransform rt = textoPontos.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(40, -30);
            rt.sizeDelta = new Vector2(800, 90);
        }

        if (textoMensagem == null)
        {
            textoMensagem = CriarTexto(canvasGo.transform, "Mensagem", 34, TextAlignmentOptions.Bottom);
            RectTransform rt = textoMensagem.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(40, 30);
            rt.offsetMax = new Vector2(-40, 130);
        }

        if (textoPontos.font == null)
            Debug.LogError("[MocapWall] Falta importar o TextMeshPro: menu Window > TextMeshPro > Import TMP Essential Resources.", this);
    }

    static TMP_Text CriarTexto(Transform pai, string nome, float tamanho, TextAlignmentOptions alinhamento)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        TMP_Text texto = go.AddComponent<TextMeshProUGUI>();
        texto.fontSize = tamanho;
        texto.alignment = alinhamento;
        texto.color = Color.white;
        texto.raycastTarget = false;
        return texto;
    }
}
