using System;
using System.Collections.Generic;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Modules;
using LastOutpost.Gameplay;
using UnityEngine;

namespace LastOutpost.Presentation
{
    /// <summary>Pooled projectile and muzzle views only. No collider, damage callback or second simulation clock.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class SpaceCombatView : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Material material;
        private SpaceCombatRun bound;
        private readonly List<SpriteRenderer> projectiles = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> muzzles = new List<SpriteRenderer>();
        public int VisibleProjectileCount { get; private set; }

        public void Configure(OutpostSession owner, Camera camera, Sprite placeholder, Material sharedMaterial)
        {
            session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            gameplayCamera = camera != null ? camera : throw new ArgumentNullException(nameof(camera));
            sprite = placeholder; material = sharedMaterial;
        }
        private SpriteRenderer Make(string label, Vector2 size)
        {
            var item = new GameObject(label); item.transform.SetParent(transform, false);
            item.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = item.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sharedMaterial = material;
            renderer.sortingOrder = 12; return renderer;
        }
        private static Vector3 Position(ShipPoint p) => new Vector3((float)p.X, (float)p.Y, 0);
        private static Color ColorFor(string id) => id == "laser" ? Color.cyan : id == "missile" ? Color.yellow : new Color(1, 0.55f, 0.2f);
        private void LateUpdate() => Synchronize();
        public void Synchronize()
        {
            SpaceCombatRun run = session != null ? session.Run?.Space : null;
            if (bound != run)
            {
                bound = run;
                // Reuse the existing pool across restarts; only grow to the configured bounded capacity.
                foreach (var renderer in projectiles) renderer.enabled = false;
                foreach (var renderer in muzzles) renderer.enabled = false;
            }
            VisibleProjectileCount = 0;
            if (run == null) return;
            if (gameplayCamera != null) run.SetViewport(gameplayCamera.orthographicSize * gameplayCamera.aspect, gameplayCamera.orthographicSize);
            for (int i = 0; i < run.Projectiles.Count; i++)
            {
                if (i == projectiles.Count) projectiles.Add(Make("Space projectile " + i, new Vector2(0.14f, 0.24f)));
                SpaceProjectile projectile = run.Projectiles[i]; SpriteRenderer renderer = projectiles[i];
                renderer.enabled = projectile.Active;
                if (!projectile.Active) continue;
                VisibleProjectileCount++; renderer.color = projectile.Hostile ? Color.red : ColorFor(projectile.WeaponId);
                renderer.transform.position = Position(projectile.Position);
                renderer.transform.rotation = Quaternion.Euler(0, 0, (float)(Math.Atan2(projectile.Velocity.Y, projectile.Velocity.X) * 180 / Math.PI - 90));
            }
            for (int i = 0; i < run.Weapons.Count; i++)
            {
                if (i == muzzles.Count) muzzles.Add(Make("Mounted space weapon " + i, new Vector2(0.14f, 0.5f)));
                var weapon = run.Weapons[i]; var renderer = muzzles[i]; renderer.enabled = weapon.Enabled;
                renderer.color = ColorFor(weapon.Definition.Id);
                renderer.transform.position = Position(weapon.Origin(run.ShipPosition, run.Forward));
                renderer.transform.rotation = Quaternion.Euler(0, 0, (float)(Math.Atan2(weapon.LocalFacing.Y, weapon.LocalFacing.X) * 180 / Math.PI - 90));
            }
            for (int i = run.Weapons.Count; i < muzzles.Count; i++) muzzles[i].enabled = false;
        }
        private void OnDisable()
        {
            foreach (var renderer in projectiles) if (renderer != null) renderer.enabled = false;
            foreach (var renderer in muzzles) if (renderer != null) renderer.enabled = false;
            bound = null;
        }
    }
}
