using System.Collections;
using UnityEngine;

namespace THEBADDEST.GameDebugSystem
{
    public sealed class DebugCanvasFader : MonoBehaviour
    {
        private Coroutine _fadeCoroutine;

        public void Fade(CanvasGroup cg, bool show, float duration)
        {
            if (cg == null) return;
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(FadeRoutine(cg, show, duration));
        }

        private IEnumerator FadeRoutine(CanvasGroup cg, bool show, float duration)
        {
            if (show)
            {
                if (!cg.gameObject.activeSelf) cg.gameObject.SetActive(true);
                cg.blocksRaycasts = true;
                cg.interactable = true;
                float t = 0f;
                float start = cg.alpha;
                float end = 1f;
                while (t < duration)
                {
                    cg.alpha = Mathf.Lerp(start, end, t / duration);
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
                cg.alpha = end;
            }
            else
            {
                float t = 0f;
                float start = cg.alpha;
                float end = 0f;
                while (t < duration)
                {
                    cg.alpha = Mathf.Lerp(start, end, t / duration);
                    t += Time.unscaledDeltaTime;
                    yield return null;
                }
                cg.alpha = end;
                cg.blocksRaycasts = false;
                cg.interactable = false;
                cg.gameObject.SetActive(false);
            }
            _fadeCoroutine = null;
        }
    }
}