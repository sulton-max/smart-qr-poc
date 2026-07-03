/** Dynamic + device-routed — server derives the App Store / Google Play / fallback rules from these links. */
export interface MobileAppLinkContent {
  type: "mobileApp";
  appStore?: string;
  playStore?: string;
  other?: string;
  fallback?: string;
}
