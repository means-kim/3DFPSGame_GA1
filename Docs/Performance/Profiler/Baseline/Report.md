# 최적화 1단계: Unity Profiler 기준 측정 (2026-11-02)

조명(노을, 포인트 조명, 반사 프로브, 작업등, 폭발 조명)과 포스트 프로세싱(색 보정, 블룸, 그레인)을 모두 적용한 상태에서 최적화 전 기준값을 측정했다. 이후 최적화 작업은 이 값과 비교한다.

## 측정 방법
- 에디터 Play 모드, GPU 프로파일링 켬(DX12), 에디터 자체 프로파일링(`profileEditor`) 끔
- Main Camera를 고정 시점 4곳에 둔다. 시점은 이전 리포트(`../../OptimizationReport.md`)와 같다.

| 시점 | 위치 / 회전 | 설명 |
|---|---|---|
| A | (0, 1.6, 0) / (5, 90, 0) | 시작 위치 1인칭, Player2(카타나) 바로 옆 |
| B | (-19.7, 1.6, -25.8) / (5, 90, 0) | 서쪽 도로, 건물에 시야가 가려짐 |
| C | (-24.4, 1.6, -61.7) / (5, 45, 0) | 남서쪽 끝에서 맵 대각선(넓은 시야) |
| D | (24.4, 60, -100) / (35, 0, 0) | 상공 조감, 가림 거의 없음 |

- 시점마다 30프레임을 버린 뒤 300프레임을 수집했다.
  - `ProfilerRecorder` 카운터 요약(평균, 중앙값, 상위 5%): `baseline_<시점>.json`
  - 같은 구간의 Profiler 녹화에서 마커별 평균 Self 시간(GPU, CPU의 PlayerLoop 하위): `baseline_<시점>_breakdown.json`
  - Profiler 원본 데이터: `baseline_<시점>.data` (Profiler 창의 Load로 열 수 있다)
- 값은 프레임 전체 기준이다. Main 카메라, Minimap 카메라, 그림자 패스가 모두 들어간다.

## 결과 (중앙값)

| 시점 | CPU Main | CPU Render | GPU | Batches | SetPass | Draw Calls | Triangles | Shadow Casters |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| A | 2.87 ms | 1.91 ms | 6.79 ms | 249 | 71 | 783 | 2,427,053 | 724 |
| B | 2.68 ms | 1.92 ms | 7.44 ms | 259 | 78 | 818 | 226,858 | 632 |
| C | 3.13 ms | 1.93 ms | 7.12 ms | 390 | 78 | 1,013 | 2,309,393 | 642 |
| D | 2.99 ms | 1.94 ms | 7.25 ms | 277 | 71 | 814 | 1,218,550 | 221 |

| 시점 | GPU p95 | CPU Main p95 |
|---|---:|---:|
| A | 8.42 ms | 3.61 ms |
| B | 8.43 ms | 3.31 ms |
| C | 8.06 ms | 3.99 ms |
| D | 8.48 ms | 3.74 ms |

메모리(4개 시점 공통): 텍스처 약 757~759 MB, 메시 131 MB

- 카운터의 평균과 상위 5%가 중앙값의 약 1.25배, 2배로 나온다. 일부 프레임에서 카운터가 두 번 합산되는 것으로 보인다(Profiler 녹화와 동시 수집). 그래서 **표는 중앙값을 기준으로 한다.** 중앙값은 Profiler 창 Rendering 모듈의 값(예: D의 SetPass 71, Draw Calls 814, Batches 277, Shadow Casters 221)과 일치한다.
- CPU Main 2.7~3.1 ms는 게임 PlayerLoop만의 값이다. Profiler 창의 CPU 프레임 시간(약 10 ms)에는 `EditorLoop` 약 7 ms가 더해져 있다(`profiler_D_cpu.png`에서 70%). 에디터 오버헤드이므로 빌드에서는 빠진다.

## 비용 상위 항목 (breakdown, 프레임당 평균)

### GPU
| 항목 | A | B | C | D |
|---|---:|---:|---:|---:|
| UpdateReflectionProbeAtlas | 7.63 | 8.35 | 8.00 | 7.93 |
| Graphics.DrawProcedural | 0.74 | 0.60 | 0.61 | 0.50 |
| RenderLoop.ScheduleDraw | 0.54 | 0.21 | 0.40 | 0.47 |
| Shadows.ScheduleRenderJobs | 0.27 | 0.10 | 0.24 | 0.06 |
| 스키닝(MeshSkinning) | 0.14 | - | 0.15 | 0.15 |
| 합계 | 9.35 | 9.33 | 9.46 | 9.16 |

