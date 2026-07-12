import { ContentType } from "../registry";

/** Defines the static vCard contact content — only the first name is required. */
export interface VCardContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentType.VCard;

  /** Gets or sets the contact's first name. */
  firstName: string;

  /** Gets or sets the contact's optional last name. */
  lastName?: string;

  /** Gets or sets the contact's optional organization. */
  org?: string;

  /** Gets or sets the contact's optional job title. */
  title?: string;

  /** Gets or sets the contact's optional phone number. */
  phone?: string;

  /** Gets or sets the contact's optional email address. */
  email?: string;

  /** Gets or sets the contact's optional website URL. */
  url?: string;

  /** Gets or sets the contact's optional postal address. */
  address?: string;

  /** Gets or sets the contact's optional free-text note. */
  note?: string;
}
