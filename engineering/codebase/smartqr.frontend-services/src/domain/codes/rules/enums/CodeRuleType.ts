/** Defines the role a rule plays in a code's routing — the wire discriminator for the rule hierarchy. */
export const CodeRuleType = {
  /** A rule matched against a scan signal, in order. */
  Conditional: "conditional",

  /** The catch-all serving its own content when no conditional rule matches. */
  Default: "default",

  /** The catch-all delegating to another rule's content. */
  DefaultPointer: "defaultPointer",
} as const;

export type CodeRuleType = (typeof CodeRuleType)[keyof typeof CodeRuleType];
