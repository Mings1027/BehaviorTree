![image](https://github.com/user-attachments/assets/41e9e4f5-9dbd-4d64-875b-7eb6ccc40c59)

Behavior Tree Editor 창을 여는 세가지 방법
1. 상단 메뉴에서 Editor를 선택합니다.

![스크린샷 2024-09-13 오후 2 30 56](https://github.com/user-attachments/assets/3fafcf8a-38b8-476a-9b8f-e3f91ed4c173)

2. Project 탭에서 Tree Asset을 더블클립합니다.

![스크린샷 2024-09-13 오후 2 31 54](https://github.com/user-attachments/assets/65765bd7-3d48-417a-9ba4-48f2cfe95d40)

3. BehaviorTreeRunner 컴포넌트에서 Open 버튼을 누릅니다.

![스크린샷 2024-09-13 오후 2 31 25](https://github.com/user-attachments/assets/032df0c9-17f9-4f20-8c2c-b6311f421623)

Behavior Tree Editor 사용방법
1. 노드 생성
1-1. Editor의 Task 탭에서 원하는 노드를 선택하면 해당 노드가 생성됩니다.
1-2. Editor가 활성화된 상태에서 Space Bar를 누르면 검색창이 마우스 위치에 보여집니다. 원하는 노드를 검색 후 생성할 수 있습니다.
1-3. RootNode는 자동생성입니다.
1-4. 노드를 조합해 원하는 트리를 만듭니다.

![스크린샷 2024-09-13 오후 2 37 11](https://github.com/user-attachments/assets/a7fed9d9-b791-4721-b41e-5889b0a196ad)

2. 공유 변수 생성 (생략 가능)
2-1. Editor의 Variables 탭에서 노드 간에 공유할 값을 생성할 수 있습니다.
2-2. Name과 Type을 설정하고 Add 버튼으로 추가합니다.
아래 사진은 Collider변수와 Int변수가 공유변수로 생성되있는 상황입니다.

![스크린샷 2024-09-13 오후 2 40 53](https://github.com/user-attachments/assets/23979b8e-1959-4ce8-8832-b417f8b50732)

2-3. 공유변수를 생성했다면 Inspector 탭에서 공유변수를 사용할 노드를 선택합니다.
현재 ReadyToAttack 노드가 선택되있고 해당 노드에는 SharedCollider 타입변수 'enemy'가 선언되어 있습니다.
위에서 만든 공유변수 중 해당 노드에서 사용할 공유변수의 이름을 선택해줍니다.
아래 사진은 ReadyToAttack 노드에서 'Target' 이라는 이름을 선택한 사진입니다.

![스크린샷 2024-09-13 오후 2 45 53](https://github.com/user-attachments/assets/aa7078ca-52de-4a30-9cae-539fed64d039)
아래 사진은 Attack 노드에서 'Target' 이라는 이름을 선택한 사진입니다.

![스크린샷 2024-09-13 오후 2 46 21](https://github.com/user-attachments/assets/7c54e7a1-2e38-4558-980c-8a9e3d71a53a)

런타임 도중 값이 잘 공유되고 있는지 확인할 수 있습니다.

![스크린샷 2024-09-13 오후 2 49 26](https://github.com/user-attachments/assets/b5f9cb46-041a-4ed9-a201-bec370ebd006)
![스크린샷 2024-09-13 오후 2 49 43](https://github.com/user-attachments/assets/8fe3309d-b6b1-49c2-ae56-7f0abc22a132)

3. BehaviorTreeRunner
트리를 사용하는 모든 객체는 해당 컴포넌트를 하나씩 가집니다.

![스크린샷 2024-09-13 오후 2 52 28](https://github.com/user-attachments/assets/27c73cac-8980-4d8d-83bc-4f1398937714)
Runner 컴포넌트에 'Enable Variables' 토글을 활성화 시 플레이전에 공유값으로 사용할 객체를 할당할 수 있습니다.
이는 선택사항이며 공유변수 객체가 플레이전에 씬에 존재해야하는 경우 사용하는 옵션입니다.

공유변수가 씬에 미리 존재하지 않거나 런타임 도중에 할당해도 되는 경우 이 토글을 사용하지 않아도 됩니다.

![스크린샷 2024-09-13 오후 2 54 54](https://github.com/user-attachments/assets/0bb17cb4-6370-4dc3-a193-ac7d514706c7)

4. Global Variable
씬마다 하나씩 존재하는 리스트입니다.
씬에 존재하는 모든 객체가 접근할 수 있습니다.
상단메뉴에서 GlobalVariables Editor를 열 수 있습니다.

![스크린샷 2024-09-13 오후 3 01 03](https://github.com/user-attachments/assets/609d6fd9-5ef0-4c3b-a427-564d80f9b0e2)

아래 사진은 사용예시 입니다.

![스크린샷 2024-09-13 오후 2 58 42](https://github.com/user-attachments/assets/c111f53e-91d4-429b-ad39-64aa4a7ce93f)
![스크린샷 2024-09-13 오후 2 59 50](https://github.com/user-attachments/assets/74733d31-bbf3-4542-a565-e2e13f2cc56e)
![스크린샷 2024-09-13 오후 3 00 23](https://github.com/user-attachments/assets/b4ea457a-372f-4efd-b626-2f6a45cbdc0d)
