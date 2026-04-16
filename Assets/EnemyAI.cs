using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Transform king;

    public float moveDistance = 1f;

    public GameObject retryUI;

    private Transform currentTarget;

    void Start()
    {
        currentTarget = king; // initially chase king
    }

    public void TakeTurn()
    {
        UpdateTarget();

        Vector3 bestMove = transform.position;
        float minDist = Vector3.Distance(transform.position, currentTarget.position);

        Vector3[] directions = {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right
        };

        foreach (Vector3 dir in directions)
        {
            Vector3 newPos = transform.position + dir * moveDistance;

            // 🚫 Skip obstacles
            Collider2D hit = Physics2D.OverlapCircle(newPos, 0.2f);
            if (hit != null && hit.CompareTag("Obstacle"))
                continue;

            float dist = Vector3.Distance(newPos, currentTarget.position);

            if (dist < minDist)
            {
                minDist = dist;
                bestMove = newPos;
            }
        }

        transform.position = bestMove;

        CheckGameOver();
    }

    // 🔥 Decide target dynamically
    void UpdateTarget()
    {
        if (king == null)
        {
            currentTarget = player; // king picked → chase player
        }
        else
        {
            currentTarget = king; // chase king
        }
    }

    void CheckGameOver()
    {
        // Touch player
        if (Vector2.Distance(transform.position, player.position) < 0.5f)
        {
            GameOver();
        }

        // Touch king (only if still exists)
        if (king != null && Vector2.Distance(transform.position, king.position) < 0.5f)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Time.timeScale = 0f;

        if (retryUI != null)
            retryUI.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}