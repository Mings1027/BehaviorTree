How To Use  

Open Behavior Tree Editor  
![스크린샷 2024-09-27 오전 10 07 12](https://github.com/user-attachments/assets/415802bc-afad-4abb-a9f3-ff5485e61430)  

1. Create a Tree Asset in the Project tab.  
![스크린샷 2024-09-27 오전 10 26 07](https://github.com/user-attachments/assets/71e25f5c-1796-4fd1-b65c-496edda15c75)

2. Choose a node from the Tasks tab to create it.  
![스크린샷 2024-09-27 오전 10 11 28](https://github.com/user-attachments/assets/8ecf0bbc-fdf5-4ae6-a2f0-fc6d615aee33)

3. If you need to share values between nodes, create shared variables in the Variables tab.  
![스크린샷 2024-09-27 오전 10 14 34](https://github.com/user-attachments/assets/87bc6710-e0b9-401b-b78e-48f91509bcac)

4. Enter a name, select a type, and click the Add button.  
![스크린샷 2024-09-27 오전 10 15 36](https://github.com/user-attachments/assets/28253c93-f2d5-4e8c-af8c-b9aa0bf2a319)

5. In the Inspector tab, you can view information about the selected node and choose a shared variable if available.  
![스크린샷 2024-09-27 오전 10 17 26](https://github.com/user-attachments/assets/f0afec07-e83d-4282-b497-7b86cf995b3c)

6. Under Shared Variables, you'll see the shared variables declared for the node, and under Node Variables, you'll find the regular variables specific to that node.  
![스크린샷 2024-09-27 오전 10 20 47](https://github.com/user-attachments/assets/ecbbfca2-07ff-4086-9a7e-e3c1ed305a4b)

7. In the Shared Variables section of the two images below, you can see that "Target Collider" is selected for both. By selecting the same name, the two nodes will share the same value.  
![스크린샷 2024-09-27 오전 10 21 48](https://github.com/user-attachments/assets/2ad0d71c-3302-49fc-8db4-18f2c2e050b2)
![스크린샷 2024-09-27 오전 10 21 55](https://github.com/user-attachments/assets/7fcc0d9c-2c11-46b3-ba08-bac30d17cf74)

8. To use the behavior tree, attach the "Behavior Tree Runner" component to an object and assign a Tree Asset.  
![스크린샷 2024-09-27 오전 10 24 43](https://github.com/user-attachments/assets/ecba07db-a685-4bf4-ae51-2552e3c9c394)
