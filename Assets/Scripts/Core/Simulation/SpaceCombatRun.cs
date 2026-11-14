using System;
using System.Collections.Generic;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Platform;

namespace LastOutpost.Core.Combat
{
    public sealed class SpaceEnemy
    {
        public Simulation.EnemyAgent Agent { get; }
        public ShipPoint Position { get; internal set; }
        public ShipPoint PreviousPosition { get; internal set; }
        public ArrivalSide Side { get; internal set; }
        public bool Active { get; internal set; }
        public bool Targetable { get; internal set; } = true;
        public bool Escaped { get; internal set; }
        internal double Age, NextAttack;
        public double Radius => Agent.State.Definition.IsBoss ? 0.75 : 0.38;
        public SpaceEnemy(Simulation.EnemyAgent agent, ShipPoint position, ArrivalSide side = ArrivalSide.Front, bool active = true)
        {
            Agent = agent ?? throw new ArgumentNullException(nameof(agent)); SpaceGeometry.Validate(position);
            Position = PreviousPosition = position; Side = side; Active = active;
        }
    }

    /// <summary>Reusable core projectile slot. Hostile and friendly collisions are mutually exclusive.</summary>
    public sealed class SpaceProjectile
    {
        public bool Active { get; internal set; }
        public bool Hostile { get; internal set; }
        public long Serial { get; internal set; }
        public string WeaponId { get; internal set; }
        public ShipPoint Origin { get; internal set; }
        public ShipPoint Position { get; internal set; }
        public ShipPoint Velocity { get; internal set; }
        public int TargetId { get; internal set; }
        internal int Damage;
        internal double RemainingLife;
        public const double Radius = 0.07;
    }

    /// <summary>
    /// Space-only combat adapter over the existing platform health, immutable gear and wave roster.
    /// Unity is a view: only this simulation applies projectile/contact damage.
    /// </summary>
    public sealed class SpaceCombatRun
    {
        private readonly SpaceCombatConfiguration config;
        private readonly PlatformState platform;
        private readonly List<SpaceWeaponInstance> weapons = new List<SpaceWeaponInstance>();
        private readonly List<SpaceEnemy> enemies = new List<SpaceEnemy>();
        private readonly SpaceProjectile[] projectiles;
        private readonly Func<SpaceWeaponInstance, SpaceEnemy, bool> emitWeapon;
        private readonly IReadOnlyList<ShipHitBox> hull;
        private double halfWidth, halfHeight, nextSpawn;
        private int pendingIndex, spawnSequence;
        private long serial;
        public IReadOnlyList<SpaceWeaponInstance> Weapons { get; }
        public IReadOnlyList<SpaceEnemy> Enemies { get; }
        public IReadOnlyList<SpaceProjectile> Projectiles { get; }
        public IReadOnlyList<ShipHitBox> Hull => hull;
        public ShipPoint ShipPosition { get; private set; }
        public ShipPoint Forward => new ShipPoint(0, 1);
        public double HalfWidth => halfWidth;
        public double HalfHeight => halfHeight;
        public int SpawnCount { get; private set; }
        public int ContactCount { get; private set; }
        public int FriendlyHits { get; private set; }
        public int HostileHits { get; private set; }
        public int ExpiredEnemies { get; private set; }
        public event Action<SpaceProjectile> ProjectileLaunched;
        public event Action<SpaceEnemy> EnemyArrived;

