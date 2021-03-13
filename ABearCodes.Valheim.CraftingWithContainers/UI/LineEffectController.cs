using UnityEngine;

namespace ABearCodes.Valheim.CraftingWithContainers.UI
{
    public static class LineEffectCreator
    {
        public static GameObject Create(Vector3 spawn, Transform target, float startWidth, float endWidth,
            float maxAlpha,
            float lifetime)
        {
            var go = new GameObject("Line");
            go.SetActive(false);
            Object.Destroy(go, lifetime);

            go.transform.position = spawn + Vector3.up * 0.35f;
            var targetPosition = target.position + Vector3.up;

            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.enabled = false;
            lineRenderer.numCornerVertices = 5;
            lineRenderer.numCapVertices = 5;
            lineRenderer.positionCount = 3;
            lineRenderer.SetPositions(new[]
            {
                go.transform.position,
                (go.transform.position + targetPosition) / 2,
                targetPosition
            });
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
            lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
            var gradient = new Gradient();

            gradient.colorKeys = new[]
            {
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 0.5f),
                new GradientColorKey(Color.white, 1)
            };
            gradient.alphaKeys = new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(maxAlpha, .5f),
                new GradientAlphaKey(0f, 1f)
            };
            lineRenderer.colorGradient = gradient;

            var lineEffectController = go.AddComponent<LineEffectController>();
            lineEffectController.Lifetime = lifetime;
            lineEffectController.Target = target;
            lineRenderer.enabled = true;
            go.SetActive(true);
            return go;
        }
    }

    public class LineEffectController : MonoBehaviour
    {
        public float Lifetime;
        public Transform Target;
        private LineRenderer _lineRenderer;
        private Vector3 _startPosition;
        private float _t;

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _startPosition = transform.position;
        }

        private void FixedUpdate()
        {
            _t += Time.fixedDeltaTime / Lifetime;
            var targetPosition = Target.position + Vector3.up;
            var newPos = Vector3.Lerp(_startPosition, targetPosition, _t);
            _lineRenderer.SetPositions(new[]
            {
                newPos,
                (newPos + targetPosition) / 2,
                targetPosition
            });
        }
    }
}