# 로그인 화면 UI 성능 점검: 중첩 Canvas 분리 실험 (2026-11-03)

로그인 화면에 연출이 많아지면서(입자, 커서 꼬리, 새, 플레어 고스트, 좀비 실루엣, 필름 그레인, 제목 웨이브 등), 하나뿐인 Canvas가 매 프레임 다시 배치되는 비용을 의심했다. 매 프레임 움직이는 레이어를 **중첩 Canvas**로 분리해 정적인 로그인 패널과 다시 그리는 범위를 나누는 방법을 실험했다.

**결론: 효과가 없고 오히려 손해라서 적용하지 않았다(씬은 원래대로 되돌림).**

## 실험 내용
| 항목 | 전 | 후(실험) |
|---|---|---|
| Canvas 구성 | Canvas 1개 | 루트 Canvas + 중첩 Canvas 9개 |
| 중첩 Canvas 대상 | - | Sky, WindowLights, Zombies, SunFX, FlareGhosts, Particles, TitleText, CursorTrail, FilmGrain |

중첩 Canvas에는 입력이 필요 없어서 GraphicRaycaster를 붙이지 않았고, `overrideSorting`은 꺼서 그리는 순서를 유지했다.

## 측정 방법
- `LoginScene`을 Play 모드로 실행하고, 등장 시퀀스가 끝난 뒤 연출이 계속 재생되는 정상 상태에서 측정했다.
  - 로그인 화면의 연출은 unscaled time으로 움직이므로 `timeScale`은 바꾸지 않았다.
- 유휴 상태 연출(`UI_IdleAttract`)은 측정하는 동안 런타임에서만 막았다(`_idleTime = 99999`). 전후 조건을 같게 하기 위해서다.
- 30프레임을 버린 뒤 300프레임 동안 `ProfilerRecorder`로 카운터를 수집해 중앙값을 기록했다. 기록은 `before.json`, `after.json`이다.
- 다음 화면을 함께 캡처했다.
  - Game 뷰 Stats
  - Profiler 창의 CPU Usage, UI, Rendering 모듈. 메인 스레드 프레임 시간이 중앙값인 프레임을 골랐다.
- 해상도는 Game 뷰 Full HD(1920×1080)이고, 에디터에서 측정했다.

## 결과 (중앙값, 전 → 후)
| 항목 | 전 | 후 | 변화 |
|---|---:|---:|---|
| CPU Main Thread | 1.447 ms | 1.407 ms | -0.04 ms (오차 범위) |
| CPU Render Thread | 0.887 ms | 0.817 ms | -0.07 ms (오차 범위) |
| UI 캔버스 갱신 `PostLateUpdate.PlayerUpdateCanvases` | 0.407 ms | 0.446 ms | **+0.04 ms** |
| UI 배치 갱신 `UGUI.Rendering.UpdateBatches` | 0.404 ms | 0.443 ms | **+0.04 ms** |
| `Canvas.BuildBatch` | 0.011 ms | 0.047 ms | +0.04 ms |
| Batches / Draw Calls | 32 / 32 | 32 / 32 | 같음 |
| SetPass Calls | 13 | **21** | **+8** |
| 루트 Canvas Self Batch Count (UI 모듈) | 23 | 8 | 나머지 15개가 중첩 Canvas로 이동했다. 합계(Cumulative)는 23으로 같다. |
| Used Buffers (Rendering 모듈) | 1,447 / 22.8 MB | 16,207 / 56.9 MB | **크게 증가** |

## 해석
1. **다시 그릴 정적인 부분이 거의 없다.**
   - 중첩 Canvas는 "자주 바뀌는 부분"과 "거의 안 바뀌는 부분"을 나눌 때 효과가 있다.
   - 로그인 화면은 배경 연출 대부분이 매 프레임 움직이고, 정적인 부분은 로그인 패널(입력칸 3개, 버튼 4개)뿐이다. 분리해도 매 프레임 다시 계산할 양이 거의 줄지 않는다.
2. **Canvas마다 고정 비용이 붙는다.**
   - Canvas가 9개 늘면서 Canvas별 배치 작업(`Canvas.BuildBatch`)과 머티리얼 상태 전환(SetPass +8)이 늘었다.
   - Canvas별 메시·버퍼도 따로 잡혀서 Used Buffers가 크게 늘었다.
3. **원래 비용이 작다.**
   - 전 상태에서 UI 배치 갱신이 0.4 ms로, 메인 스레드 1.45 ms의 약 28%다.
   - 다만 절대값이 작아서 60 FPS 예산(16.7 ms)에 비하면 여유가 크다. 에디터 측정이라 실제 빌드에서는 더 작을 것이다.

## 결정과 다음 단계
- 중첩 Canvas 분리는 **적용하지 않는다**. 씬은 실험 전 상태로 되돌렸다.
- UI 비용을 더 줄여야 한다면, Canvas를 쪼개기보다 **움직이는 UI 요소 수 자체를 줄이는 쪽**이 맞다.
  - 대상: 먼지·불씨 30개, 커서 꼬리 40개, 새 7개 등
  - 같은 3차 작업의 연출 세기 설정(Low/Medium/High)에서 입자·커서 꼬리 개수를 줄이는 것으로 다룬다.

## 참고: 버린 측정
- 처음 측정한 `before`는 측정하는 동안 입력이 없어서 유휴 상태(패널이 흐려짐)로 들어간 상태였다.
- 전후 조건을 맞추기 위해 유휴 연출을 막고 다시 측정했고, 첫 측정값(메인 1.246 ms, UI 배치 갱신 0.344 ms)은 버렸다.

## 파일
- 숫자: `before.json`, `after.json`
- Stats: `stats_before.png`, `stats_after.png`, `stats_compare.png`
- Profiler: `profiler_before_<cpu|ui|rendering>.png`, `profiler_after_<cpu|ui|rendering>.png`, `profiler_compare_<cpu|ui|rendering>.png`