        public SpaceCombatRun(SpaceCombatConfiguration configuration, PlatformState platform, RunLoadout loadout)
        {
            config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.platform = platform ?? throw new ArgumentNullException(nameof(platform));
            var occupied = new List<string>();
            AddEmitters("core", config.HullLaser, new ShipPoint(0, 0.8));
            if (loadout != null)
            {
                foreach (var item in loadout.Installations) AddInstallation(item.SlotId, item.Definition.Id,
                    item.Definition.Mechanism == ModuleKind.BasicGun || item.Definition.Mechanism == ModuleKind.MissileLauncher, occupied);
            }
            else
            {
                foreach (var item in EquipmentConfiguration.Prototype().CreateInitialProfile().Installations)
                    AddInstallation(item.SlotId, item.EquipmentId, item.SlotId == "primary" || item.SlotId == "secondary", occupied);
            }
            hull = ShipHitBox.Build(config.Layout, occupied);
            Weapons = weapons.AsReadOnly(); Enemies = enemies.AsReadOnly();
            projectiles = new SpaceProjectile[config.MaximumProjectiles];
            for (int i = 0; i < projectiles.Length; i++) projectiles[i] = new SpaceProjectile();
            Projectiles = Array.AsReadOnly(projectiles);
            halfWidth = config.ReferenceHalfWidth; halfHeight = config.ReferenceHalfHeight;
            emitWeapon = EmitWeapon;
        }

        private void AddInstallation(string slot, string equipmentId, bool weapon, List<string> occupied)
        {
            ShipPoint position = config.Layout.Position(slot); occupied.Add(slot);
            if (!config.EquipmentWeapons.TryGetValue(equipmentId, out var definition))
            { if (weapon) throw new ArgumentException("No space weapon definition for installed item: " + equipmentId); return; }
            AddEmitters(slot, definition, position);
        }

        private void AddEmitters(string slot, SpaceWeaponDefinition definition, ShipPoint position)
        {
            // A twin/triple battery reuses one owned module/definition, but each physical emitter
            // has its own clock. Offsets remain inside its existing 0.9-unit module footprint.
            for (int i = 0; i < definition.Instances; i++)
            {
                string id = slot + "/" + definition.Id + (definition.Instances == 1 ? "" : "/" + i);
                var offset = new ShipPoint(position.X, position.Y + (i - (definition.Instances - 1) / 2d) * 0.25);
                weapons.Add(new SpaceWeaponInstance(id, slot, definition, offset));
            }
        }

        /// <summary>Actual orthographic viewport, independent of Editor Scene view. Changes affect future spawns only.</summary>
        public void SetViewport(double width, double height)
        {
            Guard.PositiveFinite(width, nameof(width)); Guard.PositiveFinite(height, nameof(height));
            if (width <= config.Layout.MaximumHalfExtent || height <= 1.2) throw new ArgumentException("Viewport must leave room around the ship.");
            halfWidth = width; halfHeight = height;
        }
        public bool RemoveWeapon(string instanceId)
        {
            // Run equipment remains immutable. Destruction removes only this transient emitter.
            int index = weapons.FindIndex(w => w.InstanceId == instanceId);
            if (index < 0) return false;
            weapons[index].Enabled = false; weapons.RemoveAt(index); return true;
        }
        public SpaceEnemy FindEnemy(int id) => enemies.Find(e => e.Agent.State.Id == id);
        public void AdvanceTravel(double routeDistance) => ShipPosition = ShipPoint.AlongRoute(routeDistance);

        public void BeginEncounter(IReadOnlyList<Simulation.EnemyAgent> roster, double now, double routeDistance)
        {
            ClearProjectiles(); enemies.Clear(); pendingIndex = 0; nextSpawn = now;
            ShipPosition = ShipPoint.AlongRoute(routeDistance);
            foreach (var enemy in roster) enemies.Add(new SpaceEnemy(enemy, ShipPosition, active: false) { Targetable = false });
            SpawnPending(now);
        }

