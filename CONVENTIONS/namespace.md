# 네임스페이스 컨벤션

`Assets/02.Scripts/` 하위의 모든 코드는 **폴더 경로 = 네임스페이스**를 그대로 미러링합니다.
(예: `Domain/Interactables/` → `namespace Domain.Interactables`, `GameData/` → `namespace GameData`)

폴더 이름 자체는 아래 6가지 기준을 따릅니다.

## 분류 기준

| # | 분류 | 네임스페이스 | 포함되는 것 / 역할 |
|---|---|---|---|
| 1 | 인터페이스 | `Interfaces` | `IInteractable`, `ICollectible`, `IDamageable` |
| 2 | 범용 프레임워크 | `Framework` | `Singleton<T>` |
| 3 | 코어 시스템 & 매니저 | `Systems` | `Managers`, `InputManager`, `SoundManager`, `UIManager`, `DataManager`, `ResourceManager` |
| 4 | 도메인 인게임 객체 | `Domain.*` | `Domain.Player`, `Domain.Interactables`, `Domain.Combat`, `Domain.Enemy`, `Domain.Items`, `Domain.Camera` |
| 5 | 정적 데이터 & SO | `GameData` | `PlayerData`, `ItemData`, `Stats` (ScriptableObject 및 정적 데이터 정의) |
| 6 | 헬퍼 & 유틸리티 | `Utils` | `Define` (정적 상수 및 확장 메서드) |

## 판단 시 주의할 함정

- **"Data"라는 단순 명칭은 지양합니다.** 정적 데이터 및 ScriptableObject는 `GameData`, 런타임 엔티티/오브젝트는 `Domain`으로 역할을 명확히 분리합니다.
- **`MonoBehaviour` 상속 여부는 분류 기준이 아닙니다.** 컴포넌트 형태라도 런타임 엔티티 행동을 다루면 `Domain`, 시스템 관리를 총괄하면 `Systems`에 배치합니다.

## 접미사(suffix) 규칙

| 접미사 | 의미 | 전제조건 |
|---|---|---|
| `*Manager` | 전역 시스템을 관리하는 싱글톤/매니저 객체 | `Managers.*` 통해 중앙 접근 가능 |
| `*Controller` | 하나의 GameObject 컴포넌트 독립 제어자 (1:1) | Unity standard 컴포넌트 컨트롤러 |
| `*Interactable` | 플레이어 상호작용 가능한 객체 (`IInteractable`) | `Interact(GameObject interactor)` 구현 |
| `*Pickup` | 플레이어 접촉 시 자동 획득되는 객체 (`ICollectible`) | `Collect(GameObject collector)` 구현 |
| `*Data` | ScriptableObject 기반 데이터 컨테이너 | 정적 속성 및 밸런스 테이블 보관 |
