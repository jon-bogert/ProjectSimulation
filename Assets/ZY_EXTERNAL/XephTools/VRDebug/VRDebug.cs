using UnityEngine;
using TMPro;
using System;

namespace XephTools
{
    /// <summary>
    /// Worldspace Debug logger for use for VR development
    /// Can be parented places in the world (such as attached to the player's arm)
    /// Intercepts the Debug.Log Commands, so no additional call is nessicary
    /// Adds Monitor field so constant value streams can me monitored without taking up 
    ///     console lines -> VRDebug.Monitor(num, value).
    /// Includes FPS counter
    /// Will Destroy itself in release build
    /// </summary>
    public class VRDebug : MonoBehaviour, ILogHandler
    {
        [SerializeField] float messageTime = 3f;
        [SerializeField] TMP_Text FPSField;
        [SerializeField] TMP_Text[] Monitors;
        [SerializeField] TMP_Text[] Messages;

        float[] timers;

        ILogHandler defaultLogger;

        static VRDebug inst = null;

        void Awake()
        {
            if (!Debug.isDebugBuild)
            {
                Destroy(gameObject);
                return;
            }
            if (inst != null)
            {
                Debug.LogWarning("VRDebug already found instance in scene");
                Destroy(this);
                return;
            }

            defaultLogger = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = this;

            inst = this;
            timers = new float[Messages.Length];
        }

        void OnDestroy()
        {
            if (inst != this) return;
            inst = null;
            Debug.unityLogger.logHandler = defaultLogger;
        }


        void LateUpdate()
        {
            UpdateFPS();
            UpdateOpacity();
            UpdateTimers();
        }

        void UpdateFPS()
        {
            string newStr;
            if (Time.deltaTime != 0)
                newStr = ((int)(1f / Time.deltaTime)).ToString();
            else newStr = "0";
            FPSField.text = newStr;
        }

        void Log(string msg, Color color)
        {
            for (int i = Messages.Length - 1; i >= 1; --i)
            {
                Messages[i].text = Messages[i - 1].text;
                timers[i] = timers[i - 1];
                Messages[i].color = Messages[i - 1].color;
            }
            Messages[0].text = msg;
            timers[0] = messageTime;
            Messages[0].color = color;
        }

        float ToOpacity(float timer)
        {
            return timer / messageTime;
        }

        void UpdateOpacity()
        {
            for (int i = 0; i < timers.Length; ++i)
            {
                Color tmp = new Color(Messages[i].color.r, Messages[i].color.g, Messages[i].color.b, ToOpacity(timers[i]));
                Messages[i].color = tmp;
            }
        }

        void UpdateTimers()
        {
            for (int i = 0; i < timers.Length; ++i)
            {
                timers[i] = (timers[i] <= 0) ? 0 : timers[i] - Time.deltaTime;
            }
        }

        public static void Monitor(int num, object str)
        {
            if (inst == null)
                return;

            if (num <= 0 || num > inst.Monitors.Length)
            {
                Debug.LogWarning("VRDebug.Monitor index out of range");
                return;
            }
            inst.Monitors[num - 1].text = str.ToString();
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            Color color = Color.white;
            if (logType == LogType.Error || logType == LogType.Assert) color = Color.red;
            else if (logType == LogType.Warning) color = Color.yellow;

            string str = string.Format(format, args);

            Log(str, color);
            defaultLogger.LogFormat(logType, context, format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            Log(exception.Message, Color.red);
            defaultLogger.LogException(exception, context);
        }
    }
}
