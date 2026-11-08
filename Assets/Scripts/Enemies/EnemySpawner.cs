using System;
using System.Collections.Generic;
using LastOutpost.Core.Simulation;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        [SerializeField] private EnemyActor basicPrefab;
        [SerializeField] private EnemyActor fastPrefab;
        [SerializeField] private EnemyActor bossPrefab;
        [SerializeField] private Transform actorRoot;
        [SerializeField, Tooltip("Must match PlatformMotor's world-space route origin.")] private Vector3 routeOrigin;
        private readonly Dictionary<int, EnemyActor> actors = new Dictionary<int, EnemyActor>();
        private GameRun boundRun;
        public int ActiveCount => actors.Count;

        private void OnEnable()
        {
            if (session == null) return;
            session.RunCreated += BindRun;
            BindRun(session.Run);
        }

        private void OnDisable() => Disconnect();

        public void Configure(OutpostSession owner, EnemyActor basic, EnemyActor fast, EnemyActor boss,
            Transform parent, Vector3 origin)
        {
            Disconnect();
            session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            basicPrefab = basic != null ? basic : throw new ArgumentNullException(nameof(basic));
            fastPrefab = fast != null ? fast : throw new ArgumentNullException(nameof(fast));
            bossPrefab = boss != null ? boss : throw new ArgumentNullException(nameof(boss));
            actorRoot = parent != null ? parent : transform;
            routeOrigin = origin;
            if (isActiveAndEnabled) OnEnable();
        }

        private void LateUpdate()
        {
            foreach (EnemyActor actor in actors.Values)
                if (actor != null) actor.Synchronize();
        }

        public EnemyActor FindActor(int enemyId) => actors.TryGetValue(enemyId, out EnemyActor actor) ? actor : null;

        private void Disconnect()
        {
            if (session != null) session.RunCreated -= BindRun;
            if (boundRun != null)
            {
                boundRun.EnemySpawned -= Spawn;
                boundRun.EnemyDefeated -= Despawn;
            }
            boundRun = null;
            ClearViews();
        }

        private void BindRun(GameRun run)
        {
            if (boundRun != null)
            {
                boundRun.EnemySpawned -= Spawn;
                boundRun.EnemyDefeated -= Despawn;
            }
            ClearViews();
            boundRun = run;
            if (run == null) return;
            run.EnemySpawned += Spawn;
            run.EnemyDefeated += Despawn;
            // Also handles a component enabled after the encounter has already started.
            for (int i = 0; i < run.Enemies.Count; i++)
                if (!run.Enemies[i].State.Health.IsDead) Spawn(run.Enemies[i]);
        }

        private void Spawn(EnemyAgent enemy)
        {
            if (actors.ContainsKey(enemy.State.Id)) return;
            EnemyActor prefab = enemy.State.Definition.IsBoss ? bossPrefab :
                enemy.State.Definition.Id == "fast" ? fastPrefab : basicPrefab;
            if (prefab == null) throw new InvalidOperationException("Assign all three Last Outpost enemy prefabs.");
            EnemyActor actor = Instantiate(prefab, actorRoot);
            actor.name = enemy.State.Definition.Id + "-" + enemy.State.Id;
            actor.Bind(enemy, routeOrigin);
            actor.gameObject.SetActive(true);
            actors.Add(enemy.State.Id, actor);
        }

        private void Despawn(EnemyAgent enemy)
        {
            if (!actors.TryGetValue(enemy.State.Id, out EnemyActor actor)) return;
            actors.Remove(enemy.State.Id);
            DestroyView(actor);
        }

        private void ClearViews()
        {
            foreach (EnemyActor actor in actors.Values) DestroyView(actor);
            actors.Clear();
        }

        private static void DestroyView(EnemyActor actor)
        {
            if (actor == null) return;
            actor.gameObject.SetActive(false);
            // Destruction is presentation lifecycle only: never award Energy here.
            if (Application.isPlaying) Destroy(actor.gameObject);
            else DestroyImmediate(actor.gameObject);
        }
    }
}
