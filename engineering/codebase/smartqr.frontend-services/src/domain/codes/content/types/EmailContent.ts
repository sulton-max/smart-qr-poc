import { ContentTypeId } from "../registry";

/** Defines the static mailto content — a recipient plus optional subject and body. */
export interface EmailContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentTypeId.Email;

  /** Gets or sets the recipient email address. */
  to: string;

  /** Gets or sets the optional prefilled subject line. */
  subject?: string;

  /** Gets or sets the optional prefilled message body. */
  body?: string;
}
