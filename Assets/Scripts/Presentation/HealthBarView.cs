using LastOutpost.Core;
using LastOutpost.Gameplay;
using UnityEngine;

namespace LastOutpost.Presentation
{
    public sealed class HealthBarView : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        [SerializeField] private EnemyActor enemy;
        [SerializeField] private Transform fill;
        [SerializeField] private float fullWidth = 1;

        public void Configure(OutpostSession owner, EnemyActor actor, Transform fillTransform, float width)
        {
            session = owner;
            enemy = actor;
            fill = fillTransform;
            fullWidth = width;
        }

        private void LateUpdate()
        {
            Health health = enemy != null ? enemy.Agent?.State.Health : session != null ? session.Run?.Platform.Health : null;
            if (health == null || fill == null) return;
            float fraction = (float)health.Current / health.Maximum;
            Vector3 scale = fill.localScale;
            scale.x = fullWidth * fraction;
            fill.localScale = scale;
            Vector3 position = fill.localPosition;
            position.x = -fullWidth * (1 - fraction) / 2;
            fill.localPosition = position;
        }
    }
}
