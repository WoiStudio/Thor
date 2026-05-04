using System.Collections;
using UnityEngine;

namespace Woi.Ninja.Player.Services
{
    /// <summary>
    /// Local-space pivot rotation for capsule prototype swings.
    /// </summary>
    public sealed class PlayerSwordFeedback : MonoBehaviour, IPlayerSwordFeedback
    {
        [SerializeField] private Transform _swordPivot;

        [SerializeField] [Min(0.01f)] private float _swingDuration = 0.18f;

        [SerializeField] private Vector3 _attack1StartEuler = new Vector3(-55f, 0f, 25f);

        [SerializeField] private Vector3 _attack1EndEuler = new Vector3(40f, 0f, -15f);

        [SerializeField] private Vector3 _attack2StartEuler = new Vector3(50f, 0f, -20f);

        [SerializeField] private Vector3 _attack2EndEuler = new Vector3(-45f, 0f, 20f);

        [SerializeField] private Vector3 _attack3StartEuler = new Vector3(-60f, 0f, 0f);

        [SerializeField] private Vector3 _attack3EndEuler = new Vector3(55f, 0f, 10f);

        private Coroutine _swingRoutine;

        public void PlaySwing(int comboIndex)
        {
            if (_swordPivot == null)
                return;

            if (_swingRoutine != null)
                StopCoroutine(_swingRoutine);

            _swingRoutine = StartCoroutine(SwingRoutine(comboIndex));
        }

        private IEnumerator SwingRoutine(int comboIndex)
        {
            if (!TryGetSwingAngles(comboIndex, out var startEuler, out var endEuler))
            {
                _swingRoutine = null;
                yield break;
            }

            var start = Quaternion.Euler(startEuler);
            var end = Quaternion.Euler(endEuler);
            float duration = Mathf.Max(0.01f, _swingDuration);
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                float u = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
                _swordPivot.localRotation = Quaternion.Slerp(start, end, u);
                yield return null;
            }

            _swordPivot.localRotation = Quaternion.identity;
            _swingRoutine = null;
        }

        private bool TryGetSwingAngles(int comboIndex, out Vector3 startEuler, out Vector3 endEuler)
        {
            switch (comboIndex)
            {
                case 1:
                    startEuler = _attack1StartEuler;
                    endEuler = _attack1EndEuler;
                    return true;
                case 2:
                    startEuler = _attack2StartEuler;
                    endEuler = _attack2EndEuler;
                    return true;
                case 3:
                    startEuler = _attack3StartEuler;
                    endEuler = _attack3EndEuler;
                    return true;
                default:
                    startEuler = endEuler = Vector3.zero;
                    return false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _swingDuration = Mathf.Max(0.01f, _swingDuration);
        }
#endif
    }
}
