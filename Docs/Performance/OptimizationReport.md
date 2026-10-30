# GameScene 렌더링 최적화 리포트 (2026-10-30)

오클루전 컬링 · LOD Group · Mesh LOD 적용 전후 비교. 원본 수치는 `baseline.json`(전), `after.json`(중간·후)에 있다.

## 측정 방법
- Play 모드 → `Time.timeScale = 0`으로 정지 → Main Camera의 `CameraFollow`/`CameraRotate`를 런타임에만 끄고 고정 시점 4곳에 배치
- 시점마다 300프레임 동안 `ProfilerRecorder`(Render 카운터, Main Thread)를 수집하고 앞 30프레임은 버림. 카운터는 중앙값
- 값은 **프레임 전체** 기준: Main 카메라 + Minimap 카메라 + 그림자 패스(4 cascade) 포함

| 시점 | 위치 / 회전 | 설명 |
|---|---|---|
| A | (0, 1.6, 0) / (5, 90, 0) | 시작 위치 1인칭, Player2(카타나) 바로 옆 |
| B | (-19.7, 1.6, -25.8) / (5, 90, 0) | 서쪽 도로, 건물에 시야가 가려짐 |
| C | (-24.4, 1.6, -61.7) / (5, 45, 0) | 남서쪽 끝에서 맵 대각선(넓은 시야) |
| D | (24.4, 60, -100) / (35, 0, 0) | 상공 조감, 가림 거의 없음(대조군) |

## 결과 (전 → 오클루전만 → 최종)

| 시점 | 항목 | 전 | 오클루전만 | 최종 | 변화 |
|---|---|---:|---:|---:|---:|
| A | Draw Calls | 697 | 655 | 639 | **-8.3%** |
| A | Batches | 258 | 251 | 249 | -3.5% |
| A | Triangles | 3,659,278 | 3,644,035 | 2,365,927 | **-35.3%** |
| A | Vertices | 2,507,552 | 2,483,264 | 2,222,006 | -11.4% |
| A | Shadow Casters | 433 | 425 | 401 | -7.4% |
| B | Draw Calls | 742 | 698 | 679 | **-8.5%** |
| B | Batches | 263 | 263 | 250 | -4.9% |
| B | Triangles | 181,077 | 167,727 | 164,891 | -8.9% |
| B | Shadow Casters | 390 | 386 | 362 | -7.2% |
| C | Draw Calls | 901 | 862 | 813 | **-9.8%** |
| C | Batches | 364 | 360 | 317 | -12.9% |
| C | Triangles | 2,220,100 | 2,211,374 | 1,208,831 | **-45.6%** |
| C | Vertices | 1,616,446 | 1,604,492 | 980,144 | -39.4% |
| D | Draw Calls | 816 | 812 | 714 | **-12.5%** |
| D | Batches | 328 | 326 | 263 | -19.8% |
| D | Triangles | 2,195,681 | 2,194,595 | 1,186,693 | **-46.0%** |
| D | Vertices | 1,575,948 | 1,574,065 | 942,442 | -40.2% |

SetPass Calls는 모든 시점에서 거의 같다(69~77). 머티리얼 수는 바뀌지 않았기 때문이다.

### 메인 스레드 시간(평균, ms) — 참고용
| 시점 | 전 | 오클루전만 | 최종 |
|---|---:|---:|---:|
| A | 12.94 | 11.94 | 10.94 |
| B | 13.48 | 12.57 | 11.25 |
| C | 13.25 | 12.30 | 10.37 |
| D | 12.67 | 11.91 | 9.74 |

에디터 오버헤드가 포함된 값이라 오차가 크다. 오클루전이 거의 아무것도 컬링하지 못한 D에서도 약 0.8ms가 줄었으므로 **측정 간 오차는 약 1ms**로 본다. 결론은 Draw Call과 삼각형 수를 기준으로 내린다.

## 기법별 해석
1. **오클루전 컬링**: 지면 높이 시점(A·B·C)에서 Draw Call이 4~6% 줄었고, 대조군 D에서는 0.5%만 줄었다. 기대한 패턴이다. 다만 효과가 작은데 그 이유는 다음과 같다.
   - 위에서 내려다보는 Minimap 카메라와 그림자 패스는 오클루전의 효과를 거의 받지 못한다.
   - 맵 메시 대부분이 이미 저폴리(수십~수백 tris)라서 삼각형 절감 폭이 작다.
