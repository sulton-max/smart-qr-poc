/** Static `sms:` — recipient phone plus optional prefilled message. */
export interface SmsContent {
  type: "sms";
  phone: string;
  message?: string;
}
