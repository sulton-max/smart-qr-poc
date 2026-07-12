import { ContentType } from "../registry";

/** Defines the dynamic URL-forwarder content — the QR carries the redirect short link to this destination. */
export interface UrlContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentType.Url;

  /** Gets or sets the destination URL the short link forwards to. */
  url: string;
}
