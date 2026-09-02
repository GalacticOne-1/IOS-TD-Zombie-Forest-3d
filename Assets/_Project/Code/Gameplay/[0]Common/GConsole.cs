#if UNITY_EDITOR
using System.Reflection;
#endif


namespace Galactic1
{
    public static class GConsole
    {
        /*
         *      Для очисти консоли во время проигрывания
         */
        public static void ClearLog()
        {
#if UNITY_EDITOR
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
#endif
        }
    }
}