using System;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    internal static class BuilderLog
    {
        internal static event Action<string> Changed;
        internal static string Last { get; private set; } = "Pronto. Selecione prefabs e ative a pintura.";
        internal static void Info(string message)
        {
            Last = message;
            Debug.Log("[Easy Level Design Tool] " + message);
            Changed?.Invoke(message);
        }
    }
}
