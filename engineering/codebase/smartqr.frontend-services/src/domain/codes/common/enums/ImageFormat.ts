/** Defines the downloadable code image format. */
export const ImageFormat = {
  /** Refers to a vector SVG render. */
  Svg: "svg",
  /** Refers to a raster PNG render. */
  Png: "png",
} as const;

export type ImageFormat = (typeof ImageFormat)[keyof typeof ImageFormat];
