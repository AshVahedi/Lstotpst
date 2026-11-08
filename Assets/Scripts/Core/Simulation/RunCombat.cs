using System;
using System.Collections.Generic;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Platform;

namespace LastOutpost.Core.Simulation
{
    public enum ShotKind { BasicGun, Missile, Enemy, Special }

    /// <summary>Presentation notification of damage already resolved by the core; never an impact command.</summary>
    public readonly struct ShotEvent
    {
        public ShotKind Kind { get; }
        public double FromX { get; }
        public double ToX { get; }
        /// <summary>Zero denotes the platform; enemy IDs are positive.</summary>
        public int TargetEnemyId { get; }

        public ShotEvent(ShotKind kind, double fromX, double toX, int targetEnemyId)
        {
            Kind = kind;
            FromX = fromX;
            ToX = toX;
            TargetEnemyId = targetEnemyId;
        }
    }

    public sealed class RunCombat
    {
        private readonly PlatformState platform;
        private readonly Weapon basicGun;
        private readonly Weapon missileLauncher;
        private readonly List<Target> targets = new List<Target>(8);
        private readonly Target[] platformTarget = new Target[1];
        public event Action<ShotEvent> ShotFired;

        public RunCombat(PlatformState platform, RunConfiguration config)
        {
            this.platform = platform ?? throw new ArgumentNullException(nameof(platform));
            if (config == null) throw new ArgumentNullException(nameof(config));
            basicGun = new Weapon(config.BasicGun);
            missileLauncher = new Weapon(config.MissileLauncher, platform.AdvancedAmmunition);
        }

        public IReadOnlyList<Target> BuildTargets(IReadOnlyList<EnemyAgent> enemies, double platformX)
        {
            targets.Clear();
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].State.Health.IsDead)
                    targets.Add(new Target(enemies[i].State.Id, enemies[i].State.Health,
                        Math.Abs(enemies[i].PositionX - platformX)));
            return targets;
        }

        public void Tick(IReadOnlyList<EnemyAgent> enemies, double platformX, double elapsedSeconds)
        {
            if (platform.Health.IsDead) return;
            BuildTargets(enemies, platformX);
            Fire(basicGun, ShotKind.BasicGun, enemies, platformX, elapsedSeconds);
            if (HasMissileModule()) Fire(missileLauncher, ShotKind.Missile, enemies, platformX, elapsedSeconds);
            for (int i = 0; i < enemies.Count && !platform.Health.IsDead; i++)
            {
                EnemyAgent enemy = enemies[i];
                if (enemy.State.Health.IsDead) continue;
                platformTarget[0] = new Target(1, platform.Health, Math.Abs(enemy.PositionX - platformX));
                if (enemy.State.Attack.TryFire(platformTarget, elapsedSeconds, out _))
                    ShotFired?.Invoke(new ShotEvent(ShotKind.Enemy, enemy.PositionX, platformX, 0));
            }
        }

        private void Fire(Weapon weapon, ShotKind kind, IReadOnlyList<EnemyAgent> enemies,
            double platformX, double elapsedSeconds)
        {
            if (!weapon.TryFire(targets, elapsedSeconds, out Target hit)) return;
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i].State.Id == hit.Id)
                {
                    ShotFired?.Invoke(new ShotEvent(kind, platformX, enemies[i].PositionX, hit.Id));
                    return;
                }
        }

        private bool HasMissileModule()
        {
            for (int i = 0; i < platform.Modules.Capacity; i++)
                if (platform.Modules.GetAt(i)?.Kind == ModuleKind.MissileLauncher) return true;
            return false;
        }
    }
}
