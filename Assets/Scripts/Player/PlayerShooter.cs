using UnityEngine;

/// <summary>
/// Отвечает за стрельбу игрока.
/// Предполагается, что точка вылета пули ориентирована вдоль локальной оси Y.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    private static int lastShotFrame = -1;                  // защита от мультиспавна в один кадр
    [SerializeField] private Bullet bulletPrefab;           // префаб пули
    [SerializeField] private Transform muzzle;              // точка вылета
    [SerializeField] private float fireCooldown = 0.25f;    // время между выстрелами (с)
    [SerializeField] private bool useMuzzleRight = false;   // стрелять вдоль локальной оси Y (вперёд по стволу)

    private float cooldownTimer;                            // таймер перезарядки
    
#if UNITY_EDITOR
    // В редакторе автоматически подставляем ссылки, если они пустые
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (bulletPrefab == null)
                EnsureBulletPrefab();

            if (muzzle == null)
            {
                var t = transform.Find("Muzzle");
                if (t != null) muzzle = t;
            }
        }
    }
#endif

    private bool EnsureBulletPrefab()
    {
        if (bulletPrefab != null) return true;
        // Повторная попытка автозагрузки
        Bullet loadedComp = Resources.Load<Bullet>("Prefabs/Bullet");
        if (loadedComp == null)
            loadedComp = Resources.Load<Bullet>("Bullet");
        if (loadedComp == null)
        {
            GameObject go = Resources.Load<GameObject>("Prefabs/Bullet");
            if (go == null) go = Resources.Load<GameObject>("Bullet");
            if (go != null) loadedComp = go.GetComponent<Bullet>();
        }
        if (loadedComp != null)
        {
            bulletPrefab = loadedComp;
            return true;
        }
        return false;
    }

    private void Awake()
    {
        // Автозагрузка префаба пули, если не назначен (через Resources)
        if (bulletPrefab == null)
            EnsureBulletPrefab();

        // Автопоиск точки вылета, если не назначена
        if (muzzle == null)
        {
            var direct = transform.Find("Muzzle");
            if (direct != null) muzzle = direct;
            else
            {
                foreach (var t in GetComponentsInChildren<Transform>(true))
                {
                    if (t == transform) continue;
                    string n = t.name.ToLowerInvariant();
                    if (n.Contains("muzzle") || n.Contains("barrel") || n.Contains("gun"))
                    {
                        muzzle = t;
                        break;
                    }
                }
            }
        }
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;                   // уменьшаем таймер
        bool wantShoot = false;

        // Предпочитаем централизованный ввод, но поддерживаем резерв
        if (GameInput.Instance != null)
        {
            wantShoot = GameInput.Instance.Fire || GameInput.Instance.FireHeld; // клик или удержание
        }
        else
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = UnityEngine.InputSystem.Mouse.current;
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (mouse != null && mouse.leftButton.isPressed) wantShoot = true;
            if (kb != null && kb.spaceKey.isPressed) wantShoot = true;
#else
            if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space)) wantShoot = true;
#endif
        }

        if (wantShoot && cooldownTimer <= 0f)              // есть запрос на выстрел
        {
            Shoot();                                       // выполняем выстрел
            cooldownTimer = fireCooldown;                  // сбрасываем таймер
        }
    }

    private void Shoot()
    {
        // Глобальная защита от дубликатов в этот же кадр (если компонент продублирован)
        if (Time.frameCount == lastShotFrame)
            return;
        if ((bulletPrefab == null && !EnsureBulletPrefab()) || muzzle == null) // проверяем ссылки и пытаемся подхватить
        {
            if (bulletPrefab == null && muzzle == null)
                Debug.LogError("PlayerShooter: отсутствуют bulletPrefab и muzzle — назначьте их в инспекторе", this);
            else if (bulletPrefab == null)
                Debug.LogError("PlayerShooter: отсутствует bulletPrefab — назначьте префаб пули в инспекторе", this);
            else
                Debug.LogError("PlayerShooter: отсутствует muzzle — назначьте точку вылета (Transform)", this);
            return;
        }

        Bullet bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation); // создаём пулю
        Vector2 dir = useMuzzleRight ? (Vector2)muzzle.right : (Vector2)muzzle.up; // ось выстрела
        var ownerCol = GetComponent<Collider2D>();         // коллайдер владельца для игнора
        if (ownerCol != null)
            bullet.Launch(dir, ownerCol);
        else
            bullet.Launch(dir);

        lastShotFrame = Time.frameCount;                   // фиксируем кадр выстрела
    }
}