        private void SpawnPending(double now)
        {
            int living = 0;
            foreach (var enemy in enemies) if (enemy.Active && !enemy.Agent.State.Health.IsDead) living++;
            while (pendingIndex < enemies.Count && living < config.MaximumEnemies && now + 1e-12 >= nextSpawn)
            {
                SpaceEnemy enemy = enemies[pendingIndex++];
                int index = (int)((uint)config.Seed % (uint)config.SpawnDistribution.Count + (uint)spawnSequence) % config.SpawnDistribution.Count;
                enemy.Side = enemy.Agent.State.Definition.IsBoss ? ArrivalSide.Front : config.SpawnDistribution[index];
                // Integer hash gives injectable deterministic jitter without global Random state.
                uint hash = unchecked((uint)(config.Seed + spawnSequence * 7919) * 1664525u + 1013904223u);
                double lateral = ((hash % 10001) / 10000d - 0.5) * 1.2;
                double x = lateral * halfWidth * 0.6, y = lateral * halfHeight * 0.6;
                double safeX = Math.Max(halfWidth, config.Layout.MaximumHalfExtent) + config.SpawnMargin + enemy.Radius;
                double safeY = halfHeight + config.SpawnMargin + enemy.Radius;
                switch (enemy.Side)
                {
                    case ArrivalSide.Front: y = safeY; if (enemy.Agent.State.Definition.IsBoss) x = 0; break;
                    case ArrivalSide.Left: x = -safeX; break;
                    case ArrivalSide.Right: x = safeX; break;
                    case ArrivalSide.Rear: y = -safeY; break;
                }
                enemy.Position = enemy.PreviousPosition = SpaceGeometry.Add(ShipPosition, new ShipPoint(x, y));
                enemy.Active = true; enemy.NextAttack = now; enemy.Age = 0;
                nextSpawn = now + config.SpawnInterval; living++; spawnSequence++; SpawnCount++;
                EnemyArrived?.Invoke(enemy);
            }
        }

        public void Tick(double dt, double now, double routeDistance)
        {
            Guard.NonNegativeFinite(dt, nameof(dt)); Guard.NonNegativeFinite(now, nameof(now));
            if (dt == 0) return;
            ShipPoint previousShip = ShipPosition; ShipPosition = ShipPoint.AlongRoute(routeDistance);
            SpawnPending(now);
            foreach (var enemy in enemies)
            {
                if (!enemy.Active || enemy.Agent.State.Health.IsDead) continue;
                enemy.Age += dt; enemy.PreviousPosition = enemy.Position;
                if (!enemy.Agent.State.Definition.IsBoss && enemy.Age > config.EnemyLifetime)
                { enemy.Escaped = true; enemy.Agent.State.Health.TakeDamage(enemy.Agent.State.Health.Current); ExpiredEnemies++; continue; }
                ShipPoint delta = SpaceGeometry.Subtract(ShipPosition, enemy.Position);
                double distance = Math.Sqrt(SpaceGeometry.LengthSquared(delta));
                double stop = enemy.Agent.State.Definition.IsBoss ? enemy.Agent.State.Definition.Attack.Range * 0.8 : 0;
                double step = Math.Min(Math.Max(0, distance - stop), enemy.Agent.State.Definition.MovementSpeed * dt);
                if (distance > 0) enemy.Position = SpaceGeometry.Add(enemy.Position, SpaceGeometry.Scale(delta, step / distance));
                ShipPoint relative = SpaceGeometry.Subtract(enemy.Position, ShipPosition);
                // Acquiring offscreen arrivals made long-range side batteries erase threats
                // before the player could see them. Range/arc checks still apply after entry.
                enemy.Targetable = Math.Abs(relative.X) <= halfWidth - enemy.Radius && Math.Abs(relative.Y) <= halfHeight - enemy.Radius;
                if (!enemy.Agent.State.Definition.IsBoss)
                {
                    if (ShipHit(SpaceGeometry.Subtract(enemy.PreviousPosition, previousShip),
                        SpaceGeometry.Subtract(enemy.Position, ShipPosition), enemy.Radius) <= 1)
                    {
                        // One aggregated impact, even if the sweep intersects multiple module/rail boxes.
                        platform.Health.TakeDamage(enemy.Agent.State.Definition.Attack.Damage); ContactCount++;
                        enemy.Agent.State.Health.TakeDamage(enemy.Agent.State.Health.Current);
                    }
                }
                else if (enemy.Targetable && !platform.Health.IsDead && distance <= enemy.Agent.State.Definition.Attack.Range + 0.01 && now + 1e-12 >= enemy.NextAttack)
                {
                    if (Launch(true, "boss-cannon", enemy.Position, ShipPosition, enemy.Agent.State.Definition.Attack.Damage,
                        config.EnemyProjectileSpeed, Math.Max(halfHeight, halfWidth) * 3, 0))
                        enemy.NextAttack = now + enemy.Agent.State.Definition.Attack.AttackIntervalSeconds;
                }
            }
            if (!platform.Health.IsDead)
                foreach (var weapon in weapons) weapon.TryFire(now, ShipPosition, Forward, Enemies, emitWeapon);
            AdvanceProjectiles(dt, previousShip);
        }

