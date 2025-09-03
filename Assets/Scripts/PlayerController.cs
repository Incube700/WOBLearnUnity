using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 5f; // юниты/сек
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // топ-даун — без гравитации
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // сглаживание
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // быстрые движения
    }

    void Update()
    {
        // Ввод читаем каждый кадр
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(x, y).normalized; // диагональ не ускоряет
    }

    void FixedUpdate()
    {
        // Двигаем через физику
        Vector2 target = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(target);
    }

    public void SetMoveSpeed(float speed) => moveSpeed = Mathf.Max(0f, speed);
}