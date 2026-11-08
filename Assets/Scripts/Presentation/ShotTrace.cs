using UnityEngine;

namespace LastOutpost.Presentation
{
    /// <summary>A visual-only hitscan tracer. No collider, impact callback, damage, or gameplay timer.</summary>
    public sealed class ShotTrace : MonoBehaviour
    {
        private float secondsRemaining;

        public void Initialize(Vector3 from, Vector3 to, Material material, Color color, float lifetimeSeconds)
        {
            var line = gameObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, from);
            line.SetPosition(1, to);
            line.startWidth = line.endWidth = 0.045f;
            line.startColor = line.endColor = color;
            line.sortingOrder = 20;
            secondsRemaining = lifetimeSeconds;
        }

        private void Update()
        {
            secondsRemaining -= Time.unscaledDeltaTime;
            if (secondsRemaining <= 0) Destroy(gameObject);
        }
    }
}
