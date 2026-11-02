
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class FinishZone : MonoBehaviour
{
    [SerializeField] private string winMessage = "Você chegou ao fim!";
    [SerializeField] private float restartDelay = 4f;

    private bool finished;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (finished || !other.CompareTag("Player")) return;

        finished = true;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
