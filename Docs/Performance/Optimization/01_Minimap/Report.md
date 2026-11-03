# 최적화 3-1: 미니맵 카메라 경량화 (2026-11-03)

2단계 병목 분석(`../../Profiler/Ablation/Report.md`)에서 미니맵 카메라가 CPU 비용 1순위로 나왔다. 미니맵을 끄면 CPU Main이 약 25% 줄었다. 미니맵을 없애지 않고 불필요한 작업만 덜어 냈다.

## 변경 내용
| 항목 | 전 | 후 | 이유 |
|---|---|---|---|
| 미니맵 그림자 렌더링 | 켬 | **끔** | 위에서 내려다보는 512×512 미니맵에는 그림자가 거의 보이지 않는데, 그림자 맵(4 cascade)을 한 번 더 그리고 있었다. |
| 미니맵 렌더러 | `PC_Renderer`(SSAO 포함) | **`Minimap_Renderer`(피처 없음)** | SSAO가 노멀을 얻기 위해 장면을 한 번 더 그리는 사전 패스가 미니맵에도 돌고 있었다. |
| 미니맵 Depth/Opaque 텍스처 | 파이프라인 설정 따름(켬) | **끔** | 미니맵은 깊이·색 복사본이 필요 없다. |

- `Assets/Settings/Minimap_Renderer.asset`
  - `PC_Renderer`를 복사하고 렌더러 피처를 모두 제거한 에셋이다.
  - `PC_RPAsset`, `Mobile_RPAsset` 두 파이프라인의 렌더러 목록 1번에 추가했다. 품질 단계를 바꿔도 같은 번호가 유효하다.
- `GameScene`의 `MinimapCamera` 설정: Renderer 1번, Render Shadows 끔, Depth/Opaque Texture 끔

## 측정 방법
- 1·2단계와 같은 고정 시점 A~D에서 측정했다.
- 시간을 멈추고(timeScale 0) 30프레임을 버린 뒤, 300프레임 동안 Profiler를 녹화하며 카운터 중앙값을 기록했다. 기록은 `before_*.json`, `after_*.json`이다.
- 시점마다 다음 화면을 함께 캡처했다.
  - Game 뷰 Stats
  - Profiler 창의 CPU Usage, Rendering 모듈. 메인 스레드 시간이 중앙값인 프레임을 골랐다.
- 미니맵 화면은 렌더 텍스처를 그대로 저장했다(`minimap_*.png`).

## 결과 (중앙값, 전 → 후)

| 시점 | CPU Main | CPU Render | Batches | SetPass | Draw Calls | Triangles | Shadow Casters |
|---|---:|---:|---:|---:|---:|---:|---:|
| A | 2.69 → **2.39** (-11%) | 1.99 → 1.80 | 237 → 218 | 71 → **59** | 783 → **703** | 2.43M → 2.40M | 724 → **621** |
| B | 2.82 → **2.34** (-17%) | 2.13 → 1.69 | 247 → 228 | 78 → **66** | 818 → **738** | 227k → 204k | 632 → **529** |
| C | 2.92 → **2.36** (-19%) | 2.04 → 1.59 | 382 → 363 | 78 → **66** | 1,013 → **933** | 2.31M → 2.29M | 642 → **539** |
| D | 2.98 → **2.65** (-11%) | 1.99 → 1.64 | 265 → 246 | 71 → **59** | 814 → **734** | 1.22M → 1.20M | 221 → **118** |

(CPU 단위 ms)

- **모든 시점에서 같은 폭으로 줄었다.**
  - SetPass -12, Draw Calls -80, Batches -19, Shadow Casters -103
  - 미니맵 하나가 그리던 그림자 패스와 SSAO 사전 패스가 사라진 만큼이다.
- **CPU Main은 0.3~0.56 ms(11~19%), Render Thread는 0.19~0.45 ms 줄었다.**
  - 2단계에서 정한 측정 오차(약 ±0.2 ms)보다 크다.
  - 미니맵을 아예 끈 경우(-0.5~0.8 ms)의 약 60~70%를 미니맵을 유지한 채 얻었다.
- GPU Frame Time은 6.6~7.5 ms에서 6.2~7.1 ms로 조금 줄었다. 다만 2단계에서 확인했듯이 에디터의 GPU 값은 장면 비용을 잘 반영하지 못하므로 참고만 한다.
- **화질 변화**: `minimap_compare.png`에서 차이가 거의 보이지 않는다. 미니맵이 그늘진 바닥을 위에서 비추고 있어서, 그림자나 SSAO가 원래 거의 드러나지 않았다.

## 파일
- 숫자: `before_<A~D>.json`, `after_<A~D>.json`
  - Profiler 원본 `.data`는 `.gitignore` 대상이라 로컬에만 있다.
- Stats: `stats_before_*`, `stats_after_*`, `stats_compare_<A~D>.png`
- Profiler: `profiler_before_*`, `profiler_after_*`, `profiler_compare_<A~D>_cpu.png`, `profiler_compare_<A~D>_rendering.png`
- 미니맵 화면: `minimap_before.png`, `minimap_after.png`, `minimap_compare.png`

## 남은 후보
- 미니맵 Culling Mask 축소: 지금은 Default 레이어 전체(렌더러 450개)를 그린다. 레이어를 나누는 작업이 필요하다.
- 미니맵 갱신 주기 낮추기: 스크립트 변경이 필요하다.
- 다음 작업: SSAO 설정 조정(메인 카메라)
