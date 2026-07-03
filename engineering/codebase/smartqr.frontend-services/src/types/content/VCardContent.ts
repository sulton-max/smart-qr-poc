/** Static vCard contact — only `firstName` is required. */
export interface VCardContent {
  type: "vcard";
  firstName: string;
  lastName?: string;
  org?: string;
  title?: string;
  phone?: string;
  email?: string;
  url?: string;
  address?: string;
  note?: string;
}
