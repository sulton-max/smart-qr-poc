import { ContentType } from "../ContentType";

/** Defines the static sms content — a recipient phone plus optional prefilled message. */
export interface SmsContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Sms;

  /** The recipient phone number. */
  phone: string;

  /** The optional prefilled message text. */
  message?: string;
}
