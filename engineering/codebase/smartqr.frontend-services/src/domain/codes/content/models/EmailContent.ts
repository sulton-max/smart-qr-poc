import { ContentType } from "../ContentType";

/** Defines the static mailto content — a recipient plus optional subject and body. */
export interface EmailContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Email;

  /** The recipient email address. */
  to: string;

  /** The optional prefilled subject line. */
  subject?: string;

  /** The optional prefilled message body. */
  body?: string;
}
