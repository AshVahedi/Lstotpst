using System;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using UnityEngine;

namespace LastOutpost.Presentation
{
    public sealed class CombatFeedback : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        [SerializeField] private Material tracerMaterial;
        [SerializeField] private Transform effectRoot;
        [SerializeField] private Vector3 routeOrigin;
        [SerializeField, Min(0.01f), Tooltip("Unscaled seconds a resolved shot remains visible.")]
        private float tracerLifetimeSeconds = 0.12f;
        private GameRun boundRun;

        private void OnEnable()
        {
            if (session == null) return;
            session.RunCreated += BindRun;
            BindRun(session.Run);
        }

        private void OnDisable() => Disconnect();

        public void Configure(OutpostSession owner, Material material, Transform parent, Vector3 origin)
        {
            Disconnect();
            session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            tracerMaterial = material;
            effectRoot = parent != null ? parent : transform;
            routeOrigin = origin;
            if (isActiveAndEnabled) OnEnable();
        }

        private void Disconnect()
        {
            if (session != null) session.RunCreated -= BindRun;
            if (boundRun != null) boundRun.ShotFired -= ShowShot;
            boundRun = null;
        }

        private void BindRun(GameRun run)
        {
            if (boundRun != null) boundRun.ShotFired -= ShowShot;
            boundRun = run;
            if (run != null) run.ShotFired += ShowShot;
            if (effectRoot == null) return;
            for (int i = effectRoot.childCount - 1; i >= 0; i--)
            {
                GameObject effect = effectRoot.GetChild(i).gameObject;
                effect.SetActive(false);
                if (Application.isPlaying) Destroy(effect);
                else DestroyImmediate(effect);
            }
        }

        private void ShowShot(ShotEvent shot)
        {
            if (tracerMaterial == null) return;
            Color color = shot.Kind == ShotKind.Enemy ? new Color(1, 0.35f, 0.25f) :
                shot.Kind == ShotKind.Special ? Color.magenta : shot.Kind == ShotKind.Missile ? Color.yellow : Color.cyan;
            var effect = new GameObject("Resolved " + shot.Kind);
            effect.transform.SetParent(effectRoot, false);
            var trace = effect.AddComponent<ShotTrace>();
            trace.Initialize(routeOrigin + new Vector3((float)shot.FromX, 0.25f, 0),
                routeOrigin + new Vector3((float)shot.ToX, 0.25f, 0), tracerMaterial, color, tracerLifetimeSeconds);
        }
    }
}
