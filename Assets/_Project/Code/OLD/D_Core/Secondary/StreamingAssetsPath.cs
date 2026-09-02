using UnityEngine;

namespace Galactic1
{
    public static class StreamingAssetsPath
    {
        public static string StreamingAssetPathForWWW()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
            return "file://" + Application.dataPath + "/StreamingAssets/";
#endif
#if UNITY_ANDROID
            return "jar:file://" + Application.dataPath + "!/assets/";
#endif
#if UNITY_IOS
        return "file://" + Application.dataPath + "/Raw/";
#endif
            throw new System.NotImplementedException( "Check the ifdefs above.");
        }
        public static string StreamingAssetPathForFileOpen()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
        throw new System.NotImplementedException( "You cannot open files on Android. Must use WWW");
#endif
 
            Debug.Log( "Application.streamingAssetsPath:" + Application.streamingAssetsPath);
            return Application.streamingAssetsPath + "/";
        }
    }
}