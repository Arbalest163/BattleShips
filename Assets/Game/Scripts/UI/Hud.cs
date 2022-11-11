using System;
using Common;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class Hud : MonoBehaviour
    {
        [SerializeField]
        private Button _quitButton;
        public event Action QuitGame;

        [SerializeField]
        private Button _newGameButton;
        public event Action NewGame;

        private void Awake()
        {
            _quitButton.onClick.AddListener(OnQuitButtonClicked);
            _newGameButton?.onClick.AddListener(OnNewGameButtonClicked);
        }

        private async void OnQuitButtonClicked()
        {
            var isConfirmed = await AlertPopup.Instance.AwaitForDecision("Вы уверены, что хотите выйти?");
            if (isConfirmed)
                QuitGame?.Invoke();
        }

        private void OnNewGameButtonClicked()
        {
            NewGame?.Invoke();
        }

    }
}