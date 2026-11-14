# Space redesign Task 2: independent combat

Based on Task 1 commit `0f0f435ca1d54b79c022b6c5b66d61b03423b33c`,
branch `development/space-independent-combat`, Unity 6000.6.7f1.
The PR targets `development/space-redesign-foundation` (#10); earlier dependencies
remain unmerged. The existing project, inventory and persistence owner are retained.

## Architecture and compatibility

| Responsibility | Reuse / adaptation |
| --- | --- |
| Route, fixed driver, wave roster, boss/outcome ordering | GameRun / RouteMotion / OutpostSession / LevelProgression |
| Permanent Credits, upgrades, owned items and installed slots | Existing ProgressionController/Service and immutable RunLoadout |
| Malformed/unsupported save handling, recovery, atomic writes, stale writers | Existing schema-v2 persistence, unchanged |
| Ship topology and portrait camera | Task 1 HorizontalShipLayout / ShipAssemblyView / ShipCameraFraming |
| Space combat and collision | Optional SpaceCombatConfiguration / SpaceCombatRun; legacy RunCombat stays intact |
| Space rendering | EnemyActor's optional XY model binding and a separate pooled SpaceCombatView |
| Safe scene generation | Existing preflight, explicit dirty-scene consent, transactional replacement and rollback |

GameRun's optional space configuration defaults to null. The serialized legacy
scene has no new settings reference and continues using +X movement, direct-hit
Weapon, Energy drops, ammunition and tactical purchases. The verified legacy
scene/prefabs/configuration must not be regenerated. Space keeps its existing
`SpaceFoundation.unity` path and scene GUID; safe regeneration updates its wiring.

LoadoutRunBuilder carries the optional configuration and scales space damage by
the same permanent/equipment multiplier, rounding once away from zero. Geometry,
weapon definitions and run equipment are immutable snapshots. No schema, owned ID,
slot ID, reward entitlement or migration is added. Invalid installed weapon
definitions fail before run registration or a save write.

## Weapons and cooldowns

The mandatory hull supplies a built-in forward laser, just as its hull itself is
not a new owned item. Existing `basic-cannon` and `missile-launcher` equipment
activate the other two emitters at their actual Task 1 mounts. Optional removal
removes its emitter and footprint; ownership is preserved. This gives the default
ship three independent weapons without inventing a second inventory or granting
new saved items. Armor retains its stat effect. Reactor/Energy and ammunition
capacity retain their legacy effects only; they currently give no space benefit.

Each definition also supports one to three independent emitters in its existing
module footprint. Twin/triple emitters share the same immutable equipment weapon
definition but have distinct IDs, muzzle offsets and clocks. This is an installed
battery, not duplicated ownership; the existing legacy prohibition on installing a
second unsupported persistent firing channel remains intact. Defaults use one
emitter per definition. Core and Unity tests explicitly exercise twin batteries.

SpaceCombatSettings contains the inspector-editable hull definition and
equipment-keyed weapon definitions. Core defaults seed new assets only:

| Definition | Damage | Cooldown (s) | Range | Full arc | Speed | Facing |
| --- | ---: | ---: | ---: | ---: | ---: | --- |
| Hull laser | 5 | 0.25 | 7 | 100 degrees | 24 | Forward |
| Equipped cannon | 24 | 0.9 | 6 | 220 degrees | 13 | Outboard |
| Equipped missile | 55 | 2.4 | 8 | 220 degrees | 9 | Outboard |

Ranges are world units from the muzzle, speeds world units/second. Numeric values
are provisional configuration, not constants scattered through simulation code.
Missiles are straight ballistic projectiles in this task, not homing missiles.

Each SpaceWeaponInstance owns its own next-fire timestamp and shot count, even
when instances share a definition. A new emitter starts ready. Only successful
projectile emission starts its cooldown. Losing a target, a full projectile pool
or a disabled emitter never resets the timestamp. Cooldown time continues while
disabled; reenabling can fire immediately if it has expired. Removing/destroying
an emitter removes only that transient channel. Its already launched projectiles
continue until impact/range expiry. Run equipment changes remain prohibited.

Space GameRun advances in 1/120-second quanta, carrying the remainder across input
frames and never dropping elapsed time. Every emitter checks its own timestamp;
there is no shared cooldown. Quantization can delay a shot by less than one quantum
but cannot make it fire early beyond floating-point boundary tolerance. Long input
frames advance ordinary simulation steps, not a burst of overlapping instant hits.
Legacy time-jump behavior is unchanged. No space shot, command or drop requires,
spends or awards shared Energy or ammunition. Legacy tactical commands return
NoEffect in an active space run; the space HUD replaces them with cooldown status.

## Local firing geometry and targeting

The ship remains unrotated with forward +Y in the prototype. Geometry explicitly
transforms local positions/directions by a forward basis, so forward/left/right/rear
are ship-relative. Outboard facing resolves to the side of the actual mount.
Swapping a side mount reverses its coverage without rewriting inventory.

Alive, active enemies must have fully entered the gameplay camera, be within
maximum range and satisfy the configured full cone angle. Entry is an explicit
acquisition condition: otherwise long-range side guns could erase enemies before
the player saw their approach. Waiting offscreen never consumes a ready cooldown.
A dot/cosine comparison avoids acos instability. Boundaries are inclusive
with numerical tolerance; coincident targets have no defined angle and are eligible.
Zero/nonfinite directions or invalid range/arc configuration are rejected. No
prototype weapon has omnidirectional coverage. Broad side cones cover their own
flank and portions of front/rear, leaving close rear blind spots. The forward laser
cannot cover those blind spots. This is deliberate placement/coverage tradeoff.

Each emitter independently selects the closest eligible enemy from its muzzle;
exact distance ties use ascending stable roster ID, independent of list order.
Dead, queued and null targets are excluded. Launch position is the actual local
mount (hull laser at local Y=0.8), and velocity aims at that target's current world
position. There is no steering, aim input, manual fire, lead prediction or homing.

## Arrivals, movement and collision

The existing three waves and final boss retain their enemy definitions, IDs and
health. Space arrival settings use a seeded cyclic distribution of front/left/
right/rear, with repeated sides acting as weights. A local integer hash adds
deterministic tangential jitter; neither Unity Random nor a global RNG is touched.
The boss is explicitly a front arrival. This is reproducible distribution, not a
claim of independent stochastic sampling.

New arrivals start beyond the actual orthographic half extents plus enemy radius
and configured safety margin. Side placement also considers maximum configured
ship extent. Camera extents are supplied by SpaceCombatView after portrait framing;
reference defaults cover headless simulation and the first frame. Changing the
viewport affects future arrivals, never teleports existing enemies. Arrivals are
staggered, at most eight enemies are active, and a bounded queue retains the wave
roster. Non-boss lifetime is 90 seconds; expired enemies are cleaned up without an
Energy drop. The boss never expires into an unearned victory.

Ordinary enemies approach the advancing hull and ram; one contact applies their
configured attack damage and consumes that enemy. The boss approaches its configured
standoff distance and fires red projectiles on its own attack schedule. Speeds,
boss attack parameters and wave counts remain in PrototypeSettings. The committed
space tuning deliberately changes the copied boss attack range from 3 to 6 world
units so it returns visible fire against the default loadout; legacy tuning is
unchanged. Safe regeneration preserves that inspector value. Arrival and
projectile limits remain in SpaceCombatSettings. Movement has no obstacle routing.

Collision is entirely core-owned. Enemy circular bodies use radii 0.38 (normal)
and 0.75 (boss). Ship collision is the union of the hull/engine rectangle, installed
module rectangles and connecting rail. Modules share the hull's existing armored
Health; component-specific damage is deferred. Swept segment/circle and expanded
rectangle checks include motion of both participants, so fast objects do not tunnel
through wider ships. A projectile hits the first enemy along its path, with ID
tie-breaks, and is consumed once. Enemy projectiles test only the ship, player
projectiles only enemies. Contact across overlapping hull boxes is aggregated to
one event, and no Unity collider callbacks apply additional damage.

The core projectile pool is bounded (128 default); view objects reuse those slots
across restarts. A full pool leaves an emitter ready rather than spending cooldown.
Space presentation has its own hierarchy: legacy CombatFeedback clears only its
own descendants on menu/restart. All projectiles clear between waves and on terminal
outcomes. Existing LevelProgression retains defeat priority over simultaneous boss
death, and the progression service remains the only reward/transaction authority.

## Manual Unity checkpoint

1. Open the project with **Unity 6000.6.7f1** and wait for compilation/import.
2. Open `Assets/SpacePrototypeGenerated/Scenes/SpaceFoundation.unity` alone. Do not
   regenerate IndustrialWasteland. In Game view choose Fixed Resolution **540x960**.
3. Press Play, then START LEVEL 1. Observe automatic forward travel with no steering.
   Cyan hull shots, orange side cannon shots and yellow missiles should launch
   automatically, with independent READY/recharge labels at the bottom.
4. Observe left/right/rear arrivals, their approach, missed shots and impacts. Check
   that projectiles start at the visible mount, side guns cover their own flank,
   health bars respond, and the hull remains centered. Early arrivals can begin
   beyond the camera or behind the retained HUD; this is not final threat UI.
5. Let the waves and front boss play out. Red boss shots damage the hull. Verify
   victory/defeat feedback and inspect Console for new messages. The default loadout
   wins in the recorded deterministic automated encounter; weaker configurations
   can lose. No space repair/ammunition/special purchase is available.
6. Return to the menu, inspect saved Credits, buy a permanent upgrade if affordable,
   unequip the optional missile module and start again. Observe reduced right/rear
   coverage and the missing module. Reload to inspect persistence. These are real
   profile transactions in normal Play Mode; automated tests use isolated stores.
7. For a repeatable defeat, start a fresh run, select the **Last Outpost Space
   Foundation** root in the Hierarchy, then click **Last Outpost > Verification >
   Disarm Selected Space Run**. This Editor-only aid disables that selected run's
   transient emitters; it changes no settings or ownership and does not assign an
   outcome. Let ordinary enemy attacks produce SHIP LOST and verify no Credits
   reward. Start another run to restore the configured emitters. This tool is not
   included as a player control or manual firing feature.

No human visual acceptance, IMGUI capture, Android behavior, obstacle routing,
planetary environments or exploration implementation is claimed. See the Task 2
verification report for executed results and any unresolved issues before Task 3.

Cloud Unity is still pinned to 6000.3. Its existing exact-branch exclusion also
covers this locally verified 6000.6 branch; Core and Graphify CI remain enabled.
No Android workflow is dispatched and no cloud Unity result substitutes for the
recorded installed-Editor runs.
