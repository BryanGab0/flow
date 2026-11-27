
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class FinishZone : MonoBehaviour
{
    [SerializeField] private string winMessage = "Você chegou ao fim!";
    [SerializeField] private float restartDelay = 4f;
    [SerializeField] private int bonusChegada = 100; // pontos extras por terminar a fase

    private bool finished;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished || !other.CompareTag("Player")) return;

        finished = true;

        // Bônus de chegada no HUD de pontos (o "if" protege as cenas sem GameManager).
        if (GameManager.Instance != null) GameManager.Instance.AdicionarPontos(bonusChegada);

        PlayerMovement movement = other.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.LockCursor(false);
            movement.enabled = false;
        }

        PlayerHUD hud = other.GetComponentInChildren<PlayerHUD>();

        if (hud != null)
        {
            hud.ShowMessage(winMessage);
        }

        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);

        // Fase concluída: a próxima tentativa começa do zero (pontos e vidas).
        // Sem isso, o GameManager (que sobrevive ao recarregamento) somaria as partidas.
        if (GameManager.Instance != null) GameManager.Instance.ReiniciarPartida();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
