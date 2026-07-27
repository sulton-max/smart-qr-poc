import { ContentType } from "../enums/ContentType";

/** Represents sms content — opens the scanner's SMS composer addressed to the number. */
export interface SmsContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Sms;

  /** The recipient phone number. */
  phone: string;

  /** The message text prefilled in the composer. */
  message?: string;
}
