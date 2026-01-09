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
    private string _animation;
    private int _level;
    private float _exp;
    private State _state;
    private string _name;
    private Species _species;
    private string _equipment;
    private string _inventory;
}

public struct Stat
{
    
}

public enum State
{
    
}

public enum Species
{
    Dog,
    Cat
}