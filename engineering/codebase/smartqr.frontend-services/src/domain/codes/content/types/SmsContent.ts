import { ContentTypeId } from "../registry";

/** Defines the static sms content — a recipient phone plus optional prefilled message. */
export interface SmsContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentTypeId.Sms;

  /** Gets or sets the recipient phone number. */
  phone: string;

  /** Gets or sets the optional prefilled message text. */
  message?: string;
}
