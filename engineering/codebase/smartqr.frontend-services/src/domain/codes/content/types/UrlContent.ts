/** Dynamic forwarder — the QR carries the redirect short link to this destination. */
export interface UrlContent {
  type: "url";
  url: string;
}
