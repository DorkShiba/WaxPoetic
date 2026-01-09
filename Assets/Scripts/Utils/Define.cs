using UnityEngine;

public class Define
{
    public enum InputType
    {
        
    }

    public enum SceneType
    {
        
    }

    public enum State
    {
        
    }

    public enum SoundType
    {
        
    }
}

public struct PlayerInfo
{
    private Stat _stat;
    private Animation _animation;
    private int _level;
    private float _exp;
    private State _state;
    private string _name;
    private Species _species;
    private List<Item> _equipment;
    private List<Item> _inventory;
}
