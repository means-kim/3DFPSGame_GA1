using UnityEngine;

public enum MeleeHitMode
{
    Collider,
    Fan,
}

public class PlayerMeleeAttack : MonoBehaviour
{
    private Player _player;
    private Animator _animator;
    private int _meleeLayerIndex;
    [SerializeField] private Collider _katanaCollider;
    [SerializeField] private MeleeHitMode _meleeHitMode;

    [Header("부채꼴")]
    [SerializeField] private float _range = 2f;

    [SerializeField] private float _angle = 90f;
    [SerializeField] private LayerMask _targetMask;

    private void Awake()
    {
        _player = GetComponentInParent<Player>();
        _animator = GetComponentInChildren<Animator>();
        _meleeLayerIndex = _animator.GetLayerIndex("Melee Layer");

        _katanaCollider.enabled = false;
    }

    public void FanAttack()
    {
        if (_meleeHitMode != MeleeHitMode.Fan) return;

        Vector3 origin = _player.transform.position;

        // Physics.OverlapSphere(원점, 반지름, 찾을 레이어들)
        Collider[] colliders = Physics.OverlapSphere(origin, _range, _targetMask);

        foreach (Collider col in colliders)
        {
            // 적의 타격점을 찾는다.
            Vector3 hitPoint = col.ClosestPointOnBounds(origin);

            // 방향을 판단한다.
            Vector3 direction = hitPoint - origin;
            direction.y = 0f;
            float angle = Vector3.Angle(_player.transform.forward, direction);
            if (angle > _angle / 2f) continue;

            // 적과 나 사이에 벽이 있다면 떄리지 않는다.
            if (Physics.Linecast(origin, hitPoint, LayerMask.NameToLayer("Wall"))) continue;


            if (col.TryGetComponent(out IDamageAble damage))
            {
                DamageInfo damageInfo = new DamageInfo();
                damageInfo.Value = 25f;
                damageInfo.HitPoint = hitPoint;
                damageInfo.HitNormal = (origin - hitPoint).normalized;
                damage.TakeDamage(damageInfo);
            }
        }
    }

    // 람다식을 활용한 함수
    public void ActivateCollier()
    {
        if (_meleeHitMode != MeleeHitMode.Collider) return;

        _katanaCollider.enabled = true;
        Debug.Log("카타나 콜라이더 활성화");
    }

    public void DeactivateCollier()
    {
        _katanaCollider.enabled = false;
        Debug.Log("카타나 콜라이더 비활성화");
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        if (_player.WeaponMode != WeaponMode.Melee) return;

        if (Input.GetButtonDown("Fire2"))
        {
            _animator.SetTrigger("Slash");
        }
    }
}