/** Static WiFi join — `hidden` is a real bool (the only non-string field). */
export interface WifiContent {
  type: "wifi";
  ssid: string;
  password?: string;
  encryption?: string;
  hidden: boolean;
}
