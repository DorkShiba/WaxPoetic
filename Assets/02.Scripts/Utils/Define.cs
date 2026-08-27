using UnityEngine;

namespace Utils
{
    public class Define
    {
        public enum InputType
        {
            Left,
            Right,
            Up,
            Down,
            Guard,
            Dodge,
            WeakAttack,
            StrongAttack,
            Interact,
            Menu
        }

        public enum AttackType
        {
            Skill1 = 0,
            Skill2 = 1,
            Skill3 = 2,
            Skill4 = 3
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
}