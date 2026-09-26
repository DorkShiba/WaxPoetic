## BaseUI
모든 UI 컴포넌트들의 부모 클래스
앞으로 정의하는 UI들의 컴포넌트는 모두 BaseUI를 상속하도록 해주세요.

### Bind\<T\>(Type type)
enum을 인자로 받아 자식 오브젝트들에게서 T 컴포넌트를 추출해오는 메서드입니다.
예를들어,
```Text
Test
├Text1
├Image1
└Text2
```
의 구조로 Test라는 UI 프리팹을 만들었다고 가정합니다.

```C#
enum Texts {
    Text1,
    Text2
}
```
와 같이 자식 오브젝트 중에서 원하는 클래스의 컴포넌트(여기선 Text가 되겠죠)를 가진 자식의 이름을 enum으로 선언합니다.

그 후
```C#
Bind<Text>(typeof(Texts));
```
와 같이 사용하면 Text1과 Text2의 Text 컴포넌트를 추출해 내부 딕셔너리에 바인드합니다.

만약 Text2의 컴포넌트는 필요없다면, enum에는 Text1만 선언하면 됩니다. 다만, 컴포넌트를 가진 자식의 순서를 지켜서 선언해주어야 합니다. 현재 프리팹의 하이어라키 구조는 Text1 > Text2 순입니다. 따라서, enum을 선언할 때도 같은 순서로 선언해 주어야 올바른 인덱스로 바인드됩니다.

### Get\<T\>(int idx)
바인드한 자식 오브젝트의 컴포넌트를 가져옵니다.
```C#
Text text1 = Get<Text>(0); // 혹은 Get<Text>((int)Texts.Text1);
```
과 같이 사용합니다.

### public void BindEvent()
해당 UI의 게임오브젝트에 이벤트를 바인드할 때 사용합니다.

파라미터
- clickAction: 클릭 시 발생 이벤트
- dragAction: 드래그 시 발생 이벤트
- enterAction / exitAction: 커서 진입/탈출 시 이벤트
- downAction / upAction: 누를 때/뗄 때 이벤트


## UI_EventHandler
UI를 위한 이벤트핸들러 컴포넌트입니다.
각 UI마다 클릭, 드래그, 누를 때 등 특정 시점에 발동시키려는 이벤트가 있을텐데, 이를 관리하는 클래스라 생각하면 됩니다.

메서드
- 

## UIManager
Managers.UI로 접근가능

### SetSceneCanvas(GameObject go, int order)
인자로 받은 UI에 캔버스를 부여하고 순서는 order로 설정합니다.