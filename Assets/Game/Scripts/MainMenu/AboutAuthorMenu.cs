using UnityEngine;
using UnityEngine.UI;

namespace MainMenu
{
    public class AboutAuthorMenu : MonoBehaviour
    {
        [SerializeField]
        private Canvas _canvas;
        [SerializeField]
        private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseBtnClicked);
        }

        public void Show()
        {
            _canvas.enabled = true;
        }

        private void OnCloseBtnClicked()
        {
            _canvas.enabled = false;
        }
    }
}
