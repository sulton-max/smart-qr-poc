import { ContentTypeId } from "../registry";

/** Defines the static geo-coordinate content — latitude and longitude carried verbatim as strings. */
export interface GeoContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentTypeId.Geo;

  /** Gets or sets the latitude coordinate as a string. */
  latitude: string;

  /** Gets or sets the longitude coordinate as a string. */
  longitude: string;
}
