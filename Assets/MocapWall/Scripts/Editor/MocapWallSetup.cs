using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Ferramenta opcional: volta a preparar as animações dos FBX
// (corta a T-pose do início, põe o idle em loop e usa o esqueleto certo).
// Só é precisa se gravares mocap novo. Podes apagar este ficheiro se não precisares.
public static class MocapWallSetup
{
    const string Pasta = "Assets/MocapWall";
    const string PastaClips = Pasta + "/Clips";
    const string ControllerNovo = Pasta + "/Anima_MocapWall.controller";

    static readonly (string fbx, float corte, bool loop)[] Ficheiros =
    {
        ("Assets/idle.fbx", 3.0f, true),
        ("Assets/wall4.fbx", 1.5f, false),
        ("Assets/wall6.fbx", 1.5f, false),
        ("Assets/wall7.fbx", 1.5f, false),
    };

    [MenuItem("Mocap Wall/Preparar animações")]
    public static void PrepararAnimacoes()
    {
        GameObject personagem = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ViconActor.fbx");
        Transform alvo = personagem != null ? Procurar(personagem.transform, "Hips") : null;
        if (alvo == null)
        {
            EditorUtility.DisplayDialog("Mocap Wall", "Não encontrei o ViconActor.fbx com o osso Hips.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(PastaClips)) AssetDatabase.CreateFolder(Pasta, "Clips");
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerNovo);

        foreach (var f in Ficheiros)
        {
            GameObject modelo = AssetDatabase.LoadAssetAtPath<GameObject>(f.fbx);
            AnimationClip original = AssetDatabase.LoadAllAssetsAtPath(f.fbx).OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
            if (modelo == null || original == null) continue;

            // Cada FBX tem 2 esqueletos; o certo é o que tem Neck1 (igual ao ViconActor).
            Transform fonte = modelo.GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => Limpo(t.name) == "Hips" && Procurar(t, "Neck1") != null);
            if (fonte == null) continue;

            var mapa = new Dictionary<string, string>();
            Mapear(fonte, alvo, modelo.transform, personagem.transform, mapa);

            float fim = original.length;
            float inicio = Mathf.Clamp(f.corte, 0f, fim - 0.1f);
            var bindings = new List<EditorCurveBinding>();
            var curvas = new List<AnimationCurve>();
            foreach (EditorCurveBinding b in AnimationUtility.GetCurveBindings(original))
            {
                if (b.type != typeof(Transform) || !mapa.TryGetValue(b.path, out string destino)) continue;
                AnimationCurve c = AnimationUtility.GetEditorCurve(original, b);
                if (c == null || c.length == 0) continue;
                EditorCurveBinding nb = b;
                nb.path = destino;
                bindings.Add(nb);
                curvas.Add(Cortar(c, inicio, fim));
            }

            var clip = new AnimationClip { name = original.name, frameRate = original.frameRate };
            AnimationUtility.SetEditorCurves(clip, bindings.ToArray(), curvas.ToArray());
            clip.EnsureQuaternionContinuity();
            AnimationClipSettings cfg = AnimationUtility.GetAnimationClipSettings(clip);
            cfg.loopTime = f.loop;
            cfg.loopBlend = f.loop;
            AnimationUtility.SetAnimationClipSettings(clip, cfg);

            string caminho = $"{PastaClips}/{original.name}.anim";
            var existente = AssetDatabase.LoadAssetAtPath<AnimationClip>(caminho);
            if (existente != null)
            {
                EditorUtility.CopySerialized(clip, existente);
                EditorUtility.SetDirty(existente);
                clip = existente;
            }
            else AssetDatabase.CreateAsset(clip, caminho);

            if (ctrl != null)
            {
                foreach (ChildAnimatorState s in ctrl.layers[0].stateMachine.states)
                    if (s.state.name == original.name) s.state.motion = clip;
                EditorUtility.SetDirty(ctrl);
            }
            Debug.Log($"[MocapWall] {original.name}: {bindings.Count} curvas, {fim - inicio:0.0}s{(f.loop ? " (loop)" : "")}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void Mapear(Transform fonte, Transform alvo, Transform raizFonte, Transform raizAlvo, Dictionary<string, string> mapa)
    {
        mapa[AnimationUtility.CalculateTransformPath(fonte, raizFonte)] = AnimationUtility.CalculateTransformPath(alvo, raizAlvo);
        foreach (Transform f in fonte)
        {
            Transform a = null;
            foreach (Transform c in alvo)
                if (c.name == f.name || Limpo(c.name) == Limpo(f.name)) { a = c; break; }
            if (a != null) Mapear(f, a, raizFonte, raizAlvo, mapa);
        }
    }

    static string Limpo(string nome) => Regex.Replace(nome, @"(\s*\(\d+\)|[ _.]\d+)$", "");

    static AnimationCurve Cortar(AnimationCurve c, float inicio, float fim)
    {
        var keys = new List<Keyframe> { Key(c, inicio, 0f) };
        foreach (Keyframe k in c.keys)
            if (k.time > inicio + 0.0005f && k.time < fim - 0.0005f)
            {
                Keyframe n = k;
                n.time -= inicio;
                keys.Add(n);
            }
        keys.Add(Key(c, fim, fim - inicio));
        return new AnimationCurve(keys.ToArray());
    }

    static Keyframe Key(AnimationCurve c, float t, float novoTempo)
    {
        const float d = 1f / 120f;
        float inc = (c.Evaluate(t + d) - c.Evaluate(t - d)) / (2f * d);
        return new Keyframe(novoTempo, c.Evaluate(t), inc, inc);
    }

    static Transform Procurar(Transform raiz, string nome)
    {
        if (Limpo(raiz.name) == nome) return raiz;
        foreach (Transform filho in raiz)
        {
            Transform r = Procurar(filho, nome);
            if (r != null) return r;
        }
        return null;
    }
}
