using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Централизованный обработчик ввода.
/// </summary>
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; } // глобальный доступ

    [SerializeField] private GameInputActions actions; // набор действий

    private InputAction moveAction;   // действие «двигаться»
    private InputAction aimAction;    // действие «наводить»
    private InputAction fireAction;   // действие «стрелять»

    private Vector2 move;             // текущий вектор движения
    private Vector2 aim;              // позиция курсора
    private bool firePressed;         // флаг выстрела в этом кадре

    private void Awake()
    {
        if (Instance != null)         // проверяем, что синглтон один
        {
            Destroy(gameObject);      // уничтожаем дубликат
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // не уничтожаем при смене сцены

        actions = new GameInputActions();        // создаём экземпляр набора
        moveAction = actions.Gameplay.Move;      // получаем действие Move
        aimAction = actions.Gameplay.Aim;        // получаем действие Aim
        fireAction = actions.Gameplay.Fire;      // получаем действие Fire
    }

    private void OnEnable()
    {
        moveAction.Enable();                     // начинаем читать движение
        aimAction.Enable();                      // начинаем читать наведение
        fireAction.Enable();                     // начинаем читать выстрел
    }

    private void OnDisable()
    {
        moveAction.Disable();                    // выключаем движение
        aimAction.Disable();                     // выключаем наведение
        fireAction.Disable();                    // выключаем выстрел
    }

    private void Update()
    {
        move = moveAction.ReadValue<Vector2>();  // читаем Vector2 с клавиатуры
        aim = aimAction.ReadValue<Vector2>();    // получаем позицию мыши
        firePressed = fireAction.WasPressedThisFrame(); // фиксируем нажатие
    }

    // Публичные геттеры для других скриптов
    public Vector2 Move => move;
    public Vector2 Aim => aim;
    public bool Fire => firePressed;
}
