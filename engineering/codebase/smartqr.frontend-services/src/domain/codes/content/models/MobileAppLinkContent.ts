import { ContentType } from "../enums/ContentType";

/** Represents the dynamic device-routed app-link content — the server derives the App Store / Google Play / fallback rules from these links. */
export interface MobileAppLinkContent {
  /** The content-type discriminator. */
  type: typeof ContentType.MobileApp;

  /** The App Store URL for iOS devices. */
  appStore?: string;

  /** The Google Play URL for Android devices. */
  playStore?: string;

  /** The alternate store URL for other platforms. */
  other?: string;

  /** The fallback URL when no device rule matches. */
  fallback?: string;
}
