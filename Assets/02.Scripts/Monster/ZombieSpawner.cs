using UnityEngine;
using System.Collections;
using Lean.Pool;

public class ZombieSpawner : MonoBehaviour
{
    [Header("스포너 설정")]
    [SerializeField] private GameObject _zombiePrefab;

    [SerializeField] private float _spawnInterval = 3f; // 스폰 주기
    [SerializeField] private int _maxZombieCount = 10; // 최대 동시 존재할 좀비 수
    [SerializeField] private float _spawnRadius = 5f; // 스포너 반경

    private int _aliveCount = 0;
    private WaitForSeconds _wait;

    private void Start()
    {
        _wait = new WaitForSeconds(_spawnInterval);
        StartCoroutine(Spawn_Coroutine());
    }

    // 스포너 코루틴
    private IEnumerator Spawn_Coroutine()
    {
        // 게임이 시작되기 전까지 대기
        while (GameManager.Instance.State != GameState.Playing)
        {
            yield return null;
        }

        while (GameManager.Instance.State == GameState.Playing)
        {
            yield return _wait;

            // 플레이 중 게임 오버가 된다면 종료
            if (GameManager.Instance.State != GameState.Playing)
            {
                yield break;
            }

            TrySpawn();
        }
    }

    private void TrySpawn()
    {
        // 좀비가 최대 수 라면 종료
        if (_aliveCount >= _maxZombieCount) return;

        Vector3 position = GetRandomPosition();

        GameObject zombieObject = LeanPool.Spawn(_zombiePrefab, position, Quaternion.identity);

        Zombie zombie = zombieObject.GetComponent<Zombie>();

        zombie.Setup(position);

        zombie.OnDied += HandleZombieDied;
        _aliveCount++;
    }

    // 스포너 반경의 랜덤 위치
    private Vector3 GetRandomPosition()
    {
        Vector2 offset = Random.insideUnitCircle * _spawnRadius;
        return transform.position + new Vector3(offset.x, 0f, offset.y);
    }

    // 좀비가 죽으면 구독 해제하고 카운터를 줄인다.
    private void HandleZombieDied(Zombie zombie)
    {
        zombie.OnDied -= HandleZombieDied;
        _aliveCount--;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _spawnRadius);
    }
}