import { ContentType } from "../enums/ContentType";

/** Represents the dynamic URL-forwarder content — the QR carries the redirect short link to this destination. */
export interface UrlContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Url;

  /** The destination URL the short link forwards to. */
  url: string;
}
