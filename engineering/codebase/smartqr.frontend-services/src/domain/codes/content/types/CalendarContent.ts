/** Static iCalendar event — `title` + `start` required, everything else optional. */
export interface CalendarContent {
  type: "calendar";
  title: string;
  start: string;
  end?: string;
  location?: string;
  description?: string;
}
