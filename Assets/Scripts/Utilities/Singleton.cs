using UnityEngine;

/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// </summary>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();

   public static T Instance
{
    get
    {
        if (applicationIsQuitting)
        {
            Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit. Won't create again - returning null.");
            return null;
        }

        if (_instance == null)
        {
            // Busca cualquier instancia suelta en la escena
            _instance = Object.FindAnyObjectByType<T>();

            // CORRECCIÓN AQUÍ: Quitamos el FindObjectsSortMode obsoleto
            if (Object.FindObjectsByType<T>().Length > 1)
            {
                Debug.LogError($"[Singleton] Something went really wrong - there should never be more than 1 singleton of {typeof(T)}!");
                return _instance;
            }

            if (_instance == null)
            {
                GameObject singleton = new GameObject($"(singleton) {typeof(T)}");
                _instance = singleton.AddComponent<T>();

                Debug.Log($"[Singleton] An instance of {typeof(T)} was needed in the scene, so '{singleton.name}' was created.");
            }
        }

        return _instance;
    }
}

    private static bool IsDontDestroyOnLoad()
    {
        if (_instance == null)
        {
            return false;
        }
        // Object exists independent of Scene lifecycle, assume that means it has DontDestroyOnLoad set
        if ((_instance.gameObject.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
        {
            return true;
        }
        return false;
    }

    private static bool applicationIsQuitting = false;
    /// <summary>
    /// When Unity quits, it destroys objects in a random order.
    /// In principle, a Singleton is only destroyed when application quits.
    /// If any script calls Instance after it have been destroyed, 
    ///   it will create a buggy ghost object that will stay on the Editor scene
    ///   even after stopping playing the Application. Really bad!
    /// So, this was made to be sure we're not creating that buggy ghost object.
    /// </summary>
    public void OnDestroy()
    {
        if (IsDontDestroyOnLoad())
        {
            applicationIsQuitting = true;
        }
    }
}