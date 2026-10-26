# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

Unity **6000.3.22f1** (URP 17) 기반 3D FPS 학습용 게임 프로젝트. 플레이어가 총·폭탄으로 FSM 기반 좀비를 처치한다. 씬은 `Assets/01.Scenes/GameScene.unity` 하나뿐이다.

- 게임 코드는 `Assets/02.Scripts/`에만 있다. `Assets/Plugins/CW`(Lean Pool/Common), `BloodDecalsAndEffects`, `TextMesh Pro`, `TutorialInfo` 등은 외부 에셋이므로 수정하지 않는다.
- `Assets/02.Scripts/Example/`은 C# 문법 학습용 예제로, 게임 로직과 무관하다.
- 테스트 어셈블리(asmdef)는 없다. 별도의 CLI 빌드/테스트 스크립트도 없으므로, 동작 확인은 Unity 에디터에서 플레이해야 한다. 직접 실행하지 않았다면 실행했다고 표현하지 않는다.
- `com.coplaydev.unity-mcp` 패키지가 설치되어 있어 MCP로 Unity 에디터를 조작할 수 있다.

## 아키텍처

### 게임 상태 게이트
`GameManager`(싱글톤, `GameManager.Instance`)가 `GameState`(Ready → Playing → GameOver)를 관리한다. 시작 시 코루틴으로 "준비/시작!" 연출 후 `Playing`으로 전환된다. **게임플레이 스크립트(플레이어 이동·사격, 좀비, 커서 등)의 `Update()`는 `GameManager.Instance.State != GameState.Playing`이면 즉시 return하는 패턴**을 따른다. 새 게임플레이 컴포넌트도 이 패턴을 지켜야 한다.

### 데미지 시스템
- `IDamageAble.TakeDamage(DamageInfo)` 인터페이스로 피격을 통일한다. `DamageInfo` 구조체는 `Value`, `HitPoint`, `HitNormal`을 담는다.
- 공격 측은 구체 타입을 모른 채 `GetComponent<IDamageAble>()?.TakeDamage(...)`로 호출한다 (`PlayerGunFire`의 레이캐스트, `Zombie`의 공격, `Bomb`/`Drum` 폭발 등).
- 구현체: `Player`, `Zombie`, `Drum`, `Box`. 피격 후 처리(사망 판정, 이펙트, `GameManager.GameOver()` 호출)는 각 구현체 내부에서 스스로 한다("묻지 말고 시켜라").

### 좀비 AI
`Zombie`는 `ZombieState` enum + `switch` 기반 FSM이다(Idle/Patrol/Trace/Comeback/Attack/Hit/Death). Hit·Death는 코루틴으로 처리되며, `TakeDamage`에서 `StopAllCoroutines()`로 진행 중인 코루틴을 끊는다. 이동은 `CharacterController.SimpleMove`, 플레이어는 `"Player"` 태그로 찾는다. 상태 전이 시 `Debug.Log($"{_state} -> X")` 로그를 남기는 관례가 있다.

### 오브젝트 풀링
이펙트 생성/제거는 `Instantiate/Destroy` 대신 **Lean Pool**(`LeanPool.Spawn` / `LeanPool.Despawn`)을 쓴다. 파티클 이펙트는 `EffectPool` 컴포넌트가 재생 종료 시 스스로 Despawn한다.

### 입력과 커서
- Active Input Handling이 "Both"로 설정되어 있고, 현재 코드는 **레거시 `Input` 클래스**(`Input.GetButton("Fire1")`, `"Reload"` 등)를 사용한다.
- `CursorManager`가 커서를 잠그고, LeftAlt를 누르는 동안 커서를 보이게 한다. 사격은 `Cursor.visible`이 false일 때만 가능하다.

### UI / 카메라
- UI 스크립트는 `UI_` 접두사(`UI_PlayerStats`, `UI_Minimap`, `UI_HitEffect`)를 쓴다. 텍스트는 TextMeshPro.
- 메인 카메라는 `CameraFollow`/`CameraRotate`, 미니맵은 `MinimapCamera`(LateUpdate로 플레이어 머리 위 추적) + `UI_Minimap`(확대/축소 버튼).

## 코드 규칙

`.editorconfig`(`Assets/02.Scripts/**.cs`에만 적용)의 규칙:
- 4칸 스페이스, Allman 스타일 중괄호(모든 여는 중괄호 새 줄).
- 단일 행 조기 리턴은 중괄호 생략 허용(`if (...) return;`), 여러 줄이면 중괄호 필수.
- private 필드 `_camelCase`, private const / private static readonly는 `PascalCase`, 타입명 PascalCase(`_` 금지), 지역변수 camelCase.
- Inspector 노출 값은 `[SerializeField] private` + `[Header("스탯")]` 형태로 둔다. 외부 공개는 읽기 전용 프로퍼티(`public float Health => _health;`).
- 코드 주석은 한국어로 작성한다.

## 커밋 규칙

`.husky/commit-msg` 훅(Husky.Net, `dotnet-tools.json`)이 강제한다:
- 제목은 `type: 설명` 또는 `type(scope): 설명` 형식. type은 `asset|build|chore|ci|design|docs|feat|fix|hotfix|perf|refactor|revert|style|test`.
- **본문이 최소 1줄 필요**하다(없으면 커밋 거부).
- 훅이 제목 앞에 type별 이모지(✨ feat, 🐛 fix, 🔧 chore 등)를 자동으로 붙이므로 **이모지를 직접 쓰지 않는다**.

## 코드 리뷰

- `main`에 `Assets/02.Scripts/**/*.cs` 변경이 push되면 `.github/workflows/gemini-commit-review.yml`이 Gemini로 커밋 리뷰를 남긴다.
- 리뷰 기준은 `Docs/CodeReviewGuide.md`에 있다. 코드 리뷰 요청 시 이 문서의 기준(중요도 Critical/High/Medium/Low, 리뷰 결과 형식)을 따른다.

## 작업 지침

- 답변은 한국어로 작성한다.
- 요청받지 않았다면 코드를 직접 변경하거나 커밋하지 않는다. 요청과 관련 없는 파일은 수정하지 않는다.
- `.unity`, `.prefab`, `.meta` 파일은 Unity가 관리하는 YAML이므로 가급적 에디터(또는 Unity MCP)를 통해 변경한다.
