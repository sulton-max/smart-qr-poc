import { ContentType } from "../enums/ContentType";

/** Represents the static geo-coordinate content — a latitude / longitude pair. */
export interface GeoContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Geo;

  /** The latitude coordinate, in the range -90 to 90. */
  latitude: number;

  /** The longitude coordinate, in the range -180 to 180. */
  longitude: number;
}
