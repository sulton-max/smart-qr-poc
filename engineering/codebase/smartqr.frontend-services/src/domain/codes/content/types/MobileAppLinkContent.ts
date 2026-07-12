import { ContentType } from "../registry";

/** Defines the dynamic device-routed app-link content — the server derives the App Store / Google Play / fallback rules from these links. */
export interface MobileAppLinkContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentType.MobileApp;

  /** Gets or sets the App Store URL for iOS devices. */
  appStore?: string;

  /** Gets or sets the Google Play URL for Android devices. */
  playStore?: string;

  /** Gets or sets the alternate store URL for other platforms. */
  other?: string;

  /** Gets or sets the fallback URL when no device rule matches. */
  fallback?: string;
}
