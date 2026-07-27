import { ContentType } from "../enums/ContentType";

/** Represents mailto content — opens the scanner's mail client with the message prefilled. */
export interface EmailContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Email;

  /** The recipient email address. */
  to: string;

  /** The subject line prefilled in the composer. */
  subject?: string;

  /** The body text prefilled in the composer. */
  body?: string;
}
