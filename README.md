노드 기반의 Behavior Tree Editor입니다.
Behavior Designer를 참고해 제작하였습니다.

![스크린샷 2024-05-21 오후 3 32 32](https://github.com/Mings1027/Mings1027/assets/100500113/072b5a19-0dfa-4056-b3c9-9b2262f8cac5)

![스크린샷 2024-05-30 오전 11 44 57](https://github.com/Mings1027/Mings1027/assets/100500113/be2aeba7-523d-42a0-afd1-4a91d3f501de)

사용방법
1. Tasks 탭 - 원하는 노드를 생성해 트리에 연결 합니다.
2. Variables 탭 - 노드간에 공유할 변수들을 만들어 줍니다.
3. Inspector 탭 - 만든 변수의 이름을 노드의 변수에 연결해줍니다.

<img width="496" alt="image" src="https://github.com/Mings1027/Mings1027/assets/100500113/4a5738c9-a8b7-493e-b264-c5ae3ca96b6b">

1. Name - 공유할 데이터의 이름을 지정합니다.
2. Type - 공유할 데이터 타입을 선택합니다.
3. Add - 위에서 정한 이름, 타입에 맞는 클래스를 리스트에 추가합니다.
4. Variables - 리스트 원소들을 표시해줍니다.

<img width="490" alt="image" src="https://github.com/Mings1027/Mings1027/assets/100500113/2d1fc2a6-634d-4a5e-bd34-f5fa976d0d8f">

1. Description - 노드의 설명을 적을 수 있습니다.
2. Shared Data - 노드끼리 공유할 데이터가 들어있는 Scriptable Object입니다.
3. Shared Variables - 해당 노드에 있는 SharedVariable 변수들을 보여줍니다.
4. Show Values - 토글을 이용해 각 SharedVariable의 Value부분을 보여줍니다.
5. Non-SharedVariables - 해당 노드에 정의된 변수 중 SharedVariable이 아닌 직렬화 가능한 모든 변수를 보여줍니다.
6. 모든 탭의 제일 아래쪽에는 이름을 할당하지 않은 변수가 있는 노드들을 표시해줍니다.
