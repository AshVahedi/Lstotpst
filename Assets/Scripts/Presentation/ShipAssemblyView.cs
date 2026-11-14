using System;
using System.Collections.Generic;
using System.Linq;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Progression;
using LastOutpost.Gameplay;
using UnityEngine;

namespace LastOutpost.Presentation
{
    /// <summary>Read-only equipment projection. Does not own, purchase or persist any item.</summary>
    public sealed class ShipAssemblyView : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        [SerializeField] private SpaceShipSettings settings;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Material material;
        private HorizontalShipLayout layout;
        private ProgressionMenuBinding binding;
        private readonly Dictionary<string, Transform> mounts = new Dictionary<string, Transform>(StringComparer.Ordinal);
        private readonly Dictionary<string, SpriteRenderer> modules = new Dictionary<string, SpriteRenderer>(StringComparer.Ordinal);
        [SerializeField, HideInInspector] private Transform ownedRoot;
        private Transform rail;
        public ShipBounds Bounds { get; private set; }
        public string ValidationError { get; private set; }
        public int InstalledModuleCount => modules.Values.Count(renderer => renderer.enabled);
        public Transform GetMount(string slotId) => mounts.TryGetValue(slotId ?? "", out var mount) ? mount : null;
        public bool IsInstalled(string slotId) => modules.TryGetValue(slotId ?? "", out var renderer) && renderer.enabled;

        public void Configure(OutpostSession owner, SpaceShipSettings configuration, Sprite placeholder, Material sharedMaterial)
        {
            OnDisable();
            session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            settings = configuration != null ? configuration : throw new ArgumentNullException(nameof(configuration));
            sprite = placeholder != null ? placeholder : throw new ArgumentNullException(nameof(placeholder));
            material = sharedMaterial != null ? sharedMaterial : throw new ArgumentNullException(nameof(sharedMaterial));
            BuildMounts();
            if (isActiveAndEnabled) OnEnable();
        }

        private void OnEnable()
        {
            if (session == null || settings == null) return;
            if (layout == null || ownedRoot == null) BuildMounts();
            session.ProgressionChanged -= Rebind;
            session.ProgressionChanged += Rebind;
            Rebind();
        }
        private void OnDisable()
        {
            if (session != null) session.ProgressionChanged -= Rebind;
            binding?.Dispose(); binding = null;
        }
        private void Rebind()
        {
            binding?.Dispose(); binding = null;
            if (session.Progression != null) binding = new ProgressionMenuBinding(session.Progression, Refresh);
            else Refresh(null); // Editor preview uses existing defaults, never loads/writes a profile.
        }

        private void BuildMounts()
        {
            layout = settings.Build();
            // Only this view's generated child is rebuilt, never the hull or unrelated objects.
            if (ownedRoot != null)
            {
                ownedRoot.gameObject.SetActive(false);
                if (Application.isPlaying) Destroy(ownedRoot.gameObject); else DestroyImmediate(ownedRoot.gameObject);
            }
            mounts.Clear(); modules.Clear();
            ownedRoot = new GameObject("Horizontal attachment mounts").transform;
            ownedRoot.SetParent(transform, false);
            rail = Block("Structural rail", ownedRoot, new Vector2(1, 0.16f), new Color(0.25f, 0.35f, 0.45f), -1).transform;
            foreach (var point in layout.Points)
            {
                ShipPoint position = layout.Position(point.SlotId);
                var mount = new GameObject("Mount " + point.SlotId).transform;
                mount.SetParent(ownedRoot, false); mount.localPosition = new Vector3((float)position.X, 0, 0);
                mounts.Add(point.SlotId, mount);
                Color color = point.SlotId == "primary" ? Color.cyan : point.SlotId == "secondary" ? Color.yellow :
                    point.SlotId == "defense" ? new Color(0.4f, 0.65f, 0.9f) : Color.green;
                modules.Add(point.SlotId, Block("Installed module", mount, new Vector2((float)layout.BayWidth, 0.9f), color, 1));
            }
            Refresh(null);
        }

        private SpriteRenderer Block(string name, Transform parent, Vector2 size, Color color, int order)
        {
            var child = new GameObject(name); child.transform.SetParent(parent, false);
            child.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sharedMaterial = material;
            renderer.color = color; renderer.sortingOrder = order; return renderer;
        }

        private void Refresh(ProgressionMenuView view)
        {
            if (layout == null) return;
            var occupied = new HashSet<string>(StringComparer.Ordinal);
            ValidationError = null;
            if (view == null && !Application.isPlaying)
            {
                foreach (var item in EquipmentConfiguration.Prototype().CreateInitialProfile().Installations) occupied.Add(item.SlotId);
            }
            else if (view != null && view.Loadout != null && view.Loadout.Validation == LoadoutResult.Succeeded)
            {
                // An empty optional bay is valid; do not attach the profile-error else to this inner if.
                foreach (var row in view.Loadout.Slots) if (row.EquipmentId != null) occupied.Add(row.SlotId);
            }
            else ValidationError = "Equipment unavailable/invalid; use the existing recovery or loadout UI.";
            foreach (var entry in modules) entry.Value.enabled = occupied.Contains(entry.Key);
            Bounds = layout.Bounds(occupied);
            rail.localScale = new Vector3((float)Bounds.Width, 0.16f, 1);
            rail.localPosition = new Vector3((float)(Bounds.Left + Bounds.Right) / 2, 0, 0);
        }
    }
}
