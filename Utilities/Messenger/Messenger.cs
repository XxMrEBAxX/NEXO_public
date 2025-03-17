using System;
using System.Collections.Generic;
using UnityEngine;

namespace USingleton.Utility
{
    /// <summary>
    /// Messager 클래스는 메시지를 등록하고 보내는 기능을 제공합니다.
    /// </summary>
    public static class Messenger
    {
        /// <summary>
        /// 매개 변수와 반환 값이 없는 메서드를 정의하는 대리자를 나타냅니다.
        /// </summary>
        public delegate void DoObject();
        
        private static readonly Dictionary<string, DoObject> RegisteredMessages;
        
        static Messenger()
        {
            // Init
            RegisteredMessages = new Dictionary<string, DoObject>();
        }

        /// <summary>
        /// 주어진 메시지 이름으로 메시지를 등록합니다.
        /// </summary>
        /// <param name="messageName">메시지의 이름입니다.</param>
        /// <param name="doObject">등록하려는 메시지 동작입니다.</param>
        public static void RegisterMessage(string messageName, DoObject doObject)
        {
            if (!RegisteredMessages.TryAdd(messageName, doObject))
                Debug.LogWarning($"Messager: The item {messageName} already contains a reference to the message.");
        }

        /// <summary>
        /// RegisteredMessages에서 이름을 기준으로 메시지를 제거합니다.
        /// </summary>
        /// <param name="messageName">제거할 메시지의 이름입니다.</param>
        public static void RemoveMessage(string messageName)
        {
            if (RegisteredMessages.ContainsKey(messageName))
                RegisteredMessages.Remove(messageName);
        }

        /// <summary>
        /// 등록된 모든 메시지를 제거합니다.
        /// </summary>
        public static void RemoveAllMessages()
        {
            RegisteredMessages.Clear();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// 지정된 이벤트 이름으로 메시지를 보냅니다.
        /// </summary>
        /// <param name="eventName">이벤트의 이름입니다.</param>
        public static void Send(string eventName)
        {
            if (RegisteredMessages.TryGetValue(eventName, out DoObject message))
            {
                try
                {
                    message?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Messager: An exception of type {e.GetType().Name} was caught while sending the {eventName} message.");
                    Debug.LogException(e);
                }
            }
            else
            {
                Debug.LogWarning($"Messager: The {eventName} event is not registered.");
            }
        }
    }
}