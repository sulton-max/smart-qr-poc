import { useCallback, useEffect, useRef, useState } from "react";

import { localStorageAdapter, type StorageAdapter } from "./StorageAdapter";

/** Defines the options that tune where and how `usePersistentState` persists. */
export interface PersistentStateOptions {
  /** The persistence seam to read and write through; defaults to `localStorageAdapter`. */
  readonly adapter?: StorageAdapter;
}

/** A React state setter that accepts a next value or an updater fn, mirroring `useState`'s dispatch. */
export type SetPersistentState<T> = (next: T | ((previous: T) => T)) => void;

/** Resolves whether cross-tab sync is available — the `storage` event only fires for the shared `localStorage`, and only in a browser. */
function canSyncAcrossTabs(adapter: StorageAdapter): boolean {
  return adapter === localStorageAdapter && typeof window !== "undefined";
}

/**
 * Manages a piece of React state mirrored into a `StorageAdapter` under `key`. Hydrates from the adapter on
 * mount (SSR-safe: the first render always uses `initial`, then a mount effect adopts any persisted value, so
 * server and client markup match), writes through on every change, and — when backed by `localStorage` —
 * adopts writes from other tabs via the window `storage` event. Returns a `[value, setValue]` tuple; `setValue`
 * accepts a next value or an updater fn, like `useState`.
 */
export function usePersistentState<T>(
  key: string,
  initial: T,
  options?: PersistentStateOptions,
): [T, SetPersistentState<T>] {
  const adapter = options?.adapter ?? localStorageAdapter;

  // `initial` is the first-paint value on both server and client; a mount effect reconciles it with storage,
  // so hydration never diverges. Later `key`/`adapter` swaps are pinned in refs read by the stable setter and
  // sync effect — the setter identity stays constant across renders regardless.
  const [value, setValue] = useState<T>(initial);

  const adapterRef = useRef(adapter);
  adapterRef.current = adapter;

  const keyRef = useRef(key);
  keyRef.current = key;

  // Hydrate from the adapter after mount and whenever the key or adapter changes — a persisted value wins over
  // `initial`; its absence leaves `initial` in place.
  useEffect(() => {
    const persisted = adapter.read<T>(key);
    if (persisted !== null) setValue(persisted);
  }, [adapter, key]);

  const setPersistentState = useCallback<SetPersistentState<T>>((next) => {
    setValue((previous) => {
      const resolved = typeof next === "function" ? (next as (previous: T) => T)(previous) : next;
      adapterRef.current.write(keyRef.current, resolved);
      return resolved;
    });
  }, []);

  // Adopt writes made under the same key in another tab. Guarded so a non-localStorage or SSR context attaches
  // no listener. `event.newValue === null` means the key was removed elsewhere — fall back to `initial`.
  useEffect(() => {
    if (!canSyncAcrossTabs(adapter)) return;

    function onStorage(event: StorageEvent): void {
      if (event.key !== key) return;
      setValue(event.newValue === null ? initial : adapter.read<T>(key) ?? initial);
    }

    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, [adapter, key, initial]);

  return [value, setPersistentState];
}
