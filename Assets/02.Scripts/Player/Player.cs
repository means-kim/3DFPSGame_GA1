using UnityEngine;

public class Player : MonoBehaviour, IDamageAble
{
    [SerializeField] private float _maxHealth = 200f;
    private float _health;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    private void Awake()
    {
        _health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _health -= damageInfo.Value;
    }
}