/** Static `mailto:` — recipient plus optional subject/body. */
export interface EmailContent {
  type: "email";
  to: string;
  subject?: string;
  body?: string;
}
