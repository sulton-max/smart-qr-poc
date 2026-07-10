import { ContentTypeId } from "../registry";

/** Defines the static iCalendar event content — title and start are required, the rest optional. */
export interface CalendarContent {
  /** Gets or sets the content-type discriminator. */
  type: typeof ContentTypeId.Calendar;

  /** Gets or sets the event title. */
  title: string;

  /** Gets or sets the event start date-time. */
  start: string;

  /** Gets or sets the optional event end date-time. */
  end?: string;

  /** Gets or sets the optional event location. */
  location?: string;

  /** Gets or sets the optional event description. */
  description?: string;
}
