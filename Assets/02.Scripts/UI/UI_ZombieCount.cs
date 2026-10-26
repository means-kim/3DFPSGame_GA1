using UnityEngine;
using TMPro;

public class UI_ZombieCount : MonoBehaviour
{
    [SerializeField] private ZombieSpawner _zombieSpawner;
    [SerializeField] private TextMeshProUGUI _zombieCountText;

    private void OnEnable()
    {
        if (_zombieSpawner == null || _zombieCountText == null) return;

        _zombieSpawner.OnZombieCountChanged += UpdateCount;

        UpdateCount(_zombieSpawner.AliveCount, _zombieSpawner.MaxZombieCount);
    }

    private void OnDisable()
    {
        if (_zombieSpawner != null)
        {
            _zombieSpawner.OnZombieCountChanged -= UpdateCount;
        }
    }

    private void UpdateCount(int count, int maxCount)
    {
        _zombieCountText.text = $"좀비: {count} / {maxCount}";
    }
}