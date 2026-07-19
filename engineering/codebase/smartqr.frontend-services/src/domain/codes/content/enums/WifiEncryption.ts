/** Defines the Wi-Fi encryption scheme a network uses — the `T:` field of the WIFI payload. */
export const WifiEncryption = {
  /** WPA / WPA2 / WPA3 personal. */
  Wpa: "wpa",

  /** Legacy WEP. */
  Wep: "wep",

  /** An open network, carrying no password. */
  None: "none",
} as const;

export type WifiEncryption = (typeof WifiEncryption)[keyof typeof WifiEncryption];
