/** Defines a top-level emoji picker category, in display order (Telegram / Windows-keyboard parity). */
export const EmojiCategory = {
  /** Refers to smileys, emotion, and people. */
  SmileysPeople: "smileys-people",
  /** Refers to animals and nature. */
  AnimalsNature: "animals-nature",
  /** Refers to food and drink. */
  FoodDrink: "food-drink",
  /** Refers to activities, sport, and events. */
  Activity: "activity",
  /** Refers to travel and places. */
  TravelPlaces: "travel-places",
  /** Refers to objects. */
  Objects: "objects",
  /** Refers to symbols. */
  Symbols: "symbols",
  /** Refers to flags. */
  Flags: "flags",
} as const;

export type EmojiCategory = (typeof EmojiCategory)[keyof typeof EmojiCategory];
