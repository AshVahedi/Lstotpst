using System;
using LastOutpost.Core.Simulation;
using LastOutpost.Core.Combat;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    /// <summary>View binding only. Movement, attacks, health, and rewards belong to the bound model.</summary>
    public sealed class EnemyActor : MonoBehaviour
    {
        private Vector3 routeOrigin;
        private RouteOrientation orientation;
        private SpaceEnemy spaceEnemy;
        public EnemyAgent Agent { get; private set; }

        public void Bind(EnemyAgent agent, Vector3 origin, RouteOrientation routeOrientation = RouteOrientation.LegacyHorizontal,
            SpaceEnemy space = null)
        {
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));
            routeOrigin = origin;
            orientation = routeOrientation;
            spaceEnemy = space;
            Synchronize();
        }

        public void Synchronize()
        {
            if (spaceEnemy != null)
            {
                transform.position = routeOrigin + new Vector3((float)spaceEnemy.Position.X, (float)spaceEnemy.Position.Y, 0);
                gameObject.SetActive(spaceEnemy.Active && !Agent.State.Health.IsDead);
            }
            else if (Agent != null) transform.position = RouteProjection.Position(routeOrigin, Agent.PositionX, orientation);
        }
    }
}
