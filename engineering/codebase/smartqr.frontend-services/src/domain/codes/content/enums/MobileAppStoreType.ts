/** Defines which app store a mobile-app link points at. */
export const MobileAppStoreType = {
  /** Apple App Store. */
  AppStore: "appStore",

  /** Google Play. */
  PlayStore: "playStore",

  /** Any other store or a direct download page. */
  Other: "other",
} as const;

export type MobileAppStoreType = (typeof MobileAppStoreType)[keyof typeof MobileAppStoreType];
