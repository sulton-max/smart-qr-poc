import { ContentTypeId } from "../registry";

/** Defines the static Wi-Fi join content — network credentials plus a hidden-SSID flag. */
export interface WifiContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentTypeId.Wifi;

  /** Gets or sets the network SSID. */
  ssid: string;

  /** Gets or sets the optional network password. */
  password?: string;

  /** Gets or sets the optional encryption type (e.g. WPA, WEP). */
  encryption?: string;

  /** Gets or sets whether the network SSID is hidden. */
  hidden: boolean;
}
