import { ContentType } from "../ContentType";

/** Defines the static free-text content encoded directly in the QR. */
export interface TextContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Text;

  /** The free-text payload encoded in the QR. */
  text: string;
}
