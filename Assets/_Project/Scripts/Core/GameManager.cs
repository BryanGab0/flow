using UnityEngine;
using TMPro;

// GameManager: guarda os PONTOS e as VIDAS da partida e atualiza o HUD.
//
// Por que ele existe?
// O PlayerHealth recarrega a cena quando o jogador morre. Recarregar a cena
// destrói todos os objetos dela, então qualquer contador que morasse na cena
// voltaria ao valor inicial a cada morte. Este script usa o padrão SINGLETON
// com DontDestroyOnLoad para sobreviver ao recarregamento e manter os valores.
//
// Como usar a partir de outros scripts:
//     if (GameManager.Instance != null) GameManager.Instance.AdicionarPontos(10);
// O "if" deixa o código seguro nas cenas de teste (Dev_*), que não têm GameManager.
//
// CONCEITOS: singleton, propriedade estática, DontDestroyOnLoad, TextMeshPro.
public class GameManager : MonoBehaviour
{
    // "static" = pertence à CLASSE, não a um objeto. Existe uma só no jogo inteiro,
    // e qualquer script consegue chegar nela escrevendo GameManager.Instance.
    // "private set" = só o próprio GameManager pode trocar quem é a instância.
    public static GameManager Instance { get; private set; }

    [Header("Regras")]
    [SerializeField] private int vidasIniciais = 3;

    [Header("HUD (arraste os textos do Canvas filho)")]
    [SerializeField] private TextMeshProUGUI pontosText;   // ao lado da moeda, canto superior direito
    [SerializeField] private TextMeshProUGUI vidasText;    // ao lado do coração, canto superior esquerdo

    // Leitura pública, escrita só por aqui: os outros scripts podem CONSULTAR
    // os valores, mas só mudam pelos métodos abaixo (que também atualizam o HUD).
    public int Pontos { get; private set; }
    public int Vidas { get; private set; }

    // Garante que a referência estática comece vazia a cada Play, mesmo se alguém
    // desligar o "Domain Reload" nas opções do Editor (aí os statics não são zerados sozinhos).
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void LimparInstanciaAoIniciar()
    {
        Instance = null;
    }

    private void Awake()
    {
        // Quando a cena recarrega, a Main cria um GameManager NOVO (o que está salvo nela).
        // Mas o antigo sobreviveu graças ao DontDestroyOnLoad. Se deixássemos os dois,
        // teríamos dois HUDs e dois contadores. Então o novo percebe que já existe
        // um "oficial" e se destrói, mantendo os pontos e vidas do antigo.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Tira este objeto (e os filhos, como o Canvas do HUD) da cena atual,
        // para que ele NÃO seja destruído quando outra cena for carregada.
        // Só funciona em objetos na raiz da hierarquia, por isso o GameManager não tem pai.
        DontDestroyOnLoad(gameObject);

        ReiniciarPartida();
    }

    // Soma pontos (moeda, FXPad, chegada...). Valores negativos são ignorados
    // para ninguém "tirar pontos" sem querer usando este método.
    public void AdicionarPontos(int quantidade)
    {
        if (quantidade <= 0) return;

        Pontos += quantidade;
        AtualizarHUD();
    }

    // Tira uma vida. Quem chama é o PlayerHealth.Die(), que logo depois recarrega a cena.
    // Se as vidas acabarem, é game over: a partida volta do zero (pontos e vidas).
    public void PerderVida()
    {
        Vidas = Mathf.Max(Vidas - 1, 0);

        if (Vidas == 0)
        {
            Debug.Log($"Game over! Pontuação final: {Pontos}");
            ReiniciarPartida();
            return;
        }

        AtualizarHUD();
    }

    // Volta pontos e vidas aos valores iniciais. Usado ao começar o jogo,
    // no game over e quando o jogador termina a fase (nova tentativa do zero).
    public void ReiniciarPartida()
    {
        Pontos = 0;
        Vidas = vidasIniciais;
        AtualizarHUD();
    }

    // Um único lugar que escreve no HUD: assim nenhum valor fica "desatualizado"
    // na tela, porque todo método que muda Pontos/Vidas termina chamando este.
    private void AtualizarHUD()
    {
        // Checagem de null: se alguém esquecer de ligar o texto no Inspector,
        // o jogo continua funcionando (só não mostra), em vez de dar erro.
        if (pontosText != null) pontosText.text = Pontos.ToString();
        if (vidasText != null) vidasText.text = $"x {Vidas}";
    }
}
