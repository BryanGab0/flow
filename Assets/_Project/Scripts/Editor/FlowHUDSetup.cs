using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Object = UnityEngine.Object;

// FlowHUDSetup: ferramenta SÓ DE EDITOR (fica na pasta "Editor", não vai pro jogo final).
// Menu: Tools > Flow > Configurar HUD e Moedas
//
// Por que um script em vez de montar na mão?
// Montar Canvas, textos, âncoras e prefabs clicando é fácil de errar e difícil de revisar.
// Com o script, a montagem fica documentada em código e pode ser refeita a qualquer momento
// (rodar de novo sobrescreve os prefabs, mantendo as referências da cena).
//
// O que ele faz:
// 1) Gera os sprites PLACEHOLDER de moeda e coração (PNG desenhado por código).
// 2) Cria o material dourado e o prefab da moeda (Prefabs/Collectibles/Coin).
// 3) Cria o prefab do GameManager com o Canvas do HUD como filho (Prefabs/Core/GameManager).
// 4) Abre a Main, coloca 1 GameManager e 5 moedas (se ainda não existirem) e salva.
public static class FlowHUDSetup
{
    private const string ArtDir = "Assets/_Project/Art/UI/Placeholder";
    private const string CoinSpritePath = ArtDir + "/Coin_Placeholder.png";
    private const string HeartSpritePath = ArtDir + "/Heart_Placeholder.png";
    private const string CoinMaterialPath = "Assets/_Project/Materials/M_Coin.mat";
    private const string CoinPrefabPath = "Assets/_Project/Prefabs/Collectibles/Coin.prefab";
    private const string GameManagerPrefabPath = "Assets/_Project/Prefabs/Core/GameManager.prefab";
    private const string SparksPrefabPath = "Assets/_Project/Prefabs/FX/FX_Sparks.prefab";
    private const string MainScenePath = "Assets/_Project/Scenes/Main.unity";

    // Mesma fonte do HealthText do Player (LiberationSans SDF), para o HUD ficar com a mesma cara.
    private const string FonteGuid = "8f586378b4e144a9851e7b34d9b748ee";

    private const int QuantidadeMoedas = 5;
    private const int LayerUI = 5;

    private static readonly Color Dourado = new Color(1f, 0.82f, 0.15f);
    private static readonly Color DouradoEscuro = new Color(0.78f, 0.52f, 0.05f);
    private static readonly Color Vermelho = new Color(0.9f, 0.12f, 0.18f);

    [MenuItem("Tools/Flow/Configurar HUD e Moedas")]
    public static void Configurar()
    {
        // Abrir/salvar cenas não é permitido com o jogo rodando (o Unity lança exceção).
        // Avisamos de forma amigável em vez de deixar o erro aparecer no Console.
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorUtility.DisplayDialog("Flow", "Saia do modo Play antes de configurar o HUD.", "OK");
            return;
        }

        // Pergunta se o usuário quer salvar a cena aberta antes de trocarmos para a Main.
        // Se ele cancelar, paramos sem mexer em nada.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Sprite spriteMoeda = CriarSprite(CoinSpritePath, PixelMoeda);
        Sprite spriteCoracao = CriarSprite(HeartSpritePath, PixelCoracao);

        GameObject prefabMoeda = CriarPrefabMoeda();
        GameObject prefabGameManager = CriarPrefabGameManager(spriteMoeda, spriteCoracao);

        ColocarNaMain(prefabGameManager, prefabMoeda);

