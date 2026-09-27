# Managed recovery checkpoints

## Purpose

GameSaveSync may optionally protect games that continuously write their live save but do not provide a reliable native autosave/history mechanism.

This capability is **parallel to synchronization**.

It does not replace the validated central version model and a recovery checkpoint is never a central synchronization version by itself.

Conceptually:

```text
validated central version
        |
        +---- normal synchronization path

live local save
        |
        +---- optional managed recovery checkpoint path
```

A game profile may enable managed checkpoints when the game needs them. Games with sufficient native autosave/recovery do not need this policy.

## Terms

### Validated central version

The latest state accepted through the normal synchronization protocol.

It is the normal recovery baseline and remains authoritative until a later version is safely promoted.

### Local live save

The files currently used by the game.

They may change continuously while the process is running.

### Recovery checkpoint

A temporary recovery candidate captured while the game is running.

A checkpoint:

- records the machine of origin;
- records the central base version it descends from;
- records its capture time;
- contains only the changes needed to reconstruct that recovery state when practical;
- is **not** automatically trusted simply because capture succeeded;
- must never silently replace a validated central version.

## Initial retention direction

Managed recovery should keep a very small rolling set rather than an unbounded history.

Initial target:

```text
current checkpoint
previous checkpoint
```

A new checkpoint is prepared in staging and validated before rotation.

Reason: keeping only one checkpoint creates a risk that the latest capture already contains the corruption that immediately precedes a crash.

The exact storage representation is deferred until the transfer/storage tranches.

## Capture direction

The recovery worker is active only while the configured game is running, but the lightweight GameSave Agent remains available to detect process lifecycle.

Initial target cadence for the Project Zomboid profile: approximately 10 minutes.

A checkpoint must not be published from a blindly copied, actively mutating source.

The later implementation must account for source stability, changed/created files and deletions. Deleted files must be represented explicitly so reconstruction does not confuse "deleted" with "not copied".

## Clean session completion

A normal session does not immediately delete recovery material.

Target flow:

```text
game closes cleanly
→ final local state becomes eligible for normal synchronization
→ transactional PUSH succeeds
→ server publishes the new validated central version
→ client receives success acknowledgement
→ obsolete recovery checkpoints may be deleted
```

The recovery material remains available if the final synchronization cannot be confirmed.

## Unclean termination and user validation

An unclean end makes the local save require explicit validation.

The UI should present all known information in one recovery session rather than reveal one warning at a time.

Typical flow:

```text
unclean termination detected
→ local live save = RequiresValidation
→ recovery checkpoint(s) available
→ user tests current local save first
```

If the current local save works:

```text
user confirms OK
→ local save may become Trusted
→ normal synchronization assessment is recalculated
```

If the current local save is unusable:

```text
user confirms KO
→ current local save is moved to quarantine
→ selected recovery checkpoint is restored locally
→ user launches the game and tests it
```

If the restored checkpoint works:

```text
user confirms OK
→ restored local state may become Trusted
→ normal synchronization assessment is recalculated
→ after successful central promotion/acknowledgement, obsolete checkpoint and quarantine data may be removed
```

If the restored checkpoint is also unusable, another available checkpoint or the last validated central version remains the fallback.

## Quarantine invariant

Recovery must not destructively overwrite the last local state.

Before restoring a checkpoint over the local save, preserve the displaced local state in quarantine.

Quarantine is deleted only after:

1. the restored/current save has been explicitly confirmed usable by the user;
2. the corresponding normal synchronization/promotion has completed successfully;
3. the server acknowledgement is received.

This is deliberately more conservative than simply replacing a save already believed to be broken.

## User authority

Generic technical checks may detect obvious corruption or suspicious conditions, but GameSaveSync cannot prove that an arbitrary game's save is functionally playable.

For V1 recovery, the user remains the final authority for:

- current local save works / does not work;
- restored checkpoint works / does not work.

A future AI/TTS layer may interpret natural-language answers, but it must map them onto the same explicit deterministic actions and must not bypass confirmation.

## Profile policy

Managed recovery is optional per game profile.

Conceptually:

```text
RecoveryPolicy
├── Disabled
└── ManagedCheckpoints
    ├── Interval
    └── Retention
```

Project Zomboid is the first intended profile to exercise managed checkpoints.

No synchronization rule may contain hard-coded Project Zomboid behavior.

## Relationship with H1.2

H1.2 remains the deterministic **normal synchronization assessment**.

Recovery checkpoint availability is a separate concern and must not be required for the normal Local + Central synchronization calculation.

The future UI may combine:

```text
normal synchronization assessment
+
recovery assessment/options
→ one complete user-facing diagnostic/recovery session
```

This preserves generic synchronization for games that do not need managed checkpoints while allowing recovery-aware games to expose additional safe options.

## Expected implementation tranches

This feature spans later responsibilities rather than being pulled into H1:

- H3 Agent: local orchestration boundary;
- H5 Transfers: safe staging/publication primitives;
- H6 Monitoring: changed-file/process observations;
- H7 Lifecycle: clean/unclean session tracking;
- H8 Custodia: real recovery storage;
- H9 Project Zomboid: first real policy/profile validation.

The exact split may be refined when those tranches begin, but the architectural invariants above should remain stable.
