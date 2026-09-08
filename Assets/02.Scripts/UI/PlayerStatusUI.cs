using Domain.Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerStatusUI : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Slider hpBar;
        [SerializeField] private Slider staminaBar;

        private bool started;

        private void Awake()
        {
            ConfigureBar(hpBar);
            ConfigureBar(staminaBar);
        }

        private void OnEnable()
        {
            if (player == null || hpBar == null || staminaBar == null)
            {
                Debug.LogError("[PlayerStatusUI] Assign Player, HP Bar and Stamina Bar in the Inspector.", this);
                return;
            }

            player.OnHealthChanged += UpdateHealth;
            player.OnStaminaChanged += UpdateStamina;
            if (started) Refresh();
        }

        private void Start()
        {
            // All active scene objects have completed Awake before this initial read.
            started = true;
            Refresh();
        }

        private void OnDisable()
        {
            if (player == null) return;
            player.OnHealthChanged -= UpdateHealth;
            player.OnStaminaChanged -= UpdateStamina;
        }

        private static void ConfigureBar(Slider bar)
        {
            if (bar == null) return;
            bar.interactable = false;
            bar.navigation = new Navigation { mode = Navigation.Mode.None };
            bar.minValue = 0f;
            bar.maxValue = 1f;
            bar.wholeNumbers = false;
        }

        private void Refresh()
        {
            if (player == null) return;
            UpdateHealth(player.CurrentHealth, player.MaxHealth);
            UpdateStamina(player.CurrentStamina, player.MaxStamina);
        }

        private void UpdateHealth(float current, float maximum)
        {
            SetBar(hpBar, current, maximum);
        }

        private void UpdateStamina(float current, float maximum)
        {
            SetBar(staminaBar, current, maximum);
        }

        private static void SetBar(Slider bar, float current, float maximum)
        {
            if (bar != null)
                bar.SetValueWithoutNotify(maximum > 0f ? Mathf.Clamp01(current / maximum) : 0f);
        }
    }
}
