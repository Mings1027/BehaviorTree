How To Use  

Open Behavior Tree Editor  
![스크린샷 2024-09-27 오전 10 07 12](https://github.com/user-attachments/assets/415802bc-afad-4abb-a9f3-ff5485e61430)  

1. Create a Tree Asset in the Project tab.  
프로젝트 탭에서 Tree Asset을 만듭니다.  
![스크린샷 2024-09-27 오전 10 26 07](https://github.com/user-attachments/assets/71e25f5c-1796-4fd1-b65c-496edda15c75)

2. Choose a node from the Tasks tab to create it.  
Tasks 탭에서 원하는 노드를 선택해 생성합니다.  
![스크린샷 2024-09-27 오전 10 11 28](https://github.com/user-attachments/assets/8ecf0bbc-fdf5-4ae6-a2f0-fc6d615aee33)

3. If you need to share values between nodes, create shared variables in the Variables tab.  
노드 간에 값을 공유할 필요가 있다면 Variables 탭에서 공유변수를 만들어줍니다.  
![스크린샷 2024-09-27 오전 10 14 34](https://github.com/user-attachments/assets/87bc6710-e0b9-401b-b78e-48f91509bcac)

4. Enter a name, select a type, and click the Add button.  
Name을 적고 Type을 선택해 Add 버튼을 누르면 됩니다.  
![스크린샷 2024-09-27 오전 10 15 36](https://github.com/user-attachments/assets/28253c93-f2d5-4e8c-af8c-b9aa0bf2a319)

5. In the Inspector tab, you can view information about the selected node and choose a shared variable if available.  
Inspector 탭에서는 선택한 노드에 대한 정보를 확인할 수 있으며 공유변수가 있는 경우 선택할 수 있습니다.  
![스크린샷 2024-09-27 오전 10 17 26](https://github.com/user-attachments/assets/f0afec07-e83d-4282-b497-7b86cf995b3c)

6. Under Shared Variables, you'll see the shared variables declared for the node, and under Node Variables, you'll find the regular variables specific to that node.  
Shared Variables 아래에는 해당 노드에서 선언한 공유변수가 나타나고 Node Variables 아래에는 해당 노드에서 선언한 일반 변수들이 나타납니다.  
![스크린샷 2024-09-27 오전 10 20 47](https://github.com/user-attachments/assets/ecbbfca2-07ff-4086-9a7e-e3c1ed305a4b)

7. In the Shared Variables section of the two images below, you can see that "Target Collider" is selected for both. By selecting the same name, the two nodes will share the same value.  
아래 두 사진의 Shared Variables 부분을 보면 “Target Collider”로 같은 이름이 선택되어 있습니다. 이렇게 같은 이름을 선택하면 두 노드는 같은 값을 공유하게 됩니다.  
![스크린샷 2024-09-27 오전 10 21 48](https://github.com/user-attachments/assets/2ad0d71c-3302-49fc-8db4-18f2c2e050b2)
![스크린샷 2024-09-27 오전 10 21 55](https://github.com/user-attachments/assets/7fcc0d9c-2c11-46b3-ba08-bac30d17cf74)

8. To use the behavior tree, attach the "Behavior Tree Runner" component to an object and assign a Tree Asset.  
If any shared values need to be pre-assigned, you can assign them in the Behavior Tree Runner component before use.  
트리를 사용하기 위해선 객체에 “Behavior Tree Runner” 컴포넌트를 붙이고 Tree Asset을 할당해야 합니다.  
공유값 중 미리 할당이 필요한 경우 Behavior Tree Runner 컴포넌트에서 할당해두고 사용합니다.  
![스크린샷 2024-09-27 오전 10 24 43](https://github.com/user-attachments/assets/ecba07db-a685-4bf4-ae51-2552e3c9c394)
