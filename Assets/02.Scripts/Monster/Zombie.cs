using UnityEngine;
using System.Collections;
using Lean.Pool;
using System;
using UnityEngine.AI;

// FSM(유한 상태 머신) : 유한개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// - 상태: 객체가 가질 수 있는 어떤 조건이나 상황
// - 전이: 조건에 따라 현재 상태에서 다른 상태로 넘어가는 것
// - 그리고 위 방식에 따라 규칙을 세워둔 것을 "머신"

// 좀비가 가질 수 있는 상태
public enum ZombieState
{
    Idle, // 대기 : 제자리에서 플레이어를 기다린다.
    Trace, // 추격: 플레이어를 쫓아간다.
    Comeback, // 복귀: 처음 자리로 돌아간다.
    Attack, // 공격: 일정 시간마다 플레이어를 때린다.
    Hit, // 피격: 공격을 받아 잠깐 멈칫한다.
    Death, // 죽음: 쓰러지고 잠시 뒤 사라진다.
    Patrol, // 순찰: 두 지점을 오가면서 순찰한다.
}

[RequireComponent(typeof(NavMeshAgent))]
public class Zombie : MonoBehaviour, IDamageAble
{
    [Header("스탯")]
    [SerializeField] private float _detectDistance = 8f; // 플레이어를 발견하는 거리

    [SerializeField] private float _attackRange = 1.5f; // 공격 범위
    [SerializeField] private float _attackInterval = 1.2f;
    [SerializeField] private float _comebackDistance = 15f; // 처음 자리에서 이만큼 멀어지면 돌아가는 거리
    [SerializeField] private float _moveSpeed = 5f; // 이동속도
    [SerializeField] private float _health;
    [SerializeField] private float _maxHealth = 30f;
    [SerializeField] private Vector3 _patrolOffset = new Vector3(-5f, 0f, 0f);
    [SerializeField] private float _attackDamage = 10f;

    [Header("이펙트")]
    [SerializeField] private GameObject _bloodSprayPrefab;

    private Vector3 _patrolPosition;
    private float _attackTimer = 0f;
    private Vector3 _startPosition;
    private GameObject _player;

    private ZombieState _state = ZombieState.Idle;
    private Coroutine _idleCoroutine;
    private NavMeshAgent _agent;

    public event Action OnDamaged = null;
    public event Action<Zombie> OnDied = null;

    public float Health => _health;
    public float MaxHealth => _maxHealth;

    private void Awake()
    {
        // 길을 찾아 걷게 해주는 컴포넌트 NavMeshAgent를 찾아서 참조
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _agent.speed = _moveSpeed;
    }

    private void OnEnable()
    {
        _health = _maxHealth;
        _state = ZombieState.Idle;

        _idleCoroutine = null;
        _attackTimer = 0f;

        _startPosition = transform.position;
        _patrolPosition = _startPosition + _patrolOffset;

        // 게임을 시작하면 플레이어를 찾는다.
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
        }

