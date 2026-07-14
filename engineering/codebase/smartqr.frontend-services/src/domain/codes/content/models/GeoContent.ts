import { ContentType } from "../enums/ContentType";

/** Represents the static geo-coordinate content — latitude and longitude carried verbatim as strings. */
export interface GeoContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Geo;

  /** The latitude coordinate as a string. */
  latitude: string;

  /** The longitude coordinate as a string. */
  longitude: string;
}
