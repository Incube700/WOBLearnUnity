using System;
using UnityEngine;

/// <summary>
/// Минимальный менеджер волн с событием завершения волны.
/// Реализован как заглушка, чтобы удовлетворить ссылки в ArmorPointsManager.
/// </summary>
public class WaveManager : MonoBehaviour
{
    /// <summary>
    /// Вызывается при завершении волны. Параметр — номер завершённой волны (1..N).
    /// </summary>
    public event Action<int> OnWaveCompleted;

    /// <summary>
    /// Вызвать завершение текущей волны (временный метод-заглушка).
    /// </summary>
    public void CompleteWave(int waveNumber)
    {
        OnWaveCompleted?.Invoke(waveNumber);
    }
}

