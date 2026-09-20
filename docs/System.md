# Managers.cs

[스크립트](../../Assets/02.Scripts/Systems/Managers.cs)

게임 시스템의 동작을 총괄하는 매니저 스크립트입니다.
그 자체로는 동작하지 않지만, 하위에 인풋매니저, 리소스매니저 등을 두어 이들과의 연결지점으로 활용됩니다.
코드로는
``` C#
// Managers.(하위 매니저 이름).(메서드나 필드)의 형식
Managers.Input.MoveDirectoin;
Managers.Resource.Instantiate();
```
와 같이 Managers를 통해 하위 매니저를 부르고, 그 하위 매니저의 필드나 메서드를 호출해 사용하는 방식입니다.

Managers는 매니저 총괄뿐 아니라 코루틴 헬퍼의 기능도 합니다.
- StartCoroutineManager(Func<IEnumerator> func)
  - 코루틴 함수 하나를 받아 이를 대신 실행합니다.
- Coroutine StartCoroutineManager<T>(Func<T, IEnumerator> func, T t)
  - 코루틴 함수 하나와 인자를 받아 이를 대신 실행합니다.
- StopCoroutineManager(Coroutine coroutine)
  - 코루틴을 받아 이를 정지합니다.
- WaitForSeconds((float seconds, Action callback) args)
  - 정해진 시간만큼 기다렸다가 callback 함수를 실행합니다.

## 하위 매니저들
- 인풋매니저
  - [스크립트](../../Assets/02.Scripts/Systems/InputManager.cs)
  - [기술문서](./InputManager.md)
  - 게임 내 입출력 담당
- 리소스매니저
  - [스크립트](../../Assets/02.Scripts/Systems/ResourceManager.cs)
  - [기술문서](./ResourceManager.md)
  - 게임 내 입출력 담당
- 사운드매니저
  - [스크립트](../../Assets/02.Scripts/Systems/SoundManager.cs)
  - [기술문서](./SoundManager.md)
  - 게임 내 입출력 담당
- UI매니저
  - [스크립트](../../Assets/02.Scripts/Systems/UIManager.cs)
  - [기술문서](./UIManager.md)
  - 게임 내 입출력 담당
- 애니메이션매니저
  - [스크립트](../../Assets/02.Scripts/Systems/AnimationManager.cs)
  - [기술문서](./AnimationManager.md)
  - 게임 내 입출력 담당
- 데이터매니저
  - [스크립트](../../Assets/02.Scripts/Systems/DataManager.cs)
  - [기술문서](./DataManager.md)
  - 게임 내 입출력 담당
- 인벤토리
  - [스크립트](../../Assets/02.Scripts/Domain/Items/Inventory.cs)
  - [기술문서](./Inventory.md)
  - 게임 내 입출력 담당
- 씬매니저
  - [스크립트](../../Assets/02.Scripts/Systems/SceneManager.cs)
  - [기술문서](./SceneManager.md)
  - 게임 내 입출력 담당
- 풀링매니저
  - [스크립트](../../Assets/02.Scripts/Systems/PoolingManager.cs)
  - [기술문서](./PoolingManager.md)
  - 게임 내 입출력 담당