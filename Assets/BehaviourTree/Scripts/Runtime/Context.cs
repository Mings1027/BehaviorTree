// using Pathfinding;
// using UnityEngine;
// using UnityEngine.AI;
//
// namespace BehaviourTree.Scripts.Runtime
// {
//     // The context is a shared object every node has access to.
//     // Commonly used components and subsytems should be stored here
//     // It will be somewhat specfic to your game exactly what to add here.
//     // Feel free to extend this class 
//     public class Context
//     {
//         public Transform transform;
//         public Animator animator;
//         public AIPath aiPath;
//         // Add other game specific systems here
//
//         public static Context CreateFromGameObject(GameObject gameObject)
//         {
//             // Fetch all commonly used components
//             Context context = new Context();
//             context.transform = gameObject.transform;
//             context.animator = gameObject.GetComponent<Animator>();
//             context.aiPath = gameObject.GetComponent<AIPath>();
//             // Add whatever else you need here...
//
//             return context;
//         }
//     }
// }