using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Managers : MonoBehaviour
{
    #region Managers
    static Managers s_instance;
    private static Managers Instance { get { Init(); return s_instance; }}

    readonly InputManager _input = new();
    public static InputManager Input { get { return Instance._input; }}

    readonly ResourceManager _resource = new();
    public static ResourceManager Resource { get { return Instance._resource; }}

    readonly SoundManager _sound = new();
    public static SoundManager Sound { get { return Instance._sound; }}

    readonly UIManager _ui = new();
    public static UIManager UI { get { return Instance._ui; }}

    readonly AnimationManager _animation = new();
    public static AnimationManager Animation { get { return Instance._animation; }}

    readonly DataManager _data = new();
    public static DataManager Data { get { return Instance._data; }}
    #endregion

    void Start()
    {
        Init();
    }

    void Update()
    {
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
        }
    }
}
