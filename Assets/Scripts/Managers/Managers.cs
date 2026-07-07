using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    #region Managers
    static Managers s_instance;
    private static Managers Instance { get { Init(); return s_instance; }}

    InputManager _input;
    public static InputManager Input { get { return Instance._input; }}

    ResourceManager _resource = new();
    public static ResourceManager Resource { get { return Instance._resource; }}

    SoundManager _sound = new();
    public static SoundManager Sound { get { return Instance._sound; }}

    UIManager _ui = new();
    public static UIManager UI { get { return Instance._ui; }}

    AnimationManager _animation = new();
    public static AnimationManager Animation { get { return Instance._animation; }}

    DataManager _data = new();
    public static DataManager Data { get { return Instance._data; }}

    public static Inventory Inventory { get; private set; } = new Inventory();
    #endregion

    void Start()
    {

    }

    void Awake()
    {
        Init();
    }

    void Update()
    {
        if (_input != null)
            _input.OnUpdate();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject("@Managers");
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            s_instance._input = new InputManager();
        }
    }
}
