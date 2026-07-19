import { Temporal } from "temporal-polyfill";

import { ContentType } from "../enums/ContentType";

/** Represents the static iCalendar event content — title and start are required, the rest optional. */
export interface CalendarContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Calendar;

  /** The event title. */
  title: string;

  /** The event start date-time. */
  start: Temporal.PlainDateTime;

  /** The optional event end date-time; when present, later than the start. */
  end?: Temporal.PlainDateTime;

  /** The optional event location. */
  location?: string;

  /** The optional event description. */
  description?: string;
}