        private bool EmitWeapon(SpaceWeaponInstance weapon, SpaceEnemy target) => Launch(false, weapon.Definition.Id,
            weapon.Origin(ShipPosition, Forward), target.Position, weapon.Definition.Damage,
            weapon.Definition.ProjectileSpeed, weapon.Definition.Range, target.Agent.State.Id);

        private bool Launch(bool hostile, string weaponId, ShipPoint origin, ShipPoint target, int damage, double speed, double range, int targetId)
        {
            SpaceProjectile slot = Array.Find(projectiles, p => !p.Active);
            if (slot == null) return false;
            ShipPoint delta = SpaceGeometry.Subtract(target, origin);
            ShipPoint direction = SpaceGeometry.LengthSquared(delta) == 0 ? Forward : SpaceGeometry.Unit(delta);
            slot.Hostile = hostile; slot.Serial = ++serial; slot.WeaponId = weaponId;
            slot.Origin = slot.Position = origin; slot.Velocity = SpaceGeometry.Scale(direction, speed);
            slot.Damage = damage; slot.TargetId = targetId; slot.RemainingLife = range / speed; slot.Active = true;
            ProjectileLaunched?.Invoke(slot); return true;
        }

        private double ShipHit(ShipPoint from, ShipPoint to, double radius)
        {
            double first = double.PositiveInfinity;
            foreach (var box in hull) first = Math.Min(first, box.Hit(from, to, radius));
            return first;
        }
        private void AdvanceProjectiles(double dt, ShipPoint previousShip)
        {
            foreach (var projectile in projectiles)
            {
                if (!projectile.Active) continue;
                double step = Math.Min(dt, projectile.RemainingLife);
                ShipPoint before = projectile.Position;
                ShipPoint after = SpaceGeometry.Add(before, SpaceGeometry.Scale(projectile.Velocity, step));
                if (projectile.Hostile)
                {
                    if (ShipHit(SpaceGeometry.Subtract(before, previousShip), SpaceGeometry.Subtract(after, ShipPosition), SpaceProjectile.Radius) <= 1)
                    { platform.Health.TakeDamage(projectile.Damage); HostileHits++; projectile.Active = false; }
                }
                else
                {
                    SpaceEnemy hit = null; double first = double.PositiveInfinity;
                    foreach (var enemy in enemies)
                    {
                        if (!enemy.Active || enemy.Agent.State.Health.IsDead) continue;
                        // Relative sweep detects moving targets and very fast projectiles without tunnelling.
                        double t = SpaceGeometry.CircleHit(SpaceGeometry.Subtract(before, enemy.PreviousPosition),
                            SpaceGeometry.Subtract(after, enemy.Position), new ShipPoint(0, 0), enemy.Radius + SpaceProjectile.Radius);
                        if (t < first || (t == first && t <= 1 && (hit == null || enemy.Agent.State.Id < hit.Agent.State.Id)))
                        { first = t; hit = enemy; }
                    }
                    if (hit != null && first <= 1)
                    { hit.Agent.State.Health.TakeDamage(projectile.Damage); FriendlyHits++; projectile.Active = false; }
                }
                projectile.Position = after; projectile.RemainingLife -= step;
                if (projectile.RemainingLife <= 0) projectile.Active = false;
            }
        }

        public void ClearProjectiles() { foreach (var projectile in projectiles) projectile.Active = false; }
    }
}
