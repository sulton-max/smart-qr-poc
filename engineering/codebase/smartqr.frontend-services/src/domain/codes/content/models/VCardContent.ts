import { ContentType } from "../enums/ContentType";

/** Represents vCard contact content — the scanner saves it straight to their contacts. */
export interface VCardContent {
  /** The content-type discriminator. */
  type: typeof ContentType.VCard;

  /** The contact's first name. */
  firstName: string;

  /** The contact's last name. */
  lastName?: string;

  /** The contact's organization. */
  org?: string;

  /** The contact's job title. */
  title?: string;

  /** The contact's phone number. */
  phone?: string;

  /** The contact's email address. */
  email?: string;

  /** The contact's website URL. */
  url?: string;

  /** The contact's postal address. */
  address?: string;

  /** The contact's free-text note. */
  note?: string;
}
