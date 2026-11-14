using System;
using LastOutpost.Core.Simulation;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    /// <summary>View binding only. Movement, attacks, health, and rewards belong to the bound model.</summary>
    public sealed class EnemyActor : MonoBehaviour
    {
        private Vector3 routeOrigin;
        private RouteOrientation orientation;
        public EnemyAgent Agent { get; private set; }

        public void Bind(EnemyAgent agent, Vector3 origin, RouteOrientation routeOrientation = RouteOrientation.LegacyHorizontal)
        {
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));
            routeOrigin = origin;
            orientation = routeOrientation;
            Synchronize();
        }

        public void Synchronize()
        {
            if (Agent != null) transform.position = RouteProjection.Position(routeOrigin, Agent.PositionX, orientation);
        }
    }
}
