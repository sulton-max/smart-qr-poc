import { ContentType } from "../ContentType";

/** Defines the static Wi-Fi join content — network credentials plus a hidden-SSID flag. */
export interface WifiContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Wifi;

  /** The network SSID. */
  ssid: string;

  /** The optional network password. */
  password?: string;

  /** The optional encryption type (e.g. WPA, WEP). */
  encryption?: string;

  /** Whether the network SSID is hidden. */
  hidden: boolean;
}
