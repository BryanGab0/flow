using UnityEngine;

// Coin: moeda coletável. Ao encostar, dá pontos ao jogador e some.
// CONCEITOS: Collider com "Is Trigger", OnTriggerEnter, CompareTag, Destroy, Rotate.
public class Coin : MonoBehaviour
{
    [Header("Pontos")]
    [SerializeField] private int valor = 10;

    [Header("Visual")]
    [SerializeField] private float velocidadeGiro = 90f;   // graus por segundo: girar chama a atenção do jogador

    [Header("Efeito ao coletar (opcional)")]
    [SerializeField] private GameObject coletaFxPrefab;     // ex.: FX_Sparks
    [SerializeField] private float coletaFxLifetime = 1.5f;

    // Evita contar a mesma moeda duas vezes: o Destroy só acontece no fim do frame,
    // e nesse meio-tempo outro collider do jogador poderia disparar o trigger de novo.
    private bool coletada;

    // Reset() roda quando o script é adicionado pelo Inspector:
    // já marca o collider como Trigger (moeda não deve ser um obstáculo sólido).
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void Update()
    {
        // Space.World: gira em volta do eixo Y do mundo, mesmo se a moeda estiver inclinada.
        transform.Rotate(Vector3.up, velocidadeGiro * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Mesma regra do FXPad: só o jogador (Tag "Player") coleta.
        if (coletada || !other.CompareTag("Player")) return;

        coletada = true;

        // O "if" deixa a moeda funcionar em cenas sem GameManager (ela só some, sem pontuar).
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AdicionarPontos(valor);
        }

        if (coletaFxPrefab != null)
        {
            GameObject fx = Instantiate(coletaFxPrefab, transform.position, coletaFxPrefab.transform.rotation);
            Destroy(fx, coletaFxLifetime);
        }

        Destroy(gameObject);
    }
}
