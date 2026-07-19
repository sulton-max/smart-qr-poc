import { ContentType } from "../enums/ContentType";
import { WifiEncryption } from "../enums/WifiEncryption";

/** Represents the static Wi-Fi join content — network credentials plus a hidden-SSID flag. */
export interface WifiContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Wifi;

  /** The network SSID. */
  ssid: string;

  /** The network password; absent on an open network. */
  password?: string;

  /** The encryption scheme the network uses. */
  encryption: WifiEncryption;

  /** Whether the network SSID is hidden. */
  hidden: boolean;
}
