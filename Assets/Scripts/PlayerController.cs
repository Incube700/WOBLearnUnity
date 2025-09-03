using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
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
    [SerializeField, Range(0f, 0.3f)] private float deadZone = 0.05f;

    private Rigidbody2D rb;
    private float throttle; // -1..1 (W/S)
    private float steer;    // -1..1 (A/D)
    private float currentSpeed; // текущая поступательная скорость вдоль корпуса (м/с)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // топ-даун — без гравитации
        rb.interpolation = RigidbodyInterpolation2D.Interpolate; // сглаживание
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // быстрые движения
    }

    void Update()
    {
        // Плавный ввод (ось с интерполяцией)
        steer = Input.GetAxis("Horizontal");
        throttle = Input.GetAxis("Vertical");

        // Мёртвая зона
        if (Mathf.Abs(steer) < deadZone) steer = 0f;
        if (Mathf.Abs(throttle) < deadZone) throttle = 0f;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // 1) Продольная динамика: разгон/торможение/замедление
        if (Mathf.Abs(throttle) > 0f)
        {
            float desiredSign = Mathf.Sign(throttle);

            // Если газ противоположен текущему движению — сначала тормозим
            if (Mathf.Sign(currentSpeed) != desiredSign && Mathf.Abs(currentSpeed) > 0.01f)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeDecel * dt);
            }
            else
            {
                float targetMax = desiredSign > 0f ? maxForwardSpeed : maxReverseSpeed;
                float targetSpeed = throttle * targetMax; // пропорционально положению газа
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * dt);
            }
        }
        else
        {
            // Отпущен газ — катимся и замедляемся
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, idleDecel * dt);
        }

        // 2) Поворот корпуса (как у танка)
        float deltaAngle = 0f;
        if (Mathf.Abs(steer) > 0f)
        {
            if (Mathf.Abs(currentSpeed) > 0.02f)
            {
                // На ходу: масштабируем скорость поворота от 0..1 по текущей скорости
                float speed01 = Mathf.Clamp01(Mathf.Abs(currentSpeed) / Mathf.Max(0.001f, maxForwardSpeed));
                float turnAtSpeed = Mathf.Lerp(turnSpeedAtMax * minTurnFactor, turnSpeedAtMax, speed01);
                // Танковое управление: поворот не инвертируется при движении назад
                deltaAngle = steer * turnAtSpeed * dt;
            }
            else
            {
                // На месте — разворот вокруг центра
                deltaAngle = steer * turnSpeedInPlace * dt;
            }
        }

        // Применяем поворот и смещение вперёд по корпусу
        rb.MoveRotation(rb.rotation + deltaAngle);

        // Двигаем вдоль локальной "вперёд" (ось up у спрайта)
        Vector2 forward = rb.transform.up; // в 2D вперёд = up
        Vector2 step = forward * currentSpeed * dt;
        rb.MovePosition(rb.position + step);
    }

    // Сервис
    public float CurrentSpeed => currentSpeed;
    public void SetMaxForwardSpeed(float v) => maxForwardSpeed = Mathf.Max(0f, v);
    public void SetMaxReverseSpeed(float v) => maxReverseSpeed = Mathf.Max(0f, v);
}