2. **LOD Group**: 멀리 있는 작은 소품 84개는 컬링되고, 컨테이너 14개는 저폴리(132→12 tris)로 바뀐다. 그 결과 넓은 시야(C·D)에서 Batches가 13~20%, Draw Call이 6~12% 추가로 줄었다.
3. **Mesh LOD**: 삼각형 감소의 대부분을 담당한다. 핵심은 **카타나(500k tris)**로, 멀리서 볼 때 LOD가 낮아져 C·D에서 약 1M tris가 줄었다.

## 남은 병목 (다음 단계 제안)
- **카타나가 여전히 가장 큰 병목이다.** 시점 A에서 카타나 하나만 끄면 2,365,927 → 365,927 tris가 된다. 가까이에서는 LOD0(500k)으로 Main 카메라와 그림자 cascade마다 다시 그려지기 때문이다. Mesh LOD로는 근거리 비용을 줄일 수 없으므로 **원본 메시를 수천 tris로 리토폴로지/데시메이션**하는 것이 근본 해결책이다. 단기적으로는 카타나의 Cast Shadows를 끄는 방법이 있다.
- D(먼 거리)에서도 약 1.19M tris가 남는다. Minimap 카메라가 Player2 바로 위 8m에서 카타나를 근거리로 그리기 때문으로 추정된다. Minimap 카메라의 Culling Mask에서 캐릭터/무기를 빼거나 그림자 렌더링을 끄는 것을 권장한다(미검증).

## 변경 내역
- `GameScene.unity`
  - `Map_v1` 정적 렌더러의 Static 플래그 정리: 건물·콘크리트 벽·컨테이너·탱크 156개는 Occluder+Occludee, 철망·도로·소품 246개는 Occludee만
  - 오클루전 베이크(Smallest Occluder 3, Smallest Hole 0.25, Backface 100) → `Assets/01.Scenes/GameScene/OcclusionCullingData.asset`
  - LODGroup 98개(씬 인스턴스 오버라이드, 외부 에셋 프리팹은 수정하지 않음)
    - `Cargo_container_v1_LD2close` 14개: LOD0 = 기존 메시(15%), LOD1 = `LD1close` 자식(2%), 그 이하는 Culled
    - 작은 소품 84개(Wooden_box, Barrel_v1/v2/v3, Conditioner, Electric_box, Road_block, Generator): LOD0만 두고 4% 미만에서 Culled
    - 참고: 이 에셋 팩은 `LD1`이 저폴리, `LD2/LD3`가 고폴리다(이름과 반대).
- Mesh LOD(`generateMeshLods = true`) FBX 12개: WhiteKatana, Hangar_v2/v3/v4, Oil_tank_v1/v2, Palet_v1, Concrete_fence_v1/v2, Pipes_set_v1, Industrial_pipe_v1, Post-Apocalyptic Barrels
  - 수십 tris짜리 단순 메시(펜스 등)는 LOD가 생성되지 않는다(줄일 여지가 없음).

## 스크린샷
- **화질 비교**: `before_A~D.png` / `after_A~D.png`. 같은 시점에서 비교했을 때 눈에 띄는 화질 차이는 없다. before 이미지의 "준비" 텍스트는 GameState 차이 때문이다.
- **Stats 창 비교**: `stats_before_A~D.png` / `stats_after_A~D.png`, 시점별로 나란히 붙인 `stats_compare_A~D.png`
  - Game 뷰 Stats 창을 켠 상태로 Unity 창을 캡처했다. Scene 뷰가 같은 프레임에 렌더링되면 Stats에 합산되므로, 캡처하는 동안에는 Scene 뷰 카메라를 빈 공간으로 돌려 두었다(캡처 후 원래 위치로 복원).
  - "최적화 전" 화면은 원래 상태로 되돌려 찍은 것이 아니다. 같은 Play 세션에서 런타임에만 오클루전 끄기(`Camera.useOcclusionCulling = false`), 모든 LODGroup `ForceLOD(0)`, 모든 렌더러 `forceMeshLod = 0`을 적용해 재현했다. 재현값(A: Batches 259 / Tris 3.7M / SetPass 75 / Shadow casters 433)이 실제 최적화 전 측정값(258 / 3.66M / 75 / 433)과 일치하는 것을 확인했다.
