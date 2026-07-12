import { ContentType } from "../registry";

/** Defines the static tel dial-link content. */
export interface PhoneContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentType.Phone;

  /** Gets or sets the phone number to dial. */
  phone: string;
}
