# Synchronization domain

This folder contains pure synchronization concepts and rules.

It may describe versions, local/central state and deterministic synchronization decisions. It must not contain filesystem access, process detection, HTTP, NAS access, persistence or UI behavior.

A local `Dirty` state means that local data changed after the machine's known `BaseVersion`. It does not mean corruption and does not by itself determine which copy should win.

Timestamps may later be exposed as diagnostic information, but they must not select a winner during a conflict.
