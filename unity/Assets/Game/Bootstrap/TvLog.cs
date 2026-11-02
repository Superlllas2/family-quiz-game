using System;
using UnityEngine;

namespace FamilyQuiz.Bootstrap
{
    /// <summary>Log lines with a local timestamp, for the Day 1 latency check. Safe from any thread.</summary>
    public static class TvLog
    {
        public static void Info(string message)
        {
            Debug.Log("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message);
        }

        public static void Error(string message)
        {
            Debug.LogError("[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message);
        }
    }
}
