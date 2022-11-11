using System.Collections;
using TMPro;
using UnityEngine;

public class GameHint : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _hint;
    [SerializeField]
    private float _delay;
    [SerializeField]
    private float _time;

    private bool _inProcess;

    private void Awake()
    {
        _hint.alpha = 0;
    }

    public void TryShow()
    {
        if (_inProcess)
            return;

        StartCoroutine(ShowAndHide());
    }

    private IEnumerator ShowAndHide()
    {
        _inProcess = true;
        _hint.alpha = 0;

        var t = _time;
        while (t > 0)
        {
            _hint.alpha += Time.deltaTime / _time;
            t -= Time.deltaTime;
            yield return null;
        }

        _hint.alpha = 1;
        yield return new WaitForSeconds(_delay);

        t = _time;
        while (t > 0)
        {
            _hint.alpha -= Time.deltaTime / _time;
            t -= Time.deltaTime;
            yield return null;
        }

        _hint.alpha = 0;
        _inProcess = false;
    }
}
