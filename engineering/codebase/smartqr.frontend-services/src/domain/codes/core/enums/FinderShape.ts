// Finder (positioning) marker shapes. `finderShape` = the outer eye frame,
// `finderDotShape` = the inner eye pupil. Both mirror the backend, default `square`.
export const FinderShape = {
  Square: "square",
  Rounded: "rounded",
  Circle: "circle",
} as const;
export type FinderShape = (typeof FinderShape)[keyof typeof FinderShape];
