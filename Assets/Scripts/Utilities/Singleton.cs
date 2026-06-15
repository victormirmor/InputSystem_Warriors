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
                Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    "' already destroyed on application quit." +
                    " Won't create again - returning null.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // LA CORRECCIÓN REAL:
                    // En las APIs más recientes, usamos la sobrecarga que solo define si incluye o no los inactivos.
                    // Al omitir el parámetro 'FindObjectsSortMode', Unity usa el modo óptimo por defecto sin advertencias.
                    T[] managers = FindObjectsByType<T>(FindObjectsInactive.Include);

                    if (managers.Length > 0)
                    {
                        _instance = managers[0];

                        if (managers.Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                " - there should never be more than 1 singleton de tipo " + typeof(T) + "!" +
                                " Reopening the scene might fix it.");
                        }
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) " + typeof(T).ToString();

                        Debug.Log("[Singleton] An instance of " + typeof(T) +
                            " is needed in the scene, so '" + singleton +
                            "' was created.");
                    }
                }

                return _instance;
            }
        }
    }

    private static bool IsDontDestroyOnLoad()
    {
        if (_instance == null)
        {
            return false;
        }
        if ((_instance.gameObject.hideFlags & HideFlags.DontSave) == HideFlags.DontSave)
        {
            return true;
        }
        return false;
    }

    private static bool applicationIsQuitting = false;
    
    public void OnDestroy()
    {
        if (IsDontDestroyOnLoad())
        {
            applicationIsQuitting = true;
        }
    }
}