using System;
using System.Collections.Generic;

namespace BehaviourTree.Scripts
{
    /// <summary>
    /// Note : Register void functions with no parameter. Ensure 'AddEvent' and 'RemoveEvent' usages match.
    /// </summary>
    public static class EventManager
    {
        private static readonly Dictionary<ActionEvent, Action> EventDictionary = new();

        public static void AddEvent(ActionEvent actionEvent, Action action)
        {
            if (EventDictionary.TryAdd(actionEvent, action)) return;
            EventDictionary[actionEvent] += action;
        }

        public static void RemoveEvent(ActionEvent actionEvent, Action action)
        {
            if (EventDictionary.ContainsKey(actionEvent)) EventDictionary[actionEvent] -= action;
        }

        public static void TriggerEvent(ActionEvent actionEvent)
        {
            if (EventDictionary.TryGetValue(actionEvent, out var action)) action?.Invoke();
        }
    }

    public static class EventManager<T>
    {
        private static readonly Dictionary<ActionEvent, Action<T>> EventDictionary = new();

        public static void AddEvent(ActionEvent actionEvent, Action<T> action)
        {
            if (EventDictionary.TryAdd(actionEvent, action)) return;
            EventDictionary[actionEvent] += action;
        }

        public static void RemoveEvent(ActionEvent actionEvent, Action<T> action)
        {
            if (EventDictionary.ContainsKey(actionEvent)) EventDictionary[actionEvent] -= action;
        }

        public static void TriggerEvent(ActionEvent actionEvent, T t1)
        {
            if (EventDictionary.TryGetValue(actionEvent, out var action)) action.Invoke(t1);
        }
    }

    public static class EventManager<T1, T2>
    {
        private static readonly Dictionary<ActionEvent, Action<T1, T2>> EventDictionary = new();

        public static void AddEvent(ActionEvent actionEvent, Action<T1, T2> action)
        {
            if (EventDictionary.TryAdd(actionEvent, action)) return;
            EventDictionary[actionEvent] += action;
        }

        public static void RemoveEvent(ActionEvent actionEvent, Action<T1, T2> action)
        {
            if (EventDictionary.ContainsKey(actionEvent)) EventDictionary[actionEvent] -= action;
        }

        public static void TriggerEvent(ActionEvent actionEvent, T1 t1, T2 t2)
        {
            if (EventDictionary.TryGetValue(actionEvent, out var action)) action.Invoke(t1, t2);
        }
    }

    public static class EventManager<T1, T2, T3>
    {
        private static readonly Dictionary<ActionEvent, Action<T1, T2, T3>> EventDictionary = new();

        public static void AddEvent(ActionEvent actionEvent, Action<T1, T2, T3> action)
        {
            if (EventDictionary.TryAdd(actionEvent, action)) return;
            EventDictionary[actionEvent] += action;
        }

        public static void RemoveEvent(ActionEvent actionEvent, Action<T1, T2, T3> action)
        {
            if (EventDictionary.ContainsKey(actionEvent)) EventDictionary[actionEvent] -= action;
        }

        public static void TriggerEvent(ActionEvent actionEvent, T1 t1, T2 t2, T3 t3)
        {
            if (EventDictionary.TryGetValue(actionEvent, out var action)) action.Invoke(t1, t2, t3);
        }
    }

    public static class EventManager<T1, T2, T3, T4>
    {
        private static readonly Dictionary<ActionEvent, Action<T1, T2, T3, T4>> EventDictionary = new();

        public static void AddEvent(ActionEvent actionEvent, Action<T1, T2, T3, T4> action)
        {
            if (EventDictionary.TryAdd(actionEvent, action)) return;
            EventDictionary[actionEvent] += action;
        }

        public static void RemoveEvent(ActionEvent actionEvent, Action<T1, T2, T3, T4> action)
        {
            if (EventDictionary.ContainsKey(actionEvent)) EventDictionary[actionEvent] -= action;
        }

        public static void TriggerEvent(ActionEvent actionEvent, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            if (EventDictionary.TryGetValue(actionEvent, out var action)) action.Invoke(t1, t2, t3, t4);
        }
    }

    public static class FuncManager<T>
    {
        private static readonly Dictionary<FuncEvent, Func<T>> EventDictionary = new();

        public static void AddEvent(FuncEvent funcEvent, Func<T> func)
        {
            if (EventDictionary.TryAdd(funcEvent, func)) return;
            EventDictionary[funcEvent] += func;
        }

        public static void RemoveEvent(FuncEvent funcEvent, Func<T> func)
        {
            if (EventDictionary.ContainsKey(funcEvent)) EventDictionary[funcEvent] -= func;
        }

        public static T TriggerEvent(FuncEvent funcEvent)
        {
            return EventDictionary[funcEvent].Invoke();
        }
    }

    public static class FuncManager<T, TResult>
    {
        private static readonly Dictionary<FuncEvent, Func<T, TResult>> EventDictionary = new();

        public static void AddEvent(FuncEvent funcEvent, Func<T, TResult> func)
        {
            if (EventDictionary.TryAdd(funcEvent, func)) return;
            EventDictionary[funcEvent] += func;
        }

        public static void RemoveEvent(FuncEvent funcEvent, Func<T, TResult> func)
        {
            if (EventDictionary.ContainsKey(funcEvent)) EventDictionary[funcEvent] -= func;
        }

        public static TResult TriggerEvent(FuncEvent funcEvent, T t)
        {
            return EventDictionary[funcEvent].Invoke(t);
        }
    }

    public static class SceneEventManager
    {
        private static readonly Dictionary<SceneEvent, Action> SceneTable = new();
        private static readonly Dictionary<SceneEvent, Action<string>> SceneEventTableWithParam = new();
        public const string LoginScene = "LoginScene";
        public const string LobbyScene = "LobbyScene";
        public const string MainGameScene = "MainGameScene";
        public const string ReStartScene = "ReStartScene";

        public static void AddEvent(SceneEvent sceneEvent, Action action)
        {
            if (SceneTable.TryAdd(sceneEvent, action)) return;
            SceneTable[sceneEvent] += action;
        }

        public static void RemoveEvent(SceneEvent sceneEvent, Action action)
        {
            if (SceneTable.ContainsKey(sceneEvent)) SceneTable[sceneEvent] -= action;
        }

        public static void TriggerEvent(SceneEvent sceneEvent)
        {
            if (SceneTable.TryGetValue(sceneEvent, out var action)) action.Invoke();
        }

        public static void AddEvent(SceneEvent sceneEvent, Action<string> action)
        {
            if (SceneEventTableWithParam.TryAdd(sceneEvent, action)) return;
            SceneEventTableWithParam[sceneEvent] += action;
        }

        public static void RemoveEvent(SceneEvent sceneEvent, Action<string> action)
        {
            if (SceneEventTableWithParam.ContainsKey(sceneEvent)) SceneEventTableWithParam[sceneEvent] -= action;
        }

        public static void TriggerEvent(SceneEvent sceneEvent, string sceneName)
        {
            if (SceneEventTableWithParam.TryGetValue(sceneEvent, out var action))
                action.Invoke(sceneName);
        }
    }
}