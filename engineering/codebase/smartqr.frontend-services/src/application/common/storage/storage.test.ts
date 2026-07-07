import { afterEach, describe, expect, it } from "vitest";

import { localStorageAdapter, memoryAdapter } from "./StorageAdapter";
import { prependRecent } from "./useRecentItems";

// These tests run in vitest's default `node` environment — there is no DOM, so `window` is genuinely
// `undefined`. That makes the SSR-safe path the real default (no mocking needed) and lets us install a minimal
// fake `window.localStorage` only when exercising the browser path. The hooks are validated through their pure
// cores (`prependRecent`) + the adapter round-trip, since no React renderer (`@testing-library/react`) is a
// dependency of this repo.

/** Builds a spec-minimal in-memory `Storage`, optionally forcing `setItem` to throw (quota simulation). */
function fakeStorage(options?: { throwOnSet?: boolean }): Storage {
  const map = new Map<string, string>();
  return {
    get length() {
      return map.size;
    },
    clear: () => map.clear(),
    getItem: (k: string) => map.get(k) ?? null,
    key: (i: number) => Array.from(map.keys())[i] ?? null,
    removeItem: (k: string) => map.delete(k),
    setItem: (k: string, v: string) => {
      if (options?.throwOnSet) throw new DOMException("QuotaExceededError");
      map.set(k, v);
    },
  } satisfies Storage;
}

/** Installs a fake browser `window` exposing `localStorage`; returns a restore fn. */
function withWindow(storage: Storage): () => void {
  (globalThis as { window?: unknown }).window = { localStorage: storage };
  return () => {
    delete (globalThis as { window?: unknown }).window;
  };
}

afterEach(() => {
  // Guarantee no test leaks a fake `window` into the next (which would break the SSR assertions).
  delete (globalThis as { window?: unknown }).window;
});

describe("memoryAdapter", () => {
  it("persists then rehydrates a value round-trip", () => {
    const adapter = memoryAdapter();
    adapter.write("k", { a: 1, b: ["x", "y"] });
    expect(adapter.read<{ a: number; b: string[] }>("k")).toEqual({ a: 1, b: ["x", "y"] });
  });

  it("returns null for an absent key", () => {
    expect(memoryAdapter().read("missing")).toBeNull();
  });

  it("removes a stored value", () => {
    const adapter = memoryAdapter();
    adapter.write("k", 1);
    adapter.remove("k");
    expect(adapter.read("k")).toBeNull();
  });

  it("isolates instances — no shared backing store", () => {
    const a = memoryAdapter();
    const b = memoryAdapter();
    a.write("k", "in-a");
    expect(b.read("k")).toBeNull();
  });
});

describe("localStorageAdapter — SSR safety (no window)", () => {
  it("read returns null without throwing", () => {
    expect(typeof window).toBe("undefined");
    expect(() => localStorageAdapter.read("k")).not.toThrow();
    expect(localStorageAdapter.read("k")).toBeNull();
  });

  it("write and remove are no-ops without throwing", () => {
    expect(() => localStorageAdapter.write("k", 1)).not.toThrow();
    expect(() => localStorageAdapter.remove("k")).not.toThrow();
  });
});

describe("localStorageAdapter — browser path", () => {
  it("persists then rehydrates through window.localStorage", () => {
    const restore = withWindow(fakeStorage());
    try {
      localStorageAdapter.write("k", { hello: "world" });
      expect(localStorageAdapter.read<{ hello: string }>("k")).toEqual({ hello: "world" });
    } finally {
      restore();
    }
  });

  it("swallows a quota error on write", () => {
    const restore = withWindow(fakeStorage({ throwOnSet: true }));
    try {
      expect(() => localStorageAdapter.write("k", "big")).not.toThrow();
    } finally {
      restore();
    }
  });

  it("returns null (no throw) on a malformed JSON payload", () => {
    const storage = fakeStorage();
    storage.setItem("k", "{ not json");
    const restore = withWindow(storage);
    try {
      expect(localStorageAdapter.read("k")).toBeNull();
    } finally {
      restore();
    }
  });
});

describe("prependRecent — MRU dedupe + cap", () => {
  const identify = (n: number): string => String(n);

  it("prepends most-recent-first", () => {
    let list: ReadonlyArray<number> = [];
    list = prependRecent(list, 1, identify, 24);
    list = prependRecent(list, 2, identify, 24);
    list = prependRecent(list, 3, identify, 24);
    expect(list).toEqual([3, 2, 1]);
  });

  it("de-duplicates by identity, resurfacing the repeat at the front", () => {
    let list: ReadonlyArray<number> = prependRecent([], 1, identify, 24);
    list = prependRecent(list, 2, identify, 24);
    list = prependRecent(list, 1, identify, 24);
    expect(list).toEqual([1, 2]);
  });

  it("caps at max, dropping the oldest", () => {
    let list: ReadonlyArray<number> = [];
    for (let i = 1; i <= 5; i++) list = prependRecent(list, i, identify, 3);
    expect(list).toEqual([5, 4, 3]);
  });

  it("dedupes by a custom identity over object items", () => {
    type Item = { id: string; label: string };
    const byId = (item: Item): string => item.id;
    let list: ReadonlyArray<Item> = [];
    list = prependRecent(list, { id: "a", label: "first" }, byId, 24);
    list = prependRecent(list, { id: "a", label: "second" }, byId, 24);
    expect(list).toEqual([{ id: "a", label: "second" }]);
  });
});

describe("useRecentItems persistence — via memoryAdapter round-trip", () => {
  // Mirrors what the hook writes through its adapter: the reduced MRU list serialized under a key, read back
  // by a fresh mount. Exercises persist → rehydrate + dedupe + cap without a React renderer.
  const identify = (s: string): string => s;

  it("rehydrates the reduced recents from the adapter under the key", () => {
    const adapter = memoryAdapter();
    const key = "smart-qr:test:recents";

    let list: ReadonlyArray<string> = adapter.read<ReadonlyArray<string>>(key) ?? [];
    for (const item of ["a", "b", "a", "c"]) {
      list = prependRecent(list, item, identify, 24);
      adapter.write(key, list);
    }

    // A fresh reader (new mount) sees the persisted, de-duplicated, most-recent-first list.
    expect(adapter.read<ReadonlyArray<string>>(key)).toEqual(["c", "a", "b"]);
  });

  it("clear empties the persisted list", () => {
    const adapter = memoryAdapter();
    const key = "smart-qr:test:recents";
    adapter.write(key, prependRecent<string>([], "a", identify, 24));
    adapter.write(key, []);
    expect(adapter.read<ReadonlyArray<string>>(key)).toEqual([]);
  });
});
