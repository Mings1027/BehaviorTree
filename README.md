![image](https://github.com/Mings1027/BehaviorTree/assets/100500113/f5ad2a95-4a0d-47a9-a4e3-4c1d12a6f7f1)

![스크린샷 2024-06-26 오후 9 22 20](https://github.com/Mings1027/BehaviorTree/assets/100500113/a9d13d37-f2d7-4b9f-94bc-1d8af8bbb5a9)

1. Tasks 탭 - 원하는 노드를 생성해 트리에 연결 합니다.
2. Variables 탭 - 노드간에 공유할 변수들을 만들어 줍니다.
3. Inspector 탭 - Variables탭에서 만든 변수를 노드의 변수에 연결해줍니다.

Variables 탭

<img width="415" alt="image" src="https://github.com/Mings1027/BehaviorTree/assets/100500113/dd5c8ea8-ac15-4703-af60-56389d71ed6c">

1. Name - 공유할 데이터의 이름을 지정합니다.
2. Type - 공유할 데이터 타입을 선택합니다.
3. Add - 위에서 정한 이름, 타입에 맞는 클래스를 리스트에 추가합니다.

Inspector 탭

<img width="297" alt="image" src="https://github.com/Mings1027/BehaviorTree/assets/100500113/6c3d3d2a-6608-43de-b169-2d1b2f8a7bfa">

1. Description - 노드의 설명을 적을 수 있습니다.
2. Shared Variables - 해당 노드에 있는 SharedVariable 변수들을 보여줍니다.
3. Local Variables - 해당 노드에 정의된 변수 중 SharedVariable이 아닌 직렬화 가능한 모든 변수를 보여줍니다.
4. 제일 아래쪽에는 모든 노드에서 이름을 할당하지 않은 노드의 이름과 변수명을 띄우줍니다.

사용방법

![image](https://github.com/Mings1027/BehaviorTree/assets/100500113/0e7bf4e7-22c1-4e12-9886-4d28a26e2434)
1. 원하는 행동을 정의하는 노드 클래스를 만듭니다.

<img width="291" alt="image" src="https://github.com/Mings1027/BehaviorTree/assets/100500113/dbaf9028-1871-4c62-923e-e472324d5cc6">
2. Behavior Tree Editor의 Tasks탭에서 만든 노드 클래스를 찾아 생성합니다.

<img width="243" alt="image" src="https://github.com/Mings1027/BehaviorTree/assets/100500113/bde39174-a24a-473c-b13f-2caa42eb68f2">
3. 노드간의 공유할 값이 있다면 Variables탭에 해당 값의 이름과 타입을 정해 만듭니다

![스크린샷 2024-06-26 오후 9 25 20](https://github.com/Mings1027/BehaviorTree/assets/100500113/5a5c7f05-59dc-4654-92bd-9d50a47bd4e9)
4. 공유값을 만든 경우 Inspector탭에서 해당 노드를 선택해 Variables에서 만든 값의 		이름을 찾아 할당합니다.

<img width="321" alt="image" src="https://github.com/Mings1027/BehaviorTree/assets/100500113/f97102d3-42d1-43d9-a9de-13017c612b07">
5. 트리를 사용할 객체에 'Behavior Tree Runner' 컴포넌트를 붙입니다.
6. 플레이전에 값을 할당해야한다면 'Enable Variables'를 체크하고 값을 할당해줍니다.
6-1. 미리 할당할 필요가 없는 경우 체크하지 않아도 됩니다.
