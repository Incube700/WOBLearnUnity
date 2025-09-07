using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Управляет движением и поворотом корпуса танка.
/// Предполагается, что «нос» корпуса направлен вдоль локальной оси Y (зелёная стрелка).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]                    // гарантируем наличие Rigidbody2D
public class PlayerController : MonoBehaviour
{
    [Header("Танк — движение (м/с)")]
    [SerializeField] private float maxForwardSpeed = 6f;   // макс. скорость вперёд
    [SerializeField] private float maxReverseSpeed = 3f;   // макс. скорость назад
    [SerializeField] private float acceleration = 8f;      // разгон (м/с^2)
    [SerializeField] private float brakeDecel = 12f;       // торможение (м/с^2)
    [SerializeField] private float idleDecel = 4f;         // замедление на холостом (м/с^2)

    [Header("Танк — поворот (°/с)")]
    [SerializeField] private float turnSpeedAtMax = 120f;  // скорость поворота на ходу
    [SerializeField] private float turnSpeedInPlace = 160f;// разворот на месте
    [SerializeField] private float minTurnFactor = 0.4f;   // поворот на малых скоростях (0..1)

    [Header("Ввод")]
    [SerializeField, Range(0f, 0.3f)] private float deadZone = 0.05f; // мёртвая зона джойстика

    // Пороговые значения для улучшения читаемости
    private const float BrakeSpeedThreshold = 0.01f;       // порог скорости для активации тормоза
    private const float TurnInPlaceSpeedThreshold = 0.02f; // порог скорости для разворота на месте
    private const float MinSpeedForTurnCalc = 0.001f;      // мин. скорость для расчёта поворота (избегаем деления на ноль)

    private Rigidbody2D rb;                                // кэш Rigidbody2D
    private float throttle;                                // ось газа −1..1
    private float steer;                                   // ось поворота −1..1
    private float currentSpeed;                            // текущая скорость вдоль корпуса
    
    [Header("Debug/Gizmos")]
    [SerializeField] private bool drawFrontGizmo = true;   // рисовать ли маркер «носа»
    [SerializeField] private float frontGizmoLength = 0.6f;// длина маркера
    [SerializeField] private Color frontGizmoColor = new Color(0f, 1f, 0f, 1f); // зелёный

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();                  // получаем компонент
        rb.gravityScale = 0f;                              // топ‑даун — без гравитации
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // сглаживаем физику
        rb.freezeRotation = false;                         // разрешаем вращение
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // для быстрых объектов
    }

    private void Update()
    {
        float h = 0f;                                      // ось поворота
        float v = 0f;                                      // ось газа

#if ENABLE_INPUT_SYSTEM
        // Новый Input System: предпочтительно используем централизованный GameInput
        if (GameInput.Instance != null)
        {
            Vector2 mv = GameInput.Instance.Move;
            h = mv.x;
            v = mv.y;
        }
        else
        {
            var gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stick = gamepad.leftStick.ReadValue(); // левый стик
                h = stick.x;
                v = stick.y;
            }
            else
            {
                var kbd = Keyboard.current;
                if (kbd != null)
                {
                    if (kbd.aKey.isPressed) h -= 1f;
                    if (kbd.dKey.isPressed) h += 1f;
                    if (kbd.sKey.isPressed) v -= 1f;
                    if (kbd.wKey.isPressed) v += 1f;
                }
            }
        }
#else
        // Старый Input Manager
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
#endif

        steer = Mathf.Abs(h) < deadZone ? 0f : h;          // фильтруем мёртвую зону
        throttle = Mathf.Abs(v) < deadZone ? 0f : v;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;                    // шаг физического кадра
        Vector2 forward = rb.transform.up;                 // локальная ось Y — «нос» танка

        // -------- Продольная динамика --------
        if (Mathf.Abs(throttle) > 0f)                      // газ нажали
        {
            float desiredSign = Mathf.Sign(throttle);      // направление желаемого движения

            // Если едем задом, а жмём вперёд — сначала тормозим
            if (Mathf.Sign(currentSpeed) != desiredSign && Mathf.Abs(currentSpeed) > BrakeSpeedThreshold)
            {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, brakeDecel * dt); // плавное торможение
            currentSpeed = Vector2.Dot(rb.linearVelocity, forward); // пересчитываем скаляр скорости
            }
            else
            {
                float targetMax = desiredSign > 0f ? maxForwardSpeed : maxReverseSpeed; // предел скорости
                float targetSpeed = throttle * targetMax; // желаемая скорость
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * dt); // разгон/замедление
                rb.linearVelocity = forward * currentSpeed;           // задаём скорость вдоль корпуса
            }
        }
        else                                               // газ отпущен
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, idleDecel * dt); // катимся и тормозим
            currentSpeed = Vector2.Dot(rb.linearVelocity, forward); // пересчитываем скаляр скорости
        }

        // -------- Поворот корпуса --------
        float angularSpeed = 0f;
        if (Mathf.Abs(steer) > 0f)                         // есть ввод поворота
        {
            if (Mathf.Abs(currentSpeed) > TurnInPlaceSpeedThreshold) // едем — поворот зависит от скорости
            {
                float speed01 = Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(MinSpeedForTurnCalc, maxForwardSpeed)); // нормируем скорость
                float turnAtSpeed = Mathf.Lerp(turnSpeedAtMax * minTurnFactor, turnSpeedAtMax, speed01);     // скорость поворота
                angularSpeed = -steer * turnAtSpeed;       // инвертируем знак: A-влево, D-вправо
            }
            else                                           // стоим — разворот на месте
                angularSpeed = -steer * turnSpeedInPlace;  // инвертируем знак: A-влево, D-вправо
        }
        rb.angularVelocity = angularSpeed;                 // задаём угловую скорость
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawFrontGizmo) return;
        Gizmos.color = frontGizmoColor;
        Vector3 p = transform.position;
        Vector3 tip = p + transform.up * frontGizmoLength;
        Gizmos.DrawLine(p, tip);
        Gizmos.DrawSphere(tip, frontGizmoLength * 0.08f);
    }
#endif
}
