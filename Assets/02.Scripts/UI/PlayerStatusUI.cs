using Domain.Player;
using UnityEngine;
using UnityEngine.UI;
using Systems;

namespace UI
{
    public class PlayerStatusUI : BaseUI
    {
        private enum Sliders
        {
            expBar,
            hpBar,
            staminaBar
        }

        [SerializeField] private PlayerController player;
        [SerializeField] private GaugeUI expBar;
        [SerializeField] private GaugeUI hpBar;
        [SerializeField] private GaugeUI staminaBar;

        private bool started;

        public override void Init() {
            player = Managers.Player;
            Bind<GaugeUI>(typeof(Sliders));
            expBar = Get<GaugeUI>((int)Sliders.expBar);
            hpBar = Get<GaugeUI>((int)Sliders.hpBar);
            staminaBar = Get<GaugeUI>((int)Sliders.staminaBar);

            if (player == null)
            {
                Debug.LogError("[PlayerStatusUI] PlayerController reference is missing.", this);
                
            }

            player.OnHealthChanged -= hpBar.SetGauge;
            player.OnHealthChanged += hpBar.SetGauge;
            player.OnStaminaChanged -= staminaBar.SetGauge;
            player.OnStaminaChanged += staminaBar.SetGauge;
        }

        private void Awake()
        {
            Init();
            hpBar.Configure(0f, player.MaxHealth);
            staminaBar.Configure(0f, player.MaxStamina);
            Refresh();
        }

        private void OnEnable()
        {
            if (player == null || hpBar == null || staminaBar == null)
            {
                Debug.LogError("[PlayerStatusUI] Assign Player, HP Bar and Stamina Bar in the Inspector.", this);
                return;
            }

            if (started) Refresh();
        }

        private void Start()
        {
            // All active scene objects have completed Awake before this initial read.
            started = true;
        }

        private void OnDisable()
        {
            if (player == null) return;
            player.OnHealthChanged -= hpBar.SetGauge;
            player.OnStaminaChanged -= staminaBar.SetGauge;
        }

        private void Refresh()
        {
            if (player == null) return;
            hpBar.SetGauge(player.CurrentHealth, player.MaxHealth);
            staminaBar.SetGauge(player.CurrentStamina, player.MaxStamina);
        }

        private static void SetBar(Slider bar, float current, float maximum)
        {
            if (bar != null)
                bar.SetValueWithoutNotify(maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f);
        }
    }
}
