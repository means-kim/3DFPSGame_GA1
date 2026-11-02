# 격납고 출입문 작업등 적용 리포트 (2026-11-02)

창문 발광과 짝을 이루도록 격납고 출입문 위에 따뜻한 Spot Light를 달아 문 앞에 빛 웅덩이를 만들었다.

## 문 위치 찾기
- 격납고는 아틀라스 텍스처 하나를 쓰므로, 아틀라스에서 문 그림이 있는 UV 영역을 지정했다.
  - Hangar_v4: 큰 문, 작은 문
  - Hangar_v2: 셔터, 차고 문
  - Hangar_v3: 문 3종
  - Hangar_v1: 문 1개
- 각 격납고 메시에서 그 UV를 쓰는 벽면 삼각형을 모아 문마다 윗변 중앙, 바깥 방향, 폭을 구했다. 총 37곳이다.
- 바깥 방향 검증: 문 앞 1.5m 지점이 비어 있고, 거기서 문 쪽으로 쏜 레이가 해당 격납고에 맞아야 한다.
- 사용 조건: 문 앞 8m 중 6m 이상에 바닥이 있고, 3m 안에 벽이 없을 것. 이 조건으로 **17곳**을 남겼다.
  - 나머지 20곳은 맵 바깥이나 담장을 향하고 있다.

## 변경 내역 (`GameScene.unity`)
- `DoorWorkLights` 아래 Spot Light 17개(`DoorLight_00~16`)
  - 위치: 문 윗변에서 바깥으로 0.5m, 위로 0.4m
  - 방향: 문에서 1.6m 앞, 4.5m 아래 지점을 향해 문 면과 문 앞 바닥을 함께 비춘다.
  - 3000K, Intensity 100, Range 10, Spot Angle 110°(Inner 50°), Realtime, **그림자 없음**
  - 세기 15에서는 거의 보이지 않았다. 60과 150을 비교해 100으로 정했다.

## 화면 비교
- `compare_L_door_v4.png`, `compare_M_door_v2.png`: 문 위 등과 문 면, 문 앞 바닥의 빛 웅덩이
- `compare_D.png`: 전경에서 일부 문 앞이 주황 점으로 보인다.
- `compare_A~C.png`: 조명 범위 밖이라 차이가 거의 없다.

## Stats 비교
한 번의 Play 모드에서 `DoorWorkLights`를 런타임에만 껐다 켜며 같은 시점을 캡처했다(timeScale 0, 고정 시점).

| 시점 | Batches | SetPass | Shadow casters | CPU main |
|---|---:|---:|---:|---:|
| A | 252 → 253 | 71 → 71 | 724 → 724 | 9.0 → 9.5 ms |
| B | 263 → 262 | 78 → 78 | 632 → 632 | 10.8 → 10.3 ms |
| C | 393 → 393 | 78 → 78 | 642 → 642 | 9.8 → 9.9 ms |
| D | 274 → 275 | 71 → 71 | 221 → 221 | 10.8 → 9.8 ms |
| L (문 앞) | 299 → 297 | 76 → 76 | 676 → 676 | 9.6 → 10.1 ms |

## 해석
- Spot Light 17개를 추가해도 **Batches, SetPass, Shadow casters가 변하지 않는다.**
  - Forward+ 렌더러는 조명 수만큼 Draw Call을 늘리지 않는다.
  - 그림자를 꺼 두어서 그림자 패스도 늘지 않는다.
- CPU main 차이(±1ms)는 방향이 일정하지 않아 측정 오차로 본다.
- 실제 비용은 조명이 비추는 픽셀의 셰이딩(GPU)에 있다. Stats 창으로는 보이지 않으므로, 최적화 단계에서 GPU 프로파일러로 확인이 필요하다.

## 파일
- 화면: `before_*.png` / `after_*.png`, `compare_*.png` (A~D, L_door_v4, M_door_v2)
- Stats: `stats_before_*.png` / `stats_after_*.png`, `stats_compare_*.png` (A~D, L)
