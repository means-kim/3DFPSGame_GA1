# 폭발 순간 조명 적용 리포트 (2026-11-02)

폭탄·드럼통 폭발 때 주변 벽과 바닥이 순간적으로 밝아지도록 폭발 이펙트에 짧게 번쩍이는 포인트 조명을 붙였다.

## 변경 내역
- `Assets/02.Scripts/Effect/ExplosionLight.cs` (신규)
  - 켜질 때 세기 300으로 시작해 0.5초 동안 `AnimationCurve`(EaseInOut)를 따라 0까지 줄이고, 끝나면 `Light.enabled = false`로 비용을 없앤다.
  - Lean Pool로 재사용되므로 `OnEnable`에서 매번 처음 밝기로 초기화한다.
- `Assets/03.Prefabs/Effect/Explosion_Lit.prefab` (신규)
  - 외부 에셋 `Workshop/Effects/Explosion.prefab`의 **프리팹 변형**이다. 원본은 수정하지 않았다.
  - 자식 `ExplosionLight`: Point Light, 2200K, Range 12, Realtime, 그림자 없음, 위로 0.8m
- `Bomb.prefab`, `Drum.prefab`: `_explosionEffectPrefab`을 `Explosion_Lit`으로 교체했다. 씬의 드럼통 10개는 이 값을 덮어쓰지 않아 그대로 적용된다.

## 화면 비교
폭발 시작 0.08초 시점으로 파티클을 진행시킨 정지 화면이다.
- `compare_N_drum_east.png`: 드럼통, 바닥, 철망, 옆 건물 벽이 주황빛으로 밝아진다.
- `compare_O_drum_south.png`: 양쪽 담장, 바닥, 옆 드럼통까지 빛이 닿는다.
- `compare_A~D.png`: 폭발이 없는 평상시 화면으로 차이가 없다.

## Stats 비교 (시점 O, 폭발 순간)
같은 Play 세션에서 원본 폭발과 조명이 있는 폭발을 같은 위치에 꺼내 비교했다.

| 상태 | Batches | SetPass | Shadow casters | CPU main |
|---|---:|---:|---:|---:|
| 조명 없음 | 168 | 71 | 455 | 9.5 ms |
| 폭발 조명 | 168 | 71 | 455 | 9.8 ms |

- 그림자 없는 포인트 조명이라 Draw Call 관련 수치는 변하지 않는다.
- 조명은 0.5초 뒤 스스로 꺼지므로 평상시 비용은 0이다.
- 비용은 폭발 순간 반경 12m 안의 픽셀 셰이딩(GPU)뿐이다. 폭발이 여러 개 겹치는 경우는 최적화 단계에서 확인이 필요하다.

## 확인하지 못한 것
- **0.5초 동안 사그라드는 동작은 실제로 확인하지 못했다.** 측정용 Play 세션이 시간을 멈춘 상태(timeScale 0)였고, 시간을 다시 흐르게 한 직후 값을 읽었을 때는 아직 프레임이 진행되지 않아 300 그대로였다.
- 실제 폭탄을 던지거나 드럼통을 쏴서 확인하는 것은 직접 플레이로 해야 한다.

## 파일
- 화면: `before_*.png` / `after_*.png`, `compare_*.png` (A~D, N_drum_east, O_drum_south)
- Stats: `stats_before_O.png` / `stats_after_O.png`, `stats_compare_O.png`
