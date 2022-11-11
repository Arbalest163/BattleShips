using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Common
{
    [RequireComponent(typeof(Canvas))]
    public class ResultPopup : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _text;

        [SerializeField]
        private Button _okButton;

        private TaskCompletionSource<bool> _taskCompletion;
        private Canvas _canvas;

        public static ResultPopup Instance { get; private set; }

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
            Instance = this;
            _okButton.onClick.AddListener(OnAccept);
        }

        public async Task<bool> AwaitForDecision(string text)
        {
            _text.text = text;
            _canvas.enabled = true;
            _taskCompletion = new TaskCompletionSource<bool>();
            var result = await _taskCompletion.Task;
            _canvas.enabled = false;
            return result;
        }

        private void OnAccept()
        {
            _taskCompletion.SetResult(true);
        }
    }
}