import { ContentType } from "../enums/ContentType";

/** Represents the static tel dial-link content. */
export interface PhoneContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Phone;

  /** The phone number to dial. */
  phone: string;
}
