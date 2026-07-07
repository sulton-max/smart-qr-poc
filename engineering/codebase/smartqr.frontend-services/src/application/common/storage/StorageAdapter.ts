// Generic, emoji-agnostic client-side persistence seam. A `StorageAdapter` is the sole surface the
// persistence hooks touch, so a component tree can swap `localStorage` for an in-memory map (tests) or a
// no-op (SSR) without any hook change. JSON is the wire format; every method is failure-tolerant — a quota,
// serialization, or parse error degrades to a miss, never a throw (the GWDNBM "never bother me" stance).

/** Defines a JSON key/value persistence seam the storage hooks read and write through. */
export interface StorageAdapter {
  /** Reads and parses the value stored under `key`, or null when absent or unreadable. */
  read<T>(key: string): T | null;

  /** Serializes `value` to JSON and writes it under `key`; a quota or serialization failure is swallowed. */
  write<T>(key: string, value: T): void;

  /** Removes any value stored under `key`. */
  remove(key: string): void;
}

/** Resolves the ambient `Storage` (browser `localStorage`), or null when unavailable — SSR, private-mode, or a security exception. */
function resolveLocalStorage(): Storage | null {
  // `typeof window === "undefined"` guards SSR; the try/catch guards a `localStorage` getter that throws
  // (Safari private mode, sandboxed iframes) rather than merely being absent.
  if (typeof window === "undefined") return null;

  try {
    return window.localStorage;
  } catch {
    return null;
  }
}

/**
 * The production adapter — persists through the browser's `localStorage` as JSON, SSR-safe and failure-tolerant.
 * A read that is absent, malformed, or blocked returns null; a write that exceeds quota or fails to serialize is
 * swallowed. Never throws, so a caller never has to guard the storage call.
 */
export const localStorageAdapter: StorageAdapter = {
  read<T>(key: string): T | null {
    const store = resolveLocalStorage();
    if (store === null) return null;

    try {
      const raw = store.getItem(key);
      return raw === null ? null : (JSON.parse(raw) as T);
    } catch {
      return null;
    }
  },

  write<T>(key: string, value: T): void {
    const store = resolveLocalStorage();
    if (store === null) return;

    try {
      store.setItem(key, JSON.stringify(value));
    } catch {
      // Quota exceeded or value not serializable — drop the write silently.
    }
  },

  remove(key: string): void {
    const store = resolveLocalStorage();
    if (store === null) return;

    try {
      store.removeItem(key);
    } catch {
      // Removal blocked — nothing to recover.
    }
  },
};

/** Builds an isolated in-memory `StorageAdapter` backed by a `Map` — the test double, with no cross-tab sync and no DOM. */
export function memoryAdapter(): StorageAdapter {
  const store = new Map<string, string>();

  return {
    read<T>(key: string): T | null {
      const raw = store.get(key);
      if (raw === undefined) return null;

      try {
        return JSON.parse(raw) as T;
      } catch {
        return null;
      }
    },

    write<T>(key: string, value: T): void {
      try {
        store.set(key, JSON.stringify(value));
      } catch {
        // Mirror the localStorage adapter: an unserializable value is dropped, not thrown.
      }
    },

    remove(key: string): void {
      store.delete(key);
    },
  };
}
