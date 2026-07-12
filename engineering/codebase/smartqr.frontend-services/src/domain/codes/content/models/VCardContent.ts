import { ContentType } from "../ContentType";

/** Defines the static vCard contact content — only the first name is required. */
export interface VCardContent {
  /** The content-type discriminator. */
  type: typeof ContentType.VCard;

  /** The contact's first name. */
  firstName: string;

  /** The contact's optional last name. */
  lastName?: string;

  /** The contact's optional organization. */
  org?: string;

  /** The contact's optional job title. */
  title?: string;

  /** The contact's optional phone number. */
  phone?: string;

  /** The contact's optional email address. */
  email?: string;

  /** The contact's optional website URL. */
  url?: string;

  /** The contact's optional postal address. */
  address?: string;

  /** The contact's optional free-text note. */
  note?: string;
}
