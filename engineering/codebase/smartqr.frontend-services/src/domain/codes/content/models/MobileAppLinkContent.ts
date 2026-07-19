import { ContentType } from "../enums/ContentType";
import { MobileAppStoreType } from "../enums/MobileAppStoreType";

/** Represents one app-store link — carried by a rule, which supplies the device condition that selects it. */
export interface MobileAppLinkContent {
  /** The content-type discriminator. */
  type: typeof ContentType.MobileApp;

  /** The store this link points at. */
  store: MobileAppStoreType;

  /** The store URL. */
  url: string;
}
