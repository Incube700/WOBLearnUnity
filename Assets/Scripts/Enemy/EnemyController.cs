// Assets/Scripts/Enemy/EnemyController.cs
using UnityEngine;                                        // базовые типы Unity

/// <summary>
/// Враг, который держит дистанцию и стреляет по кулдауну.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))] // гарантируем физику
public class EnemyController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 2f;         // скорость перемещения (м/с)
    [SerializeField] private float minDistance = 4f;       // минимальная дистанция — ближе отходим
    [SerializeField] private float desiredDistance = 7f;   // стремимся держать эту дистанцию
    [SerializeField] private float maxDistance = 11f;      // дальше приближаемся

    [Header("Стрельба (новая система)")]
    [SerializeField] private GameObject bullet2DPrefab;   // префаб пули с Bullet2D
    [SerializeField] private Transform muzzle;            // точка вылета
    [SerializeField] private float fireCooldown = 1f;     // задержка между выстрелами (с)
    [SerializeField] private float fireDistance = 8f;     // дистанция, с которой открываем огонь (м)

    [Header("Линия видимости и укрытия")]
    [SerializeField] private LayerMask obstacleMask = ~0; // слои препятствий для Linecast
    [SerializeField] private float coverSearchRadius = 12f; // радиус поиска укрытия
    [SerializeField] private float coverRepathInterval = 0.5f; // как часто пересчитывать укрытие
    [SerializeField] private float coverTriggerDistance = 9f;  // стремимся к укрытию, если близко к игроку

    [Header("Башня (визуальный поворот)")]
    [SerializeField] private Transform turretPivot;         // узел башни (родитель дула)
    [SerializeField] private bool limitTurretByBody = true; // ограничивать поворот относительно корпуса
    [SerializeField] private float turretMaxAimAngle = 60f; // предел отклонения (°)
    [SerializeField] private float turretSpriteUpOffset = -90f; // если спрайт башни «смотрит» вверх

    [Header("Выглядывание из укрытия")]
    [SerializeField] private bool enablePeek = true;      // включить механику выглядывания
    [SerializeField] private float peekOffset = 1.1f;     // насколько выезжаем из укрытия (м)
    [SerializeField] private float peekDuration = 0.7f;   // сколько держим выглядывание (с)
    [SerializeField] private float peekCooldown = 1.3f;   // пауза между выглядываниями (с)

    private Rigidbody2D rb;                                 // кэш Rigidbody2D
    private Transform target;                               // текущая цель (игрок)
    private float cooldownTimer;                            // таймер перезарядки
    private CoverPoint2D currentCover;                      // выбранная точка укрытия
    private float nextCoverRepathTime;                      // тайминг пересчёта укрытий
    private bool isPeeking;                                 // сейчас выезжаем из укрытия
    private bool isRetreating;                              // возвращаемся в укрытие
    private Vector2 peekTargetPos;                          // цель выглядывания
    private float peekEndTime;                              // когда закончить выглядывание
    private float nextPeekAllowedTime;                      // когда можно снова выглянуть

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();                 // получаем Rigidbody2D
        rb.gravityScale = 0f;                             // отключаем гравитацию
        rb.freezeRotation = false;                        // разрешаем вращение корпуса

        // Пытаемся автоматически определить pivot башни, если не задан
        EnsureTurretPivot();

        // Автозагрузка префаба пули, если не назначен (через Resources/Prefabs/Bullet)
        if (bullet2DPrefab == null)
        {
            bullet2DPrefab = Resources.Load<GameObject>("Prefabs/Bullet");
            if (bullet2DPrefab == null)
                bullet2DPrefab = Resources.Load<GameObject>("Bullet");
        }
    }

    private void Update()
    {
        AcquireTarget();                                   // ищем цель
        AimAndFire();                                      // поворачиваемся и стреляем
        cooldownTimer -= Time.deltaTime;                  // уменьшаем таймер перезарядки
    }

    private void FixedUpdate()
    {
        MoveTowardsTarget();                              // двигаемся к цели
    }

    private void AcquireTarget()
    {
        if (target != null) return;                        // если цель уже есть — не ищем
        var player = FindObjectOfType<PlayerController>(); // ищем игрока по компоненту
        if (player != null) target = player.transform;     // сохраняем найденную цель
    }

    private void AimAndFire()
    {
        if (target == null || muzzle == null || bullet2DPrefab == null)
            return;                                       // без цели или ссылок стрельбы нет

        // Поворачиваем башню (и дуло как её ребёнка) к цели
        RotateTurretTowards(target.position);

        float dist = Vector2.Distance(transform.position, target.position); // расстояние до цели
        bool hasLOS = HasLineOfSight(muzzle.position, target.position, target);
        if (hasLOS && dist <= fireDistance && cooldownTimer <= 0f)   // цель в зоне огня и готовность есть
        {
            Shoot2D();                                    // создаём пулю новой системы
            cooldownTimer = fireCooldown;                 // сбрасываем таймер

            // Если стреляли из выглядывания — начинаем откат к укрытию
            if (isPeeking && currentCover != null)
            {
                isPeeking = false;
                isRetreating = true;
            }
        }
    }

    private void MoveTowardsTarget()
    {
        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;             // стоим на месте
            return;
        }

        Vector2 selfPos = transform.position;
        Vector2 targetPos = target.position;
        float dist = Vector2.Distance(selfPos, targetPos);

        // Решение по укрытию: ищем, если близко и видим игрока
        bool wantCover = dist <= coverTriggerDistance && HasLineOfSight(selfPos, targetPos, target);
        if (wantCover && Time.time >= nextCoverRepathTime)
        {
            currentCover = FindBestCover(selfPos, targetPos);
            nextCoverRepathTime = Time.time + coverRepathInterval;
        }

        Vector2 moveDir;
        if (currentCover != null)
        {
            Vector2 coverPos = currentCover.transform.position;
            float coverReach = currentCover.reachRadius;
            float dToCover = Vector2.Distance(selfPos, coverPos);
            if (dToCover > coverReach && !isPeeking)
            {
                moveDir = (coverPos - selfPos).normalized; // идём к укрытию
            }
            else
            {
                // Укрытие достигнуто
                if (!isPeeking && !isRetreating && enablePeek)
                {
                    bool losFromCover = HasLineOfSight(coverPos, targetPos, target);
                    bool readyToPeek = Time.time >= nextPeekAllowedTime && cooldownTimer <= 0f && Vector2.Distance(coverPos, targetPos) <= fireDistance + 1.5f;
                    if (!losFromCover && readyToPeek)
                    {
                        // Начинаем выглядывание: целимся на чуть выдвинутую точку к игроку
                        Vector2 outDir = (targetPos - coverPos).normalized;
                        peekTargetPos = coverPos + outDir * peekOffset;
                        isPeeking = true;
                        isRetreating = false;
                        peekEndTime = Time.time + peekDuration;
                        nextPeekAllowedTime = Time.time + peekCooldown; // базовая перезарядка выглядыв.
                    }
                }

                if (isPeeking)
                {
                    // Едем к точке выглядывания; если время вышло — откат
                    moveDir = (peekTargetPos - selfPos).normalized;
                    if (Time.time >= peekEndTime || (peekTargetPos - selfPos).sqrMagnitude < 0.04f)
                    {
                        // Если не успели выстрелить, всё равно уходим назад
                        isPeeking = false;
                        isRetreating = true;
                    }
                }
                else if (isRetreating)
                {
                    // Возврат в укрытие
                    if (dToCover > coverReach * 0.5f)
                        moveDir = (coverPos - selfPos).normalized;
                    else
                    {
                        isRetreating = false;
                        moveDir = Vector2.zero;
                    }
                }
                else
                {
                    // Стоим за укрытием
                    moveDir = Vector2.zero;
                }
            }
        }
        else
        {
            // Держим дистанцию вокруг желаемого радиуса
            if (dist < minDistance)
                moveDir = (selfPos - targetPos).normalized; // слишком близко — отходим
            else if (dist > maxDistance)
                moveDir = (targetPos - selfPos).normalized; // слишком далеко — приближаемся
            else
                moveDir = Vector2.zero;                      // в коридоре — стоим
        }

        if (moveDir.sqrMagnitude > 1e-4f)
        {
            rb.linearVelocity = moveDir * moveSpeed;
            float ang = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f; // угол корпуса
            rb.rotation = ang;                                // поворачиваем корпус по движению
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Shoot2D()
    {
        // Bullet2D берёт направление из transform.up. Создаём с ориентацией дула и небольшим выносом вперёд
        Vector3 spawnPos = muzzle.position + muzzle.up * 0.2f;
        var go = Instantiate(bullet2DPrefab, spawnPos, muzzle.rotation);
        var b = go.GetComponent<Bullet2D>();
        if (b != null) b.SetOwner(transform);
    }

    // ------ Утилиты ИИ ------
    private void EnsureTurretPivot()
    {
        if (turretPivot != null) return;
        // Сначала пробуем взять родителя дула
        if (muzzle != null && muzzle.parent != null)
        {
            turretPivot = muzzle.parent;
            return;
        }
        // Ищем по именам
        var found = transform.Find("TurretPivot");
        if (found == null) found = transform.Find("Turret");
        if (found == null)
        {
            foreach (var t in GetComponentsInChildren<Transform>(true))
            {
                if (t == transform) continue;
                var n = t.name.ToLowerInvariant();
                if (n.Contains("turret") || n.Contains("pivot")) { found = t; break; }
            }
        }
        if (found == null && transform.childCount > 0) found = transform.GetChild(0);
        turretPivot = found != null ? found : transform;
    }

    private void RotateTurretTowards(Vector2 worldTargetPos)
    {
        EnsureTurretPivot();
        Transform pivot = turretPivot != null ? turretPivot : (muzzle != null ? muzzle : transform);
        Vector2 dir = ((Vector2)worldTargetPos - (Vector2)pivot.position).normalized;
        if (dir.sqrMagnitude < 1e-6f) return;
        float desired = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + turretSpriteUpOffset;
        float final = desired;
        if (limitTurretByBody)
        {
            float bodyAngle = transform.eulerAngles.z;
            float relative = Mathf.DeltaAngle(bodyAngle, desired);
            float clamped = Mathf.Clamp(relative, -turretMaxAimAngle, turretMaxAimAngle);
            final = bodyAngle + clamped;
        }
        pivot.rotation = Quaternion.Euler(0f, 0f, final);
        // Синхронизируем дуло с башней
        if (muzzle != null)
        {
            if (muzzle.parent == pivot)
                muzzle.localRotation = Quaternion.identity; // выравниваем относительно башни
            else
                muzzle.rotation = pivot.rotation;
        }
    }

    private bool HasLineOfSight(Vector2 from, Vector2 to, Transform expectedTarget)
    {
        int mask = obstacleMask.value == 0 ? ~0 : obstacleMask.value; // если не задан — проверяем по всем слоям
        var hits = Physics2D.LinecastAll(from, to, mask);
        if (hits == null || hits.Length == 0) return true;

        float bestDist = float.PositiveInfinity;
        RaycastHit2D? best = null;
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            // Игнорируем собственные коллайдеры врага
            if (h.collider.transform == transform || h.collider.transform.IsChildOf(transform)) continue;
            if (h.distance < bestDist)
            {
                bestDist = h.distance;
                best = h;
            }
        }
        if (best == null) return true;
        var t = best.Value.collider.transform;
        if (expectedTarget != null && (t == expectedTarget || t.IsChildOf(expectedTarget))) return true;
        return false; // перекрыто препятствием
    }

    private CoverPoint2D FindBestCover(Vector2 selfPos, Vector2 targetPos)
    {
        var all = FindObjectsOfType<CoverPoint2D>();
        CoverPoint2D best = null;
        float bestScore = float.PositiveInfinity;
        foreach (var cp in all)
        {
            float dSelf = Vector2.Distance(selfPos, cp.transform.position);
            if (dSelf > coverSearchRadius) continue;           // далеко

            // Укрытие годится, если из него НЕ видно игрока
            if (HasLineOfSight(cp.transform.position, targetPos, target))
                continue;

            // Простая эвристика: ближе к нам — лучше
            float score = dSelf;
            if (score < bestScore)
            {
                bestScore = score;
                best = cp;
            }
        }
        return best;
    }
}
