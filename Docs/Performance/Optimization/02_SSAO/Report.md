# 최적화 3-2: SSAO 사전 패스 제거 (2026-11-03)

2단계 병목 분석에서 SSAO를 끄면 Batches가 약 1/3 줄었다. 원인은 SSAO가 화면을 그리기 전에 장면 전체를 한 번 더 그리는 **사전 패스**였다. SSAO 효과는 유지하면서 이 사전 패스만 없애는 설정을 찾았다.

## 후보 비교 (`candidates/`)
시점 A, C에서 설정마다 300프레임 중앙값을 재고, 같은 시점의 화면을 저장했다.

| 설정 | A Batches | A Draw Calls | C Batches | C Draw Calls | 사전 패스 |
|---|---:|---:|---:|---:|---|
| 현재: Source DepthNormals | 214 | 703 | 359 | 933 | 있음(노멀) |
| Source Depth | 214 | 703 | 359 | 933 | **있음(깊이)** |
| Source Depth + Downsample | 214 | 703 | 359 | 933 | 있음(깊이) |
| DepthNormals + After Opaque | 214 | 703 | 359 | 933 | 있음(노멀) |
| **Source Depth + After Opaque** | **141** | **525** | **243** | **641** | **없음** |
| SSAO 끔(참고) | 137 | 521 | 239 | 637 | 없음 |

- **`Source`를 Depth로만 바꾸면 효과가 없다.**
  - SSAO가 불투명 물체보다 먼저 계산되는 동안(After Opaque 끔)에는 깊이 텍스처가 미리 있어야 한다.
  - 그래서 URP가 깊이 전용 사전 패스로 장면을 다시 그린다.
- **`After Opaque`를 켜면** SSAO를 불투명 물체를 그린 뒤에 계산한다.
  - 깊이는 이미 그린 화면에서 복사해 쓰므로 사전 패스가 사라진다.
  - 노멀은 깊이에서 재구성해야 해서 `Source: Depth`와 함께 써야 한다.
  - DepthNormals + After Opaque는 노멀 사전 패스가 남는다.
- 결과적으로 Draw Call은 SSAO를 끈 것과 거의 같아졌다(차이 4개).

### 화질 비교
- `candidates_compare.png`: 시점 A, C에서 4개 설정을 비교했다.
- `zoom_A.png`: 시점 A에서 SSAO 차이가 가장 큰 영역을 2배 확대했다. 왼쪽부터 현재, Depth + After Opaque, SSAO 끔이다.
- 1280×720 화면의 평균 밝기 차이(0~255)와, 4 이상 차이 나는 픽셀 비율은 아래와 같다.

| 비교 | A | C |
|---|---:|---:|
| 같은 설정 두 번(필름 그레인 노이즈) | 0.82 (1.3%) | 0.78 (1.3%) |
| 현재 vs SSAO 끔 = 현재 SSAO의 효과 | 2.48 (14.5%) | 1.08 (3.6%) |
| **Depth + After Opaque vs SSAO 끔** = 새 설정의 효과 | **2.88 (16.6%)** | 1.11 (3.8%) |
| 현재 vs Depth + After Opaque | 1.33 (6.2%) | 0.83 (1.8%) |

- 새 설정도 SSAO 효과의 크기가 현재와 비슷하다. A에서는 2.88로 2.48보다 오히려 조금 크다.
- 확대 이미지에서도 갑옷 틈과 접촉면의 어두운 음영이 현재와 같이 남는다.
- 차이점은 다음과 같다.
  - After Opaque 방식은 AO를 조명 계산 뒤에 곱하므로 직사광 부분도 약간 어두워진다.
  - Depth 방식은 노멀을 깊이에서 추정하므로 가장자리 모양이 조금 다르다.
  - 노을 장면의 어두운 톤에서는 눈에 띄지 않았다.
- Downsample은 SSAO 계산 해상도를 절반으로 낮춰 GPU만 아낀다. 에디터에서는 GPU 비용을 측정할 수 없어 효과를 확인할 수 없었고, 화질만 조금 떨어지므로 적용하지 않았다.

## 변경 내용
`Assets/Settings/PC_Renderer.asset`의 SSAO 렌더러 피처를 바꿨다.

| 항목 | 전 | 후 |
|---|---|---|
| Source | DepthNormals | **Depth** |
| After Opaque | 끔 | **켬** |
| Downsample, 세기, 반경 등 나머지 | 그대로 | 그대로 |

저장하면서 Unity가 렌더러 에셋을 최신 URP 형식으로 다시 저장했다(`m_AssetVersion` 2 → 3). 더 이상 쓰지 않는 블루 노이즈 텍스처 참조가 정리되었다. 이는 자동 업그레이드이며 설정값 변경은 아니다.

## 결과 (중앙값, 전 → 후)
"전"은 3-1 미니맵 최적화가 적용된 상태다.

| 시점 | Batches | Draw Calls | SetPass | Triangles | CPU Main | CPU Render |
|---|---:|---:|---:|---:|---:|---:|
| A | 214 → **143** (-33%) | 703 → **525** (-25%) | 59 → 55 | 2.40M → **1.79M** | 2.38 → 2.44 | 1.82 → 1.90 |
| B | 224 → **147** (-34%) | 738 → **523** (-29%) | 66 → 59 | 204k → **153k** | 2.40 → 2.43 | 1.92 → 1.92 |
| C | 359 → **245** (-32%) | 933 → **641** (-31%) | 66 → 61 | 2.29M → **1.71M** | 2.81 → 2.66 | 1.84 → 1.73 |
| D | 248 → **150** (-40%) | 736 → **437** (-41%) | 59 → 54 | 1.20M → **0.61M** | 2.81 → 2.79 | 1.90 → 1.89 |

(CPU 단위 ms)

- **모든 시점에서 Batches 32~40%, Draw Calls 25~41%, 삼각형 0.05~0.6M이 줄었다.** 사전 패스 하나가 통째로 사라진 만큼이다.
- **CPU 시간은 오차 범위(±0.2 ms) 안에서만 움직였다.**
  - 2단계에서 SSAO를 꺼 봤을 때도 CPU 변화는 없었다.
  - 이 장면의 Draw Call 제출은 SRP Batcher와 Render Graph 덕분에 이미 싸서, 패스 하나를 줄여도 메인 스레드 시간에는 드러나지 않는다.
- 사전 패스가 없어진 만큼 GPU도 장면을 한 번 덜 그린다. 하지만 에디터의 GPU 값은 장면과 무관하게 약 7 ms로 고정되어 확인할 수 없다(2단계 참고). **실제 효과는 Development Build에서 확인해야 한다.**

## 파일
- 숫자: `before_<A~D>.json`, `after_<A~D>.json`, `candidates/candidates*.json`
- Stats: `stats_before_*`, `stats_after_*`, `stats_compare_<A~D>.png`
- Profiler: `profiler_before_*`, `profiler_after_*`, `profiler_compare_<A~D>_cpu.png`, `profiler_compare_<A~D>_rendering.png`
- 화질: `candidates/candidates_compare.png`, `candidates/zoom_A.png`, 설정별 원본 `candidates/<시점>_<설정>.png`
