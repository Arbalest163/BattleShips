using System.Collections.Generic;
using Loading;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField]
        private Button _gameBtn;
        [SerializeField]
        private Button _aboutAuthorBtn;
        [SerializeField]
        private AboutAuthorMenu _authorMenu;

        private void Start()
        {
            _gameBtn.onClick.AddListener(OnQuickGameBtnClicked);
            _aboutAuthorBtn.onClick.AddListener(OnEditorBtnClicked);
        }

        private async void OnQuickGameBtnClicked()
        {
            var operations = new Queue<ILoadingOperation>();
            operations.Enqueue(new PrepareGameLoadingOperation());
            await ProjectContext.Instance.LoadingScreenProvider.LoadAndDestroy(operations);
        }

        private void OnEditorBtnClicked()
        {
            _authorMenu.Show();
        }
    }
}