        // 체력바를 가득 채운 상태로 갱신한다.
        OnDamaged?.Invoke();
    }

    public void Setup(Vector3 position)
    {
        transform.position = position;

        _startPosition = position;
        _patrolPosition = _startPosition + _patrolOffset;
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
                break;

            case ZombieState.Trace:
                Trace();
                break;

            case ZombieState.Comeback:
                Comback();
                break;

            case ZombieState.Attack:
                Attack();
                break;

            case ZombieState.Patrol:
                Patrol();
                break;
        }
    }

    // 데미지를 받는다.
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_state == ZombieState.Death)
        {
            return;
        }

        _health -= damageInfo.Value;

        OnDamaged?.Invoke();

        StopAllCoroutines();
        _idleCoroutine = null;

        if (_health > 0f)
        {
            // 좀비의 자식으로 블러드 스프레이를 생성! -> 위치 수정
            GameObject bloodSpray = LeanPool.Spawn(_bloodSprayPrefab, transform);
            bloodSpray.transform.position = damageInfo.HitPoint;
            bloodSpray.transform.forward = damageInfo.HitNormal;

            // 2초 뒤 풀로 반환
            LeanPool.Despawn(bloodSpray, 2f);

            // 잠깐 멈칫 하고 싶다!
            Debug.Log($"{_state} -> Hit");
            _state = ZombieState.Hit;
            StartCoroutine(Hit_CoRoutine());
        }
        else
        {
            Debug.Log($"{_state} -> Death");
            _state = ZombieState.Death;

            OnDied?.Invoke(this); // 죽음이 확정되는 순간 스포너에게 알린다.

            StartCoroutine(Death_CoRoutine());
        }
    }

    private IEnumerator Hit_CoRoutine()
    {
        StopAgent();

        yield return new WaitForSeconds(0.3f); // 잠깐 멈칫 한다!

        // 맞았으니 쫓아온다.
        Debug.Log($"{_state} -> Trace");
        _state = ZombieState.Trace;
    }

    private IEnumerator Death_CoRoutine()
    {
        StopAgent();

        yield return new WaitForSeconds(0.5f);

        LeanPool.Despawn(gameObject);
    }

    private IEnumerator Idle_CoRoutine()
    {
        StopAgent();

        yield return new WaitForSeconds(1f);

        if (_state == ZombieState.Idle)
        {
            Debug.Log($"{_state} -> Patrol");
            _state = ZombieState.Patrol;
        }

        _idleCoroutine = null;
    }

    // 대기 : 제자리에서 플레이어를 기다린다.
    private void Idle()
    {
        // Todo. Idle 애니메이션 실행

        StopAgent();

        // 플레이어와 가까워 지면 Trace 상태로 전이한다.
        float distance = Vector3.Distance(transform.position, _player.transform.position);

        if (distance <= _detectDistance)
        {
            if (_idleCoroutine != null)
            {
                StopCoroutine(_idleCoroutine);
                _idleCoroutine = null;
            }

            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        if (Vector3.Distance(transform.position, _startPosition) < 1f)
        {
            if (_idleCoroutine == null)
            {
                _idleCoroutine = StartCoroutine(Idle_CoRoutine());
            }
        }
        else if (_idleCoroutine != null)
        {
            StopCoroutine(_idleCoroutine);
            _idleCoroutine = null;
        }
    }

    // 추격: 플레이어를 쫓아간다.
    private void Trace()
    {
        // Todo. Run 애니메이션 실행

        // 처음 자리에서 너무 멀어지면 돌아간다.
        if (Vector3.Distance(transform.position, _startPosition) > _comebackDistance)
        {
            Debug.Log($"{_state} -> Comeback");
            _state = ZombieState.Comeback;
            return;
        }

        // 플레이어가 공격 거리 안이면 공격한다.
        if (Vector3.Distance(transform.position, _player.transform.position) <= _attackRange)
        {
            _attackTimer = 0f;
            Debug.Log($"{_state} -> Attack");
            _state = ZombieState.Attack;
            return;
        }

        // Vector3 direction = _player.transform.position - transform.position;
        // direction.y = 0f;
        // direction.Normalize();

        // transform.forward = direction;
        // _controller.SimpleMove(direction * _moveSpeed);

        // setDestination은 목적지 설정
        // - 호출할 때마다 목적지를 향해 조금씩 이동 (X)
        // - 한 번 세팅하면 내부에서 계속해서 이동 (O)
        _agent.SetDestination(_player.transform.position);
    }

    // 복귀: 처음 자리로 돌아간다.
    private void Comback()
    {
        if (Vector3.Distance(transform.position, _startPosition) < _agent.stoppingDistance)
            // if (_agent.remainingDistance <= _agent.stoppingDistance)
            // if (_agent.pathStatus == NavMeshPathStatus.PathComplete)    
        {
            Debug.Log($"{_state} -> Idle");
            _state = ZombieState.Idle;
            return;
        }

        // Vector3 direction = _startPosition - transform.position;
        // direction.y = 0f;
        // direction.Normalize();
        //
        // transform.forward = direction;
        // _controller.SimpleMove(direction * _moveSpeed);

        _agent.SetDestination(_startPosition);
    }

    // 공격: 일정 시간마다 플레이어를 때린다.
    private void Attack()
    {
        StopAgent();

        // 플레이어가 공격 사정거리 밖으로 나가면 쫓아!
        float distance = Vector3.Distance(transform.position, _player.transform.position);

        if (distance > _attackRange)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
        }

        _attackTimer += Time.deltaTime;

        if (_attackTimer >= _attackInterval)
        {
            // Todo. 공격 애니메이션 실행

            DamageInfo damageInfo = new DamageInfo()
            {
                Value = _attackDamage,
                HitPoint = _player.transform.position,
                HitNormal = (transform.position - _player.transform.position).normalized,
            };

            _player.GetComponent<IDamageAble>()?.TakeDamage(damageInfo);

            Debug.Log("플레이어를 때렸습니다.");
            _attackTimer = 0f;
        }
    }

    private void Patrol()
    {
        if (Vector3.Distance(transform.position, _player.transform.position) <= _detectDistance)
        {
            Debug.Log($"{_state} -> Trace");
            _state = ZombieState.Trace;
            return;
        }

        Vector3 direction = _patrolPosition - transform.position;
        direction.y = 0f;

        if (direction.magnitude < 0.3f)
        {
            Debug.Log($"{_state} -> Comeback");
            _state = ZombieState.Comeback;
            return;
        }

        direction.Normalize();
        transform.forward = direction;
    }

    private void StopAgent()
    {
        _agent.ResetPath(); // 에이전트의 목적지 삭제
        _agent.velocity = Vector3.zero; // 미끄럼 방지를 위해 가속도 제로
    }
}