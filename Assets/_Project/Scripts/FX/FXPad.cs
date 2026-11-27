using UnityEngine;

// FXPad: plataforma que solta partículas quando o jogador passa por cima.
// CONCEITOS: Collider com "Is Trigger", OnTriggerEnter, CompareTag, Instantiate, Destroy.
public class FXPad : MonoBehaviour
{
    // [SerializeField] deixa a variável privada, mas visível no Inspector.
    // Assim você ajusta os valores no Unity sem mexer no código.
    [Header("Efeito")]
    [SerializeField] private GameObject fxPrefab;          // arraste aqui o prefab FX_Sparks
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.6f, 0f); // um pouco acima do centro da plataforma
    [SerializeField] private float fxLifetime = 2f;        // depois desse tempo a cópia é apagada

    [Header("Controle")]
    [SerializeField] private float cooldown = 0.5f;        // evita disparar 50 efeitos seguidos

    [Header("Pontos")]
    [SerializeField] private int pontosPrimeiraPassada = 25; // dado só na 1ª vez, pra ninguém ficar pulando no pad pra farmar

    private float nextFxTime;
    private bool jaPontuou;

    // O Unity chama esta função sozinho quando outro collider ENTRA num collider
    // marcado como "Is Trigger" neste objeto.
    private void OnTriggerEnter(Collider other)
    {
        // 1) Só reage ao jogador (o Player precisa ter a Tag "Player").
        if (!other.CompareTag("Player")) return;

        // 1.5) Pontos na primeira passada. Fica ANTES do cooldown e do check do prefab
        //      para pontuar mesmo se o efeito estiver desligado ou sem prefab.
        //      O "if" do GameManager deixa o pad funcionar nas cenas Dev (sem GameManager).
        if (!jaPontuou && GameManager.Instance != null)
        {
            GameManager.Instance.AdicionarPontos(pontosPrimeiraPassada);
            jaPontuou = true;
        }

        // 2) Respeita o cooldown.
        if (Time.time < nextFxTime) return;

        // 3) Evita erro se esqueceram de arrastar o prefab no Inspector.
        if (fxPrefab == null)
        {
            Debug.LogWarning($"{name}: nenhum fxPrefab foi definido no FXPad.");
            return;
        }

        // 4) Cria uma CÓPIA do prefab na cena.
        //    Uso a rotação do próprio prefab porque o Particle System padrão vem
        //    girado -90° em X para soltar partículas pra cima.
        Vector3 spawnPos = transform.position + spawnOffset;
        GameObject fx = Instantiate(fxPrefab, spawnPos, fxPrefab.transform.rotation);

        // 5) Agenda a destruição da cópia, senão elas se acumulam na cena.
        Destroy(fx, fxLifetime);

        nextFxTime = Time.time + cooldown;
    }
}