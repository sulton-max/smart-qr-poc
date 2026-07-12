import { ContentType } from "../registry";

/** Defines the static free-text content encoded directly in the QR. */
export interface TextContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentType.Text;

  /** Gets or sets the free-text payload encoded in the QR. */
  text: string;
}
