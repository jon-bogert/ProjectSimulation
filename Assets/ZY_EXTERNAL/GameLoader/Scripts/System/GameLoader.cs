using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XephTools;

public class GameLoader : AsyncLoader
{
    [SerializeField] private int sceneIndexToLoad = 1;
    [Space]
    [SerializeField] private List<Component> GameModules = new List<Component>();
    //[Header("Prefabs")]
    //[SerializeField] private GameObject saveManagerPrefab;
    private static GameLoader _instance = null;
    private static int _sceneIndex = 1;
    private static bool _isDontDestroyLoaded = false;

    public static Transform SystemsParent { get => _systemsParent; }
    private static Transform _systemsParent = null;

    private readonly static List<Action> _queuedCallbacks = new List<Action>();

    protected override void Awake()
    {
        Debug.Log("<b><color=cyan>GameLoader Starting</color></b>");

        // Safety check
        if (_instance != null && _instance != this)
        {
            Debug.Log("A duplicate instance of the GameLoader was found, and will be ignored. Only one instance is permitted");
            Destroy(gameObject);
            return;
        }

        // Set reference to this instance
        _instance = this;

        // Make persistent
        DontDestroyOnLoad(gameObject);

        // Scene Index Check
        if (sceneIndexToLoad < 0 || sceneIndexToLoad >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"Invalid Scene Index {sceneIndexToLoad} ... using default value of {_sceneIndex}");
        }
        else
        {
            Debug.Log($"Scene index to load is set to {sceneIndexToLoad}");
            _sceneIndex = sceneIndexToLoad;
        }

        // Setup System GameObject
        if (!_isDontDestroyLoaded)
        {
            GameObject systemsGO = new GameObject("[Services]");
            _systemsParent = systemsGO.transform;
            DontDestroyOnLoad(systemsGO);
            _isDontDestroyLoaded = true;
        }

        // Queue up loading routines
        Enqueue(IntializeCoreSystems(_systemsParent), 1); // Things we need to have
        Enqueue(InitializeModularSystems(_systemsParent), 2); // Optional

        // Set completion callback
        GameLoader.CallOnComplete(OnComplete);
    }

    private IEnumerator IntializeCoreSystems(Transform systemsParent)
    {
        Debug.Log("Loading Core Systems");

        // Example non-monobehaviour system registration
        var eventBusSystem = new EventBusSystemHub();
        ServiceLocator.Register<IEventBusSystemHub>(eventBusSystem);

        // EventBusCallbacks requires the system to be intialized first.
        var eventBusCallbacks = new EventBusCallbacks();
        eventBusCallbacks.Initialize();

        // Example monobehaviour system registration
        //var dummyGO = new GameObject("DummyMonoSystem");
        //dummyGO.transform.SetParent(systemsParent);
        //var dummySys = dummyGO.AddComponent<DummyMonoSystem>();
        //ServiceLocator.Register<IDummyMonoSystem>(dummySys);

        //Register<GameManager>("GameManager");

        //Prefabs
        //RegisterPrefab<SaveManager>(saveManagerPrefab);

        yield return null;
    }

    private void Register<T>(string gameObjName) where T : UnityEngine.Component
    {
        if (FindObjectOfType<T>() != null)
        {
            Debug.LogWarning("GameLoader.RegisterPrefab -> Prefab of type already found in scene");
            return;
        }
        var go = new GameObject(gameObjName);
        go.transform.SetParent(_systemsParent);
        var comp = go.AddComponent<T>();
        ServiceLocator.Register<T>(comp);
    }
    private void RegisterPrefab<T>(GameObject prefab) where T : UnityEngine.Component
    {
        if (FindObjectOfType<T>() != null)
        {
            Debug.LogWarning("GameLoader.RegisterPrefab -> Prefab of type already found in scene");
            return;
        }

        var go = Instantiate(prefab, _systemsParent);
        go.name = prefab.name; // Removes "(Clone)"
        ServiceLocator.Register<T>(go.GetComponent<T>());
    }

    private IEnumerator InitializeModularSystems(Transform systemsParent)
    {
        // Setup Additional Systems as needed
        Debug.Log("Loading Modular Systems");

        foreach (var module in GameModules)
        {
            if (module is IGameModule)
            {
                IGameModule gameModule = module as IGameModule;
                gameModule.Load();
            }
        }

        yield return null;
    }

    private IEnumerator LoadInitialScene(int index)
    {
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (index <= 0)
        {
            Debug.Log("GameLoader -> Skipping Scene Load: Scene index is <= 0");
            yield break;
        }
        if (index != activeSceneIndex)
        {
            Debug.Log($"GameLoader -> Starting Scene Load: {index}");
            yield return SceneManager.LoadSceneAsync(index);
        }
        else
        {
            // We already have the desired scene loaded.
            Debug.Log("GameLoader -> Skipping Scene Load: Scene is already active");
            yield break;
        }
    }

    protected override void ResetVariables()
    {
        Debug.Log("<color=orange>GameLoader Reset Variables</color>");
        base.ResetVariables();
    }

    public static void CallOnComplete(Action callback)
    {
        if (!_instance)
        {
            Debug.Log("<b><color=lime>GameLoader CallOnComplete -> QUEUED</color></b>");
            _queuedCallbacks.Add(callback); // Uses the static List declared in the header of the script
            return;
        }

        Debug.Log("<b><color=lime>GameLoader CallOnComplete -> INVOKED</color></b>");
        _instance.CallOnComplete_Internal(callback); // Adds the function in the inherited queue. Each queue belongs to their instance, THEY ARE NOT SHARED!
    }

    // Handles everything that is added in the static List before the instance of the GameLoader is created
    private void ProcessQueuedCallbacks()
    {
        Debug.Log($"<b><color=lime>GameLoader Processing {_queuedCallbacks.Count} QueuedCallbacks</color></b>");
        foreach (var callback in _queuedCallbacks)
        {
            callback?.Invoke();
        }
        _queuedCallbacks.Clear();
    }

    // AsyncLoader completion callback
    private void OnComplete()
    {
        Debug.Log("GameLoader Finished Initializing");
        ProcessQueuedCallbacks();
        StartCoroutine(LoadInitialScene(_sceneIndex));
    }
}