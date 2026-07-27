import { Temporal } from "temporal-polyfill";

import { ContentType } from "../enums/ContentType";

/** Represents iCalendar event content — the scanner adds the event to their calendar. */
export interface CalendarContent {
  /** The content-type discriminator. */
  type: typeof ContentType.Calendar;

  /** The event title. */
  title: string;

  /** The event start date-time. */
  start: Temporal.PlainDateTime;

  /** The event end date-time; later than the start when given. */
  end?: Temporal.PlainDateTime;

  /** The event location. */
  location?: string;

  /** The event description. */
  description?: string;
}
