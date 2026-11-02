# 맵 가장자리 지면색(스카이박스 지평선) 정리 리포트 (2026-11-02)

맵 밖을 볼 때 스카이박스 지면이 검붉게 넓게 보이던 문제를 정리했다.

## 원인
1. 스카이박스(`Skybox/Procedural`) Ground 색이 어두웠다(0.35, 0.22, 0.18). 태양 고도가 낮아 실제 화면에서는 거의 검게 보였다.
2. Atmosphere Thickness 2.5에서는 지평선 바로 위에 진한 검붉은 띠가 생긴다. Ground 색만 바꿔서는 이 띠가 남는다.
- Fog는 스카이박스에 적용되지 않으므로 Fog 값으로는 해결되지 않는다.

## 변경 내역 (`Assets/09.Materials/Skybox_Sunset.mat`)
- Atmosphere Thickness: 2.5 → **2.0** (사용자 선택). 검붉은 띠가 사라진다. 대신 하늘 전체가 덜 붉고 더 노래진다.
- Ground Color: (0.35, 0.22, 0.18) → **(0.45, 0.33, 0.28)**
  - 밝은 주황(1.2, 0.78, 0.58)은 용암처럼 보여서 제외했다.
  - 흙빛 후보 3개 중 맵 바닥 톤과 가장 자연스럽게 이어지는 값을 골랐다.
- 후보 비교: `candidates_atmosphere_thickness.png`, `candidates_ground_color.png`

## 화면 비교
- `compare_J_edge_low.png`, `compare_K_edge_high.png`: 맵 밖을 바로 본 시점. 검은 바닥과 검붉은 띠가 흙빛 지면과 자연스러운 지평선으로 바뀌었다.
- `compare_B.png`, `compare_C.png`: 맵 안에서 가장자리가 보이는 시점. 검붉은 영역이 갈색 지면으로 이어진다.
- `compare_A.png`, `compare_D.png`: 하늘 톤 변화(더 노란 하늘)를 확인할 수 있다.

## Stats 비교
스카이박스 머티리얼 값만 바꿨으므로 렌더링 비용은 변하지 않는 것이 정상이다.

| 시점 | Batches | SetPass | Shadow casters | CPU main |
|---|---:|---:|---:|---:|
| A | 253 → 249 | 71 → 71 | 724 → 724 | 9.5 → 9.2 ms |

- 시점 A에서 SetPass와 Shadow casters가 같다. Batches 차이(−4)는 한 프레임 스냅샷 오차 수준이다.
- **B~D와 J의 Stats는 아직 없다.** 캡처 도중 다른 창이 Unity 위로 올라와 잘못 찍혀서 삭제했다. 재캡처가 필요하다.

## 파일
- 화면: `before_*.png` / `after_*.png`, 나란히 붙인 `compare_*.png` (A~D, J_edge_low, K_edge_high)
- Stats: `stats_before_A.png` / `stats_after_A.png`, `stats_compare_A.png`
