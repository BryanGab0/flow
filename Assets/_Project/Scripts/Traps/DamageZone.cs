using UnityEngine;

// DamageZone: qualquer objeto com este script tira vida do jogador ao encostar.
// O MESMO script serve para os espinhos (25) e para o laser (34): só muda o valor no Inspector.
// CONCEITOS: OnTriggerEnter / OnTriggerStay, GetComponentInParent, cooldown com Time.time.
public class DamageZone : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private int damage = 25;
    [SerializeField] private float damageCooldown = 1f;   // tempo mínimo entre dois danos

    [Header("Efeito ao acertar (opcional)")]
    [SerializeField] private GameObject hitFxPrefab;      // ex.: FX_Sparks quando o laser acerta
    [SerializeField] private float hitFxLifetime = 1.5f;

    private float nextDamageTime;

    // Reset() roda quando você adiciona o script pelo Inspector:
    // já marca o collider como Trigger pra você não esquecer.
    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // Enter: no momento em que o jogador encosta.
    private void OnTriggerEnter(Collider other) => TryDamage(other);

    // Stay: enquanto ele continua dentro. Sem isso, quem fica parado
    // em cima do espinho só tomaria dano uma vez.
    private void OnTriggerStay(Collider other) => TryDamage(other);

    private void TryDamage(Collider other)
    {
        if (Time.time < nextDamageTime) return;

        // Procura o PlayerHealth no objeto que encostou ou em algum "pai" dele.
        // Se não achar, quem encostou não é o jogador e nada acontece.
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(damage);
        nextDamageTime = Time.time + damageCooldown;

        // Log pra conferir no Console (o PlayerHealth do grupo não escreve nada).
        Debug.Log($"{name} causou {damage} de dano. HP: {health.CurrentHealth}/{health.MaxHealth}");

        if (hitFxPrefab != null)
        {
            // Ponto do jogador mais perto da armadilha = onde "acertou".
            Vector3 hitPoint = other.bounds.ClosestPoint(transform.position);
            GameObject fx = Instantiate(hitFxPrefab, hitPoint, hitFxPrefab.transform.rotation);
            Destroy(fx, hitFxLifetime);
        }
    }
}