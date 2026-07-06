// application/common/storage — generic, domain-agnostic client-side persistence: the `StorageAdapter` seam
// (localStorage / in-memory), `usePersistentState` (hydrate + write-through + cross-tab sync), and the
// `useRecentItems` MRU list built on it. No feature coupling — any domain slice may consume it.
export * from "./StorageAdapter";
export * from "./usePersistentState";
export * from "./useRecentItems";
