/** Static `geo:` coordinates (lat/long as strings — the wire carries them verbatim). */
export interface GeoContent {
  type: "geo";
  latitude: string;
  longitude: string;
}
