using System;
using LastOutpost.Core.Simulation;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    /// <summary>View binding only. Movement, attacks, health, and rewards belong to the bound model.</summary>
    public sealed class EnemyActor : MonoBehaviour
    {
        private Vector3 routeOrigin;
        public EnemyAgent Agent { get; private set; }

        public void Bind(EnemyAgent agent, Vector3 origin)
        {
            Agent = agent ?? throw new ArgumentNullException(nameof(agent));
            routeOrigin = origin;
            Synchronize();
        }

        public void Synchronize()
        {
            if (Agent != null) transform.position = routeOrigin + new Vector3((float)Agent.PositionX, 0, 0);
        }
    }
}
