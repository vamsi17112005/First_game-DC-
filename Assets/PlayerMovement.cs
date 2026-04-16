using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveDistance = 1.2f;
    public EnemyAI enemy;

    // 🔥 King transformation
    public Sprite mountedHorseSprite;
    public Transform king;
    public float triggerDistance = 2f;

    // 🔥 Game Over UI
    public GameObject gameEndUI;

    // 🔥 Boundaries
    public float minX = -1f;
    public float maxX = 1f;
    

    private SpriteRenderer sr;
    private bool hasTransformed = false;
    private bool isMoving = false;

    Vector2 startTouch;
    Vector2 endTouch;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isMoving) return;

        // 🔥 Check king distance
        if (!hasTransformed && king != null)
        {
            float distance = Vector2.Distance(transform.position, king.position);
            if (distance <= triggerDistance)
            {
                TransformHorse();
            }
        }

        // -------- PC CONTROLS --------
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (h != 0 || v != 0)
        {
            if (Mathf.Abs(h) > Mathf.Abs(v))
                Move(new Vector3(h, 0, 0));
            else
                Move(new Vector3(0, v, 0));

            return;
        }

        // -------- MOBILE TOUCH --------
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                startTouch = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                endTouch = touch.position;
                HandleSwipe(endTouch - startTouch);
            }
        }

        // -------- PC MOUSE SWIPE --------
        if (Input.GetMouseButtonDown(0))
            startTouch = Input.mousePosition;

        if (Input.GetMouseButtonUp(0))
        {
            endTouch = Input.mousePosition;
            HandleSwipe(endTouch - startTouch);
        }
    }

    void HandleSwipe(Vector2 swipe)
    {
        if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
        {
            if (swipe.x > 0)
                Move(Vector3.right);
            else
                Move(Vector3.left);
        }
        else
        {
            if (swipe.y > 0)
                Move(Vector3.up);
            else
                Move(Vector3.down);
        }
    }

    void Move(Vector3 dir)
    {
        float step = 0.1f;
        float moved = 0f;

        Vector3 currentPos = transform.position;

        while (moved < moveDistance)
        {
            Vector3 nextPos = currentPos + dir * step;

            if (IsBlocked(nextPos))
                break;

            currentPos = nextPos;
            moved += step;
        }

        // 🔥 Clamp inside map
        currentPos.x = Mathf.Clamp(currentPos.x, minX, maxX);
       // currentPos.y = Mathf.Clamp(currentPos.y, minY, maxY);

        transform.position = currentPos;

        isMoving = true;

        if (enemy != null)
            enemy.TakeTurn();

        Invoke(nameof(ResetMove), 0.2f);
    }

    void ResetMove()
    {
        isMoving = false;
    }

    // 🔥 Obstacle check
    bool IsBlocked(Vector3 targetPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.2f);

        if (hit != null && hit.CompareTag("Obstacle"))
            return true;

        return false;
    }

    // 🔥 Transform horse
    void TransformHorse()
    {
        Vector2 originalSize = sr.bounds.size;

        sr.sprite = mountedHorseSprite;

        Vector2 newSize = sr.bounds.size;
        if (newSize.x != 0 && newSize.y != 0)
        {
            float scaleX = originalSize.x / newSize.x;
            float scaleY = originalSize.y / newSize.y;

            transform.localScale = new Vector3(
                transform.localScale.x * scaleX,
                transform.localScale.y * scaleY,
                transform.localScale.z
            );
        }

        if (king != null)
        {
            Destroy(king.gameObject);
        }

        hasTransformed = true;
    }

    // 🔥 Gate collision (backup safety)
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Gate"))
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isMoving = true;

        if (enemy != null)
            enemy.enabled = false;

        if (gameEndUI != null)
            gameEndUI.SetActive(true);

        Time.timeScale = 0f;
    }
}