        Debug.Log("[FlowHUDSetup] Pronto: sprites placeholder, Coin, GameManager/HUD e Main atualizados.");
    }

    // ---------- 1) Sprites placeholder ----------

    // Desenha um PNG 128x128 pixel a pixel. "pixel" recebe coordenadas de -1 a 1
    // (centro da imagem = 0,0) e devolve a cor daquele ponto.
    private static Sprite CriarSprite(string caminho, Func<float, float, Color> pixel)
    {
        Directory.CreateDirectory(ArtDir);

        const int tamanho = 128;
        Texture2D tex = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false);

        for (int y = 0; y < tamanho; y++)
        {
            for (int x = 0; x < tamanho; x++)
            {
                float u = (x + 0.5f) / tamanho * 2f - 1f;
                float v = (y + 0.5f) / tamanho * 2f - 1f;
                tex.SetPixel(x, y, pixel(u, v));
            }
        }

        File.WriteAllBytes(caminho, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);

        // Importa o PNG e configura como Sprite (o componente Image da UI só aceita Sprite).
        AssetDatabase.ImportAsset(caminho, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(caminho);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
    }

    // Moeda: círculo dourado com borda mais escura. A borda é suavizada (alpha gradual)
    // para não ficar serrilhada.
    private static Color PixelMoeda(float u, float v)
    {
        float r = Mathf.Sqrt(u * u + v * v);
        Color cor = r > 0.78f ? DouradoEscuro : Dourado;
        cor.a = Mathf.Clamp01((0.95f - r) * 40f);
        return cor;
    }

    // Coração: usa a equação clássica (x² + y² - 1)³ - x²·y³ <= 0, que forma um coração.
    private static Color PixelCoracao(float u, float v)
    {
        float x = u * 1.3f;
        float y = v * 1.3f + 0.05f;
        float a = x * x + y * y - 1f;
        float f = a * a * a - x * x * y * y * y;
        Color cor = Vermelho;
        cor.a = f <= 0f ? 1f : 0f;
        return cor;
    }

    // ---------- 2) Moeda ----------

    private static GameObject CriarPrefabMoeda()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(CoinMaterialPath);
        if (material == null)
        {
            // O projeto é URP, então usamos o shader Lit do URP. Emissão leve para a moeda "brilhar".
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader);
            material.SetColor("_BaseColor", Dourado);
            material.SetFloat("_Metallic", 0.6f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Dourado * 0.5f);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            AssetDatabase.CreateAsset(material, CoinMaterialPath);
        }

        // Raiz: só o trigger + o script. O visual fica num filho, assim dá pra trocar
        // o modelo depois sem mexer na lógica.
        GameObject raiz = new GameObject("Coin");
        SphereCollider gatilho = raiz.AddComponent<SphereCollider>();
        gatilho.isTrigger = true;
        gatilho.radius = 0.5f;
        Coin coin = raiz.AddComponent<Coin>();

        // Visual: cilindro achatado e em pé = moeda. Tiramos o collider do cilindro
        // para ele não virar um obstáculo sólido.
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        visual.name = "Visual";
        Object.DestroyImmediate(visual.GetComponent<Collider>());
        visual.transform.SetParent(raiz.transform, false);
        visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        visual.transform.localScale = new Vector3(0.6f, 0.04f, 0.6f);
        visual.GetComponent<MeshRenderer>().sharedMaterial = material;

        // Reaproveita as faíscas do projeto como efeito de coleta.
        GameObject faiscas = AssetDatabase.LoadAssetAtPath<GameObject>(SparksPrefabPath);
        if (faiscas != null) DefinirReferencia(coin, "coletaFxPrefab", faiscas);

        return SalvarPrefab(raiz, CoinPrefabPath);
    }

    // ---------- 3) GameManager + HUD ----------

    private static GameObject CriarPrefabGameManager(Sprite spriteMoeda, Sprite spriteCoracao)
    {
        GameObject raiz = new GameObject("GameManager");
        GameManager gameManager = raiz.AddComponent<GameManager>();

        // Canvas como FILHO do GameManager: o DontDestroyOnLoad leva os filhos junto,
        // então o HUD também sobrevive ao recarregamento da cena.
        GameObject canvasGO = new GameObject("HUD_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasGO.layer = LayerUI;
        canvasGO.transform.SetParent(raiz.transform, false);

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10; // desenha por cima do Canvas do Player

        // Mesma configuração do Canvas do Player: escala proporcional a partir de 1920x1080,
        // assim o HUD tem o mesmo tamanho relativo em qualquer resolução.
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform hud = canvasGO.transform;

        // VIDAS no canto SUPERIOR ESQUERDO, logo abaixo do "HP: 100" do Player
        // (que ocupa de y = -20 a y = -100). Âncora e pivô em (0,1) = canto superior esquerdo:
        // as posições contam a partir desse canto, então o HUD "gruda" nele em qualquer tela.
        Vector2 cantoSupEsq = new Vector2(0f, 1f);
        CriarImagem(hud, "Coracao", spriteCoracao, cantoSupEsq, new Vector2(30f, -110f));
        TextMeshProUGUI vidasText = CriarTexto(hud, "VidasText", "x 3", cantoSupEsq,
            new Vector2(104f, -110f), new Vector2(200f, 64f), TextAlignmentOptions.MidlineLeft);

        // PONTOS no canto SUPERIOR DIREITO: âncora e pivô em (1,1).
        // Posições negativas em X andam para a esquerda, a partir da borda direita.
        Vector2 cantoSupDir = new Vector2(1f, 1f);
        CriarImagem(hud, "Moeda", spriteMoeda, cantoSupDir, new Vector2(-30f, -30f));
        TextMeshProUGUI pontosText = CriarTexto(hud, "PontosText", "0", cantoSupDir,
            new Vector2(-104f, -30f), new Vector2(300f, 64f), TextAlignmentOptions.MidlineRight);

        // Liga os textos nos campos [SerializeField] privados do GameManager.
        DefinirReferencia(gameManager, "pontosText", pontosText);
        DefinirReferencia(gameManager, "vidasText", vidasText);

        return SalvarPrefab(raiz, GameManagerPrefabPath);
    }

    private static void CriarImagem(Transform pai, string nome, Sprite sprite, Vector2 canto, Vector2 posicao)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(Image));
        go.layer = LayerUI;
        go.transform.SetParent(pai, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = canto;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = new Vector2(64f, 64f);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false; // HUD não é clicável; desligar evita trabalho à toa da UI
    }

    private static TextMeshProUGUI CriarTexto(Transform pai, string nome, string texto, Vector2 canto,
        Vector2 posicao, Vector2 tamanho, TextAlignmentOptions alinhamento)
    {
        GameObject go = new GameObject(nome, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.layer = LayerUI;
        go.transform.SetParent(pai, false);

        RectTransform rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = canto;
        rt.anchoredPosition = posicao;
        rt.sizeDelta = tamanho;

        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        TMP_FontAsset fonte = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(FonteGuid));
        tmp.font = fonte != null ? fonte : TMP_Settings.defaultFontAsset;
        tmp.text = texto;
        tmp.fontSize = 44;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = alinhamento;
        tmp.raycastTarget = false;
        return tmp;
    }

    // ---------- 4) Cena Main ----------

    private static void ColocarNaMain(GameObject prefabGameManager, GameObject prefabMoeda)
    {
        var cena = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

        // Só um GameManager na cena (rodar o menu duas vezes não duplica).
        // Ele fica na RAIZ da hierarquia porque o DontDestroyOnLoad exige isso.
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            PrefabUtility.InstantiatePrefab(prefabGameManager, cena);
        }

        // Moedas só são criadas se ainda não houver nenhuma (pra não apagar o seu posicionamento).
        if (Object.FindFirstObjectByType<Coin>() == null)
        {
            GameObject grupo = new GameObject("Moedas"); // pai só para organizar a hierarquia

            PlayerMovement player = Object.FindFirstObjectByType<PlayerMovement>();
            FinishZone chegada = Object.FindFirstObjectByType<FinishZone>();
            Vector3 inicio = player != null ? player.transform.position : Vector3.zero;
            Vector3 fim = chegada != null ? chegada.transform.position : inicio + Vector3.forward * 30f;

            // Garante que a física do editor conheça a posição atual dos colliders antes do Raycast.
            Physics.SyncTransforms();

            // Ponto de partida: espalha as moedas em linha reta entre o spawn e a chegada.
            // Para cada ponto, um raio de cima pra baixo acha o chão/plataforma, e a moeda
            // fica 1 m acima dele. Você reposiciona depois no editor.
            for (int i = 1; i <= QuantidadeMoedas; i++)
            {
                float t = i / (QuantidadeMoedas + 1f);
                Vector3 ponto = Vector3.Lerp(inicio, fim, t);

                Vector3 origemRaio = ponto + Vector3.up * 10f;
                if (Physics.Raycast(origemRaio, Vector3.down, out RaycastHit hit, 40f, ~0, QueryTriggerInteraction.Ignore))
                {
                    ponto = hit.point + Vector3.up;
                }
                else
                {
                    ponto += Vector3.up;
                }

                GameObject moeda = (GameObject)PrefabUtility.InstantiatePrefab(prefabMoeda, cena);
                moeda.name = $"Coin_{i}";
                moeda.transform.SetParent(grupo.transform, true);
                moeda.transform.position = ponto;
            }
        }

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
    }

    // ---------- utilitários ----------

    // Campos [SerializeField] privados não são acessíveis por código comum.
    // O SerializedObject edita o que o Inspector edita, do mesmo jeito que arrastar no Inspector.
    private static void DefinirReferencia(Object alvo, string campo, Object valor)
    {
        SerializedObject so = new SerializedObject(alvo);
        so.FindProperty(campo).objectReferenceValue = valor;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // Salva o objeto temporário como prefab e apaga o temporário da cena.
    // Se o prefab já existe, ele é sobrescrito mantendo o mesmo GUID (as cenas continuam ligadas).
    private static GameObject SalvarPrefab(GameObject raiz, string caminho)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(caminho));
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
        Object.DestroyImmediate(raiz);
        return prefab;
    }
}
