using UnityEngine;

// Mover: faz o objeto ir e voltar numa direção (o laser sobe e desce).
// CONCEITOS: Start x Update/FixedUpdate, Mathf.PingPong, Rigidbody cinemático.
public class Mover : MonoBehaviour
{
    [SerializeField] private Vector3 direction = Vector3.up; // pra onde se move (up = sobe e desce)
    [SerializeField] private float distance = 2f;            // quantos metros percorre
    [SerializeField] private float speed = 1f;               // maior = mais rápido
    [SerializeField] private float startOffset = 0f;         // use valores diferentes em vários lasers pra eles não andarem sincronizados

    private Vector3 startPos;
    private Rigidbody rb;

    private void Start()
    {
        // Guarda a posição inicial. Todo o movimento é calculado a partir dela.
        startPos = transform.position;
        rb = GetComponent<Rigidbody>();
    }

    // FixedUpdate roda no ritmo da física. Como o laser tem Rigidbody,
    // movê-lo aqui deixa a detecção de colisão mais confiável.
    private void FixedUpdate()
    {
        // PingPong vai de 0 até 1 e volta pra 0, repetindo pra sempre.
        float t = Mathf.PingPong(Time.time * speed + startOffset, 1f);

        // SmoothStep suaviza: desacelera nas pontas em vez de bater e voltar.
        t = Mathf.SmoothStep(0f, 1f, t);

        Vector3 target = startPos + direction.normalized * (t * distance);

        if (rb != null) rb.MovePosition(target);
        else transform.position = target;
    }
}