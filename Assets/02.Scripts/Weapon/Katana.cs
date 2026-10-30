using System;
using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private float _damage = 25f;

    private Player _player;

    private void Awake()
    {
        _player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("부닥");
        if (other.TryGetComponent<IDamageAble>(out IDamageAble damageAble))
        {
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.Value = _damage;
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            damageInfo.HitPoint = hitPoint;
            damageInfo.HitNormal = (_player.transform.position - hitPoint).normalized;

            damageAble.TakeDamage(damageInfo);
        }
    }
}