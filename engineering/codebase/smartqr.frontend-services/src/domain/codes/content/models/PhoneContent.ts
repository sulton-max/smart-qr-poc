import { ContentType } from "../ContentType";

/** Defines the static tel dial-link content. */
export interface PhoneContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Phone;

  /** The phone number to dial. */
  phone: string;
}