(단위 ms)

- **`UpdateReflectionProbeAtlas`가 GPU 시간의 80% 이상으로 집계되었다. 다만 이 값은 그대로 믿기 어렵다.**
  - 씬의 반사 프로브 4개는 모두 Baked 모드(해상도 128)다. 매 프레임 다시 그릴 이유가 없다.
  - 시점마다 거의 같다(7.6~8.4 ms). 화면 내용과 무관하게 일정하다.
  - breakdown 합계(약 9.3 ms)가 GPU Frame Time 중앙값(약 7 ms)보다 크다.
  - 따라서 렌더링 전체의 GPU 시간이 프레임에서 처음 열린 CPU 마커(Forward+의 프로브 아틀라스 갱신)로 몰려 집계되었을 가능성이 높다. **2단계에서 프로브 아틀라스를 끄고 GPU Frame Time이 실제로 줄어드는지 확인해야 한다.**
- 나머지 항목은 모두 1 ms 미만이다.

### CPU (PlayerLoop 하위 Self)
- 상위 항목은 잡 대기(`WaitForJobGroupID` 0.27~0.38 ms)와 URP 렌더 그래프 기록(`On Record Render Graph` 0.23~0.28 ms)이다.
- 그 밖에는 모두 0.1 ms 미만이다. 스크립트(게임 로직)는 상위 15개에 들지 않는다.
- 결론: **CPU는 병목이 아니다.** 최적화 대상은 GPU와 렌더링 부하다.

## 이전 측정(2026-10-30)과 비교

시점 A, 같은 위치 기준이다. 그 사이에 조명과 포스트 프로세싱 작업이 들어갔다.

| 항목 | 10-30 최종 | 이번 기준 |
|---|---:|---:|
| Batches | 249 | 249 |
| Triangles | 2,365,927 | 2,427,053 |
| Shadow Casters | 401 | 724 |

- Shadow Casters가 약 1.8배로 늘었다. 씬의 포인트 조명 8개와 스폿 조명 17개는 모두 그림자가 꺼져 있고, 그림자를 그리는 조명은 방향광 2개뿐이다. 따라서 증가 원인은 그림자 거리 확장(50 → 80 m)으로 본다.
- 삼각형은 여전히 카타나(근거리 약 500k)가 대부분이다.

## 2단계 병목 분석 후보 (우선순위)
1. **반사 프로브 아틀라스:** 위의 `UpdateReflectionProbeAtlas` 집계가 실제 비용인지 확인한다. 프로브 아틀라스와 블렌딩을 끄고 켜며 GPU 시간을 비교한다.
2. **그림자:** 그림자 거리 80 m와 cascade 수를 확인한다. 그림자를 그리는 방향광이 2개인데, 둘 다 필요한지도 확인한다. A·B·C에서 Shadow Casters가 630~720개다.
3. **카타나 메시:** 근거리 500k tris다. 시점 A·C에서 삼각형의 대부분을 차지한다.
4. **포스트 프로세싱:** HDR 색 보정, 블룸(렌즈 먼지), 그레인, SSAO를 하나씩 끄며 GPU 시간을 비교한다.
5. **Minimap 카메라:** 장면을 한 번 더 그린다. 시점 D(상공)에서도 1.2M tris가 남는 원인 후보다.
6. **텍스처 메모리 약 758 MB:** Render Texture가 약 447 MB(시점 D Rendering 모듈)를 차지한다. 텍스처 1,405개(`profiler_A_memory.png`)의 압축과 해상도도 확인한다.

## 파일
- `baseline_<A~D>.json`: 카운터 요약
- `baseline_<A~D>_breakdown.json`: 마커별 GPU/CPU 비용 상위 항목
- `baseline_<A~D>.data`: Profiler 원본 녹화(시점당 약 70 MB)
- `profiler_<A~D>_cpu.png`, `profiler_<A~D>_rendering.png`: CPU Usage, Rendering 모듈 화면
- `profiler_A_gpu.png`, `profiler_A_memory.png`: GPU Usage, Memory 모듈 화면(시점 A만)
